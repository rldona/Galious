using System.Collections.Generic;
using UnityEngine;

namespace MazeOfKnight.Game.Visuals
{
    // Lámina de tutorial de Galious: The Hollow Kingdom, como las de Hollow Knight: la primera vez que se recoge cada arma,
    // objeto o amuleto (GameBootstrap.Learned), el juego se para y sale su icono iluminado, su nombre, qué hace y cómo
    // se usa (ItemTexts). Los amuletos llevan además el dibujo del Caballero en el banco. Aceptar la cierra.
    public class ItemTutorial : MonoBehaviour
    {
        public GameBootstrap game;
        public bool Open { get; private set; }
        public const float MinSeconds = 0.8f;      // antes no se cierra, para no saltarla sin querer

        readonly Queue<int> pending = new Queue<int>();
        int type;
        float shownAt;
        static Sprite glow, line, charmArt, crest;
        static bool loaded;

        void Start()
        {
            if (game != null) game.Learned += t => { if (!game.Busy) pending.Enqueue(t); };
        }

        // Para las capturas.
        public void ShowNow(int t) { pending.Clear(); Show(t); }
        public void HideNow() { pending.Clear(); Open = false; game.SetPaused(false); }

        void Show(int t)
        {
            type = t;
            Open = true;
            shownAt = Time.unscaledTime;
            game.SetPaused(true);
            Sound.Effect(0x2E);
        }

        void Update()
        {
            if (game == null) return;
            if (!Open)
            {
                if (pending.Count > 0 && game.Motor != null && !game.Busy && !game.MenuOpen) Show(pending.Dequeue());
                return;
            }
            if (Time.unscaledTime - shownAt < MinSeconds) return;
            if (HollowMenus.Accept || HollowMenus.Back)
            {
                Open = false;
                if (pending.Count > 0) Show(pending.Dequeue());
                else game.SetPaused(false);
            }
        }

        static void Load()
        {
            if (loaded) return;
            loaded = true;
            glow = Resources.Load<Sprite>("Hollow/inventario/luz");
            line = Resources.Load<Sprite>("Hollow/inventario/lamina_separador");
            charmArt = Resources.Load<Sprite>("Hollow/inventario/lamina_amuletos");
            crest = Resources.Load<Sprite>("Hollow/menu/remate");
        }

        void OnGUI()
        {
            if (!Open) return;
            Load();
            GUI.depth = -10;
            float s = UiKit.Scale, W = Screen.width, H = Screen.height;
            float a = Mathf.Clamp01((Time.unscaledTime - shownAt) / 0.35f);
            UiKit.Box(new Rect(0, 0, W, H), new Color(0.01f, 0.01f, 0.03f, 0.88f * a));
            bool charm = Inventory.IsCharm(type);
            var e = ItemTexts.Get(type);
            float cx = W / 2, y = H * 0.12f;
            // El icono con su halo, que late despacio.
            float pulse = 0.85f + 0.15f * Mathf.Sin(Time.unscaledTime * 2.2f);
            float gw = 340 * s * pulse;
            if (glow != null) UiKit.Icon(new Rect(cx - gw / 2, y + 75 * s - gw * 0.44f, gw, gw * 0.88f), glow, 0.55f * a);
            var icon = HollowItems.Icon(type) ?? Atmosphere.Props?.Item(type);
            UiKit.Icon(new Rect(cx - 75 * s, y, 150 * s, 150 * s), icon, a);
            y += 175 * s;
            UiKit.Text(new Rect(0, y, W, 56 * s), e.Name, 40, TextAnchor.MiddleCenter, UiKit.Bone, true, a);
            y += 60 * s;
            float lw = Mathf.Min(W * 0.6f, 640 * s);
            if (line != null) UiKit.Icon(new Rect(cx - lw / 2, y, lw, lw * line.rect.height / line.rect.width), line, 0.9f * a);
            y += 34 * s;
            float tw = Mathf.Min(W - 80 * s, 700 * s);
            UiKit.Text(new Rect(cx - tw / 2, y, tw, 120 * s), e.Text, 21, TextAnchor.UpperCenter, UiKit.Bone, false, a);
            y += 110 * s;
            if (e.Use != null)
            {
                UiKit.Text(new Rect(cx - tw / 2, y, tw, 60 * s), e.Use, 19, TextAnchor.UpperCenter, UiKit.Gold, false, a);
                y += 56 * s;
            }
            if (charm)
            {
                float ch = 120 * s, cw = charmArt != null ? ch * charmArt.rect.width / charmArt.rect.height : 0;
                if (charmArt != null) UiKit.Icon(new Rect(cx - cw / 2, y, cw, ch), charmArt, 0.9f * a);
                y += ch + 8 * s;
                UiKit.Text(new Rect(cx - tw / 2, y, tw, 50 * s), "Es un amuleto: cuesta muescas y solo hace efecto equipado. Se equipa sentado en un banco, en la pestaña Amuletos (I).",
                    17, TextAnchor.UpperCenter, UiKit.Dim, false, a);
            }
            // La cresta abajo y, pasado el rato, cómo seguir.
            if (crest != null) UiKit.Icon(new Rect(cx - 26 * s, H - 120 * s, 52 * s, 52 * s * crest.rect.height / crest.rect.width), crest, 0.8f * a);
            if (Time.unscaledTime - shownAt >= MinSeconds)
                UiKit.Text(new Rect(0, H - 50 * s, W, 30 * s), "Aceptar", 16, TextAnchor.MiddleCenter, UiKit.Dim, true, a);
        }
    }
}
