using UnityEngine;

namespace MazeOfKnight.Game.Visuals
{
    // Selector de letras para escribir con el mando las palabras del juego (las de los jefes, que salen en las lápidas,
    // y UMBRELLA con la Daga), que en el original se teclean. Se abre con el botón "Escribir palabras" (RB) y para el
    // juego; la cruceta o el stick eligen la letra, A la escribe (GameBootstrap.TypeLetter, como tecleada) y B o RB
    // cierran. Arriba se ven las últimas letras escritas.
    public class WordPad : MonoBehaviour
    {
        public GameBootstrap game;
        public bool Open { get; private set; }
        const string Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const int Cols = 7;
        int sel;
        string typed = "";
        float openedAt;

        // Para las capturas.
        public void ShowNow() { Open = true; openedAt = Time.unscaledTime; game.SetPaused(true); }
        public void HideNow() { Open = false; game.SetPaused(false); }

        void Update()
        {
            if (game == null) return;
            if (!Open)
            {
                if (Controls.Pressed("palabras") && game.Motor != null && !game.Busy && !game.MenuOpen)
                {
                    Open = true;
                    openedAt = Time.unscaledTime;
                    game.SetPaused(true);
                }
                return;
            }
            if (Time.unscaledTime - openedAt < 0.1f) return;
            if (HollowMenus.Back || Controls.Pressed("palabras")) { Open = false; game.SetPaused(false); return; }
            int n = Letters.Length;
            if (HollowMenus.Left) sel = (sel + n - 1) % n;
            if (HollowMenus.Right) sel = (sel + 1) % n;
            if (HollowMenus.Up) sel = sel - Cols >= 0 ? sel - Cols : sel;
            if (HollowMenus.Down) sel = sel + Cols < n ? sel + Cols : sel;
            if (HollowMenus.Accept)
            {
                char ch = Letters[sel];
                typed = (typed + ch).Length > 12 ? (typed + ch).Substring((typed + ch).Length - 12) : typed + ch;
                game.TypeLetter(ch);
                Sound.Effect(0x13);
            }
        }

        // Sin la pasada de maquetación de IMGUI (no se usa GUILayout): era la mayor parte de la basura de cada fotograma.
        void OnEnable() => useGUILayout = false;

        void OnGUI()
        {
            if (!Open) return;
            GUI.depth = -9;
            float s = UiKit.Scale, W = Screen.width, H = Screen.height;
            int rows = (Letters.Length + Cols - 1) / Cols;
            float cell = 64 * s, pw = Cols * cell + 60 * s, ph = rows * cell + 150 * s;
            var r = new Rect((W - pw) / 2, H - ph - 40 * s, pw, ph);
            UiKit.Box(r, new Color(0, 0, 0, 0.85f));
            HollowMenus.Divider(r.center.x, r.y - 30 * s, 220 * s);
            UiKit.Text(new Rect(r.x, r.y + 10 * s, r.width, 30 * s), "Escribir", 22, TextAnchor.MiddleCenter, UiKit.Bone, true);
            UiKit.Text(new Rect(r.x, r.y + 44 * s, r.width, 34 * s), typed.Length > 0 ? typed : "· · ·", 26, TextAnchor.MiddleCenter, UiKit.Gold, true);
            for (int i = 0; i < Letters.Length; i++)
            {
                var c = new Rect(r.x + 30 * s + (i % Cols) * cell, r.y + 90 * s + (i / Cols) * cell, cell, cell);
                if (i == sel) UiKit.Box(new Rect(c.x + 6 * s, c.y + 6 * s, c.width - 12 * s, c.height - 12 * s), new Color(1, 1, 1, 0.12f));
                UiKit.Text(c, Letters[i].ToString(), 28, TextAnchor.MiddleCenter, i == sel ? UiKit.Bone : UiKit.Dim, true);
            }
            UiKit.Text(new Rect(r.x, r.yMax - 34 * s, r.width, 28 * s), "A  escribir     B  cerrar", 15, TextAnchor.MiddleCenter, UiKit.Dim);
        }
    }
}
