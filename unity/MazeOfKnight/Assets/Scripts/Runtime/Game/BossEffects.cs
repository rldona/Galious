using MazeOfKnight.Data;
using UnityEngine;

namespace MazeOfKnight.Game
{
    // Efectos del combate contra el jefe (docs/boss_ai.md, "Invocación" y "Derrota"), sin los gráficos del original:
    //  - invocación (estado 15): el borde parpadea 60 frames (color 1 ↔ 14 cada 2) y la decoración se deshace en puntos
    //    durante 128 frames;
    //  - combate y hasta la celebración: fondo negro (solo quedan muros, suelos, escaleras y agua);
    //  - derrota (estado 19): el jefe se deshace en puntos durante 128 frames;
    //  - celebración (estado 20): 9 saltitos de 40 frames en el sitio, girándose al aterrizar. [I] La altura (6 px) es
    //    una lectura de vy = −10 y g = 0,5 en unidades de 1/16 px.
    public class BossEffects : MonoBehaviour
    {
        public GameBootstrap game;
        public PlayerController player;

        const int Cells = 128, CellRows = 80;          // puntos de 2 × 2 px sobre la sala de 256 × 160
        SpriteRenderer backdrop, bossDots;
        Texture2D backTex, bossTex;
        int[] order;                                   // orden aleatorio en que aparecen (o se van) los puntos
        float backShown = -1, bossShown = -1;
        Color? cameraColor;
        bool hopping;
        static readonly Color Msx14 = new Color(0.73f, 0.73f, 0.73f);

        void Start()
        {
            var rnd = new System.Random(7);
            order = new int[Cells * CellRows];
            for (int i = 0; i < order.Length; i++) order[i] = i;
            for (int i = order.Length - 1; i > 0; i--) { int j = rnd.Next(i + 1); (order[i], order[j]) = (order[j], order[i]); }
            backdrop = MakeRenderer("Fondo negro del jefe", -9, out backTex, Cells, CellRows);
            bossDots = MakeRenderer("Jefe deshaciéndose", 9, out bossTex, Cells, CellRows);
        }

        SpriteRenderer MakeRenderer(string name, int order, out Texture2D tex, int w, int h)
        {
            tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            // 2 px del original por texel: 128 texels = 256 px = 16 unidades.
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, w, h), Vector2.zero, RoomCoords.PixelsPerUnit / 2f);
            sr.sortingOrder = order;
            sr.enabled = false;
            return sr;
        }

        // Rellena los primeros n puntos del orden aleatorio con 'on' y el resto con 'off'.
        void Paint(Texture2D tex, float fraction, Color on, Color off)
        {
            int n = Mathf.RoundToInt(Mathf.Clamp01(fraction) * order.Length);
            var px = new Color32[order.Length];
            Color32 a = on, b = off;
            for (int i = 0; i < order.Length; i++) px[order[i]] = i < n ? a : b;
            tex.SetPixels32(px);
            tex.Apply(false);
        }

        void LateUpdate()
        {
            var f = game != null ? game.Bosses : null;
            var m = game != null ? game.Motor : null;
            bool here = f != null && m != null && f.Room == m.Room && f.Active;
            int frames = here ? f.StageTicks * 2 : 0;

            // Borde que parpadea al empezar la invocación.
            var cam = Camera.main;
            if (cam != null)
            {
                bool flash = here && f.Current == BossFight.Stage.Invoking && frames < 60;
                if (flash) { cameraColor ??= cam.backgroundColor; cam.backgroundColor = (frames / 2) % 2 == 0 ? Color.black : Msx14; }
                else if (cameraColor != null) { cam.backgroundColor = cameraColor.Value; cameraColor = null; }
            }

            // Fondo negro: se forma en puntos durante la invocación y se queda hasta que acaba la celebración.
            float back = !here ? 0 : f.Current == BossFight.Stage.Invoking ? (frames - 60) / 128f : f.Current == BossFight.Stage.RightExitOnly ? 0 : 1;
            back = Mathf.Clamp01(back);
            backdrop.enabled = back > 0;
            if (backdrop.enabled)
            {
                backdrop.transform.position = RoomCoords.RoomOrigin(m.Room);
                if (!Mathf.Approximately(back, backShown)) { Paint(backTex, back, Color.black, Color.clear); backShown = back; }
            }

            // El jefe vencido se deshace en puntos.
            bool dissolving = here && f.Current == BossFight.Stage.Dissolving;
            bossDots.enabled = dissolving;
            if (dissolving)
            {
                var (x0, y0, x1, y1) = f.LastBossRect;
                var bl = RoomCoords.PixelToWorld(m.Room, (int)x0, (int)y1);
                bossDots.transform.position = bl;
                bossDots.transform.localScale = new Vector3((x1 - x0) / 256f, (y1 - y0) / 160f, 1);
                float left = 1 - frames / 128f;
                if (!Mathf.Approximately(left, bossShown)) { Paint(bossTex, left, new Color(0.9f, 0.3f, 0.3f), Color.clear); bossShown = left; }
            }

            // Celebración: 9 saltitos de 40 frames.
            if (player != null)
            {
                bool hop = here && f.Current == BossFight.Stage.Fanfare && frames < 9 * 40;
                if (hop)
                {
                    int k = frames / 40;
                    float t = (frames % 40) / 40f;
                    player.HopPx = 24f * t * (1 - t);          // 6 px en lo más alto
                    player.HopFlip = (k & 1) == 1;
                }
                else if (hopping) { player.HopPx = 0; player.HopFlip = false; }   // sin pisar el hundimiento del santuario
                hopping = hop;
            }
        }
    }
}
