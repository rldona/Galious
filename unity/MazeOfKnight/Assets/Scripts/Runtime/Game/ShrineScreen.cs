using UnityEngine;
using UnityEngine.InputSystem;

namespace MazeOfKnight.Game
{
    // Pantalla de un santuario (estado 16 en el castillo, 14 en los mundos; docs/design_bible/08_santuarios.md):
    // nombre y retrato del dios, ventana de texto (filas 4–10) y, en las tiendas, tres huecos con su precio y la mano
    // debajo del elegido. ← → eligen, espacio compra o responde, ↓ sale (también con mando: cruceta o stick, A y B). El
    // juego queda en pausa. Con el arte de Hollow Knight, DrawHollow.
    // [I] Sin gráficos del original: cuadros y texto. Sin la animación de bajar al santuario.
    public class ShrineScreen : MonoBehaviour
    {
        public GameBootstrap game;
        public ShrineVisit Visit { get; private set; }
        GUIStyle text, small;
        float autoExit;

        public bool Open => Visit != null;

        // Salir del santuario como con ↓ (lo usan las capturas).
        public void Close()
        {
            if (Visit == null) return;
            var v = Visit;
            Visit = null;
            game.LeaveShrine(v);
        }

        public void Show(ShrineVisit visit)
        {
            Visit = visit;
            shownAt = Time.unscaledTime;   // el ↓ con el que se ha entrado no cuenta para salir
        }

        float shownAt;

        void Update()
        {
            if (Visit == null) return;
            if (Visit.AutoExitFrames > 0)
            {
                // La Muerte ya ha respondido: se sale sola a los 180 frames y ↓ no hace nada.
                autoExit += Time.unscaledDeltaTime * 60f;
                if (autoExit >= Visit.AutoExitFrames) { var w = Visit; Visit = null; autoExit = 0; game.LeaveShrine(w); }
                return;
            }
            // Con teclado y con mando (Visuals/HollowMenus): ← → eligen (cruceta o stick), Aceptar (A, Espacio, Intro o Z)
            // compra o responde, y ↓ (como en el original), B o Esc salen. El ↓ o el stick con que se ha entrado no cuentan.
            var kb = Keyboard.current;
            bool settled = Time.unscaledTime - shownAt > 0.35f;
            bool exit = settled && (Visuals.HollowMenus.Down || Visuals.HollowMenus.Back);
            if (exit)
            {
                var v = Visit;
                Visit = null;
                game.LeaveShrine(v);
                return;
            }
            if (Visuals.HollowMenus.Left || kb != null && kb.aKey.wasPressedThisFrame) { Visit.Move(-1); Sound.Effect(0x2C); }
            if (Visuals.HollowMenus.Right || kb != null && kb.dKey.wasPressedThisFrame) { Visit.Move(1); Sound.Effect(0x2C); }
            if (settled && Visuals.HollowMenus.Accept)
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
            if (Visuals.HollowMenus.Available)
            {
                DrawHollow();
                return;
            }
            if (Visuals.Atmosphere.Styled)
            {
                {
                    // Centrado en la ventana: el recuadro del menú (y = 28…196) en el medio.
                    var (CR, cscale) = Visuals.UiKit.Centered(-4, 28, 264, 168);
                    DrawStyled(CR, cscale);
                }
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
            Visuals.UiKit.TitleLine(R(0, 30, 256, 10), s.god.ToUpperInvariant(), 8 * scale / Visuals.UiKit.Scale);
            // Retrato del dios.
            if (props != null) Visuals.UiKit.Icon(R(114, 40, 28, 28), props.God(s.god));
            else Visuals.UiKit.Medallion(R(118, 48, 20, 20));
            Visuals.UiKit.Panel(R(28, 70, 200, 50));
            T(R(44, 76, 176, 42), Visit.Text, 6.2f, TextAnchor.UpperLeft, Visuals.UiKit.Bone);

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

        // Como en Hollow Knight (Galious: The Hollow Kingdom): el dios lo hace un personaje de Hollow Knight (tools/hollow/
        // dioses.py) a la izquierda, con un halo; a la derecha su nombre con la filigrana, lo que dice en castellano
        // (ShrineTexts) y, en las tiendas, los tres objetos con su icono, su nombre y su precio en geo, las flechas de los
        // menús en el elegido y su descripción debajo. Abajo, lo que se lleva y los botones.
        void DrawHollow()
        {
            var sh = Visit.Shrine;
            float s = Visuals.UiKit.Scale, W = Screen.width, H = Screen.height;
            GUI.depth = -9;
            Visuals.UiKit.Box(new Rect(0, 0, W, H), new Color(0.01f, 0.01f, 0.02f, 0.985f));
            // El dios, de pie y respirando.
            var frames = GodFrames(sh.god);
            float px = W * 0.26f, feet = H * 0.80f;
            var old = GUI.color;
            GUI.color = new Color(0.75f, 0.8f, 1f, 0.22f);
            GUI.DrawTexture(new Rect(px - 260 * s, feet - 520 * s, 520 * s, 520 * s), Visuals.HollowMenus.Blob);
            GUI.color = old;
            if (frames.Length > 0)
            {
                var sp = frames[(int)(Time.unscaledTime * 10) % frames.Length];
                // Todos del mismo alto en pantalla (Sly es diminuto y Grimm muy alto en Hollow Knight); el más ancho, que quepa.
                float k = H * 0.42f / frames[0].rect.height;
                k = Mathf.Min(k, W * 0.36f / frames[0].rect.width);
                float w = sp.rect.width * k, h = sp.rect.height * k;
                Visuals.UiKit.Icon(new Rect(px - w / 2, feet - h, w, h), sp);
            }
            // Nombre y lo que dice.
            float cx = W * 0.64f, cw = Mathf.Min(W * 0.5f, 760 * s), x0 = cx - cw / 2;
            float y = H * 0.12f;
            Visuals.UiKit.Text(new Rect(x0, y, cw, 50 * s), ShrineTexts.God(sh.god).ToUpperInvariant(), 38, TextAnchor.MiddleCenter, Visuals.UiKit.Bone, true);
            Visuals.HollowMenus.Divider(cx, y + 54 * s, 260 * s);
            y += 54 * s + Visuals.HollowMenus.DividerHeight(260 * s) + 18 * s;
            Visuals.UiKit.Text(new Rect(x0, y, cw, 110 * s), ShrineTexts.Spanish(Visit.Text), 22, TextAnchor.UpperCenter, Visuals.UiKit.Bone);
            y += 120 * s;
            if (Visit.AskYesNo)
            {
                // Sí y No, uno al lado del otro (← → como en el original).
                foreach (var (label, on, ox) in new[] { ("Sí", Visit.Yes, -110f), ("No", !Visit.Yes, 110f) })
                {
                    var r = new Rect(cx + ox * s - 100 * s, y, 200 * s, 40 * s);
                    Visuals.UiKit.Text(r, label, 26, TextAnchor.MiddleCenter, on ? Visuals.UiKit.Bone : Visuals.UiKit.Dim, true);
                    if (on) Visuals.HollowMenus.Arrows(r.center.x, r.center.y, label, 26);
                }
            }
            if (sh.kind == "shop")
            {
                float cell = cw / 3;
                for (int slot = 0; slot < 3; slot++)
                {
                    var item = Visit.ItemAt(slot);
                    if (item == null) continue;
                    float ccx = x0 + cell * (slot + 0.5f);
                    bool sold = Visit.Sold.Contains(slot), on = Visit.Cursor == slot;
                    float a = sold ? 0.3f : 1f;
                    if (on)
                    {
                        old = GUI.color;
                        GUI.color = new Color(1, 0.95f, 0.8f, 0.12f);
                        GUI.DrawTexture(new Rect(ccx - 90 * s, y - 20 * s, 180 * s, 200 * s), Visuals.HollowMenus.Blob);
                        GUI.color = old;
                    }
                    if (Visuals.HollowItems.Icon(item.type) is Sprite icon)
                        Visuals.UiKit.Icon(new Rect(ccx - 44 * s, y, 88 * s, 88 * s), icon, a);
                    string name = sold ? "Vendido" : ShrineTexts.Item(item.type);
                    var nr = new Rect(ccx - cell / 2, y + 96 * s, cell, 30 * s);
                    Visuals.UiKit.Text(nr, name, 19, TextAnchor.MiddleCenter, on ? Visuals.UiKit.Bone : Visuals.UiKit.Dim, true, a);
                    if (on && !sold) Visuals.HollowMenus.Arrows(nr.center.x, nr.center.y, name, 19);
                    Visuals.UiKit.Text(new Rect(ccx - cell / 2, y + 128 * s, cell, 26 * s), $"{item.price} geo", 18, TextAnchor.MiddleCenter, Visuals.UiKit.Gold, true, a);
                }
                if (Visit.Cursor >= 0 && Visit.ItemAt(Visit.Cursor) is MazeOfKnight.Data.ShrineItem sel)
                    Visuals.UiKit.Text(new Rect(x0, y + 172 * s, cw, 70 * s), ShrineTexts.ItemText(sel.type), 17, TextAnchor.UpperCenter, Visuals.UiKit.Dim);
            }
            // Lo que se lleva y los botones.
            float by = H - 64 * s;
            float ix = W / 2 - 330 * s;
            foreach (var (type, n) in new[] { (0x2E, Visit.Coins), (0x2D, Visit.Arrows), (0x2F, Visit.Keys) })
            {
                if (Visuals.HollowItems.Icon(type) is Sprite ic) Visuals.UiKit.Icon(new Rect(ix, by, 30 * s, 30 * s), ic);
                Visuals.UiKit.Text(new Rect(ix + 36 * s, by, 80 * s, 30 * s), n.ToString(), 19, TextAnchor.MiddleLeft, Visuals.UiKit.Bone, true);
                ix += 110 * s;
            }
            string accept = sh.kind == "shop" ? "Comprar" : "Aceptar";
            Visuals.UiKit.Text(new Rect(W / 2, by, 330 * s, 30 * s), Visit.AutoExitFrames > 0 ? "" : $"{accept}  A · Espacio     Salir  B · Esc · ↓",
                15, TextAnchor.MiddleRight, Visuals.UiKit.Dim);
        }

        static readonly System.Collections.Generic.Dictionary<string, Sprite[]> godFrames = new System.Collections.Generic.Dictionary<string, Sprite[]>();

        // Los fotogramas del personaje de Hollow Knight que hace de ese dios (vacío si no se ha importado).
        static Sprite[] GodFrames(string god)
        {
            if (godFrames.TryGetValue(god, out var f)) return f;
            var l = new System.Collections.Generic.List<Sprite>();
            for (int i = 0; ; i++)
            {
                var sp = Resources.Load<Sprite>($"Hollow/dioses/{god.ToLowerInvariant()}_{i:00}");
                if (sp == null) break;
                l.Add(sp);
            }
            return godFrames[god] = l.ToArray();
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
