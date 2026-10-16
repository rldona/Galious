using System.Collections.Generic;
using UnityEngine;

namespace MazeOfKnight.Game
{
    // Música y efectos con el PSG emulado (Psg.cs; docs/music.md). La música son los registros del PSG grabados frame a
    // frame en openMSX y recortados a una vuelta (tools/extract/audio_export.py → Resources/Audio). Los efectos son
    // los guiones de b13:73DC y suenan en el canal C, que deja de tocar la música mientras duran. Un efecto solo entra
    // si su número es mayor o igual que el del que está sonando (E1B0). Los registros se aplican a 60 Hz.
    // [I] Es el audio original de Konami: solo está en las compilaciones locales (Assets/Generated no se versiona).
    public class Sound : MonoBehaviour
    {
        public static Sound Instance { get; private set; }

        class Song { public byte[] Data; public int Frames, Loop; }

        readonly Dictionary<int, Song> songs = new Dictionary<int, Song>();
        readonly Dictionary<int, byte[]> effects = new Dictionary<int, byte[]>();   // 5 bytes por frame
        readonly object gate = new object();
        Psg psg;
        int rate, sampleCounter;
        Song song;
        int songId = -1, songFrame, nextSong = -1;
        byte[] fx;
        int fxId = -1, fxFrame;
        byte lastR13 = 0xFF;
        public float Volume = 0.35f;
        public float MusicGain = 1, EffectsGain = 1;    // GameSettings: música y sonido (Volume ya lleva el general)
        public bool MusicMuted;

        public int CurrentSong => songId;

        // Llamadas cómodas desde cualquier sitio (no hacen nada sin el componente o sin los datos importados).
        public static void PlayMusic(int id, int then = -1) => Instance?.Music(id, then);
        public static void StopMusic() => Instance?.Music(-1);
        public static void Effect(int id) => Instance?.PlayEffect(id);

        // Un solo reproductor para todo el juego: sigue sonando al recargar la escena (del título a la partida, la 0x87
        // de PLAY START continúa en el castillo).
        public static Sound Ensure()
        {
            if (Instance != null) return Instance;
            var go = new GameObject("Sonido (PSG)");
            DontDestroyOnLoad(go);
            return go.AddComponent<Sound>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            foreach (var t in Resources.LoadAll<TextAsset>("Audio"))
            {
                var b = t.bytes;
                if (t.name.StartsWith("music_") && b.Length >= 8 && b[0] == 'P')
                {
                    int id = System.Convert.ToInt32(t.name.Substring(6), 16);
                    int frames = b[4] | b[5] << 8, loop = b[6] | b[7] << 8;
                    songs[id] = new Song { Data = b, Frames = frames, Loop = loop == 0xFFFF ? -1 : loop };
                }
                else if (t.name == "sfx" && b.Length > 5 && b[0] == 'S')
                {
                    int n = b[4], p = 5;
                    for (int i = 0; i < n && p + 3 <= b.Length; i++)
                    {
                        int id = b[p], frames = b[p + 1] | b[p + 2] << 8;
                        p += 3;
                        var f = new byte[frames * 5];
                        System.Array.Copy(b, p, f, 0, Mathf.Min(f.Length, b.Length - p));
                        effects[id] = f;
                        p += frames * 5;
                    }
                }
            }
            rate = AudioSettings.outputSampleRate > 0 ? AudioSettings.outputSampleRate : 44100;
            psg = new Psg(rate);
            Silence();
            var src = gameObject.AddComponent<AudioSource>();
            src.clip = AudioClip.Create("PSG", rate, 1, rate, true, OnRead);
            src.loop = true;
            src.Play();
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        public void Music(int id, int then = -1)
        {
            lock (gate)
            {
                if (id == songId && song != null) { nextSong = then; return; }
                songs.TryGetValue(id, out song);
                songId = song != null ? id : -1;
                songFrame = 0;
                nextSong = then;
            }
        }

        public void PlayEffect(int id)
        {
            lock (gate)
            {
                if (!effects.TryGetValue(id, out var f)) return;
                if (fx != null && id < fxId) return;
                fx = f;
                fxId = id;
                fxFrame = 0;
            }
        }

        void Silence()
        {
            for (int r = 0; r < 14; r++) psg.Regs[r] = 0;
            psg.Regs[7] = 0x3F;
        }

        // Un frame de 60 Hz: registros de la música y, encima, el efecto en el canal C.
        void Frame()
        {
            lock (gate)
            {
                if (song != null && !MusicMuted)
                {
                    int o = 8 + songFrame * 14;
                    for (int r = 0; r < 13; r++) psg.Regs[r] = song.Data[o + r];
                    byte r13 = song.Data[o + 13];
                    if (r13 != lastR13) { psg.Write(13, r13); lastR13 = r13; }
                    if (++songFrame >= song.Frames)
                    {
                        if (song.Loop >= 0) songFrame = song.Loop;
                        else if (nextSong >= 0 && songs.TryGetValue(nextSong, out var n)) { song = n; songId = nextSong; songFrame = 0; nextSong = -1; }
                        else { song = null; songId = -1; }
                    }
                }
                else
                    Silence();
                // Volúmenes de Opciones: los canales de la música y, el C, el del efecto mientras suena uno.
                psg.Gain[0] = psg.Gain[1] = MusicGain;
                psg.Gain[2] = fx != null ? EffectsGain : MusicGain;
                if (fx != null)
                {
                    int o = fxFrame * 5;
                    int period = fx[o] | fx[o + 1] << 8, vol = fx[o + 2], noise = fx[o + 3];
                    psg.Regs[4] = (byte)(period & 0xFF);
                    psg.Regs[5] = (byte)(period >> 8);
                    psg.Regs[10] = (byte)(vol & 15);
                    byte mixer = (byte)(psg.Regs[7] | 0x24);                // canal C sin tono ni ruido
                    if (period != 0) mixer &= 0xFB;
                    if (noise != 0xFF) { mixer &= 0xDF; psg.Regs[6] = (byte)noise; }
                    psg.Regs[7] = mixer;
                    if (++fxFrame * 5 >= fx.Length) { fx = null; fxId = -1; }
                }
            }
        }

        void OnRead(float[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                sampleCounter += 60;
                if (sampleCounter >= rate) { sampleCounter -= rate; Frame(); }
                data[i] = psg.Sample() * Volume;
            }
        }
    }
}
