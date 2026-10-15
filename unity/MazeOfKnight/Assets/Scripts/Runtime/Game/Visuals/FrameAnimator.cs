using System;
using System.Collections.Generic;
using UnityEngine;

namespace MazeOfKnight.Game.Visuals
{
    // Un clip de animación fotograma a fotograma (arte dibujado): los fotogramas en orden, a Fps, en bucle o no, y una
    // etiqueta opcional por fotograma ("golpe", "pisada"…) para colocar eventos y alinear poses clave con los ticks del
    // juego (docs/design_bible/12_estilo_visual.md, "Animación").
    public class FrameClip
    {
        public string Name;
        public Sprite[] Frames = Array.Empty<Sprite>();
        public float Fps = FrameAnimator.DefaultFps;
        public bool Loop = true;
        public string[] Tags;                 // null o una por fotograma (null = sin etiqueta)

        public int Length => Frames?.Length ?? 0;
        public float Duration => Length / Fps;

        // Primer fotograma con la etiqueta, o −1.
        public int TagFrame(string tag)
        {
            if (Tags == null) return -1;
            for (int i = 0; i < Tags.Length; i++)
                if (Tags[i] == tag) return i;
            return -1;
        }
    }

    // Reproductor de clips a 24 fps, independiente del tick del juego (30 ticks/s): avanza con el tiempo real y avisa al
    // entrar en cada fotograma (FrameEntered), también de los que se salten en un fotograma lento, en orden. PlayAt
    // arranca en un fotograma dado: así la pose clave (el fotograma "golpe" de la estocada) cae en el tick medido del
    // original y la anticipación o la recuperación se reparten alrededor.
    public class FrameAnimator
    {
        public const float DefaultFps = 24f;

        public FrameClip Clip { get; private set; }
        public int Frame { get; private set; }
        public float Time { get; private set; }
        int absolute;                          // fotogramas entrados desde el arranque (sin el módulo del bucle)

        public event Action<FrameClip, int> FrameEntered;

        public Sprite Current => Clip == null || Clip.Length == 0 ? null : Clip.Frames[Frame];
        public bool Finished => Clip != null && !Clip.Loop && Clip.Length > 0 && absolute >= Clip.Length - 1 && Time >= Clip.Duration;

        // Reproduce el clip desde el principio; si ya es el que suena y no se pide reiniciar, sigue donde está.
        public void Play(FrameClip clip, bool restart = false)
        {
            if (clip == Clip && !restart) return;
            PlayAt(clip, 0);
        }

        // Reproduce el clip desde un fotograma concreto (recortado al clip).
        public void PlayAt(FrameClip clip, int frame)
        {
            Clip = clip;
            if (clip == null || clip.Length == 0) { Frame = 0; Time = 0; absolute = 0; return; }
            frame = Mathf.Clamp(frame, 0, clip.Length - 1);
            Frame = frame;
            absolute = frame;
            Time = frame / clip.Fps;
            FrameEntered?.Invoke(clip, frame);
        }

        public void Stop()
        {
            Clip = null;
            Frame = 0;
            Time = 0;
            absolute = 0;
        }

        // Avanza dt segundos.
        public void Tick(float dt)
        {
            if (Clip == null || Clip.Length == 0 || dt <= 0) return;
            int n = Clip.Length;
            Time += dt;
            int target = (int)Math.Floor(Time * Clip.Fps);
            if (!Clip.Loop) target = Math.Min(target, n - 1);
            while (absolute < target)
            {
                absolute++;
                Frame = Clip.Loop ? absolute % n : absolute;
                FrameEntered?.Invoke(Clip, Frame);
            }
            if (!Clip.Loop && Time > Clip.Duration) Time = Clip.Duration;
            // Que el tiempo no crezca sin fin en los bucles largos.
            if (Clip.Loop && absolute >= n * 1000)
            {
                int laps = absolute / n;
                absolute -= laps * n;
                Time -= laps * Clip.Duration;
            }
        }
    }
}
