using System;
using System.Collections.Generic;
using P = MazeOfKnight.Game.OriginalPhysics;

namespace MazeOfKnight.Game.Enemies
{
    // Hijos: proyectiles y enemigos que crea otro enemigo (docs/enemy_ai.md, "hijos"). Los apuntados van hacia
    // (jugador_y − 16, jugador_x − 8) con la velocidad fijada al nacer y atraviesan paredes; los horizontales (rutina
    // 98A7) mueren al tocar un tile sólido. Todos se borran al salir de la pantalla. Desaparecen al dar al jugador
    // (IX+3 bit 6, b03:B285).
    public partial class EnemySystem
    {
        static readonly Dictionary<int, Func<Enemy>> Projectiles = new Dictionary<int, Func<Enemy>>
        {
            { 0x06, () => new Lightning() },
            { 0x1A, () => new Shot { Life = 32, StopsAtWalls = true, OnlyLeftVanishes = true } },  // escupitajo de 0x05
            { 0x1D, () => new Shot { StopsAtWalls = true } },        // púa de 0x11
            { 0x1E, () => new Shot { StopsAtWalls = true } },        // de 0x12
            { 0x33, () => new Shot { StopsAtWalls = true } },        // flecha de 0x32 / 0x51
            { 0x3F, () => new Shot { StopsAtWalls = true, Stuns = true } },   // del soldado 0x2D: paraliza
            { 0x3B, () => new Shot { AimSpeed = 0x60, ScatterY = true } },    // de las bocas 0x3A / 0x50
            { 0x3D, () => new Shot { AimSpeed = 0x40 } },            // del hechicero 0x3C
            { 0x44, () => new Shot { AimSpeed = 0x40 } },            // del espectro 0x13
            { 0x56, () => new Shot { AimSpeed = 0x40 } },            // del hombre-llama 0x0C
            { 0x45, () => new Shot { AimSpeed = 0x40 } },            // del guardián 0x15
            { 0x46, () => new Shot { AimSpeed = 0x80 } },            // de la aparición 0x16
            { 0x4B, () => new Shot { AimSpeed = 0x60 } },            // del fantasma 0x26
            { 0x61, () => new Shot { AimSpeed = 0x40 } },            // del monstruo del agua 0x42
            { 0x41, () => new EightWayShot() },
            { 0x47, () => new RollingBomb() },
            { 0x48, () => new DriftShot() },
            { 0x4A, () => new Ember() },
            { 0x4D, () => new Spore() },
            { 0x4F, () => new Fairy() },
            { 0x60, () => new WaterJet() },
            { 0x62, () => new Beam() },
            // de los jefes (docs/boss_ai.md, "Hijos de los jefes")
            { 0x54, () => new Bone() },
            { 0x55, () => new Bone() },
            { 0x57, () => new FlameRow() },
            { 0x58, () => new FanShot() },
            { 0x59, () => new Spore2() },
            { 0x63, () => new SummonSpark() },
            { 0x65, () => new LipShot() },
            { 0x67, () => new GaliousOrb() },
            { 0x68, () => new MonsterBall() },
            // final del juego (docs/ending.md)
            { 0x5C, () => new BabyRock() },
            { 0x39, () => new SpiralGhost() },
            { 0x6B, () => new Baby() },
        };
    }

    // Disparo en línea recta: horizontal (la vx la pone el padre) o apuntado al jugador al nacer.
    public class Shot : Enemy
    {
        public int AimSpeed;            // módulo S (S/32 px/tick); 0 = la vx del padre
        public bool StopsAtWalls;       // rutina 98A7: muere en (x + 16 o x − 1, y) sólido
        public bool ScatterY;           // 0x3B: apunta como si saliera de y + (E604 & 31) − 8
        public bool OnlyLeftVanishes;   // 0x1A: hacia la derecha no lleva IX+3 bit 6 [C]
        public bool Stuns;              // 0x3F: no quita vida; paraliza 60 ticks
        public int Life = -1;
        bool started;

        public override void Init(EnemyContext ctx)
        {
            DiesOnTouch = true;
            if (AimSpeed == 0) return;
            float y = Y;
            if (ScatterY) Y += (ctx.Tick & 31) - 8;
            AimAtPlayer(ctx, AimSpeed);
            Y = y;
            FacingLeft = Vx < 0;
        }

        public override void Tick(EnemyContext ctx)
        {
            if (!started)
            {
                started = true;
                if (OnlyLeftVanishes) DiesOnTouch = Vx < 0;
            }
            Move();
            if (Life > 0 && --Life == 0) { Alive = false; return; }
            if (StopsAtWalls && Solid(ctx, Vx < 0 ? X - 1 : X + 16, Y)) Alive = false;
        }
    }

    // 0x41 (del espíritu 0x40): rumbo al jugador redondeado a 8 direcciones, a 4 px/tick, con el desplazamiento de
    // salida de la tabla 8DC5 [I: qué índice es qué ángulo está deducido de la tabla]. Caja según el sprite: horizontal
    // A4 → 3A (17 × 4), vertical A8 → 3B (4 × 17), diagonal C8/BC → 75.
    public class EightWayShot : Enemy
    {
        static readonly (int vx, int vy, int dy, int dx)[] Table =
        {
            (4, 0, 13, 16), (4, 4, 13, 16), (0, 4, 13, 16), (-4, 4, 13, 0),
            (-4, 0, 13, 0), (-4, -4, -3, 0), (0, -4, -3, 16), (4, -4, -3, 16),
        };

        public override void Init(EnemyContext ctx)
        {
            DiesOnTouch = true;
            double a = Math.Atan2(ctx.Py - 16 - (Y + 8), ctx.Px - 8 - (X + 8));   // y hacia abajo
            int i = ((int)Math.Round(a / (Math.PI / 4)) + 8) & 7;
            var t = Table[i];
            Vx = t.vx; Vy = t.vy; X += t.dx; Y += t.dy;
            SetBox(ctx, t.vy == 0 ? 0x3A : t.vx == 0 ? 0x3B : 0x75);
        }

        public override void Tick(EnemyContext ctx) => Move();
    }

    // 0x47 (del centinela 0x17): bomba que rueda hacia la x del jugador al lanzarla, más despacio cuanto más cerca
    // (3,5 → 1 px/tick); se para al pasarla, sin suelo o ante una pared, espera 32..63 ticks y explota 16 (32×32).
    // Cajas: rodando 5C ↔ 5D con el sprite (cada 3, 6, 9, 12, 15 o 18 ticks según la velocidad); al explotar 3D
    // (patrón 0A) y a los 8 ticks 5B (32 × 32, patrón 0B).
    public class RollingBomb : Enemy
    {
        float target;
        int phase, timer, anim;

        public override void Init(EnemyContext ctx) { target = ctx.Px - 8; FacingLeft = (Param & 1) == 1; SetBox(ctx, 0x5C); }

        public override void Tick(EnemyContext ctx)
        {
            switch (phase)
            {
                case 0:
                {
                    float d = Math.Abs(target - X);
                    float speed = d >= 132 ? 3.5f : d >= 116 ? 3f : d >= 96 ? 2.5f : d >= 72 ? 2f : d >= 40 ? 1.5f : 1f;
                    Walk(speed);
                    // Periodo de la animación: 3 a 3,5 px/tick, 6 a 3, … 18 a 1.
                    if (++anim >= (int)Math.Round(3 + (3.5f - speed) * 6))
                    {
                        anim = 0;
                        SetBox(ctx, BoxIndex == 0x5C ? 0x5D : 0x5C);
                    }
                    bool passed = FacingLeft ? X + Vx <= target : X + Vx >= target;
                    if (passed || Blocked(ctx)) { phase = 1; timer = ctx.Rand(32, 63); Vx = 0; break; }
                    X += Vx;
                    break;
                }
                case 1:
                    if (--timer > 0) break;
                    phase = 2; timer = 16;
                    X -= 8; Y -= 8; Width = Height = 32;
                    SetBox(ctx, 0x3D);
                    break;
                case 2:
                    if (--timer == 8) SetBox(ctx, 0x5B);
                    if (timer <= 0) Alive = false;
                    break;
            }
        }
    }

    // 0x48 (de los murciélagos 0x19 / 0x37): rumbo al azar de 16 a 1 px/tick, nuevo cada 16 ticks.
    public class DriftShot : Enemy
    {
        int timer;

        public override void Init(EnemyContext ctx) => DiesOnTouch = true;

        public override void Tick(EnemyContext ctx)
        {
            if (--timer <= 0) { timer = 16; Heading(ctx.Rand(0, 15), 1); }
            Move();
        }
    }

    // 0x4A (del escupefuego 0x22): sale hacia arriba a 4 px/tick con vx ±0, 2, 4 o 3, gravedad 0,25, y cae hasta salir
    // por abajo.
    public class Ember : Enemy
    {
        static readonly int[] Side = { 0, 2, 4, 3 };

        public override void Init(EnemyContext ctx)
        {
            DiesOnTouch = true;
            Vy = -4;
            Vx = Side[ctx.Rand(0, 3)] * (ctx.Coin() ? -1 : 1);
        }

        public override void Tick(EnemyContext ctx)
        {
            Vy += 0.25f;
            Move();
        }
    }

    // 0x4D (de la nube 0x4C): ondula ±45° alrededor de un rumbo fijo, cambiando cada 8 ticks, a 1 px/tick.
    public class Spore : Enemy
    {
        static readonly int[] Swing = { 1, 2, 3, 4, 3, 2, 1, 0 };
        int baseDir, k, timer;

        public override void Init(EnemyContext ctx) { DiesOnTouch = true; baseDir = ctx.Rand(0, 15); }

        public override void Tick(EnemyContext ctx)
        {
            if (--timer <= 0)
            {
                timer = 8;
                Heading((baseDir + Swing[k] + 1) & 15, 1);
                k = (k + 1) % Swing.Length;
            }
            Move();
        }
    }

    // 0x4F: el hada que aparece al reunirse las almas 0x4E. 80 ticks parpadeando sin poder tocarla; luego deambula a
    // 1 px/tick y al tocarla rellena la vitalidad (b04:612C). [I] Al tocarla desaparece.
    public class Fairy : Wanderer
    {
        int wait = 80;

        public bool Touchable => wait <= 0;

        public override void Init(EnemyContext ctx) { Harmful = Hittable = false; BigBox = false; }

        public override void Tick(EnemyContext ctx)
        {
            if (wait > 0)
            {
                Visible = (--wait & 1) == 0 || wait == 0;
                if (wait == 0) Harmful = true;   // "tocable": el sistema lo trata como premio, no como daño
                return;
            }
            Wander(ctx);
            Move();
        }
    }

    // 0x60 (del monstruo del agua 0x42): chorro que sube desde y = 0xA0 a 4 px/tick hasta 0x40 (24 ticks) y vuelve a
    // bajar; al pasar por 0x90 bajando avisa al padre (E628) y se borra en 0xA0. Toda la columna hace daño.
    // Caja calculada (`columnDown`, b03:B916): 8 de ancho (w 7) desde y hacia abajo. [I] El alto del original es
    // 8·((IX+9 & 7) − 1) y la ficha no dice cómo cambia IX+9; se usa la columna hasta 0xB0.
    public class WaterJet : Enemy
    {
        bool down;

        public override void Init(EnemyContext ctx) { Y = 0xA0; Height = 16; SetBox(0, 0, Height - 1, 7); }

        public override void Tick(EnemyContext ctx)
        {
            if (!down)
            {
                Y -= 4;
                if (Y <= 0x40) { Y = 0x40; down = true; }
            }
            else
            {
                float before = Y;
                Y += 4;
                if (before < 0x90 && Y >= 0x90) ctx.WaterJetDone = true;
                if (Y >= 0xA0) { Alive = false; return; }
            }
            Height = (int)(0xB0 - Y);
            SetBox(0, 0, Height - 1, 7);
        }
    }

    // 0x62 (de la criatura ancha 0x43): rayo vertical que crece a 4 px/tick (hasta 6 segmentos de 16 px), hacia abajo
    // si y < 0x30 o el jugador está más abajo, y si no hacia arriba; se queda 16 ticks y se recoge a 4 px/tick.
    // Caja calculada (`columnUpDown`, b03:B933): 8 de ancho (w 7) y el largo del rayo; aquí (X, Y) ya es el extremo
    // de arriba, así que el dy de los rayos hacia arriba (−8·n) está incluido en Y.
    public class Beam : Enemy
    {
        float origin, length;
        bool downwards;
        int phase, hold = 16;

        public override void Init(EnemyContext ctx)
        {
            origin = Y;
            downwards = Y < 0x30 || Y < 0xA0 && ctx.Py - 16 >= Y;
            Height = 1;
            SetBox(0, 0, 0, 7);
        }

        public override void Tick(EnemyContext ctx)
        {
            switch (phase)
            {
                case 0:
                    length += 4;
                    float head = downwards ? origin + 16 + length : origin - length;
                    if (length >= 96 || head >= 0xB0 || head < 0x20) phase = 1;
                    break;
                case 1:
                    if (--hold <= 0) phase = 2;
                    break;
                case 2:
                    length -= 4;
                    if (length <= 0) { Alive = false; return; }
                    break;
            }
            Y = downwards ? origin + 16 : origin - length;
            Height = Math.Max(1, (int)length);
            SetBox(0, 0, Height - 1, 7);
        }
    }

    // 0x06 (de las cabezas 0x69): rayo en cadena de segmentos de 16 px. Crece un segmento por tick hacia abajo
    // (desviándose a un lado según la dirección 0..9), baja entero a 16 px/tick hasta y ≥ 0x90, se recoge y, si
    // abajo hay suelo, deja una chispa 48 ticks. Solo el segmento de abajo hace daño. [I] La desviación se reparte
    // por igual entre los segmentos (tabla 77FB resumida en el total de 8 segmentos).
    public class Lightning : Enemy
    {
        static readonly int[] Drift = { -40, -36, -23, -21, -4, 24, 14, 26, 54, 55 };
        int phase, timer, segments = 1;
        float step;

        public override void Init(EnemyContext ctx)
        {
            ShieldAll = true;   // IX+2 bit 2: la espada hace «clinc» y las armas rebotan (AE68)
            int dir = Param;
            if (X < 0x27 && dir < 5) dir += 5;
            else if (X >= 0xD5 && dir >= 5) dir -= 5;
            step = Drift[Math.Max(0, Math.Min(9, dir))] / 8f;
            X -= 8;
        }

        public override void Tick(EnemyContext ctx)
        {
            switch (phase)
            {
                case 0:   // crece y baja
                    if (Y + 16 < 0x90) { Y += 16; X = Math.Max(0, Math.Min(P.RoomWidthPx - 16, X + step)); segments = Math.Min(6, segments + 1); }
                    else { phase = 1; timer = segments; }
                    break;
                case 1:   // se recoge desde arriba
                    if (--timer > 0) break;
                    if (Solid(ctx, X + 8, 0xB8)) { phase = 2; timer = 48; Y = 0xA8; }
                    else Alive = false;
                    break;
                case 2:   // chispa en el suelo
                    if (--timer <= 0) Alive = false;
                    break;
            }
        }
    }
}
