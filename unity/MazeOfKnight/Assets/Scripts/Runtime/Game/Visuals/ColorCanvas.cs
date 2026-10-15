using System;
using UnityEngine;

namespace MazeOfKnight.Game.Visuals
{
    // Lienzo de color para el arte provisional de personajes y enemigos (CharacterArt, EnemyArt): formas con funciones
    // de distancia en píxeles del original, con borde suave, pintadas una encima de otra, y un contorno de tinta.
    public class ColorCanvas
    {
        public readonly int W, H, S;          // texels y texels por píxel del original
        readonly float ox, oy;                // el (0, 0) del dibujo, en píxeles del original desde abajo a la izquierda
        public readonly Color[] Px;

        public ColorCanvas(int w, int h, int s, float originX, float originY)
        {
            W = w; H = h; S = s; ox = originX; oy = originY;
            Px = new Color[w * h];
        }

        // Forma (sdf < 0 dentro) en su caja; color encima con borde suave de 1 texel.
        public void Fill(Func<float, float, float> sdf, Color color, float x0, float y0, float x1, float y1)
        {
            int ix0 = Mathf.Max(0, (int)((x0 + ox) * S) - 2), ix1 = Mathf.Min(W - 1, (int)((x1 + ox) * S) + 2);
            int iy0 = Mathf.Max(0, (int)((y0 + oy) * S) - 2), iy1 = Mathf.Min(H - 1, (int)((y1 + oy) * S) + 2);
            float soft = 0.5f / S;
            for (int y = iy0; y <= iy1; y++)
                for (int x = ix0; x <= ix1; x++)
                {
                    float px = (x + 0.5f) / S - ox, py = (y + 0.5f) / S - oy;
                    float d = sdf(px, py);
                    float a = Mathf.Clamp01(0.5f - d / (2 * soft)) * color.a;
                    if (a <= 0) continue;
                    int i = y * W + x;
                    var o = Px[i];
                    float na = a + o.a * (1 - a);
                    var rgb = (new Color(color.r, color.g, color.b) * a + new Color(o.r, o.g, o.b) * o.a * (1 - a)) / Mathf.Max(na, 1e-5f);
                    Px[i] = new Color(rgb.r, rgb.g, rgb.b, na);
                }
        }

        // Contorno de tinta: dilata la silueta 'radius' texels por debajo de lo pintado.
        public void Ink(Color ink, int radius = 2)
        {
            var src = (Color[])Px.Clone();
            int r2 = radius * radius + 1;
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    int i = y * W + x;
                    if (src[i].a >= 0.99f) continue;
                    float m = 0;
                    for (int dy = -radius; dy <= radius; dy++)
                        for (int dx = -radius; dx <= radius; dx++)
                        {
                            if (dx * dx + dy * dy > r2) continue;
                            int xx = x + dx, yy = y + dy;
                            if (xx < 0 || yy < 0 || xx >= W || yy >= H) continue;
                            m = Mathf.Max(m, src[yy * W + xx].a);
                        }
                    if (m <= 0) continue;
                    var o = src[i];
                    float na = o.a + m * (1 - o.a);
                    var rgb = (new Color(o.r, o.g, o.b) * o.a + new Color(ink.r, ink.g, ink.b) * m * (1 - o.a)) / Mathf.Max(na, 1e-5f);
                    Px[i] = new Color(rgb.r, rgb.g, rgb.b, na);
                }
        }

        public Texture2D ToTexture()
        {
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            tex.SetPixels(Px);
            tex.Apply(false, false);
            return tex;
        }
    }
}
