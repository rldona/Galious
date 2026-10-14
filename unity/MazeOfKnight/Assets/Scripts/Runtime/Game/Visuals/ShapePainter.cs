using System;
using UnityEngine;

namespace MazeOfKnight.Game.Visuals
{
    // Arte provisional del remake: siluetas de fondo pintadas por código con funciones de distancia (bordes suaves,
    // degradado dentro de cada forma y algo de ruido). Cada tema tiene su vocabulario: columnas y arcos en el templo,
    // estalactitas en la cueva, corales, costillas, cristales, hongos… La textura sale en gris con alfa y se tiñe con la
    // paleta del mundo (SpriteRenderer.color). Sustituible por arte dibujado sin tocar Atmosphere.
    public static class ShapePainter
    {
        public const int Width = 768, Height = 480;     // 24 × 15 unidades a 32 texels por unidad (sala: 16 × 10)
        public const float PixelsPerUnit = 32f;

        public enum Depth { Far, Mid, Near, Front }

        class Canvas
        {
            public readonly int W, H;
            public readonly float[] A, V;
            public Canvas(int w, int h) { W = w; H = h; A = new float[w * h]; V = new float[w * h]; }

            // Pinta una forma (sdf < 0 dentro) en su caja, con borde suave y luz de arriba abajo dentro de la forma.
            public void Fill(Func<float, float, float> sdf, float x0, float y0, float x1, float y1, float value, float alpha = 1f, float soft = 1.5f)
            {
                int ix0 = Mathf.Max(0, (int)(x0 - soft - 1)), ix1 = Mathf.Min(W - 1, (int)(x1 + soft + 1));
                int iy0 = Mathf.Max(0, (int)(y0 - soft - 1)), iy1 = Mathf.Min(H - 1, (int)(y1 + soft + 1));
                float span = Mathf.Max(1f, y1 - y0);
                for (int y = iy0; y <= iy1; y++)
                {
                    float light = 0.82f + 0.18f * Mathf.Clamp01((y - y0) / span);
                    for (int x = ix0; x <= ix1; x++)
                    {
                        float d = sdf(x + 0.5f, y + 0.5f);
                        if (d > soft) continue;
                        float a = alpha * Mathf.Clamp01(0.5f - d / (2 * soft));
                        if (a <= 0) continue;
                        int i = y * W + x;
                        float v = value * light;
                        // Encima de lo que haya (mezcla "normal").
                        V[i] = Mathf.Lerp(V[i], v, a);
                        A[i] = A[i] + a * (1 - A[i]);
                    }
                }
            }

            public Texture2D ToTexture(float noise, int seed, Func<int, float> fade)
            {
                var tex = new Texture2D(W, H, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
                var px = new Color32[W * H];
                var rng = new System.Random(seed);
                for (int y = 0; y < H; y++)
                {
                    float f = fade(y);
                    for (int x = 0; x < W; x++)
                    {
                        int i = y * W + x;
                        float n = 1f + noise * ((float)rng.NextDouble() - 0.5f);
                        byte v = (byte)(Mathf.Clamp01(V[i] * n) * 255);
                        px[i] = new Color32(v, v, v, (byte)(Mathf.Clamp01(A[i] * f) * 255));
                    }
                }
                tex.SetPixels32(px);
                tex.Apply(false, false);
                return tex;
            }
        }

        // ---------- distancias ----------

        static float Box(float px, float py, float cx, float cy, float hw, float hh)
        {
            float dx = Mathf.Abs(px - cx) - hw, dy = Mathf.Abs(py - cy) - hh;
            return Mathf.Sqrt(Mathf.Max(dx, 0) * Mathf.Max(dx, 0) + Mathf.Max(dy, 0) * Mathf.Max(dy, 0)) + Mathf.Min(Mathf.Max(dx, dy), 0);
        }

        static float Circle(float px, float py, float cx, float cy, float r) => Mathf.Sqrt((px - cx) * (px - cx) + (py - cy) * (py - cy)) - r;

        static float Segment(float px, float py, float ax, float ay, float bx, float by, float r)
        {
            float pax = px - ax, pay = py - ay, bax = bx - ax, bay = by - ay;
            float h = Mathf.Clamp01((pax * bax + pay * bay) / (bax * bax + bay * bay));
            float dx = pax - bax * h, dy = pay - bay * h;
            return Mathf.Sqrt(dx * dx + dy * dy) - r;
        }

        // Triángulo isósceles: base centrada en (cx, by) de semiancho hw, punta en (cx, ty) (arriba o abajo).
        static float Spike(float px, float py, float cx, float by, float hw, float ty)
        {
            float h = ty - by;
            float t = (py - by) / h;                   // 0 en la base, 1 en la punta
            float w = hw * (1 - Mathf.Clamp01(t));
            float dx = Mathf.Abs(px - cx) - w;
            float dy = Mathf.Max(-t, t - 1) * Mathf.Abs(h);
            return Mathf.Max(dx * 0.9f, dy);
        }

        // ---------- piezas ----------

        static void Column(Canvas c, float cx, float hw, float y0, float y1, float v, bool broken, System.Random r)
        {
            float top = broken ? y0 + (y1 - y0) * (0.45f + 0.4f * (float)r.NextDouble()) : y1;
            c.Fill((x, y) => Box(x, y, cx, (y0 + top) / 2, hw, (top - y0) / 2), cx - hw, y0, cx + hw, top, v);
            // Estrías.
            for (int k = -1; k <= 1; k++)
            {
                float fx = cx + k * hw * 0.5f;
                c.Fill((x, y) => Box(x, y, fx, (y0 + top) / 2, hw * 0.08f, (top - y0) / 2 - 6), fx - 2, y0, fx + 2, top, v * 0.8f, 0.6f);
            }
            c.Fill((x, y) => Box(x, y, cx, y0 + 6, hw * 1.35f, 6), cx - hw * 1.4f, y0, cx + hw * 1.4f, y0 + 12, v);
            if (!broken)
                c.Fill((x, y) => Box(x, y, cx, top - 5, hw * 1.45f, 7), cx - hw * 1.5f, top - 12, cx + hw * 1.5f, top + 2, v);
            else
                c.Fill((x, y) => Spike(x, y, cx + hw * 0.3f, top - 1, hw * 0.7f, top + 14), cx - hw, top - 2, cx + hw, top + 15, v);
        }

        static void Arch(Canvas c, float cx, float hw, float y0, float spring, float thick, float v, bool pointed)
        {
            float r = hw;
            Func<float, float, float> ring = pointed
                ? (x, y) =>
                {
                    // Arco apuntado: intersección de dos círculos desplazados.
                    float a = Circle(x, y, cx - r * 0.35f, spring, r * 1.35f), b = Circle(x, y, cx + r * 0.35f, spring, r * 1.35f);
                    float outer = Mathf.Max(a, b);
                    float ai = Circle(x, y, cx - r * 0.35f, spring, r * 1.35f - thick), bi = Circle(x, y, cx + r * 0.35f, spring, r * 1.35f - thick);
                    float inner = Mathf.Max(ai, bi);
                    return Mathf.Max(outer, -inner, spring - y);
                }
                : (x, y) => Mathf.Max(Mathf.Abs(Circle(x, y, cx, spring, r - thick / 2)) - thick / 2, spring - y);
            c.Fill(ring, cx - r - 2, spring, cx + r + 2, spring + r * 1.4f + 2, v);
            c.Fill((x, y) => Box(x, y, cx - r + thick / 2, (y0 + spring) / 2, thick / 2, (spring - y0) / 2), cx - r, y0, cx - r + thick, spring, v);
            c.Fill((x, y) => Box(x, y, cx + r - thick / 2, (y0 + spring) / 2, thick / 2, (spring - y0) / 2), cx + r - thick, y0, cx + r, spring, v);
        }

        static void Vine(Canvas c, float cx, float yTop, float len, float thick, float amp, float freq, float phase, float v, bool leaves, System.Random r)
        {
            float y1 = yTop, y0 = yTop - len;
            c.Fill((x, y) =>
            {
                float t = (yTop - y) / len;
                if (t < 0 || t > 1) return 999f;
                float w = thick * (1 - 0.6f * t);
                return Mathf.Abs(x - (cx + amp * Mathf.Sin(y * freq + phase))) - w;
            }, cx - amp - thick, y0, cx + amp + thick, y1, v);
            if (!leaves) return;
            for (float y = y1 - 10; y > y0 + 6; y -= 9 + (float)r.NextDouble() * 10)
            {
                float lx = cx + amp * Mathf.Sin(y * freq + phase);
                float side = r.NextDouble() < 0.5 ? -1 : 1, ly = y;
                c.Fill((x, yy) => Circle(x, yy * 1.6f, lx + side * 5, ly * 1.6f, 4.5f), lx - 12, ly - 6, lx + 12, ly + 6, v);
            }
        }

        static void Branching(Canvas c, float x, float y, float angle, float len, float thick, int depth, float v, System.Random r)
        {
            float ex = x + Mathf.Cos(angle) * len, ey = y + Mathf.Sin(angle) * len;
            c.Fill((px, py) => Segment(px, py, x, y, ex, ey, thick), Mathf.Min(x, ex) - thick, Mathf.Min(y, ey) - thick, Mathf.Max(x, ex) + thick, Mathf.Max(y, ey) + thick, v);
            if (depth <= 0)
            {
                c.Fill((px, py) => Circle(px, py, ex, ey, thick * 1.6f), ex - thick * 2, ey - thick * 2, ex + thick * 2, ey + thick * 2, v);
                return;
            }
            int n = 2 + (r.NextDouble() < 0.3 ? 1 : 0);
            for (int i = 0; i < n; i++)
                Branching(c, ex, ey, angle + ((float)r.NextDouble() - 0.5f) * 1.3f, len * (0.6f + 0.2f * (float)r.NextDouble()), thick * 0.7f, depth - 1, v, r);
        }

        static void Mushroom(Canvas c, float cx, float y0, float h, float capR, float v)
        {
            float stem = capR * 0.22f;
            c.Fill((x, y) => Box(x, y, cx + Mathf.Sin(y * 0.03f) * 3, y0 + h / 2, stem, h / 2), cx - stem - 4, y0, cx + stem + 4, y0 + h, v * 0.9f);
            float cy = y0 + h;
            c.Fill((x, y) => Mathf.Max(Circle(x, (y - cy) * 1.8f + cy, cx, cy, capR), cy - 2 - y), cx - capR, cy - 2, cx + capR, cy + capR / 1.8f, v);
            for (int k = -2; k <= 2; k++)
            {
                float sx = cx + k * capR * 0.35f, sy = cy + capR * 0.3f * (1 - Mathf.Abs(k) * 0.3f);
                c.Fill((x, y) => Circle(x, y, sx, sy, capR * 0.09f), sx - 6, sy - 6, sx + 6, sy + 6, v * 1.35f, 0.7f);
            }
        }

        static void Crystal(Canvas c, float bx, float by, float angle, float len, float hw, float v)
        {
            float dx = Mathf.Cos(angle), dy = Mathf.Sin(angle);
            c.Fill((x, y) =>
            {
                float u = (x - bx) * dx + (y - by) * dy, w = -(x - bx) * dy + (y - by) * dx;
                float t = u / len;
                float half = t < 0.75f ? hw : hw * (1 - (t - 0.75f) / 0.25f);
                return Mathf.Max(Mathf.Abs(w) - half, Mathf.Max(-u, u - len));
            }, bx - len - hw, by - len - hw, bx + len + hw, by + len + hw, v);
            // Cara iluminada.
            c.Fill((x, y) =>
            {
                float u = (x - bx) * dx + (y - by) * dy, w = -(x - bx) * dy + (y - by) * dx;
                return Mathf.Max(Mathf.Max(-w, w - hw * 0.4f), Mathf.Max(-u, u - len * 0.8f));
            }, bx - len - hw, by - len - hw, bx + len + hw, by + len + hw, v * 1.4f, 0.5f);
        }

        static void Hills(Canvas c, float baseY, float amp, float v, System.Random r)
        {
            float p1 = (float)r.NextDouble() * 10, p2 = (float)r.NextDouble() * 10;
            c.Fill((x, y) => y - (baseY + amp * (Mathf.Sin(x * 0.011f + p1) * 0.6f + Mathf.Sin(x * 0.037f + p2) * 0.3f + Mathf.Sin(x * 0.11f) * 0.1f)),
                0, 0, c.W, baseY + amp, v);
        }

        static float Rnd(System.Random r, float a, float b) => a + (float)r.NextDouble() * (b - a);

        // ---------- capas por tema ----------

        public static Texture2D Layer(Theme theme, Depth depth, int seed)
        {
            var c = new Canvas(Width, Height);
            var r = new System.Random(seed * 7919 + (int)theme * 131 + (int)depth);
            bool front = depth == Depth.Front;
            float v = depth == Depth.Far ? 0.75f : depth == Depth.Mid ? 0.85f : depth == Depth.Near ? 0.95f : 1f;
            float scale = depth == Depth.Far ? 0.75f : depth == Depth.Mid ? 1.1f : 1.5f;
            int count = depth == Depth.Far ? 7 : depth == Depth.Mid ? 4 : depth == Depth.Near ? 2 : 0;
            float W = Width, H = Height;

            if (front)
            {
                PaintFront(c, theme, r);
                return c.ToTexture(0.04f, seed, _ => 1f);
            }

            if (depth == Depth.Far) Hills(c, H * 0.12f, H * 0.05f, v * 0.9f, r);
            switch (theme)
            {
                case Theme.Temple:
                case Theme.Sunken:
                case Theme.Ossuary:
                case Theme.Throne:
                {
                    bool pointed = theme == Theme.Throne;
                    float step = W / (count + 1);
                    for (int i = 0; i < count + 1; i++)
                    {
                        float cx = step * (i + 0.5f) + Rnd(r, -10, 10);
                        float hw = 16 * scale;
                        bool broken = theme != Theme.Throne && r.NextDouble() < (theme == Theme.Temple ? 0.25 : 0.55);
                        Column(c, cx, hw, H * 0.08f, H * (depth == Depth.Far ? 0.78f : 0.92f), v, broken, r);
                        if (i < count && r.NextDouble() < 0.7)
                        {
                            float ax = cx + step / 2, ahw = step / 2 - hw * 0.4f;
                            Arch(c, ax, ahw, H * 0.08f, H * (depth == Depth.Far ? 0.55f : 0.62f), 12 * scale, v * 0.95f, pointed);
                        }
                    }
                    if (theme == Theme.Ossuary)
                        for (int i = 0; i < 3; i++)
                        {
                            float cx = Rnd(r, 60, W - 60), cy = Rnd(r, H * 0.25f, H * 0.6f), rad = Rnd(r, 30, 60) * scale;
                            c.Fill((x, y) => Mathf.Abs(Circle(x, y, cx, cy, rad)) - 5 * scale, cx - rad - 8, cy, cx + rad + 8, cy + rad + 8, v);
                        }
                    if (theme == Theme.Sunken || theme == Theme.Temple)
                        for (int i = 0; i < count; i++)
                            Vine(c, Rnd(r, 0, W), H + 4, Rnd(r, 80, 200) * scale, 3 * scale, 6, 0.04f, Rnd(r, 0, 6), v, true, r);
                    break;
                }
                case Theme.Cavern:
                case Theme.Magma:
                    for (int i = 0; i < count * 3; i++)
                    {
                        float cx = Rnd(r, -20, W + 20), hw = Rnd(r, 10, 34) * scale;
                        float len = Rnd(r, 60, 200) * scale;
                        c.Fill((x, y) => Spike(x, y, cx, H + 2, hw, H - len), cx - hw, H - len, cx + hw, H + 2, v);
                        if (r.NextDouble() < 0.6)
                        {
                            float gx = Rnd(r, 0, W), ghw = Rnd(r, 12, 40) * scale, glen = Rnd(r, 50, 160) * scale;
                            c.Fill((x, y) => Spike(x, y, gx, -2, ghw, glen), gx - ghw, -2, gx + ghw, glen, v);
                        }
                    }
                    if (theme == Theme.Magma)
                        for (int i = 0; i < count; i++)
                        {
                            float cx = Rnd(r, 30, W - 30);
                            for (float y = H; y > H * 0.35f; y -= 14 * scale)
                            {
                                float yy = y, flip = ((int)(y / (14 * scale)) & 1) == 0 ? 1 : 0.4f;
                                c.Fill((x, py) => Mathf.Abs(Circle(x * (2 - flip), py, cx * (2 - flip), yy, 6 * scale)) - 1.8f * scale, cx - 12, yy - 12, cx + 12, yy + 12, v);
                            }
                        }
                    break;
                case Theme.Coral:
                case Theme.Abyss:
                    for (int i = 0; i < count; i++)
                    {
                        if (theme == Theme.Coral)
                            Branching(c, Rnd(r, 0, W), 0, Mathf.PI / 2 + Rnd(r, -0.3f, 0.3f), Rnd(r, 40, 80) * scale, 7 * scale, 3, v, r);
                        Vine(c, Rnd(r, 0, W), 0 + Rnd(r, 180, 360) * scale, Rnd(r, 180, 360) * scale, 5 * scale, 8, 0.03f, Rnd(r, 0, 6), v * 0.95f, false, r);
                    }
                    for (int i = 0; i < count * 2; i++)
                    {
                        float cx = Rnd(r, 0, W), cy = Rnd(r, H * 0.3f, H), rad = Rnd(r, 4, 16) * scale;
                        if (theme == Theme.Abyss)
                        {
                            c.Fill((x, y) => Mathf.Max(Circle(x, y, cx, cy, rad * 1.6f), cy - y), cx - rad * 2, cy, cx + rad * 2, cy + rad * 2, v * 1.1f, 0.8f);
                            for (int k = -1; k <= 1; k++)
                            {
                                float tx = cx + k * rad * 0.7f;
                                Vine(c, tx, cy, rad * 4, 1.2f * scale, 2, 0.2f, k, v, false, r);
                            }
                        }
                        else
                            c.Fill((x, y) => Mathf.Abs(Circle(x, y, cx, cy, rad)) - 1.5f, cx - rad - 3, cy - rad - 3, cx + rad + 3, cy + rad + 3, v * 1.2f, 0.7f);
                    }
                    break;
                case Theme.Lair:
                    for (int i = 0; i < count; i++)
                    {
                        float cx = Rnd(r, 40, W - 40), cy = Rnd(r, -40, H * 0.2f), rad = Rnd(r, 90, 170) * scale;
                        for (int k = 0; k < 4; k++)
                        {
                            float rr = rad - k * 26 * scale;
                            c.Fill((x, y) => Mathf.Max(Mathf.Abs(Circle(x, y, cx, cy, rr)) - 6 * scale, cy + rr * 0.15f - y), cx - rr - 10, cy, cx + rr + 10, cy + rr + 10, v);
                        }
                        float sx = Rnd(r, 0, W), shw = Rnd(r, 8, 20) * scale, sl = Rnd(r, 50, 140) * scale;
                        c.Fill((x, y) => Spike(x, y, sx, -2, shw, sl), sx - shw, -2, sx + shw, sl, v);
                    }
                    break;
                case Theme.Crystal:
                    for (int i = 0; i < count * 2; i++)
                    {
                        float bx = Rnd(r, 0, W), by = r.NextDouble() < 0.6 ? 0 : H;
                        int n = 2 + r.Next(3);
                        for (int k = 0; k < n; k++)
                        {
                            float ang = (by == 0 ? Mathf.PI / 2 : -Mathf.PI / 2) + Rnd(r, -0.6f, 0.6f);
                            Crystal(c, bx + Rnd(r, -15, 15), by, ang, Rnd(r, 60, 180) * scale, Rnd(r, 8, 18) * scale, v);
                        }
                    }
                    break;
                case Theme.Fungal:
                    for (int i = 0; i < count; i++)
                        Mushroom(c, Rnd(r, 20, W - 20), H * 0.05f, Rnd(r, 80, 260) * scale, Rnd(r, 40, 90) * scale, v);
                    for (int i = 0; i < count * 3; i++)
                    {
                        float cx = Rnd(r, 0, W), cy = Rnd(r, H * 0.5f, H), rad = Rnd(r, 10, 26) * scale;
                        c.Fill((x, y) => Circle(x, y, cx, cy, rad), cx - rad, cy - rad, cx + rad, cy + rad, v * 0.9f);
                    }
                    break;
            }
            // Más niebla abajo: lo lejano se funde con el suelo de bruma.
            float fogBottom = depth == Depth.Far ? 0.35f : depth == Depth.Mid ? 0.6f : 0.8f;
            return c.ToTexture(0.06f, seed, y => Mathf.Lerp(fogBottom, 1f, Mathf.Clamp01((float)y / (H * 0.45f))));
        }

        // Primer plano en silueta: solo en los márgenes (abajo, esquinas y algo colgando arriba), para no tapar el juego.
        static void PaintFront(Canvas c, Theme theme, System.Random r)
        {
            float W = c.W, H = c.H;
            // Matorral abajo, sobre el borde inferior de la sala.
            for (float x = -10; x < W + 10; x += Rnd(r, 6, 14))
            {
                float bx = x, h = Rnd(r, 10, 32), hw = Rnd(r, 1.5f, 3.5f);
                float lean = Rnd(r, -10, 10);
                c.Fill((px, py) => Segment(px, py, bx, -2, bx + lean, h, hw * (1 - Mathf.Clamp01(py / h) * 0.7f)), bx - 14, -2, bx + 14, h + 4, 1f);
            }
            // Pilares o rocas en las esquinas.
            foreach (float side in new[] { -1f, 1f })
            {
                float hw = Rnd(r, 26, 40);
                float cx = side < 0 ? -hw * 0.35f : W + hw * 0.35f;     // apenas asoma: no tapa las salidas
                if (theme == Theme.Temple || theme == Theme.Sunken || theme == Theme.Throne || theme == Theme.Ossuary)
                    c.Fill((x, y) => Box(x, y, cx, H / 2, hw, H / 2 + 4), cx - hw, -4, cx + hw, H + 4, 1f);
                else if (theme == Theme.Crystal)
                    Crystal(c, cx, 0, Mathf.PI / 2 - side * 0.25f, Rnd(r, 160, 260), hw * 0.8f, 1f);
                else
                    c.Fill((x, y) => Spike(x, y, cx, -2, hw * 1.4f, Rnd(r, 0, 1) + 200), cx - hw * 1.5f, -2, cx + hw * 1.5f, 205, 1f);
            }
            // Algo colgando del techo.
            int vines = 4 + r.Next(4);
            for (int i = 0; i < vines; i++)
            {
                float x = Rnd(r, 0, W);
                if (theme == Theme.Cavern || theme == Theme.Magma || theme == Theme.Lair)
                {
                    float hw = Rnd(r, 8, 20), len = Rnd(r, 30, 70);
                    c.Fill((px, py) => Spike(px, py, x, H + 2, hw, H - len), x - hw, H - len, x + hw, H + 2, 1f);
                }
                else
                    Vine(c, x, H + 4, Rnd(r, 40, 110), 2.5f, 4, 0.06f, Rnd(r, 0, 6), 1f, true, r);
            }
        }

        // ---------- texturas sueltas ----------

        // Punto suave (motas, halos, brillos).
        public static Texture2D SoftDot(int size = 64)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(size / 2f, size / 2f)) / (size / 2f);
                    float a = Mathf.Clamp01(1 - d);
                    tex.SetPixel(x, y, new Color(1, 1, 1, a * a));
                }
            tex.Apply();
            return tex;
        }

        // Rayo de luz: trapecio que se abre hacia abajo y se apaga en los bordes y al final.
        public static Texture2D Shaft(int w = 64, int h = 256)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < h; y++)
            {
                float t = 1 - (float)y / h;                     // 0 arriba, 1 abajo
                float half = 0.25f + 0.25f * t;
                for (int x = 0; x < w; x++)
                {
                    float u = Mathf.Abs((x + 0.5f) / w - 0.5f) / half;
                    float a = Mathf.Clamp01(1 - u) * Mathf.Clamp01(1 - t) * Mathf.SmoothStep(0, 1, Mathf.Min(1, (1 - t) * 6));
                    tex.SetPixel(x, y, new Color(1, 1, 1, a * a));
                }
            }
            tex.Apply();
            return tex;
        }

        // Banda de niebla horizontal, repetible en x.
        public static Texture2D FogBand(int seed, int w = 256, int h = 64)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Repeat };
            var r = new System.Random(seed);
            float p1 = (float)r.NextDouble() * 6, p2 = (float)r.NextDouble() * 6;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float u = x / (float)w * Mathf.PI * 2;
                    float n = 0.5f + 0.3f * Mathf.Sin(u * 2 + p1) + 0.2f * Mathf.Sin(u * 5 + p2 + y * 0.05f);
                    float band = Mathf.Sin(Mathf.PI * y / (h - 1));
                    tex.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(n) * band * band));
                }
            tex.Apply();
            return tex;
        }

        // Degradado vertical de dos colores (cielo / fondo del todo).
        public static Texture2D Gradient(Color top, Color bottom, int h = 128)
        {
            var tex = new Texture2D(1, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < h; y++)
                tex.SetPixel(0, y, Color.Lerp(bottom, top, (float)y / (h - 1)));
            tex.Apply();
            return tex;
        }

        public static Sprite ToSprite(Texture2D tex, float ppu, Vector2 pivot) =>
            Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), pivot, ppu, 0, SpriteMeshType.FullRect);
    }
}
