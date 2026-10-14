using MazeOfKnight.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace MazeOfKnight.Game
{
    // Qué arranca la escena: el ciclo de presentación, una demo o una partida (se recarga la escena para cada cosa,
    // como el original vuelve a inicializar la RAM). Trips = E025 (vueltas del estado 2: par demo, impar historia);
    // Demo = E023 (qué demo toca).
    public static class AttractMode
    {
        public enum Mode { Attract, Demo, Game }
        public static Mode Current = Mode.Attract;
        public static int Trips, Demo;
        public static bool StartAtTitle;
        public static bool LoadSave;          // L en el título: partida guardada (en el original, la contraseña)

        public static void Go(Mode mode, bool atTitle = false)
        {
            Current = mode;
            StartAtTitle = atTitle;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        // Al acabar una demo o la historia (o al interrumpirlas): cuenta la vuelta y vuelve al logo (o al título).
        public static void TripDone(bool wasDemo, bool toTitle)
        {
            Trips++;
            if (wasDemo) Demo ^= 1;
            Go(Mode.Attract, toTitle);
        }
    }

    // Ciclo de presentación (docs/attract.md): logo (366 frames), título (264), y luego la demo (en la escena de juego) o
    // la historia (1710). Espacio o M en el título → "PLAY START" parpadeando 45 frames y partida nueva. En el logo,
    // cualquier tecla lleva al título; en la historia, al título contando la vuelta.
    // Remake: el logo de Konami se sustituye por el nombre del remake y una línea que reconoce el juego original; el
    // título del original (su dibujo) por el del remake. Sin gráficos ni música: texto y los tiempos del original.
    public class AttractScreen : MonoBehaviour
    {
        enum Stage { Logo, Title, Story, PlayStart }

        AttractData data;
        Stage stage;
        float frame;
        ScrollingText story;
        GUIStyle big, small, mid;

        public bool Running { get; private set; }

        public void Begin(AttractData d)
        {
            data = d ?? new AttractData();
            Sound.Ensure();
            Sound.StopMusic();
            story = new ScrollingText(data.storyLines, data.story.stepFramesMeasured, 2);
            Running = true;
            stage = AttractMode.StartAtTitle ? Stage.Title : Stage.Logo;
            frame = 0;
        }

        bool AnyGameKey()
        {
            var kb = Keyboard.current;
            if (kb == null) return false;
            return kb.leftArrowKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame ||
                   kb.downArrowKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame || kb.mKey.wasPressedThisFrame ||
                   Gamepad.current?.buttonSouth.wasPressedThisFrame == true || Gamepad.current?.startButton.wasPressedThisFrame == true;
        }

        void Update()
        {
            if (!Running) return;
            frame += Time.deltaTime * 60f;
            var kb = Keyboard.current;
            switch (stage)
            {
                case Stage.Logo:
                    if (AnyGameKey() || frame >= data.Frames("konamiLogo", 366)) Next(Stage.Title);
                    break;
                case Stage.Title:
                    bool start = kb != null && (kb.spaceKey.wasPressedThisFrame || kb.mKey.wasPressedThisFrame) ||
                                 Gamepad.current?.buttonSouth.wasPressedThisFrame == true || Gamepad.current?.startButton.wasPressedThisFrame == true;
                    if (start) { Next(Stage.PlayStart); break; }
                    if (kb != null && kb.lKey.wasPressedThisFrame && SaveData.Exists)
                    {
                        AttractMode.LoadSave = true;
                        Next(Stage.PlayStart);
                        break;
                    }
                    if (frame >= data.Frames("title", 264))
                    {
                        if ((AttractMode.Trips & 1) == 0 && data.demos.Length > 0) { Running = false; AttractMode.Go(AttractMode.Mode.Demo); }
                        else Next(Stage.Story);
                    }
                    break;
                case Stage.Story:
                    if (AnyGameKey()) { Running = false; AttractMode.TripDone(false, true); break; }
                    if (frame >= story.DurationFrames) { Running = false; AttractMode.TripDone(false, false); }
                    break;
                case Stage.PlayStart:
                    if (frame >= 45) { Running = false; AttractMode.Go(AttractMode.Mode.Game); }
                    break;
            }
        }

        void Next(Stage s)
        {
            stage = s;
            frame = 0;
            if (s == Stage.Story) Sound.PlayMusic(0x86);        // Story (b00:4371)
            else if (s == Stage.PlayStart) Sound.PlayMusic(0x87);  // castillo, con su entrada (estado 3)
            else Sound.StopMusic();
        }

        void OnGUI()
        {
            if (!Running) return;
            float scale = Mathf.Min(Screen.width / 256f, Screen.height / 192f);
            float ox = (Screen.width - 256 * scale) / 2, oy = (Screen.height - 192 * scale) / 2;
            Rect R(float x, float y, float w, float h) => new Rect(ox + x * scale, oy + y * scale, w * scale, h * scale);
            big ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            mid ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft };
            small ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            big.fontSize = Mathf.Max(14, (int)(16 * scale));
            mid.fontSize = Mathf.Max(10, (int)(8 * scale));
            small.fontSize = Mathf.Max(8, (int)(5 * scale));
            Box(new Rect(0, 0, Screen.width, Screen.height), Color.black);

            switch (stage)
            {
                case Stage.Logo:
                {
                    // Fondo azul oscuro; el nombre se descubre de arriba abajo, una línea de píxeles cada 2 frames
                    // (48 líneas en 96 frames), y se queda quieto.
                    Box(R(0, 0, 256, 192), new Color(0.05f, 0.05f, 0.35f));
                    GUI.Label(R(56, 64, 168, 48), "MAZE OF KNIGHT", big);
                    float shown = Mathf.Min(48, frame / 2);
                    Box(R(56, 64 + shown, 168, 48 - shown), new Color(0.05f, 0.05f, 0.35f));
                    if (shown >= 48)
                        GUI.Label(R(0, 128, 256, 24), "remake de Knightmare II: The Maze of Galious (Konami, 1987)", small);
                    break;
                }
                case Stage.Title:
                case Stage.PlayStart:
                    GUI.Label(R(72, 8, 112, 96), "MAZE\nOF\nKNIGHT", big);
                    Sword(R);
                    if (stage == Stage.Title)
                    {
                        GUI.Label(R(72, 136, 120, 8), "PUSH SPACE KEY", mid);
                        if (SaveData.Exists) GUI.Label(R(0, 160, 256, 8), "L: CONTINUAR LA PARTIDA GUARDADA", small);
                    }
                    else if (frame >= 1 && ((44 - (int)frame) & 4) == 0)   // E004 de 44 a 1: 4 frames sí, 4 no
                        GUI.Label(R(88, 136, 120, 8), "PLAY START", mid);
                    break;
                case Stage.Story:
                    story.Draw(R, frame, mid);
                    break;
            }
        }

        // Destello que recorre la espada (a los 48 frames, 2 px por frame de x 0x60 a 0x98) y estrella en la punta.
        void Sword(System.Func<float, float, float, float, Rect> R)
        {
            Box(R(0x58, 0x4C, 0x50, 3), new Color(0.6f, 0.6f, 0.7f));
            if (stage != Stage.Title) return;
            float g = frame - 48;
            if (g >= 0 && g < 28) Box(R(0x60 + 2 * g, 0x47 + 4, 6, 6), new Color(0.3f, 1f, 1f));
            else if (g >= 28 && g < 28 + 64)
            {
                float s = 2 + ((int)(g - 28) / 16) * 2;
                Box(R(0xA0 + 4 - s / 2, 0x3F + 12 - s / 2, s, s), new Color(0.3f, 1f, 1f));
            }
        }

        static void Box(Rect r, Color c)
        {
            var old = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = old;
        }
    }
}
