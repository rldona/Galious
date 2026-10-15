using UnityEngine;

namespace MazeOfKnight.Game.Visuals
{
    // Piezas de la interfaz del remake (docs/design_bible/12_estilo_visual.md, "Interfaz"): marcos de filigrana con
    // meandro griego en las esquinas, tipografía clásica del sistema (Optima para títulos y Palatino para el texto, con
    // Georgia de reserva), barras ornamentadas e iconos. Se dibuja con OnGUI, como el resto de pantallas.
    public static class UiKit
    {
        static Font title, body;
        static Texture2D frameTex, panelTex, ringTex, white;
        static GUIStyle frameStyle, panelStyle;
        static readonly GUIStyle textStyle = new GUIStyle();

        public static readonly Color Ink = new Color(0.02f, 0.02f, 0.04f, 0.88f);
        public static readonly Color Bone = new Color(0.95f, 0.93f, 0.88f);
        public static readonly Color Gold = new Color(0.92f, 0.76f, 0.35f);
        public static readonly Color Dim = new Color(0.62f, 0.6f, 0.58f);

        // Escala de la interfaz: 1 a 800 px de alto.
        public static float Scale => Mathf.Max(0.6f, Screen.height / 800f);

        public static Font Title => title != null ? title : title = Font.CreateDynamicFontFromOSFont(new[] { "Optima", "Palatino", "Georgia" }, 32);
        public static Font Body => body != null ? body : body = Font.CreateDynamicFontFromOSFont(new[] { "Palatino", "Georgia", "Optima" }, 24);

        static Texture2D White
        {
            get
            {
                if (white != null) return white;
                white = new Texture2D(1, 1);
                white.SetPixel(0, 0, Color.white);
                white.Apply();
                return white;
            }
        }

        // ---------- texturas ----------

        // Marco de 64 × 64 para recortar en 9 (borde 20): doble filete blanco y un meandro en cada esquina.
        static Texture2D FrameTexture()
        {
            const int n = 64;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var px = new Color[n * n];
            void Set(int x, int y, float a)
            {
                if (x < 0 || y < 0 || x >= n || y >= n) return;
                var o = px[y * n + x];
                px[y * n + x] = new Color(1, 1, 1, Mathf.Max(o.a, a));
            }
            for (int i = 0; i < n; i++)
            {
                foreach (int d in new[] { 4, 5 }) { Set(i, d, 1); Set(i, n - 1 - d, 1); Set(d, i, 1); Set(n - 1 - d, i, 1); }
                Set(i, 9, 0.55f); Set(i, n - 10, 0.55f); Set(9, i, 0.55f); Set(n - 10, i, 0.55f);
            }
            // Meandro (greca) en las esquinas: una espiral cuadrada de 10 px.
            int[,] key =
            {
                { 1,1,1,1,1,1,1,1,1,1 }, { 1,0,0,0,0,0,0,0,0,1 }, { 1,0,1,1,1,1,1,1,0,1 }, { 1,0,1,0,0,0,0,1,0,1 }, { 1,0,1,0,1,1,0,1,0,1 },
                { 1,0,1,0,1,0,0,1,0,1 }, { 1,0,1,0,1,1,1,1,0,1 }, { 1,0,1,0,0,0,0,0,0,1 }, { 1,0,1,1,1,1,1,1,1,1 }, { 0,0,0,0,0,0,0,0,0,0 },
            };
            foreach (var (cx, cy, fx, fy) in new[] { (0, 0, false, false), (n - 10, 0, true, false), (0, n - 10, false, true), (n - 10, n - 10, true, true) })
                for (int y = 0; y < 10; y++)
                    for (int x = 0; x < 10; x++)
                    {
                        int kx = fx ? 9 - x : x, ky = fy ? 9 - y : y;
                        if (key[ky, kx] == 1) Set(cx + x, cy + y, 1);
                    }
            // Fondo: negro translúcido dentro del filete.
            for (int y = 6; y < n - 6; y++)
                for (int x = 6; x < n - 6; x++)
                    if (px[y * n + x].a == 0) px[y * n + x] = Ink;
            t.SetPixels(px);
            t.Apply();
            return t;
        }

        static Texture2D RingTexture()
        {
            const int n = 128;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(n / 2f, n / 2f)) / (n / 2f);
                    float ring = Mathf.Clamp01(1 - Mathf.Abs(d - 0.93f) / 0.05f), inner = Mathf.Clamp01(1 - Mathf.Abs(d - 0.82f) / 0.02f);
                    float fill = d < 0.9f ? 0.9f : 0;
                    var c = Color.Lerp(Ink, Bone, Mathf.Max(ring, inner * 0.6f));
                    c.a = Mathf.Max(fill, Mathf.Max(ring, inner * 0.6f));
                    t.SetPixel(x, y, c);
                }
            t.Apply();
            return t;
        }

        static GUIStyle Sliced(Texture2D tex, int border)
        {
            var s = new GUIStyle { border = new RectOffset(border, border, border, border) };
            s.normal.background = tex;
            return s;
        }

        // ---------- piezas ----------

        // Panel con marco de filigrana.
        public static void Panel(Rect r)
        {
            if (frameStyle == null) frameStyle = Sliced(frameTex = FrameTexture(), 20);
            if (Event.current.type == EventType.Repaint) frameStyle.Draw(r, false, false, false, false);
        }

        // Fondo oscuro sin marco (oscurece lo de detrás).
        public static void Shade(Rect r, float alpha = 0.75f) => Box(r, new Color(0, 0, 0, alpha));

        public static void Medallion(Rect r)
        {
            if (ringTex == null) ringTex = RingTexture();
            GUI.DrawTexture(r, ringTex);
        }

        public static void Box(Rect r, Color c)
        {
            var old = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, White);
            GUI.color = old;
        }

        public static void Line(Rect r, Color c) => Box(r, c);

        public static void Frame(Rect r, Color c, float w = 1)
        {
            Box(new Rect(r.x, r.y, r.width, w), c);
            Box(new Rect(r.x, r.yMax - w, r.width, w), c);
            Box(new Rect(r.x, r.y, w, r.height), c);
            Box(new Rect(r.xMax - w, r.y, w, r.height), c);
        }

        // Texto con sombra. size en píxeles a escala 1.
        public static void Text(Rect r, string s, float size, TextAnchor align = TextAnchor.UpperLeft, Color? color = null, bool titleFont = false, float alpha = 1)
        {
            textStyle.font = titleFont ? Title : Body;
            textStyle.fontSize = Mathf.Max(8, Mathf.RoundToInt(size * Scale));
            textStyle.alignment = align;
            textStyle.wordWrap = true;
            textStyle.richText = false;
            var c = color ?? Bone;
            textStyle.normal.textColor = new Color(0, 0, 0, 0.8f * alpha);
            GUI.Label(new Rect(r.x + 1.5f * Scale, r.y + 1.5f * Scale, r.width, r.height), s, textStyle);
            textStyle.normal.textColor = new Color(c.r, c.g, c.b, c.a * alpha);
            GUI.Label(r, s, textStyle);
        }

        // Título con dos filetes a los lados (nombre de zona, cabecera de menú).
        public static void TitleLine(Rect r, string s, float size, float alpha = 1)
        {
            Text(r, s, size, TextAnchor.MiddleCenter, Bone, true, alpha);
            float w = r.width * 0.18f, y = r.center.y, gap = s.Length * size * 0.36f * Scale;
            var c = new Color(Bone.r, Bone.g, Bone.b, 0.7f * alpha);
            Line(new Rect(r.center.x - gap - w, y, w, Mathf.Max(1, Scale)), c);
            Line(new Rect(r.center.x + gap, y, w, Mathf.Max(1, Scale)), c);
            Box(new Rect(r.center.x - gap - w - 3 * Scale, y - 2 * Scale, 4 * Scale, 4 * Scale), c);
            Box(new Rect(r.center.x + gap + w - 1 * Scale, y - 2 * Scale, 4 * Scale, 4 * Scale), c);
        }

        // Un sprite dentro de un rectángulo (conserva la proporción).
        // crop: la parte del sprite que se dibuja, en fracciones (0–1) de su rectángulo.
        public static void Icon(Rect r, Sprite sp, float alpha = 1, bool flip = false, Rect? crop = null)
        {
            if (sp == null) return;
            var tr = sp.textureRect;
            if (crop is Rect c)
                tr = new Rect(tr.x + tr.width * c.x, tr.y + tr.height * c.y, tr.width * c.width, tr.height * c.height);
            var tex = sp.texture;
            var uv = new Rect(tr.x / tex.width, tr.y / tex.height, tr.width / tex.width, tr.height / tex.height);
            if (flip) uv = new Rect(uv.xMax, uv.y, -uv.width, uv.height);
            float aspect = tr.width / tr.height;
            var fit = r.width / r.height > aspect ? new Rect(r.center.x - r.height * aspect / 2, r.y, r.height * aspect, r.height)
                                                  : new Rect(r.x, r.center.y - r.width / aspect / 2, r.width, r.width / aspect);
            var old = GUI.color;
            GUI.color = new Color(1, 1, 1, alpha);
            GUI.DrawTextureWithTexCoords(fit, tex, uv);
            GUI.color = old;
        }

        // Barra ornamentada: marco, lo que hay, lo que se va a perder o ganar (pendiente) y un brillo arriba.
        public static void Vessel(Rect r, float value, float pending, float max, Color fill)
        {
            Box(r, new Color(0, 0, 0, 0.7f));
            float inner = r.width - 4 * Scale;
            float v = max > 0 ? Mathf.Clamp01(value / max) : 0, p = max > 0 ? Mathf.Clamp01((value + pending) / max) : 0;
            var ir = new Rect(r.x + 2 * Scale, r.y + 2 * Scale, inner, r.height - 4 * Scale);
            if (p < v) Box(new Rect(ir.x, ir.y, ir.width * v, ir.height), new Color(1f, 0.9f, 0.8f, 0.85f));   // daño por aplicar
            Box(new Rect(ir.x, ir.y, ir.width * Mathf.Min(v, p), ir.height), fill);
            if (p > v) Box(new Rect(ir.x + ir.width * v, ir.y, ir.width * (p - v), ir.height), new Color(fill.r, fill.g, fill.b, 0.45f));
            Box(new Rect(ir.x, ir.y, ir.width * Mathf.Min(v, p), Mathf.Max(1, ir.height * 0.3f)), new Color(1, 1, 1, 0.25f));
            Frame(r, Bone, Mathf.Max(1, Scale));
            // Marcas cada 8 unidades (una Gran Llave).
            for (int u = 8; u < max; u += 8)
                Box(new Rect(ir.x + ir.width * u / max, ir.y, Mathf.Max(1, Scale * 0.8f), ir.height), new Color(0, 0, 0, 0.45f));
        }
    }
}
