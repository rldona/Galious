using System;

namespace MazeOfKnight.Game
{
    // Emulación del PSG del MSX (AY-3-8910 a 1,789772 MHz; docs/music.md): 3 canales de tono, ruido y envolvente, a
    // partir de sus 14 registros. Genera muestras mono en [-1, 1]. Clase sin Unity para poder probarla.
    public class Psg
    {
        public const double Clock = 3579545 / 2.0;
        public readonly byte[] Regs = new byte[14];

        readonly double step;            // ciclos del reloj /8 por muestra
        readonly int[] toneCount = new int[3];
        readonly bool[] toneOut = new bool[3];
        int noiseCount, lfsr = 1;
        bool noiseOut;
        int envCount, envStep, envHeld;
        bool envAttack, envDone;
        double acc;

        // Volúmenes del AY: escala logarítmica de ~1,5 dB por paso (16 niveles).
        static readonly float[] Volume = BuildVolume();

        static float[] BuildVolume()
        {
            var v = new float[16];
            for (int i = 1; i < 16; i++) v[i] = (float)Math.Pow(10, (i - 15) * 1.5 / 20);
            return v;
        }

        public Psg(int sampleRate) => step = Clock / 8 / sampleRate;

        // Escribe un registro (R13 reinicia la envolvente, como en el chip).
        public void Write(int reg, byte value)
        {
            Regs[reg] = value;
            if (reg == 13) StartEnvelope();
        }

        void StartEnvelope()
        {
            envAttack = (Regs[13] & 4) != 0;
            envStep = 0;
            envDone = false;
            envCount = 0;
        }

        int EnvelopeLevel() => envDone ? envHeld : envAttack ? envStep : 15 - envStep;

        // Fin de un ciclo de 16 pasos. R13: bit 3 seguir, bit 2 subida, bit 1 alternar, bit 0 mantener.
        void EnvelopeCycleEnd()
        {
            int shape = Regs[13] & 15;
            bool cont = (shape & 8) != 0, alt = (shape & 2) != 0, hold = (shape & 1) != 0;
            if (!cont) { envDone = true; envHeld = 0; return; }
            if (hold) { envDone = true; envHeld = (envAttack ^ alt) ? 15 : 0; return; }
            envStep = 0;
            if (alt) envAttack = !envAttack;
        }

        // Una muestra. Se itera a reloj/8: el tono cambia de nivel cada periodo (f = reloj / 16·periodo), el ruido se
        // desplaza cada 2·periodo (reloj/16 por unidad) y la envolvente da un paso cada 2·periodo (16 ciclos por unidad).
        public float Sample()
        {
            acc += step;
            int ticks = (int)acc;
            acc -= ticks;
            float sum = 0;
            int samples = 0;
            for (int t = 0; t < ticks; t++)
            {
                for (int c = 0; c < 3; c++)
                {
                    int period = (Regs[c * 2] | (Regs[c * 2 + 1] & 15) << 8);
                    if (period == 0) period = 1;
                    if (++toneCount[c] >= period) { toneCount[c] = 0; toneOut[c] = !toneOut[c]; }
                }
                int np = Regs[6] & 31;
                if (np == 0) np = 1;
                if (++noiseCount >= np * 2)
                {
                    noiseCount = 0;
                    int bit = (lfsr ^ (lfsr >> 3)) & 1;
                    lfsr = (lfsr >> 1) | (bit << 16);
                    noiseOut = (lfsr & 1) != 0;
                }
                int ep = Regs[11] | Regs[12] << 8;
                if (ep == 0) ep = 1;
                if (!envDone && ++envCount >= ep * 2)
                {
                    envCount = 0;
                    if (++envStep > 15) EnvelopeCycleEnd();
                }
                float s = 0;
                byte mixer = Regs[7];
                for (int c = 0; c < 3; c++)
                {
                    bool toneOn = (mixer & (1 << c)) == 0, noiseOn = (mixer & (8 << c)) == 0;
                    bool output = (!toneOn || toneOut[c]) && (!noiseOn || noiseOut);
                    int vol = Regs[8 + c];
                    float a = (vol & 16) != 0 ? Volume[EnvelopeLevel()] : Volume[vol & 15];
                    s += output ? a : 0;
                }
                sum += s;
                samples++;
            }
            if (samples == 0) return last;
            // Media de la ventana y sin componente continua (el AY solo da niveles positivos).
            float v = sum / samples / 3f;
            dc += (v - dc) * 0.001f;
            last = (v - dc) * 1.6f;
            return last;
        }

        float dc, last;
    }
}
