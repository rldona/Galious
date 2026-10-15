using System;
using UnityEngine;

namespace MazeOfKnight.Game.Visuals
{
    // Oclusión ambiental calculada a partir de la colisión de la sala (docs/design_bible/12_estilo_visual.md, "Oclusión
    // ambiental y luz"): oscurece el aire donde el suelo toca la pared, los rincones y la base de cada saliente. Es una
    // textura de negro con alfa por sala, a TexelsPerTile texels por tile, que se dibuja encima de los tiles y debajo de
    // los personajes. Solo aspecto: no toca la colisión. Se suma a la que venga pintada en el arte.
    public static class AmbientOcclusion
    {
        public const int TexelsPerTile = 4;
        public const float Radius = 1.6f;       // alcance, en tiles
        public const float MaxAlpha = 0.55f;    // oscuridad máxima (en un rincón)
        const float Gain = 0.9f;

        // Oclusión de cada texel (0..1 = alfa del negro) de una rejilla cols × rows cuyo solid(col, fila) dice si el tile
        // es macizo (fila 0 arriba). El resultado va por filas desde abajo, como una textura. Dentro del macizo es 0.
        public static float[] Compute(Func<int, int, bool> solid, int cols, int rows)
        {
            int w = cols * TexelsPerTile, h = rows * TexelsPerTile;
            var a = new float[w * h];
            int reach = Mathf.CeilToInt(Radius) + 1;
            for (int ty = 0; ty < h; ty++)
            {
                float py = (ty + 0.5f) / TexelsPerTile;            // en tiles, desde abajo
                int rowHere = rows - 1 - (int)py;                   // fila (desde arriba) del tile del texel
                for (int tx = 0; tx < w; tx++)
                {
                    float px = (tx + 0.5f) / TexelsPerTile;
                    int colHere = (int)px;
                    if (solid(colHere, rowHere)) continue;
                    float ao = 0;
                    for (int c = colHere - reach; c <= colHere + reach; c++)
                        for (int r = rowHere - reach; r <= rowHere + reach; r++)
                        {
                            if (c < 0 || c >= cols || r < 0 || r >= rows || !solid(c, r)) continue;
                            // Distancia del punto al cuadrado del tile (x en [c, c + 1], y desde abajo en [rows − 1 − r, rows − r]).
                            float dx = Mathf.Max(0, Mathf.Max(c - px, px - (c + 1)));
                            float bottom = rows - 1 - r;
                            float dy = Mathf.Max(0, Mathf.Max(bottom - py, py - (bottom + 1)));
                            float d = Mathf.Sqrt(dx * dx + dy * dy);
                            if (d >= Radius) continue;
                            float t = 1 - d / Radius;
                            ao += t * t;
                        }
                    a[ty * w + tx] = MaxAlpha * (1 - Mathf.Exp(-Gain * ao));
                }
            }
            return a;
        }

        // Textura de negro con el alfa calculado (bilineal: las transiciones salen suaves a cualquier escala).
        public static Texture2D ToTexture(float[] alpha, int cols, int rows)
        {
            int w = cols * TexelsPerTile, h = rows * TexelsPerTile;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp, name = "Oclusión" };
            var px = new Color32[w * h];
            for (int i = 0; i < px.Length; i++) px[i] = new Color32(0, 0, 0, (byte)Mathf.RoundToInt(Mathf.Clamp01(alpha[i]) * 255));
            tex.SetPixels32(px);
            tex.Apply(false, false);
            return tex;
        }

        // Resumen de la colisión para saber si ha cambiado (muros que se abren) y recalcular.
        public static int Hash(Func<int, int, bool> solid, int cols, int rows)
        {
            unchecked
            {
                int h = 17;
                for (int r = 0; r < rows; r++)
                    for (int c = 0; c < cols; c++)
                        h = h * 31 + (solid(c, r) ? 1 : 0);
                return h;
            }
        }
    }
}
