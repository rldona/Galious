using System.Collections.Generic;
using MazeOfKnight.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace MazeOfKnight.Game
{
    // Secuencia final (estado 17; docs/ending.md), sobre la pantalla del original de 256 × 192 px, a 60 frames por
    // segundo: la sala se congela (40), se borra en anillos de fuera adentro (101), negro con el jugador y el bebé
    // (120), el castillo (19 + 766: la bandera roja de Galious baja y sube la azul con la cruz, el bebé sube entre
    // Popolon y Aphrodite, bocadillos "NICE!" y "GREAT!"), y el epílogo y los créditos subiendo (1 px cada 3 frames;
    // las pausas, 2 frames por paso). Al acabar, vuelta al principio (partida nueva).
    // [I] Sin gráficos ni música: cuadros, texto y los tiempos del original. F3 (depuración) salta a la pantalla siguiente.
    public class EndingScreen : MonoBehaviour
    {
        public bool Running { get; private set; }
        EndingData data;
        float frame;
        int stage;                    // 0 congelado, 1 anillos, 2 negro, 3 castillo, 4 epílogo
        float stageStart;
        Vector2 player, baby;         // en píxeles del original
        GUIStyle text, big;
        readonly List<(float start, EpilogueLine line, float tapeY)> tape = new List<(float, EpilogueLine, float)>();
        float epilogueEnd;
        int balloonSide = -1;
        readonly System.Random rng = new System.Random();

        public void Begin(EndingData d, Vector2 playerPx, Vector2 babyPx)
        {
            data = d ?? new EndingData();
            player = playerPx; baby = babyPx;
            Running = true;
            frame = 0; stage = 0; stageStart = 0;
            BuildTape();
        }

        int Duration(int s) => s switch
        {
            0 => data.Frames("touchBaby", 40),
            1 => data.Frames("roomCollapse", 101),
            2 => data.Frames("blackPause", 120),
            3 => data.Frames("castleLoad", 19) + data.Frames("castle", 766),
            4 => data.Frames("epilogueLoad", 28) + (int)epilogueEnd,
            _ => 1,
        };

        // Cinta del epílogo: cada línea ocupa sus filas de 8 px y se recorre a 1 px por paso; las esperas paran el scroll.
        void BuildTape()
        {
            tape.Clear();
            float t = 0, y = 0;
            int step = Mathf.Max(1, data.epilogue.stepFramesMeasured), wait = Mathf.Max(1, data.epilogue.waitStepFramesMeasured);
            foreach (var l in data.epilogue.lines)
            {
                tape.Add((t, l, y));
                if (l.kind == "text" || l.kind == "blank") { y += 8 * l.rows; t += step * 8f * l.rows; }
                else if (l.kind == "wait") t += wait * l.steps;
                else if (l.kind == "end") break;
            }
            epilogueEnd = t;
        }

        // Píxeles de cinta recorridos en el frame f del epílogo.
        float Scrolled(float f)
        {
            int step = Mathf.Max(1, data.epilogue.stepFramesMeasured);
            float scrolled = 0;
            for (int i = 0; i < tape.Count; i++)
            {
                var (start, l, y) = tape[i];
                if (f < start) break;
                if (l.kind == "text" || l.kind == "blank")
                    scrolled = y + Mathf.Min(8f * l.rows, (f - start) / step);
            }
            return scrolled;
        }

        void Update()
        {
            if (!Running) return;
            frame += Time.deltaTime * 60f;
            bool skip = Keyboard.current?.f3Key.wasPressedThisFrame == true;
            if (frame - stageStart >= Duration(stage) || skip)
            {
                stage++;
                stageStart = frame;
                if (stage > 4)
                {
                    Running = false;
                    // Vuelta al logo y al título en el original; aquí, partida nueva desde el castillo.
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }
            }
        }

        void OnGUI()
        {
            if (!Running) return;
            float scale = Mathf.Min(Screen.width / 256f, Screen.height / 192f);
            float ox = (Screen.width - 256 * scale) / 2, oy = (Screen.height - 192 * scale) / 2;
            Rect R(float x, float y, float w, float h) => new Rect(ox + x * scale, oy + y * scale, w * scale, h * scale);
            text ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, wordWrap = false };
            big ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            text.fontSize = Mathf.Max(10, (int)(8 * scale));
            big.fontSize = Mathf.Max(10, (int)(7 * scale));
            float f = frame - stageStart;

            switch (stage)
            {
                case 0:
                    break;   // la sala congelada (el juego está en pausa)
                case 1:
                {
                    // Un anillo de tiles cada 10 frames, de fuera adentro (32 × 20, 30 × 18 … 14 × 2); el bebé no se borra.
                    int rings = Mathf.Min(10, (int)(f / 10) + 1);
                    float t = 8 * rings;
                    Box(R(0, 32, 256, t), Color.black);
                    Box(R(0, 192 - t, 256, t), Color.black);
                    Box(R(0, 32, t, 160), Color.black);
                    Box(R(256 - t, 32, t, 160), Color.black);
                    Box(R(baby.x, baby.y, 16, 16), new Color(1f, 0.9f, 0.5f));
                    break;
                }
                case 2:
                    Box(R(0, 32, 256, 160), Color.black);
                    Box(R(player.x - 8, player.y - 16, 16, 16), new Color(0.35f, 0.55f, 1f));
                    Box(R(baby.x, baby.y, 16, 16), new Color(1f, 0.9f, 0.5f));
                    break;
                case 3:
                    Castle(R, f - data.Frames("castleLoad", 19));
                    break;
                case 4:
                    Epilogue(R, f - data.Frames("epilogueLoad", 28));
                    break;
            }
        }

        void Castle(System.Func<float, float, float, float, Rect> R, float f)
        {
            Box(R(0, 0, 256, 192), Color.black);
            if (f < 0) return;
            // Marco de hojas y castillo (20 × 14 tiles).
            Frame(R(24, 24, 208, 144), new Color(0.2f, 0.6f, 0.2f), 4);
            Box(R(48, 56, 160, 96), new Color(0.35f, 0.35f, 0.4f));
            Box(R(112, 40, 32, 24), new Color(0.4f, 0.4f, 0.45f));
            Box(R(126, 24, 2, 40), Color.white);                                   // mástil
            var ph = data.castleScene.phases;
            float p0 = ph.Length > 0 ? ph[0].frames : 191, p1 = ph.Length > 1 ? ph[1].frames : 137, p2 = ph.Length > 2 ? ph[2].frames : 52;
            // Bandera: roja (Galious) hasta que baja; luego la azul con la cruz sube (1 px cada 4 frames, y 0x2E…0x3F).
            float flagY = 0x2E;
            bool blue = false;
            if (f >= p0 && f < p0 + p1)
            {
                float g = f - p0;
                if (g < 65) flagY = 0x2E + g / 4f;
                else { blue = true; flagY = 0x3F - Mathf.Min(17, (g - 65) / 4f); }
            }
            else if (f >= p0 + p1) blue = true;
            Box(R(128, flagY - 0x10, 14, 9), blue ? new Color(0.2f, 0.4f, 1f) : new Color(0.9f, 0.1f, 0.1f));
            if (blue) Box(R(133, flagY - 0x0F, 2, 7), Color.white);
            if (!blue && ((int)f / 16) % 2 == 0) Box(R(144, 0x20, 3, 3), Color.white);  // destello junto a la bandera
            // Popolon, Aphrodite y el bebé, que sube 12 px entre ellos.
            Box(R(96, 0x68, 16, 24), new Color(0.35f, 0.55f, 1f));
            Box(R(144, 0x68, 16, 24), new Color(1f, 0.45f, 0.7f));
            float babyY = 0x6B;
            if (f >= p0 + p1) babyY = Mathf.Max(0x5F, 0x6B - (f - p0 - p1) / 4f);
            if (f >= p0 + p1) Box(R(120, babyY, 16, 16), new Color(1f, 0.9f, 0.5f));
            // Bocadillos: tras 64 frames de la fase 3, cada 64 uno al azar.
            float g3 = f - p0 - p1 - p2;
            if (g3 >= 64)
            {
                int k = (int)(g3 / 64);
                if (balloonSide < 0 || k != balloonSide >> 1) balloonSide = (k << 1) | rng.Next(2);
                bool left = (balloonSide & 1) == 0;
                var b = data.castleScene.balloons;
                GUI.Label(R(left ? 64 : 144, 0x50, 48, 12), left ? b[0] : (b.Length > 1 ? b[1] : "GREAT!"), big);
            }
        }

        void Epilogue(System.Func<float, float, float, float, Rect> R, float f)
        {
            Box(R(0, 0, 256, 192), Color.black);
            if (f < 0) return;
            float scrolled = Scrolled(f);
            foreach (var (_, l, y) in tape)
            {
                if (l.kind != "text") continue;
                float sy = 192 - (scrolled - y);
                if (sy > 192 || sy < -16) continue;
                GUI.Label(R(8 * l.col, sy, 256, 16), l.text, text);
            }
        }

        static void Box(Rect r, Color c)
        {
            var old = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = old;
        }

        static void Frame(Rect r, Color c, float t)
        {
            Box(new Rect(r.x, r.y, r.width, t), c);
            Box(new Rect(r.x, r.yMax - t, r.width, t), c);
            Box(new Rect(r.x, r.y, t, r.height), c);
            Box(new Rect(r.xMax - t, r.y, t, r.height), c);
        }
    }
}
