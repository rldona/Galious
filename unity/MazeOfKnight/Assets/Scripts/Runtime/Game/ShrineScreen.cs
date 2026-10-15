using UnityEngine;
using UnityEngine.InputSystem;

namespace MazeOfKnight.Game
{
    // Pantalla de un santuario (estado 16 en el castillo, 14 en los mundos; docs/design_bible/08_santuarios.md):
    // nombre y retrato del dios, ventana de texto (filas 4–10) y, en las tiendas, tres huecos con su precio y la mano
    // debajo del elegido. ← → eligen, espacio compra o responde, ↓ sale. El juego queda en pausa.
    // [I] Sin gráficos del original: cuadros y texto. Sin la animación de bajar al santuario.
    public class ShrineScreen : MonoBehaviour
    {
        public GameBootstrap game;
        public ShrineVisit Visit { get; private set; }
        GUIStyle text, small;
        bool downHeld = true;
        float autoExit;

        public bool Open => Visit != null;

        public void Show(ShrineVisit visit)
        {
            Visit = visit;
            downHeld = true;          // el ↓ con el que se ha entrado no cuenta para salir
        }

        void Update()
        {
            if (Visit == null) return;
            var kb = Keyboard.current;
            if (kb == null) return;
            bool down = kb.downArrowKey.isPressed || kb.sKey.isPressed;
            if (Visit.AutoExitFrames > 0)
            {
                // La Muerte ya ha respondido: se sale sola a los 180 frames y ↓ no hace nada.
                autoExit += Time.deltaTime * 60f;
                if (autoExit >= Visit.AutoExitFrames) { var w = Visit; Visit = null; autoExit = 0; game.LeaveShrine(w); }
                return;
            }
            if (down && !downHeld)
            {
                var v = Visit;
                Visit = null;
                game.LeaveShrine(v);
                return;
            }
            downHeld = down;
            if (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame) { Visit.Move(-1); Sound.Effect(0x2C); }
            if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame) { Visit.Move(1); Sound.Effect(0x2C); }
            if (kb.spaceKey.wasPressedThisFrame)
            {
                if (Visit.Confirm()) { Sound.Effect(0x2D); game.Say("Comprado"); }
                else if (Visit.Refused) Sound.Effect(0x2F);
                if (Visit.Revived) Sound.Effect(0x30);
                if (Visit.SaveRequested) game.SaveGameNow(Visit);
            }
        }

        void OnGUI()
        {
            if (Visit == null) return;
            float scale = Mathf.Min(Screen.width / 256f, Screen.height / 192f);
            float ox = (Screen.width - 256 * scale) / 2, oy = (Screen.height - 192 * scale) / 2;
            Rect R(float x, float y, float w, float h) => new Rect(ox + x * scale, oy + y * scale, w * scale, h * scale);
            text ??= new GUIStyle(GUI.skin.label) { wordWrap = false };
            small ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            text.fontSize = Mathf.Max(10, (int)(6.5f * scale));
            small.fontSize = Mathf.Max(8, (int)(5f * scale));
            var s = Visit.Shrine;
            if (Visuals.Atmosphere.Styled)
            {
                DrawStyled(R, scale);
                return;
            }

            Box(R(0, 32, 256, 160), Color.black);
            GUI.Label(R(0, 40, 256, 8), s.god.ToUpperInvariant(), small);
            Box(R(120, 48, 16, 16), new Color(0.8f, 0.7f, 0.4f));                    // retrato (2 × 2 tiles)
            Frame(R(32, 64, 192, 56), Color.white);                                    // filas 4–10, columnas 4–27
            GUI.Label(R(48, 72, 176, 48), Visit.Text, text);

            if (s.kind == "shop")
            {
                for (int slot = 0; slot < 3; slot++)
                {
                    var item = Visit.ItemAt(slot);
                    if (item == null) continue;
                    float x = 72 + 48 * slot;                                         // columnas 9, 15 y 21
                    if (!Visit.Sold.Contains(slot))
                    {
                        Box(R(x, 120, 16, 16), new Color(0.2f, 0.25f, 0.45f));
                        GUI.Label(R(x - 16, 136, 48, 8), item.name, small);
                    }
                    GUI.Label(R(x - 8, 144, 32, 8), item.price.ToString("00"), small);
                }
                Frame(R(32, 152, 192, 40), Color.white);                               // ventana de abajo (filas 11–15)
                if (Visit.Cursor >= 0)
                    GUI.Label(R(72 + 48 * Visit.Cursor - 8, 156, 32, 10), "☝", small);
            }
            if (Visit.AskYesNo)
                GUI.Label(R(48, 108, 176, 10), (Visit.Yes ? "▶YES        NO" : " YES       ▶NO"), text);

            GUI.Label(R(4, 182, 250, 10),
                $"flechas {Visit.Arrows:000}   monedas {Visit.Coins:000}   llaves {Visit.Keys:000}     ← → elegir   Espacio {(s.kind == "shop" ? "comprar" : "aceptar")}   ↓ salir",
                small);
        }

        // Con el estilo del remake (Visuals/UiKit): marco de filigrana, el nombre del dios como título, iconos en la tienda
        // y la mano sustituida por un marco dorado. Las posiciones son las del original.
        void DrawStyled(System.Func<float, float, float, float, Rect> R, float scale)
        {
            var s = Visit.Shrine;
            var props = Visuals.Atmosphere.Props;
            void T(Rect r, string str, float px, TextAnchor a, Color c, bool title = false) =>
                Visuals.UiKit.Text(r, str, px * scale / Visuals.UiKit.Scale, a, c, title);
            Visuals.UiKit.Box(new Rect(0, 0, Screen.width, Screen.height), new Color(0, 0, 0, 0.85f));
            Visuals.UiKit.TitleLine(R(0, 38, 256, 10), s.god.ToUpperInvariant(), 8 * scale / Visuals.UiKit.Scale);
            // Retrato provisional: un halo dorado.
            Visuals.UiKit.Medallion(R(118, 48, 20, 20));
            Visuals.UiKit.Panel(R(28, 66, 200, 54));
            T(R(44, 72, 176, 44), Visit.Text, 6.2f, TextAnchor.UpperLeft, Visuals.UiKit.Bone);

            if (s.kind == "shop")
            {
                Visuals.UiKit.Panel(R(28, 122, 200, 60));
                for (int slot = 0; slot < 3; slot++)
                {
                    var item = Visit.ItemAt(slot);
                    if (item == null) continue;
                    float x = 72 + 48 * slot;
                    if (!Visit.Sold.Contains(slot))
                    {
                        if (props != null) Visuals.UiKit.Icon(R(x - 2, 126, 20, 20), props.Item(item.type));
                        T(R(x - 18, 146, 52, 8), item.name, 4.2f, TextAnchor.MiddleCenter, Visuals.UiKit.Dim);
                    }
                    T(R(x - 8, 154, 32, 10), item.price.ToString("00"), 6, TextAnchor.MiddleCenter, Visuals.UiKit.Gold, true);
                    if (Visit.Cursor == slot)
                        Visuals.UiKit.Frame(R(x - 10, 124, 36, 42), Visuals.UiKit.Gold, Mathf.Max(1, scale * 0.6f));
                }
            }
            if (Visit.AskYesNo)
            {
                T(R(60, 106, 60, 10), "SÍ", 6.5f, TextAnchor.MiddleCenter, Visit.Yes ? Visuals.UiKit.Gold : Visuals.UiKit.Dim, true);
                T(R(136, 106, 60, 10), "NO", 6.5f, TextAnchor.MiddleCenter, Visit.Yes ? Visuals.UiKit.Dim : Visuals.UiKit.Gold, true);
            }
            T(R(0, 184, 256, 8),
                $"flechas {Visit.Arrows:000}   monedas {Visit.Coins:000}   llaves {Visit.Keys:000}     ← → elegir   Espacio {(s.kind == "shop" ? "comprar" : "aceptar")}   ↓ salir",
                4.2f, TextAnchor.MiddleCenter, Visuals.UiKit.Dim);
        }

        static void Box(Rect r, Color c)
        {
            var old = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = old;
        }

        static void Frame(Rect r, Color c)
        {
            Box(new Rect(r.x, r.y, r.width, 1), c);
            Box(new Rect(r.x, r.yMax - 1, r.width, 1), c);
            Box(new Rect(r.x, r.y, 1, r.height), c);
            Box(new Rect(r.xMax - 1, r.y, 1, r.height), c);
        }
    }
}
