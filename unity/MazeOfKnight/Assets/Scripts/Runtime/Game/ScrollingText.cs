using System.Collections.Generic;
using MazeOfKnight.Data;
using UnityEngine;

namespace MazeOfKnight.Game
{
    // Texto que sube por la pantalla con la rutina del epílogo (b01:6A1E/6ADC): lo usan la historia del ciclo de
    // presentación y el epílogo del final. Cada línea ocupa sus filas de 8 px y se recorre a 1 px por paso (3 frames
    // medidos); las esperas paran el scroll (2 frames por paso).
    public class ScrollingText
    {
        readonly List<(float start, EpilogueLine line, float tapeY)> tape = new List<(float, EpilogueLine, float)>();
        readonly int stepFrames;
        public float DurationFrames { get; }

        public ScrollingText(EpilogueLine[] lines, int stepFrames = 3, int waitStepFrames = 2)
        {
            this.stepFrames = Mathf.Max(1, stepFrames);
            float t = 0, y = 0;
            foreach (var l in lines ?? new EpilogueLine[0])
            {
                tape.Add((t, l, y));
                if (l.kind == "text" || l.kind == "blank") { y += 8 * l.rows; t += this.stepFrames * 8f * l.rows; }
                else if (l.kind == "wait") t += Mathf.Max(1, waitStepFrames) * l.steps;
                else if (l.kind == "end") break;
            }
            DurationFrames = t;
        }

        float Scrolled(float f)
        {
            float scrolled = 0;
            foreach (var (start, l, y) in tape)
            {
                if (f < start) break;
                if (l.kind == "text" || l.kind == "blank")
                    scrolled = y + Mathf.Min(8f * l.rows, (f - start) / stepFrames);
            }
            return scrolled;
        }

        // Dibuja en la pantalla de 256 × 192 (R convierte a píxeles de la ventana).
        public void Draw(System.Func<float, float, float, float, Rect> R, float f, GUIStyle style)
        {
            float scrolled = Scrolled(f);
            foreach (var (_, l, y) in tape)
            {
                if (l.kind != "text") continue;
                float sy = 192 - (scrolled - y);
                if (sy > 192 || sy < -16) continue;
                GUI.Label(R(8 * l.col, sy, 256, 16), l.text, style);
            }
        }
    }
}
