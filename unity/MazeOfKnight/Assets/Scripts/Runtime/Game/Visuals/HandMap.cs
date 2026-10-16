using System.Collections.Generic;
using MazeOfKnight.Data;
using UnityEngine;

namespace MazeOfKnight.Game.Visuals
{
    // El mapa dibujado a mano de Galious: The Hollow Kingdom (RemakeRules.HollowMaps), como los de Cornifer en Hollow
    // Knight: cada sala se dibuja desde su colisión, con una línea donde el aire toca la roca. Dos estilos:
    //   - boceto (sin el mapa): solo las salas visitadas, a lápiz gris, con trazos que se cortan y cambian de intensidad;
    //   - entintado (con el mapa): todas las salas, con línea clara y el hueco relleno, agua en azul y lava en naranja.
    // Una textura por mapa, rehecha cuando cambia lo que se ve.
    public static class HandMap
    {
        public const int TilePx = 3;                                   // píxeles de la textura por casilla
        public static int RoomW => RoomCoords.Cols * TilePx;
        public static int RoomH => RoomCoords.Rows * TilePx;

        static Texture2D tex;
        static string key;
        public static int MinX, MaxY;                                  // la sala de arriba a la izquierda (gridX, gridY)

        static readonly Color Ink = new Color(0.93f, 0.91f, 0.85f, 0.95f), Fill = new Color(0.85f, 0.83f, 0.78f, 0.16f);
        static readonly Color Pencil = new Color(0.72f, 0.72f, 0.75f, 1f);
        static readonly Color Water = new Color(0.45f, 0.65f, 0.95f, 0.3f), Lava = new Color(1f, 0.5f, 0.2f, 0.35f);

        // Valor fijo entre 0 y 1 por posición (el trazo del lápiz no parpadea).
        static float Hash(int x, int y) => Mathf.Abs(Mathf.Sin(x * 12.9898f + y * 78.233f) * 43758.547f) % 1f;

        public static Texture2D Get(string map, RoomWorld world, ICollection<(string, int)> visited, bool inked)
        {
            int count = 0;
            foreach (var v in visited) if (v.Item1 == map) count++;
            string k = $"{map}|{inked}|{count}";
            if (tex != null && key == k) return tex;
            key = k;
            int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;
            foreach (var r in world.Rooms)
            {
                minX = Mathf.Min(minX, r.gridX); maxX = Mathf.Max(maxX, r.gridX);
                minY = Mathf.Min(minY, r.gridY); maxY = Mathf.Max(maxY, r.gridY);
            }
            MinX = minX; MaxY = maxY;
            int w = (maxX - minX + 1) * RoomW, h = (maxY - minY + 1) * RoomH;
            if (tex == null || tex.width != w || tex.height != h)
            {
                if (tex != null) Object.Destroy(tex);
                tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            }
            var px = new Color32[w * h];
            foreach (var room in world.Rooms)
            {
                if (!inked && !visited.Contains((map, room.id))) continue;
                int ox = (room.gridX - minX) * RoomW, oy = (room.gridY - minY) * RoomH;   // abajo a la izquierda (la textura va de abajo arriba)
                TileClass T(int c, int r)
                {
                    // Fuera de la sala: aire si hay sala al lado (una salida), roca si no.
                    if (c < 0) return room.left > 0 ? TileClass.Empty : TileClass.Solid;
                    if (c >= RoomCoords.Cols) return room.right > 0 ? TileClass.Empty : TileClass.Solid;
                    if (r < 0) return room.up > 0 ? TileClass.Empty : TileClass.Solid;
                    if (r >= RoomCoords.Rows) return room.down > 0 ? TileClass.Empty : TileClass.Solid;
                    return world.TileAt(room, c * 8, OriginalPhysics.RoomTopPx + r * 8);
                }
                bool Rock(TileClass t) => t == TileClass.Solid || t == TileClass.BossPassable;
                void Put(int x, int y, Color c)
                {
                    if (x < 0 || y < 0 || x >= w || y >= h) return;
                    int i = y * w + x;
                    var old = (Color)px[i];
                    px[i] = c.a >= old.a ? c : old;
                }
                for (int r = 0; r < RoomCoords.Rows; r++)
                    for (int c = 0; c < RoomCoords.Cols; c++)
                    {
                        var t = T(c, r);
                        if (Rock(t)) continue;
                        int x0 = ox + c * TilePx, y0 = oy + (RoomCoords.Rows - 1 - r) * TilePx;
                        if (inked)
                        {
                            var f = t == TileClass.Water ? Water : t == TileClass.Lava ? Lava : Fill;
                            for (int dy = 0; dy < TilePx; dy++) for (int dx = 0; dx < TilePx; dx++) Put(x0 + dx, y0 + dy, f);
                        }
                        // Una línea en cada lado que toca roca.
                        void Edge(bool vertical, int x, int y)
                        {
                            for (int i = 0; i < TilePx; i++)
                            {
                                int ex = vertical ? x : x + i, ey = vertical ? y + i : y;
                                if (inked) { Put(ex, ey, Ink); continue; }
                                float n = Hash(ex, ey);
                                if (n < 0.12f) continue;                         // el lápiz se levanta
                                Put(ex, ey, new Color(Pencil.r, Pencil.g, Pencil.b, 0.35f + 0.5f * n));
                            }
                        }
                        if (Rock(T(c - 1, r))) Edge(true, x0, y0);
                        if (Rock(T(c + 1, r))) Edge(true, x0 + TilePx - 1, y0);
                        if (Rock(T(c, r - 1))) Edge(false, x0, y0 + TilePx - 1);
                        if (Rock(T(c, r + 1))) Edge(false, x0, y0);
                    }
            }
            tex.SetPixels32(px);
            tex.Apply(false);
            return tex;
        }

        // Dónde cae un punto de una sala (px del original) en la textura, con el origen arriba a la izquierda.
        public static Vector2 Point(RoomModel room, int x, int y) =>
            new Vector2((room.gridX - MinX) * RoomW + x / 8f * TilePx,
                        (MaxY - room.gridY) * RoomH + (y - OriginalPhysics.RoomTopPx) / 8f * TilePx);

        // Nombre de la zona de Hollow Knight de cada mapa (HollowScenery.Look.Zone).
        public static string ZoneName(string zone) => zone switch
        {
            "cruces" => "Cruces Olvidados",
            "greenpath" => "Sendero Verde",
            "canales" => "Canales Reales",
            "colmena" => "La Colmena",
            "cristal" => "Picos de Cristal",
            "ciudad" => "Ciudad de las Lágrimas",
            "fungicos" => "Páramos Fúngicos",
            "abismo" => "El Abismo",
            "nido" => "Nido Profundo",
            "confines" => "Confines del Reino",
            "palacio" => "Palacio Blanco",
            _ => "",
        };
    }
}
