using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MazeOfKnight.Game.Visuals
{
    // Piezas comunes del menú principal y del de pausa, al estilo de Hollow Knight (Assets/Art/Resources/Hollow/menu, fuera
    // de git; las importa tools/hollow/menu.py): la flecha con filigrana que señala la opción, los adornos del título, las
    // opciones (guardadas en PlayerPrefs) y la navegación con teclado o mando.
    public static class HollowMenus
    {
        public const string Title = "GALIOUS", Subtitle = "THE HOLLOW KINGDOM";
        static Sprite arrow, top, bottom, divider;
        static readonly GUIStyle measure = new GUIStyle();

        // Mancha suave (niebla) y viñeta, hechas aquí.
        static Texture2D blob, vignette;
        public static Texture2D Blob => blob != null ? blob : blob = Radial(64, 0f, 1f);
        public static Texture2D Vignette => vignette != null ? vignette : vignette = Radial(64, 0.45f, 1f, true);
        static Texture2D Radial(int n, float inner, float outer, bool invert = false)
        {
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(n / 2f, n / 2f)) / (n / 2f);
                    float a = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(outer, inner, d));
                    t.SetPixel(x, y, new Color(1, 1, 1, invert ? 1 - a : a));
                }
            t.Apply(false, true);
            return t;
        }
        static bool loaded;

        static void Load()
        {
            if (loaded) return;
            loaded = true;
            arrow = Resources.Load<Sprite>("Hollow/menu/flecha");
            top = Resources.Load<Sprite>("Hollow/menu/adorno_arriba");
            bottom = Resources.Load<Sprite>("Hollow/menu/adorno_abajo");
            divider = Resources.Load<Sprite>("Hollow/menu/separador");
        }

        public static bool Available => Resources.Load<Sprite>("Hollow/menu/flecha") != null;

        // ---------- entrada ----------

        public static bool Up => Pressed(k => k.upArrowKey, g => g.dpad.up) || Pressed(k => k.wKey, null);
        public static bool Down => Pressed(k => k.downArrowKey, g => g.dpad.down) || Pressed(k => k.sKey, null);
        public static bool Left => Pressed(k => k.leftArrowKey, g => g.dpad.left);
        public static bool Right => Pressed(k => k.rightArrowKey, g => g.dpad.right);
        public static bool Accept => Pressed(k => k.enterKey, g => g.buttonSouth) || Pressed(k => k.spaceKey, null) || Pressed(k => k.zKey, null);
        public static bool Back => Pressed(k => k.escapeKey, g => g.buttonEast) || Pressed(k => k.backspaceKey, null);
        public static bool Pause => Pressed(k => k.escapeKey, g => g.startButton);

        static bool Pressed(System.Func<Keyboard, UnityEngine.InputSystem.Controls.ButtonControl> key,
            System.Func<Gamepad, UnityEngine.InputSystem.Controls.ButtonControl> pad)
        {
            var kb = Keyboard.current;
            if (kb != null && key != null && key(kb).wasPressedThisFrame) return true;
            var gp = Gamepad.current;
            return gp != null && pad != null && pad(gp).wasPressedThisFrame;
        }

        // ---------- opciones ----------

        public static float Volume
        {
            get => PlayerPrefs.GetFloat("volumen", 0.35f);
            set { PlayerPrefs.SetFloat("volumen", Mathf.Clamp01(value)); Apply(); }
        }

        public static void Apply()
        {
            if (Sound.Instance != null) Sound.Instance.Volume = Volume;
        }

        // Opciones: (texto, ← → lo cambian, Aceptar lo cambia). "Volver" cierra.
        public static List<string> OptionLines() => new List<string>
        {
            $"Volumen   {Mathf.RoundToInt(Volume * 10)}",
            $"Pantalla completa   {(Screen.fullScreen ? "Sí" : "No")}",
            "Volver",
        };

        // Devuelve true al elegir "Volver".
        public static bool TickOptions(ref int sel)
        {
            int n = OptionLines().Count;
            if (Up) sel = (sel + n - 1) % n;
            if (Down) sel = (sel + 1) % n;
            int dx = Left ? -1 : Right ? 1 : 0;
            if (sel == 0 && dx != 0) Volume += dx * 0.1f;
            if (sel == 1 && (dx != 0 || Accept)) Screen.fullScreen = !Screen.fullScreen;
            if (Back || sel == n - 1 && Accept) return true;
            return false;
        }

        // ---------- dibujo ----------

        // Una lista de opciones centrada en cx desde y, con la flecha a los dos lados de la elegida.
        public static void List(IList<string> items, int sel, float cx, float y, float step, float size, float alpha = 1)
        {
            Load();
            float s = UiKit.Scale;
            for (int i = 0; i < items.Count; i++)
            {
                var r = new Rect(cx - 300 * s, y + i * step * s, 600 * s, step * s);
                bool on = i == sel;
                UiKit.Text(r, items[i], size, TextAnchor.MiddleCenter, on ? UiKit.Bone : UiKit.Dim, true, alpha);
                if (!on || arrow == null) continue;
                measure.font = UiKit.Title;
                measure.fontSize = Mathf.Max(8, Mathf.RoundToInt(size * s));
                float half = measure.CalcSize(new GUIContent(items[i])).x / 2 + 14 * s, h = size * 0.95f * s, w = h * arrow.rect.width / arrow.rect.height;
                UiKit.Icon(new Rect(cx - half - w, r.center.y - h / 2, w, h), arrow, alpha);
                UiKit.Icon(new Rect(cx + half, r.center.y - h / 2, w, h), arrow, alpha, true);
            }
        }

        // El título con sus adornos y un halo, centrado en cx con la parte de arriba en y; devuelve su alto.
        public static float Logo(float cx, float y, float width, float alpha = 1)
        {
            Load();
            float s = UiKit.Scale;
            float topH = top != null ? width * 0.62f * top.rect.height / top.rect.width : 0;
            if (top != null) UiKit.Icon(new Rect(cx - width * 0.31f, y, width * 0.62f, topH), top, alpha);
            float ty = y + topH + 6 * s, size = width / 6.2f / s;
            // Halo: el texto varias veces, desplazado y transparente.
            var glow = new Color(0.85f, 0.85f, 1f, 0.06f);
            for (int i = 0; i < 8; i++)
            {
                float a = i * Mathf.PI / 4, d = 5 * s;
                UiKit.Text(new Rect(cx - width / 2 + Mathf.Cos(a) * d, ty + Mathf.Sin(a) * d, width, size * 1.25f * s), Title, size, TextAnchor.MiddleCenter, glow, true, alpha);
            }
            UiKit.Text(new Rect(cx - width / 2, ty, width, size * 1.25f * s), Title, size, TextAnchor.MiddleCenter, Color.white, true, alpha);
            float sy = ty + size * 1.2f * s;
            UiKit.Text(new Rect(cx - width / 2, sy, width, size * 0.42f * s), Subtitle, size * 0.3f, TextAnchor.MiddleCenter, UiKit.Bone, true, alpha);
            float by = sy + size * 0.5f * s;
            float botH = bottom != null ? width * 0.62f * bottom.rect.height / bottom.rect.width : 0;
            if (bottom != null) UiKit.Icon(new Rect(cx - width * 0.31f, by, width * 0.62f, botH), bottom, alpha);
            return by + botH - y;
        }

        // El separador con filigrana bajo un título de pantalla.
        public static void Divider(float cx, float y, float width, float alpha = 1)
        {
            Load();
            if (divider == null) return;
            float h = width * divider.rect.height / divider.rect.width;
            UiKit.Icon(new Rect(cx - width / 2, y, width, h), divider, alpha);
        }
    }

    // Menú principal (RemakeRules.HollowTitle): sustituye al logo, el título, la historia y las demos del original
    // (AttractScreen). Sobre un fondo de nubes y agujas lejanas con niebla que se mueve y motas que flotan, el título del
    // juego con las filigranas de Hollow Knight y las opciones: Continuar (si hay partida guardada), Nueva partida,
    // Opciones, Extras y Salir.
    public class HollowMainMenu : MonoBehaviour
    {
        enum Screen_ { Main, Options, Extras }

        public bool Running { get; private set; } = true;
        Screen_ screen;
        int sel, optSel;
        float t, leaving = -1;
        System.Action onLeave;
        Sprite background, fog;
        readonly List<Vector4> motes = new List<Vector4>();   // x, y (0–1), velocidad, tamaño
        readonly List<string> items = new List<string>();

        void Start()
        {
            HollowMenus.Apply();
            // El fondo: una de las vistas lejanas del Palacio Blanco (o de los Cruces), las nubes y agujas del reino.
            background = Resources.Load<Sprite>("Hollow/escenario/palacio/lejano__05") ?? Resources.Load<Sprite>("Hollow/escenario/cruces/lejano__00");
            fog = Resources.Load<Sprite>("Hollow/liquidos/niebla");
            var rng = new System.Random(7);
            for (int i = 0; i < 70; i++)
                motes.Add(new Vector4((float)rng.NextDouble(), (float)rng.NextDouble(), 0.01f + (float)rng.NextDouble() * 0.03f, 1 + (float)rng.NextDouble() * 3));
            Sound.StopMusic();
            Rebuild();
        }

        // Para las capturas: 0 principal, 1 opciones, 2 extras.
        public void Show(int which) { screen = (Screen_)which; t = Mathf.Max(t, 2f); }

        void Rebuild()
        {
            items.Clear();
            if (SaveData.Exists) items.Add("Continuar");
            items.Add("Nueva partida");
            items.Add("Opciones");
            items.Add("Extras");
            items.Add("Salir del juego");
            sel = Mathf.Clamp(sel, 0, items.Count - 1);
        }

        void Leave(System.Action then)
        {
            if (leaving >= 0) return;
            leaving = 0;
            onLeave = then;
        }

        void Update()
        {
            t += Time.unscaledDeltaTime;
            if (leaving >= 0)
            {
                leaving += Time.unscaledDeltaTime;
                if (leaving >= 0.7f) { Running = false; onLeave?.Invoke(); onLeave = null; }
                return;
            }
            if (t < 0.4f) return;
            switch (screen)
            {
                case Screen_.Main:
                    if (HollowMenus.Up) sel = (sel + items.Count - 1) % items.Count;
                    if (HollowMenus.Down) sel = (sel + 1) % items.Count;
                    if (!HollowMenus.Accept) break;
                    switch (items[sel])
                    {
                        case "Continuar": Leave(() => { AttractMode.LoadSave = true; AttractMode.Go(AttractMode.Mode.Game); }); break;
                        case "Nueva partida": Leave(() => AttractMode.Go(AttractMode.Mode.Game)); break;
                        case "Opciones": screen = Screen_.Options; optSel = 0; break;
                        case "Extras": screen = Screen_.Extras; break;
                        case "Salir del juego": Leave(() => { Application.Quit(); }); break;
                    }
                    break;
                case Screen_.Options:
                    if (HollowMenus.TickOptions(ref optSel)) screen = Screen_.Main;
                    break;
                case Screen_.Extras:
                    if (HollowMenus.Accept || HollowMenus.Back) screen = Screen_.Main;
                    break;
            }
        }

        void OnGUI()
        {
            if (!Running) return;
            GUI.depth = -10;
            float s = UiKit.Scale, W = Screen.width, H = Screen.height;
            UiKit.Box(new Rect(0, 0, W, H), new Color(0.02f, 0.02f, 0.035f, 1));
            // Fondo: la vista lejana, ampliada y moviéndose despacio, oscurecida y teñida de violeta.
            if (background != null)
            {
                float k = 1.18f + Mathf.Sin(t * 0.05f) * 0.02f, bw = W * k, bh = Mathf.Max(H * k, bw * background.rect.height / background.rect.width);
                var r = new Rect((W - bw) / 2 + Mathf.Sin(t * 0.07f) * 20 * s, (H - bh) / 2, bw, bh);
                var old = GUI.color;
                GUI.color = new Color(0.26f, 0.23f, 0.34f, 1f);
                GUI.DrawTexture(r, background.texture, ScaleMode.ScaleAndCrop);
                GUI.color = old;
            }
            // Una luz suave detrás del título, como el claro del menú de Hollow Knight.
            {
                var old = GUI.color;
                GUI.color = new Color(0.75f, 0.7f, 0.9f, 0.16f);
                GUI.DrawTexture(new Rect(W * 0.2f, -H * 0.15f, W * 0.6f, H * 0.85f), HollowMenus.Blob);
                // Niebla que pasa: manchas suaves y grandes.
                for (int i = 0; i < 7; i++)
                {
                    float fw = W * (0.5f + 0.1f * (i % 3)), fh = fw * 0.35f, x = ((t * (6 + i * 2) * s + i * 311) % (W + fw)) - fw;
                    GUI.color = new Color(0.7f, 0.66f, 0.85f, 0.08f);
                    GUI.DrawTexture(new Rect(x, H * (0.2f + (i * 0.11f) % 0.7f), fw, fh), HollowMenus.Blob);
                }
                // Viñeta: los bordes, casi negros.
                GUI.color = new Color(0, 0, 0, 0.9f);
                GUI.DrawTexture(new Rect(-W * 0.1f, -H * 0.1f, W * 1.2f, H * 1.2f), HollowMenus.Vignette);
                GUI.color = old;
            }
            // Motas: claras que suben y oscuras que bajan, como en el menú de Hollow Knight.
            for (int i = 0; i < motes.Count; i++)
            {
                var m = motes[i];
                bool dark = i % 3 == 0;
                float y = dark ? (m.y + t * m.z) % 1 : 1 - (m.y + t * m.z) % 1;
                float x = m.x + Mathf.Sin(t * 0.5f + i) * 0.01f, size = m.w * s * (dark ? 2.2f : 1);
                UiKit.Box(new Rect(x * W, y * H, size, size), dark ? new Color(0, 0, 0, 0.6f) : new Color(1, 1, 1, 0.25f + 0.2f * Mathf.Sin(t * 2 + i)));
            }
            float alpha = Mathf.Clamp01(t / 1.2f) * (leaving >= 0 ? Mathf.Clamp01(1 - leaving / 0.7f) : 1);
            float logoH = HollowMenus.Logo(W / 2, H * 0.08f, Mathf.Min(W * 0.62f, 900 * s), alpha);
            float y0 = H * 0.08f + logoH + 40 * s;
            switch (screen)
            {
                case Screen_.Main:
                    HollowMenus.List(items, sel, W / 2, y0, 46, 26, alpha);
                    break;
                case Screen_.Options:
                    UiKit.Text(new Rect(0, y0 - 10 * s, W, 40 * s), "Opciones", 30, TextAnchor.MiddleCenter, UiKit.Bone, true, alpha);
                    HollowMenus.Divider(W / 2, y0 + 30 * s, 320 * s, alpha);
                    HollowMenus.List(HollowMenus.OptionLines(), optSel, W / 2, y0 + 70 * s, 44, 24, alpha);
                    break;
                case Screen_.Extras:
                    UiKit.Text(new Rect(0, y0 - 10 * s, W, 40 * s), "Extras", 30, TextAnchor.MiddleCenter, UiKit.Bone, true, alpha);
                    HollowMenus.Divider(W / 2, y0 + 30 * s, 320 * s, alpha);
                    UiKit.Text(new Rect(W / 2 - 360 * s, y0 + 70 * s, 720 * s, 220 * s),
                        "Un homenaje a Knightmare II: The Maze of Galious (Konami, 1987), con sus salas, sus reglas y sus jefes, " +
                        "y a Hollow Knight (Team Cherry, 2017), con su arte, su música y su forma de jugar.\n\n" +
                        "Galious, Popolon y el reino de Grecia son de Konami. El Caballero, Hallownest y su arte son de Team Cherry.",
                        19, TextAnchor.UpperCenter, UiKit.Dim, false, alpha);
                    HollowMenus.List(new[] { "Volver" }, 0, W / 2, Mathf.Min(y0 + 230 * s, H - 70 * s), 44, 24, alpha);
                    break;
            }
            UiKit.Text(new Rect(20 * s, H - 40 * s, 400 * s, 30 * s), "Remake no oficial", 13, TextAnchor.MiddleLeft, UiKit.Dim, false, alpha * 0.8f);
            // Fundido al entrar y al salir.
            float fade = t < 0.6f ? 1 - t / 0.6f : leaving >= 0 ? Mathf.Clamp01(leaving / 0.7f) : 0;
            if (fade > 0) UiKit.Box(new Rect(0, 0, W, H), new Color(0, 0, 0, fade));
        }
    }

    // Menú de pausa en la partida (Esc o Start): Continuar, Opciones y Salir al menú principal, sobre el juego oscurecido.
    public class HollowPauseMenu : MonoBehaviour
    {
        public GameBootstrap game;
        public bool Open { get; private set; }
        bool options;
        int sel, optSel;
        static readonly string[] Items = { "Continuar", "Opciones", "Salir al menú principal" };

        void Update()
        {
            if (game == null) return;
            if (!Open)
            {
                if (HollowMenus.Pause && !game.Busy && !game.MenuOpen && game.Motor != null)
                {
                    Open = true;
                    options = false;
                    sel = 0;
                    game.SetPaused(true);
                }
                return;
            }
            if (options)
            {
                if (HollowMenus.TickOptions(ref optSel)) options = false;
                return;
            }
            if (HollowMenus.Up) sel = (sel + Items.Length - 1) % Items.Length;
            if (HollowMenus.Down) sel = (sel + 1) % Items.Length;
            if (HollowMenus.Back) { Close(); return; }
            if (!HollowMenus.Accept) return;
            switch (sel)
            {
                case 0: Close(); break;
                case 1: options = true; optSel = 0; break;
                case 2: Open = false; AttractMode.Go(AttractMode.Mode.Attract); break;
            }
        }

        // Para las capturas: abierto ya.
        public void ShowNow() { Open = true; options = false; sel = 0; game.SetPaused(true); }
        public void HideNow() => Close();

        void Close()
        {
            Open = false;
            game.SetPaused(false);
        }

        void OnGUI()
        {
            if (!Open) return;
            GUI.depth = -9;
            float s = UiKit.Scale, W = Screen.width, H = Screen.height;
            UiKit.Box(new Rect(0, 0, W, H), new Color(0, 0, 0, 0.72f));
            float logoH = HollowMenus.Logo(W / 2, H * 0.12f, Mathf.Min(W * 0.45f, 640 * s), 0.9f);
            float y0 = H * 0.12f + logoH + 40 * s;
            if (options)
            {
                UiKit.Text(new Rect(0, y0 - 10 * s, W, 40 * s), "Opciones", 28, TextAnchor.MiddleCenter, UiKit.Bone, true);
                HollowMenus.Divider(W / 2, y0 + 28 * s, 300 * s);
                HollowMenus.List(HollowMenus.OptionLines(), optSel, W / 2, y0 + 66 * s, 42, 22);
            }
            else
                HollowMenus.List(Items, sel, W / 2, y0, 44, 24);
        }
    }
}
