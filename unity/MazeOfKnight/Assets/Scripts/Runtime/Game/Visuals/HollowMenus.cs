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

        public static bool Up => Pressed(k => k.upArrowKey, g => g.dpad.up) || Pressed(k => k.wKey, null) || Stick().y > 0;
        public static bool Down => Pressed(k => k.downArrowKey, g => g.dpad.down) || Pressed(k => k.sKey, null) || Stick().y < 0;
        public static bool Left => Pressed(k => k.leftArrowKey, g => g.dpad.left) || Stick().x < 0;
        public static bool Right => Pressed(k => k.rightArrowKey, g => g.dpad.right) || Stick().x > 0;

        // El stick izquierdo como cruceta: una pulsación al inclinarlo y, si se mantiene, otra cada 0,15 s pasado 0,4 s.
        static int stickFrame = -1;
        static Vector2Int stickDir, stickHeld;
        static float stickNext;
        static Vector2Int Stick()
        {
            if (stickFrame == Time.frameCount) return stickDir;
            stickFrame = Time.frameCount;
            var v = Gamepad.current?.leftStick.ReadValue() ?? Vector2.zero;
            var d = new Vector2Int(v.x > 0.6f ? 1 : v.x < -0.6f ? -1 : 0, v.y > 0.6f ? 1 : v.y < -0.6f ? -1 : 0);
            if (Mathf.Abs(v.x) > Mathf.Abs(v.y)) d.y = 0; else d.x = 0;
            float now = Time.unscaledTime;
            if (d == Vector2Int.zero) { stickHeld = d; stickDir = d; }
            else if (d != stickHeld) { stickHeld = d; stickDir = d; stickNext = now + 0.4f; }
            else if (now >= stickNext) { stickDir = d; stickNext = now + 0.15f; }
            else stickDir = Vector2Int.zero;
            return stickDir;
        }
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

        // Las opciones están en GameSettings y su menú en OptionsMenu.
        public static void Apply() => GameSettings.Apply();

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
                if (on) Arrows(cx, r.center.y, items[i], size, alpha);
            }
        }

        // Las dos flechas con filigrana a los lados de un texto centrado en cx (la opción elegida).
        public static void Arrows(float cx, float cy, string text, float size, float alpha = 1)
        {
            Load();
            if (arrow == null) return;
            float s = UiKit.Scale;
            measure.font = UiKit.Title;
            measure.fontSize = Mathf.Max(8, Mathf.RoundToInt(size * s));
            float half = measure.CalcSize(new GUIContent(text)).x / 2 + 14 * s, h = size * 0.95f * s, w = h * arrow.rect.width / arrow.rect.height;
            UiKit.Icon(new Rect(cx - half - w, cy - h / 2, w, h), arrow, alpha);
            UiKit.Icon(new Rect(cx + half, cy - h / 2, w, h), arrow, alpha, true);
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
        // Alto de la filigrana con ese ancho.
        public static float DividerHeight(float width)
        {
            Load();
            return divider != null ? width * divider.rect.height / divider.rect.width : 0;
        }

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
        enum Screen_ { Notice, Main, Options, Extras, Quit }

        public bool Running { get; private set; } = true;
        Screen_ screen;
        int sel, quitSel;
        readonly OptionsMenu options = new OptionsMenu();
        AudioSource music;
        public string MusicState => music == null ? "sin fuente" : $"clip {(music.clip != null ? music.clip.name + " " + music.clip.loadState : "null")}, suena {music.isPlaying}, volumen {music.volume:0.00}, tiempo {music.time:0.00}, 3D {music.spatialBlend}, mute {music.mute}, oyente {AudioListener.volume} pausa {AudioListener.pause}";
        static bool noticeShown;            // el aviso de autoguardado, una vez al arrancar
        bool quitting;
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
            // La música del menú de Hollow Knight (Assets/Art/Resources/Hollow/musica, de tools/hollow/musica.py).
            music = gameObject.AddComponent<AudioSource>();
            music.clip = Resources.Load<AudioClip>("Hollow/musica/titulo");
            music.loop = true;
            music.volume = 0;
            if (music.clip != null) music.Play();
            Rebuild();
            if (!noticeShown && HollowMenus.Available) screen = Screen_.Notice;
            noticeShown = true;
        }

        // Para las capturas: 0 principal, 1 opciones, 2 extras, 3 salir, 4 aviso; page, la página de opciones.
        public void Show(int which, OptionsMenu.Page page = OptionsMenu.Page.Root)
        {
            screen = which switch { 1 => Screen_.Options, 2 => Screen_.Extras, 3 => Screen_.Quit, 4 => Screen_.Notice, _ => Screen_.Main };
            options.Open(page);
            t = Mathf.Max(t, 2f);
        }

        void Rebuild()
        {
            items.Clear();
            if (SaveData.Exists) items.Add("Continuar");
            items.Add("Nueva partida");
            items.Add("Opciones");
            items.Add("Extras");
            if (Application.platform != RuntimePlatform.WebGLPlayer) items.Add("Salir del juego");   // en el navegador no se puede cerrar
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
            // La música entra despacio (tras el aviso) y se apaga al salir; sigue el volumen de música de Opciones.
            if (music != null)
            {
                float fade = screen == Screen_.Notice ? 0 : leaving >= 0 ? Mathf.Clamp01(1 - leaving / 0.7f) : 1;
                music.volume = Mathf.MoveTowards(music.volume, fade * GameSettings.HollowMusicVolume, Time.unscaledDeltaTime * (leaving >= 0 ? 2f : 0.4f));
            }
            if (leaving >= 0)
            {
                leaving += Time.unscaledDeltaTime;
                if (leaving >= 0.7f && quitting)
                {
                    // Al salir del juego: todo en negro (la cámara tampoco pinta) hasta que se cierra la ventana.
                    if (Camera.main != null) { Camera.main.cullingMask = 0; Camera.main.clearFlags = CameraClearFlags.SolidColor; Camera.main.backgroundColor = Color.black; }
                    onLeave?.Invoke();
                    onLeave = null;
                    return;
                }
                if (leaving >= 0.7f) { Running = false; onLeave?.Invoke(); onLeave = null; }
                return;
            }
            if (t < 0.4f) return;
            switch (screen)
            {
                case Screen_.Notice:
                    // Como en Hollow Knight: unos segundos, o hasta que se pulse algo.
                    if (t > 4.5f || t > 1.2f && (HollowMenus.Accept || HollowMenus.Back)) { screen = Screen_.Main; t = 0.4f; }
                    break;
                case Screen_.Main:
                    if (HollowMenus.Up) sel = (sel + items.Count - 1) % items.Count;
                    if (HollowMenus.Down) sel = (sel + 1) % items.Count;
                    if (!HollowMenus.Accept) break;
                    switch (items[sel])
                    {
                        case "Continuar": Leave(() => { AttractMode.LoadSave = true; AttractMode.Go(AttractMode.Mode.Game); }); break;
                        case "Nueva partida": Leave(() => AttractMode.Go(AttractMode.Mode.Game)); break;
                        case "Opciones": screen = Screen_.Options; options.Open(); break;
                        case "Extras": screen = Screen_.Extras; break;
                        case "Salir del juego": screen = Screen_.Quit; quitSel = 1; break;
                    }
                    break;
                case Screen_.Options:
                    if (options.Tick()) screen = Screen_.Main;
                    break;
                case Screen_.Quit:
                    if (HollowMenus.Up || HollowMenus.Down || HollowMenus.Left || HollowMenus.Right) quitSel = 1 - quitSel;
                    if (HollowMenus.Back || HollowMenus.Accept && quitSel == 1) screen = Screen_.Main;
                    else if (HollowMenus.Accept) { quitting = true; Leave(() => { Application.Quit(); }); }
                    break;
                case Screen_.Extras:
                    if (HollowMenus.Accept || HollowMenus.Back) screen = Screen_.Main;
                    break;
            }
        }

        // Sin la pasada de maquetación de IMGUI (no se usa GUILayout): era la mayor parte de la basura de cada fotograma.
        void OnEnable() => useGUILayout = false;

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
            if (screen == Screen_.Notice) { DrawNotice(W, H, s); return; }
            bool deep = screen == Screen_.Options;
            float logoH = deep ? 0 : HollowMenus.Logo(W / 2, H * 0.08f, Mathf.Min(W * 0.62f, 900 * s), alpha);
            float y0 = H * 0.08f + logoH + 40 * s;
            switch (screen)
            {
                case Screen_.Main:
                    HollowMenus.List(items, sel, W / 2, y0, 46, 26, alpha);
                    break;
                case Screen_.Options:
                    options.Draw(W / 2, H * 0.1f, alpha);
                    break;
                case Screen_.Quit:
                    UiKit.Text(new Rect(0, y0 - 10 * s, W, 40 * s), "¿Salir del juego?", 30, TextAnchor.MiddleCenter, UiKit.Bone, true, alpha);
                    HollowMenus.Divider(W / 2, y0 + 44 * s, 320 * s, alpha);
                    HollowMenus.List(new[] { "Sí", "No" }, quitSel, W / 2, y0 + 44 * s + HollowMenus.DividerHeight(320 * s) + 20 * s, 46, 26, alpha);
                    break;
                case Screen_.Extras:
                    UiKit.Text(new Rect(0, y0 - 10 * s, W, 40 * s), "Extras", 30, TextAnchor.MiddleCenter, UiKit.Bone, true, alpha);
                    HollowMenus.Divider(W / 2, y0 + 44 * s, 320 * s, alpha);
                    UiKit.Text(new Rect(W / 2 - 360 * s, y0 + 44 * s + HollowMenus.DividerHeight(320 * s) + 20 * s, 720 * s, 220 * s),
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

        // El aviso del principio, como el de Hollow Knight: la cresta que brilla, entre dos filigranas, y que el juego
        // guarda solo.
        static Sprite crest;
        void DrawNotice(float W, float H, float s)
        {
            UiKit.Box(new Rect(0, 0, W, H), Color.black);
            float a = Mathf.Clamp01(t / 0.8f) * Mathf.Clamp01((4.5f - t) / 0.6f);
            crest ??= Resources.Load<Sprite>("Hollow/menu/remate");
            float pulse = 0.75f + 0.25f * Mathf.Sin(t * 3f);
            var old = GUI.color;
            GUI.color = new Color(0.9f, 0.9f, 1f, 0.25f * a * pulse);
            GUI.DrawTexture(new Rect(W / 2 - 110 * s, H * 0.36f - 110 * s, 220 * s, 220 * s), HollowMenus.Blob);
            GUI.color = old;
            if (crest != null) UiKit.Icon(new Rect(W / 2 - 40 * s, H * 0.36f - 55 * s, 80 * s, 110 * s), crest, a * pulse);
            // El texto, en medio de las dos filigranas.
            float dh = HollowMenus.DividerHeight(360 * s), top = H * 0.52f + dh, bottom = H * 0.72f;
            HollowMenus.Divider(W / 2, H * 0.52f, 360 * s, a);
            UiKit.Text(new Rect(W / 2 - 420 * s, top, 840 * s, bottom - top),
                "Este juego guarda automáticamente al descansar en los bancos.\nNo cierres el juego mientras aparezca este símbolo.",
                21, TextAnchor.MiddleCenter, UiKit.Bone, false, a);
            HollowMenus.Divider(W / 2, H * 0.72f, 360 * s, a);
        }
    }

    // Menú de pausa en la partida (Esc o Start): Continuar, Opciones y Salir al menú principal, sobre el juego oscurecido.
    public class HollowPauseMenu : MonoBehaviour
    {
        public GameBootstrap game;
        public bool Open { get; private set; }
        bool options, confirm;
        int sel, confirmSel;
        readonly OptionsMenu optionsMenu = new OptionsMenu();
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
                if (optionsMenu.Tick()) options = false;
                return;
            }
            if (confirm)
            {
                if (HollowMenus.Up || HollowMenus.Down || HollowMenus.Left || HollowMenus.Right) confirmSel = 1 - confirmSel;
                if (HollowMenus.Back || HollowMenus.Accept && confirmSel == 1) confirm = false;
                else if (HollowMenus.Accept) { Open = false; AttractMode.Go(AttractMode.Mode.Attract); }
                return;
            }
            if (HollowMenus.Up) sel = (sel + Items.Length - 1) % Items.Length;
            if (HollowMenus.Down) sel = (sel + 1) % Items.Length;
            if (HollowMenus.Back) { Close(); return; }
            if (!HollowMenus.Accept) return;
            switch (sel)
            {
                case 0: Close(); break;
                case 1: options = true; optionsMenu.Open(); break;
                case 2: confirm = true; confirmSel = 1; break;
            }
        }

        // Para las capturas: abierto ya.
        public void ShowNow() { Open = true; options = confirm = false; sel = 0; game.SetPaused(true); }
        public void HideNow() => Close();

        void Close()
        {
            Open = false;
            game.SetPaused(false);
        }

        // Sin la pasada de maquetación de IMGUI (no se usa GUILayout): era la mayor parte de la basura de cada fotograma.
        void OnEnable() => useGUILayout = false;

        void OnGUI()
        {
            if (!Open) return;
            GUI.depth = -9;
            float s = UiKit.Scale, W = Screen.width, H = Screen.height;
            UiKit.Box(new Rect(0, 0, W, H), new Color(0, 0, 0, 0.72f));
            bool deep = options;
            float logoH = deep ? 0 : HollowMenus.Logo(W / 2, H * 0.12f, Mathf.Min(W * 0.45f, 640 * s), 0.9f);
            float y0 = H * 0.12f + logoH + 40 * s;
            if (options)
                optionsMenu.Draw(W / 2, H * 0.1f);
            else if (confirm)
            {
                UiKit.Text(new Rect(0, y0 - 10 * s, W, 40 * s), "¿Salir al menú principal?", 28, TextAnchor.MiddleCenter, UiKit.Bone, true);
                UiKit.Text(new Rect(0, y0 + 30 * s, W, 30 * s), "Se pierde lo hecho desde el último banco.", 17, TextAnchor.MiddleCenter, UiKit.Dim);
                HollowMenus.List(new[] { "Sí", "No" }, confirmSel, W / 2, y0 + 80 * s, 44, 24);
            }
            else
                HollowMenus.List(Items, sel, W / 2, y0, 44, 24);
        }
    }
}
