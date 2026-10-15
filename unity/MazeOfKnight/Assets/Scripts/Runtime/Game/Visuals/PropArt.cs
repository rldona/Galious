using System;
using System.Collections.Generic;
using MazeOfKnight.Data;
using UnityEngine;
using UnityEngine.Tilemaps;
using static MazeOfKnight.Game.Visuals.ShapePainter;

namespace MazeOfKnight.Game.Visuals
{
    // Piedras, puertas, ascensores y objetos del remake (docs/design_bible/12_estilo_visual.md, "Objetos de sala"):
    // arte provisional pintado por código, con el contorno de tinta de personajes y enemigos. Las piedras y las puertas
    // son tiles de 8 px recortados de un dibujo más grande, así ocupan exactamente sus celdas de colisión.
    public class PropArt
    {
        const int S = 4;                                  // texels por píxel del original
        static Color C(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;
        static readonly Color Ink = new Color(0.03f, 0.03f, 0.05f);

        static readonly Color Gold = C("#e6c04a"), GoldDark = C("#9a7420"), Silver = C("#c9cfdc"), SilverDark = C("#7a8296"),
            Bronze = C("#c27445"), BronzeDark = C("#6e3a1c"), Wood = C("#8a6a3a"), WoodDark = C("#5a4224"), Cloth = C("#7a52b8"),
            Blue = C("#4f8fe0"), White = C("#f2efe6"), Red = C("#d2453c"), Green = C("#6fc36a"), Marble = C("#d9d6cc"),
            MarbleShade = C("#a9a69c"), Stone = C("#6b6d78"), StoneLight = C("#8d909c"), StoneDark = C("#45474f"), Hole = C("#08080d");

        public readonly TileBase[,] Rock, Door, DoorClosed, DoorHalf, KeyDoor, Revolving;
        public readonly Sprite Lift;
        readonly Dictionary<int, Sprite> items = new Dictionary<int, Sprite>();

        public PropArt()
        {
            Rock = Tiles(PaintRock(), 2, 2);
            Door = Tiles(PaintDoor(2), 4, 3);
            DoorHalf = Tiles(PaintDoor(1), 4, 3);
            DoorClosed = Tiles(PaintDoor(0), 4, 3);
            KeyDoor = Tiles(PaintKeyDoor(), 1, 3);
            Revolving = Tiles(PaintRevolving(), 1, 3);
            Lift = ToSprite(PaintLift(), S * RoomCoords.PixelsPerUnit, Vector2.zero);
        }

        // Recorta un dibujo en tiles de 8 px; [fila desde arriba, columna].
        static TileBase[,] Tiles(Texture2D tex, int cols, int rows)
        {
            var t = new TileBase[rows, cols];
            int n = 8 * S;
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                {
                    var tile = ScriptableObject.CreateInstance<Tile>();
                    tile.sprite = Sprite.Create(tex, new Rect(c * n, (rows - 1 - r) * n, n, n), new Vector2(0.5f, 0.5f), n / RoomCoords.TileSize, 0, SpriteMeshType.FullRect);   // pivote en el centro de la celda
                    t[r, c] = tile;
                }
            return t;
        }

        static float Ellipse(float x, float y, float cx, float cy, float rx, float ry)
        {
            float u = (x - cx) / rx, v = (y - cy) / ry;
            return (Mathf.Sqrt(u * u + v * v) - 1) * Mathf.Min(rx, ry);
        }

        // ---------- piedras y puertas ----------

        // Polígono convexo (vértices en sentido horario): distancia con signo aproximada (máximo de los semiplanos).
        static float Convex(float x, float y, (float x, float y)[] v)
        {
            float d = -999;
            for (int i = 0; i < v.Length; i++)
            {
                var a = v[i];
                var b = v[(i + 1) % v.Length];
                float ex = b.x - a.x, ey = b.y - a.y, len = Mathf.Sqrt(ex * ex + ey * ey);
                // Normal hacia fuera (sentido horario, y hacia arriba).
                float nx = -ey / len, ny = ex / len;
                d = Mathf.Max(d, (x - a.x) * nx + (y - a.y) * ny);
            }
            return d;
        }

        // Piedra: bloque tallado que llena sus 2 × 2 tiles, con caras iluminadas desde arriba a la izquierda, aristas
        // claras, una grieta quebrada que sale del borde y alguna mella.
        static Texture2D PaintRock()
        {
            var cv = new ColorCanvas(16 * S, 16 * S, S, 0, 0);
            var outline = new[] { (0.6f, 0.4f), (0.4f, 9.6f), (2.4f, 14.6f), (8.6f, 15.6f), (14.2f, 14.2f), (15.6f, 8.2f), (15.4f, 0.4f) };
            Func<float, float, float> body = (x, y) => Convex(x, y, outline);
            cv.Fill(body, Stone, 0, 0, 16, 16);
            // Caras: arriba (la más clara), izquierda (media) y derecha (oscura), separadas en un punto descentrado.
            var c = (6.8f, 9.4f);
            var top = new[] { (2.4f, 14.6f), (8.6f, 15.6f), (14.2f, 14.2f), (15.6f, 8.2f), c, (0.4f, 9.6f) };
            var right = new[] { c, (15.6f, 8.2f), (15.4f, 0.4f), (7.6f, 0.4f) };
            cv.Fill((x, y) => Mathf.Max(Convex(x, y, top), body(x, y)), StoneLight, 0, 8, 16, 16);
            cv.Fill((x, y) => Mathf.Max(Convex(x, y, right), body(x, y)), StoneDark, 6.5f, 0, 16, 10);
            // Aristas claras entre caras.
            cv.Fill((x, y) => Mathf.Min(Segment(x, y, 0.6f, 9.6f, c.Item1, c.Item2, 0.22f), Segment(x, y, c.Item1, c.Item2, 15.6f, 8.2f, 0.22f)),
                new Color(1, 1, 1, 0.35f), 0, 8, 16, 11);
            cv.Fill((x, y) => Segment(x, y, c.Item1, c.Item2, 7.6f, 0.6f, 0.2f), new Color(0, 0, 0, 0.3f), 6, 0, 9, 10);
            // Grieta quebrada desde el borde de arriba.
            var crack = new[] { (10.6f, 15.0f), (10.0f, 12.6f), (11.4f, 10.8f), (10.6f, 8.6f), (11.8f, 6.2f) };
            for (int i = 0; i + 1 < crack.Length; i++)
            {
                var (ax, ay) = crack[i];
                var (bx, by) = crack[i + 1];
                float w = 0.32f - i * 0.05f;
                cv.Fill((x, y) => Segment(x, y, ax, ay, bx, by, w), Hole, Mathf.Min(ax, bx) - 1, Mathf.Min(ay, by) - 1, Mathf.Max(ax, bx) + 1, Mathf.Max(ay, by) + 1);
            }
            cv.Fill((x, y) => Segment(x, y, 11.0f, 11.6f, 12.8f, 12.2f, 0.18f), Hole, 10.5f, 11, 13.5f, 13);
            // Mellas y motas.
            foreach (var (mx, my, r) in new[] { (3.6f, 4.2f, 0.7f), (12.8f, 3.0f, 0.5f), (4.4f, 12.0f, 0.45f), (13.2f, 11.6f, 0.4f) })
                cv.Fill((x, y) => Circle(x, y, mx, my, r), new Color(0, 0, 0, 0.35f), mx - 1, my - 1, mx + 1, my + 1);
            cv.Fill((x, y) => Segment(x, y, 3.0f, 13.6f, 6.4f, 14.6f, 0.3f), new Color(1, 1, 1, 0.45f), 2.5f, 13, 7, 15.2f);
            cv.Ink(Ink, 1);
            return cv.ToTexture();
        }

        // Pórtico griego: escalón, dos columnas, frontón y la entrada oscura con un brillo. Cerrada, una losa de piedra
        // con bandas de bronce y el sello de la Gran Llave tapa la entrada; al abrirse sube por detrás del friso, como en
        // el original, que abre el hueco de abajo arriba ('openRows' filas de 8 px abiertas: 0, 1 o 2).
        static Texture2D PaintDoor(int openRows)
        {
            var cv = new ColorCanvas(32 * S, 24 * S, S, 0, 0);
            Func<float, float, float> entrance = (x, y) => Box(x, y, 16, 10.5f, 10.2f, 9.6f);
            cv.Fill(entrance, Hole, 5, 0, 27, 21);
            cv.Fill((x, y) => Mathf.Max(Ellipse(x, y, 16, 3.5f, 8.4f, 9.5f), -y), C("#1c2440"), 7, 0, 25, 14);
            cv.Fill((x, y) => Mathf.Max(Ellipse(x, y, 16, 1.5f, 5.0f, 6.0f), -y), new Color(0.6f, 0.75f, 1f, 0.35f), 10, 0, 22, 8);
            if (openRows < 2)
            {
                float lift = openRows == 0 ? 0 : 7.6f;
                Func<float, float, float> slab = (x, y) => Mathf.Max(Box(x, y, 16, 10.5f + lift, 10.0f, 9.6f), entrance(x, y));
                cv.Fill(slab, Stone, 5, 0, 27, 21);
                // Sillares: juntas horizontales y una vertical alterna.
                for (int k = 1; k <= 3; k++)
                {
                    float jy = 0.9f + lift + k * 4.8f;
                    cv.Fill((x, y) => Mathf.Max(Box(x, y, 16, jy, 10.0f, 0.18f), slab(x, y)), StoneDark, 5, jy - 1, 27, jy + 1);
                    float jx = k % 2 == 0 ? 11.0f : 21.0f;
                    float my = jy - 2.4f;
                    cv.Fill((x, y) => Mathf.Max(Box(x, y, jx, my, 0.18f, 2.4f), slab(x, y)), StoneDark, jx - 1, my - 3, jx + 1, my + 3);
                }
                cv.Fill((x, y) => Mathf.Max(Box(x, y, 16, 1.4f + lift, 10.0f, 0.5f), slab(x, y)), StoneLight, 5, 0, 27, 3 + lift);
                // Bandas de bronce y el sello dorado de la llave.
                foreach (float bx in new[] { 8.2f, 23.8f })
                    cv.Fill((x, y) => Mathf.Max(Box(x, y, bx, 10.5f + lift, 0.8f, 9.6f), slab(x, y)), Bronze, bx - 1.5f, 0, bx + 1.5f, 21);
                float sy = 10.5f + lift;
                cv.Fill((x, y) => Mathf.Max(Circle(x, y, 16, sy, 3.6f), entrance(x, y)), GoldDark, 12, sy - 4, 20, sy + 4);
                cv.Fill((x, y) => Mathf.Max(Circle(x, y, 16, sy, 2.9f), entrance(x, y)), Gold, 12, sy - 4, 20, sy + 4);
                cv.Fill((x, y) => Mathf.Max(Mathf.Min(Circle(x, y, 16, sy + 0.8f, 0.9f), Box(x, y, 16, sy - 0.6f, 0.35f, 1.3f)), entrance(x, y)), Hole, 14, sy - 3, 18, sy + 3);
                cv.Fill((x, y) => Mathf.Max(Box(x, y, 16, 0.6f + lift, 10.2f, 0.35f), entrance(x, y)), new Color(0, 0, 0, 0.45f), 5, 0, 27, 2 + lift);
            }
            foreach (float cx in new[] { 3.6f, 28.4f })
            {
                cv.Fill((x, y) => Box(x, y, cx, 9.6f, 2.4f, 9.0f), Marble, cx - 3, 0, cx + 3, 19);
                for (int k = -1; k <= 1; k++)
                {
                    float fx = cx + k * 1.1f;
                    cv.Fill((x, y) => Box(x, y, fx, 9.6f, 0.22f, 8.0f), MarbleShade, fx - 0.5f, 1, fx + 0.5f, 18);
                }
                cv.Fill((x, y) => Box(x, y, cx, 1.0f, 3.2f, 1.0f), Marble, cx - 3.5f, 0, cx + 3.5f, 2);
                cv.Fill((x, y) => Box(x, y, cx, 18.8f, 3.2f, 0.9f), Marble, cx - 3.5f, 17.5f, cx + 3.5f, 20);
            }
            cv.Fill((x, y) => Box(x, y, 16, 20.4f, 15.6f, 0.9f), Marble, 0, 19, 32, 21.5f);
            cv.Fill((x, y) => Mathf.Max(Spike(x, y, 16, 21.0f, 15.6f, 24.0f), 21.0f - y), Marble, 0, 21, 32, 24);
            cv.Fill((x, y) => Mathf.Max(Spike(x, y, 16, 21.4f, 11.0f, 23.3f), 21.4f - y), MarbleShade, 4, 21, 28, 23.5f);
            cv.Fill((x, y) => Box(x, y, 16, 20.4f, 15.6f, 0.25f), MarbleShade, 0, 20, 32, 21);
            cv.Ink(Ink, 1);
            return cv.ToTexture();
        }

        static Texture2D PaintKeyDoor()
        {
            var cv = new ColorCanvas(8 * S, 24 * S, S, 0, 0);
            cv.Fill((x, y) => Box(x, y, 4, 12, 3.7f, 11.8f) - 0.2f, Bronze, 0, 0, 8, 24);
            for (int k = 0; k < 3; k++)
            {
                float bx = 1.6f + k * 2.4f;
                cv.Fill((x, y) => Box(x, y, bx, 12, 0.35f, 11.4f), BronzeDark, bx - 1, 0, bx + 1, 24);
            }
            foreach (float by in new[] { 3f, 21f })
                cv.Fill((x, y) => Box(x, y, 4, by, 3.6f, 0.6f), BronzeDark, 0, by - 1, 8, by + 1);
            cv.Fill((x, y) => Box(x, y, 4, 12, 2.4f, 2.6f) - 0.3f, Gold, 1, 9, 7, 15);
            cv.Fill((x, y) => Mathf.Min(Circle(x, y, 4, 12.8f, 0.8f), Box(x, y, 4, 11.4f, 0.35f, 1.1f)), Hole, 2.5f, 10, 5.5f, 14);
            cv.Ink(Ink, 1);
            return cv.ToTexture();
        }

        static Texture2D PaintRevolving()
        {
            var cv = new ColorCanvas(8 * S, 24 * S, S, 0, 0);
            cv.Fill((x, y) => Box(x, y, 4, 12, 3.6f, 11.8f) - 0.3f, Wood, 0, 0, 8, 24);
            for (int k = 0; k < 4; k++)
            {
                float by = 3 + k * 6;
                cv.Fill((x, y) => Box(x, y, 4, by, 3.4f, 0.3f), WoodDark, 0, by - 1, 8, by + 1);
            }
            cv.Fill((x, y) => Box(x, y, 4, 12, 0.5f, 11.6f), Silver, 3, 0, 5, 24);
            cv.Fill((x, y) => Circle(x, y, 4, 12, 1.2f), Silver, 2.5f, 10.5f, 5.5f, 13.5f);
            cv.Ink(Ink, 1);
            return cv.ToTexture();
        }

        // Ascensor: losa de mármol de 32 × 16 con una franja dorada.
        static Texture2D PaintLift()
        {
            var cv = new ColorCanvas(32 * S, 16 * S, S, 0, 0);
            cv.Fill((x, y) => Box(x, y, 16, 8, 15.4f, 7.4f) - 0.6f, Marble, 0, 0, 32, 16);
            cv.Fill((x, y) => Mathf.Max(Box(x, y, 16, 8, 15.4f, 7.4f) - 0.6f, y - 4), MarbleShade, 0, 0, 32, 4);
            cv.Fill((x, y) => Box(x, y, 16, 12.4f, 14.6f, 0.6f), Gold, 1, 11, 31, 13.5f);
            for (int k = 0; k < 7; k++)
            {
                float mx = 4 + k * 4;
                cv.Fill((x, y) => Box(x, y, mx, 7.4f, 0.8f, 0.8f), MarbleShade, mx - 1.2f, 6, mx + 1.2f, 9);
            }
            cv.Ink(Ink, 1);
            return cv.ToTexture();
        }

        // ---------- dioses ----------

        readonly Dictionary<string, Sprite> gods = new Dictionary<string, Sprite>();

        // Retrato de un dios de los santuarios (busto en un medallón de 32 × 32): Demeter, Hephaestus, Ares, Athena,
        // Asclepius y Death.
        public Sprite God(string name)
        {
            if (!gods.TryGetValue(name ?? "", out var s))
                gods[name ?? ""] = s = ToSprite(PaintGod(name ?? ""), S * RoomCoords.PixelsPerUnit, new Vector2(0.5f, 0.5f));
            return s;
        }

        static Texture2D PaintGod(string name)
        {
            var cv = new ColorCanvas(32 * S, 32 * S, S, 0, 0);
            Color skin = C("#e8cdb0"), skinShade = C("#c09a7a"), robe, bg;
            switch (name)
            {
                case "Hephaestus": robe = C("#6a4a2a"); bg = C("#3a1a10"); break;
                case "Ares": robe = C("#7a2228"); bg = C("#2a0e12"); break;
                case "Athena": robe = C("#4a5a8a"); bg = C("#141c30"); break;
                case "Asclepius": robe = C("#e6e0d0"); bg = C("#16302a"); break;
                case "Death": robe = C("#1c1a24"); bg = C("#0a0a10"); break;
                default: robe = C("#5f8a4a"); bg = C("#1e2a14"); break;   // Demeter
            }
            // Fondo del medallón y un halo.
            cv.Fill((x, y) => Circle(x, y, 16, 16, 15.2f), bg, 0, 0, 32, 32);
            cv.Fill((x, y) => Circle(x, y, 16, 18, 10f), new Color(1, 1, 1, 0.06f), 4, 6, 28, 30);
            if (name == "Hephaestus")
                cv.Fill((x, y) => Circle(x, y, 16, 4, 9f), new Color(1f, 0.5f, 0.15f, 0.35f), 5, 0, 27, 14);
            // Hombros y túnica.
            cv.Fill((x, y) => Mathf.Max(Ellipse(x, y, 16, 3.5f, 11.5f, 7.5f), Circle(x, y, 16, 16, 15.2f)), robe, 2, 0, 30, 11);
            cv.Fill((x, y) => Mathf.Max(Segment(x, y, 9, 9.6f, 19, 1.0f, 0.9f), Circle(x, y, 16, 16, 15.2f)), Color.Lerp(robe, Color.white, 0.25f), 8, 0, 20, 11);
            if (name == "Death")
            {
                // Capucha y calavera, con la guadaña detrás.
                cv.Fill((x, y) => Segment(x, y, 25, 3, 25, 27, 0.6f), C("#6a5a40"), 23, 2, 27, 28);
                cv.Fill((x, y) => Mathf.Max(Mathf.Abs(Circle(x, y, 19, 24, 8)) - 0.9f, Mathf.Max(24 - y, x - 25.4f)), C("#c9cfdc"), 10, 23, 26, 33);
                cv.Fill((x, y) => Mathf.Min(Circle(x, y, 16, 17, 7.4f), Box(x, y, 16, 11, 7.0f, 5.0f)), robe, 8, 6, 24, 25);
                cv.Fill((x, y) => Mathf.Min(Circle(x, y, 16, 16.4f, 4.2f), Box(x, y, 16, 12.4f, 2.6f, 1.6f)), C("#e8e2cf"), 11, 10, 21, 21);
                cv.Fill((x, y) => Mathf.Min(Circle(x, y, 14.4f, 16.6f, 1.2f), Circle(x, y, 17.6f, 16.6f, 1.2f)), Hole, 12.5f, 15, 19.5f, 18);
                cv.Fill((x, y) => Mathf.Min(Mathf.Min(Box(x, y, 15, 12.0f, 0.3f, 0.8f), Box(x, y, 16, 12.0f, 0.3f, 0.8f)), Box(x, y, 17, 12.0f, 0.3f, 0.8f)), Hole, 14, 11, 18, 13);
                cv.Fill((x, y) => Mathf.Min(Circle(x, y, 14.4f, 16.6f, 0.35f), Circle(x, y, 17.6f, 16.6f, 0.35f)), C("#ff4a3a"), 13.5f, 16, 18.5f, 17.5f);
            }
            else
            {
                bool beard = name == "Hephaestus" || name == "Ares" || name == "Asclepius";
                // Cuello, cara y pelo.
                cv.Fill((x, y) => Box(x, y, 16, 9.6f, 2.2f, 2.4f), skinShade, 13, 7, 19, 12.5f);
                if (name == "Demeter" || name == "Athena" || name == "")
                    cv.Fill((x, y) => Mathf.Min(Ellipse(x, y, 16, 15.2f, 6.4f, 8.6f), Box(x, y, 16, 9.0f, 6.2f, 3.0f)), name == "Athena" ? C("#3a2a1a") : C("#c9953a"), 9, 6, 23, 24);
                cv.Fill((x, y) => Ellipse(x, y, 16, 16.4f, 4.6f, 5.6f), skin, 11, 10.5f, 21, 22.5f);
                cv.Fill((x, y) => Mathf.Max(Ellipse(x, y, 16, 16.4f, 4.6f, 5.6f), 16 - x), new Color(skinShade.r, skinShade.g, skinShade.b, 0.6f), 16, 10.5f, 21, 22.5f);
                cv.Fill((x, y) => Mathf.Min(Ellipse(x, y, 14.2f, 17.2f, 0.7f, 0.5f), Ellipse(x, y, 17.8f, 17.2f, 0.7f, 0.5f)), Hole, 13, 16.5f, 19, 18);
                cv.Fill((x, y) => Segment(x, y, 14.8f, 13.4f, 17.2f, 13.4f, 0.25f), C("#8a4a3a"), 14, 13, 18, 14);
                if (beard)
                {
                    var bc = name == "Asclepius" ? C("#e8e6e0") : name == "Ares" ? C("#2a1a12") : C("#4a2a18");
                    cv.Fill((x, y) => Mathf.Max(Ellipse(x, y, 16, 12.6f, 4.8f, 4.6f), y - 14.4f), bc, 11, 7.5f, 21, 15);
                    cv.Fill((x, y) => Segment(x, y, 14.6f, 14.6f, 17.4f, 14.6f, 0.4f), bc, 14, 14, 18, 15.4f);
                }
                switch (name)
                {
                    case "Hephaestus":
                        cv.Fill((x, y) => Mathf.Max(Ellipse(x, y, 16, 20.6f, 5.2f, 3.6f), 19.6f - y), C("#5a3a22"), 10.5f, 19.5f, 21.5f, 24.5f);
                        cv.Fill((x, y) => Segment(x, y, 23.5f, 4, 26.5f, 14, 0.5f), C("#8a6a3a"), 22.5f, 3, 27.5f, 15);
                        cv.Fill((x, y) => Box(x, y, 26.6f, 14.6f, 2.2f, 1.4f) - 0.2f, C("#8d909c"), 24, 13, 29.5f, 16.5f);
                        break;
                    case "Ares":
                        cv.Fill((x, y) => Mathf.Max(Circle(x, y, 16, 18.0f, 5.6f), 16.6f - y), C("#c08a3e"), 10, 16, 22, 24);
                        cv.Fill((x, y) => Mathf.Max(Mathf.Abs(Circle(x, y, 16, 19.0f, 7.2f)) - 1.2f, 21.0f - y), C("#d23c32"), 8, 20, 24, 27.5f);
                        cv.Fill((x, y) => Box(x, y, 16, 18.2f, 5.6f, 0.45f), C("#7f5520"), 10, 17.5f, 22, 19);
                        break;
                    case "Athena":
                        cv.Fill((x, y) => Mathf.Max(Circle(x, y, 16, 20.6f, 5.4f), 19.4f - y), C("#c9cfdc"), 10, 19, 22, 26.5f);
                        cv.Fill((x, y) => Mathf.Max(Mathf.Abs(Circle(x, y, 16, 21.4f, 6.6f)) - 0.8f, 23.4f - y), C("#4f6fb0"), 9, 23, 23, 28.6f);
                        // Lechuza en el hombro.
                        cv.Fill((x, y) => Ellipse(x, y, 25.0f, 8.4f, 2.6f, 3.2f), C("#a0886a"), 22, 5, 28, 12);
                        cv.Fill((x, y) => Mathf.Min(Circle(x, y, 24.0f, 9.6f, 0.8f), Circle(x, y, 26.0f, 9.6f, 0.8f)), C("#f2d27a"), 23, 8.5f, 27, 10.6f);
                        cv.Fill((x, y) => Mathf.Min(Circle(x, y, 24.0f, 9.6f, 0.35f), Circle(x, y, 26.0f, 9.6f, 0.35f)), Hole, 23.5f, 9, 26.5f, 10.2f);
                        break;
                    case "Asclepius":
                        cv.Fill((x, y) => Mathf.Max(Ellipse(x, y, 16, 20.4f, 4.8f, 3.0f), 19.8f - y), C("#e8e6e0"), 11, 19.5f, 21, 23.5f);
                        cv.Fill((x, y) => Segment(x, y, 25, 2, 25, 24, 0.55f), C("#8a6a3a"), 24, 1, 26, 25);
                        for (int k = 0; k < 4; k++)
                        {
                            float sy = 5 + k * 4.4f;
                            cv.Fill((x, y) => Mathf.Abs(Ellipse(x, y, 25, sy + 2.2f, 1.6f, 2.0f)) - 0.35f, C("#5fae5a"), 22.5f, sy, 27.5f, sy + 4.5f);
                        }
                        break;
                    default:
                        // Demeter: corona de espigas.
                        for (int k = -2; k <= 2; k++)
                        {
                            float wx = 16 + k * 2.4f, top = 25.4f - Mathf.Abs(k) * 0.9f;
                            cv.Fill((x, y) => Mathf.Min(Segment(x, y, wx, 21, wx, top, 0.35f), Ellipse(x, y, wx, top - 0.6f, 0.7f, 1.4f)), C("#e6c04a"), wx - 1.5f, 20, wx + 1.5f, top + 1.5f);
                        }
                        break;
                }
            }
            // Marco del medallón.
            cv.Fill((x, y) => Mathf.Abs(Circle(x, y, 16, 16, 15.2f)) - 0.7f, C("#e6c04a"), 0, 0, 32, 32);
            cv.Fill((x, y) => Mathf.Abs(Circle(x, y, 16, 16, 13.9f)) - 0.25f, C("#9a7420"), 1, 1, 31, 31);
            cv.Ink(Ink, 1);
            return cv.ToTexture();
        }

        // ---------- objetos ----------

        public Sprite Item(int type)
        {
            if (!items.TryGetValue(type, out var s))
                items[type] = s = ToSprite(PaintItem(type), S * RoomCoords.PixelsPerUnit, new Vector2(0.5f, 0));
            return s;
        }

        static Texture2D PaintItem(int type)
        {
            var cv = new ColorCanvas(16 * S, 16 * S, S, 0, 0);
            // Halo suave detrás (el bloom lo hace brillar).
            for (int k = 3; k >= 1; k--)
            {
                float r = 2.2f * k;
                cv.Fill((x, y) => Circle(x, y, 8, 8, r), new Color(1f, 0.95f, 0.8f, 0.07f), 0, 0, 16, 16);
            }
            var icon = new ColorCanvas(16 * S, 16 * S, S, 0, 0);
            PaintIcon(icon, type);
            icon.Ink(Ink, 2);
            for (int i = 0; i < icon.Px.Length; i++)
            {
                var a = icon.Px[i];
                if (a.a <= 0) continue;
                var o = cv.Px[i];
                float na = a.a + o.a * (1 - a.a);
                var rgb = (new Color(a.r, a.g, a.b) * a.a + new Color(o.r, o.g, o.b) * o.a * (1 - a.a)) / Mathf.Max(na, 1e-5f);
                cv.Px[i] = new Color(rgb.r, rgb.g, rgb.b, na);
            }
            return cv.ToTexture();
        }

        static void Shine(ColorCanvas cv, float x0, float y0) =>
            cv.Fill((x, y) => Circle(x, y, x0, y0, 0.6f), new Color(1, 1, 1, 0.8f), x0 - 1, y0 - 1, x0 + 1, y0 + 1);

        static void ArrowIcon(ColorCanvas cv, Color head, Color fletch)
        {
            cv.Fill((x, y) => Segment(x, y, 3, 3, 12, 12, 0.45f), Wood, 2, 2, 13, 13);
            cv.Fill((x, y) => Mathf.Max(Mathf.Abs((x - y)) * 0.7f - (14.4f - (x + y) / 2) * 0.6f, Mathf.Max((x + y) / 2 - 14.4f, 11.6f - (x + y) / 2)), head, 10, 10, 15.5f, 15.5f);
            cv.Fill((x, y) => Mathf.Min(Segment(x, y, 3, 3, 1.4f, 5.2f, 0.45f), Segment(x, y, 3, 3, 5.2f, 1.4f, 0.45f)), fletch, 0.5f, 0.5f, 6, 6);
        }

        static void ShieldIcon(ColorCanvas cv, Color face, Color rim)
        {
            cv.Fill((x, y) => Circle(x, y, 8, 8, 6.4f), rim, 1, 1, 15, 15);
            cv.Fill((x, y) => Circle(x, y, 8, 8, 5.2f), face, 2, 2, 14, 14);
            cv.Fill((x, y) => Mathf.Abs(Circle(x, y, 8, 8, 2.4f)) - 0.5f, rim, 5, 5, 11, 11);
            Shine(cv, 5.6f, 10.6f);
        }

        static void KeyIcon(ColorCanvas cv, Color c, float scale)
        {
            float s = scale;
            cv.Fill((x, y) => Mathf.Abs(Circle(x, y, 5, 10, 2.8f * s)) - 0.9f * s, c, 1, 6, 9, 14);
            cv.Fill((x, y) => Segment(x, y, 7, 8, 13.4f, 2.6f, 0.75f * s), c, 6, 1.5f, 14.5f, 9);
            cv.Fill((x, y) => Segment(x, y, 11.6f, 4.2f, 13.2f, 5.8f, 0.6f * s), c, 10.5f, 3, 14.5f, 7);
            cv.Fill((x, y) => Segment(x, y, 9.8f, 5.8f, 11.2f, 7.4f, 0.6f * s), c, 9, 5, 12.5f, 8.5f);
            Shine(cv, 4.0f, 11.6f);
        }

        static void PaintIcon(ColorCanvas cv, int type)
        {
            switch (type)
            {
                case 0x01: ArrowIcon(cv, Silver, Red); break;
                case 0x02: ArrowIcon(cv, C("#e6d6b0"), Blue); break;
                case 0x03:
                    cv.Fill((x, y) => Circle(x, y, 8, 7, 5.6f), C("#ff8a3a"), 2, 1, 14, 13);
                    cv.Fill((x, y) => Mathf.Abs(Circle(x, y, 8, 7, 3.0f)) - 0.8f, C("#ffd36a"), 4, 3, 12, 11);
                    cv.Fill((x, y) => Circle(x, y, 8, 7, 1.2f), White, 6, 5, 10, 9);
                    break;
                case 0x04:
                    cv.Fill((x, y) => Mathf.Min(Circle(x, y, 8, 5, 4.4f), Spike(x, y, 8, 5, 4.2f, 15)), C("#ff8a3a"), 3, 0, 13, 15);
                    cv.Fill((x, y) => Mathf.Min(Circle(x, y, 8, 5, 2.6f), Spike(x, y, 8, 5, 2.4f, 11.5f)), C("#ffd36a"), 5, 2, 11, 12);
                    break;
                case 0x05:
                    for (int k = 0; k < 6; k++)
                    {
                        float a = k * Mathf.PI / 3, ex = 8 + Mathf.Cos(a) * 6.6f, ey = 7 + Mathf.Sin(a) * 6.6f;
                        cv.Fill((x, y) => Segment(x, y, 8, 7, ex, ey, 0.6f), SilverDark, 0, 0, 16, 16);
                    }
                    cv.Fill((x, y) => Circle(x, y, 8, 7, 4.4f), C("#2a2a34"), 3, 2, 13, 12);
                    cv.Fill((x, y) => Circle(x, y, 8, 7, 1.1f), Red, 6, 5, 10, 9);
                    break;
                case 0x06:
                    cv.Fill((x, y) => Segment(x, y, 9.6f, 6.4f, 14, 2, 1.0f), Wood, 8, 1, 15, 8);
                    cv.Fill((x, y) => Mathf.Abs(Circle(x, y, 6.6f, 9.4f, 4.4f)) - 0.8f, Gold, 1, 4, 12, 15);
                    cv.Fill((x, y) => Circle(x, y, 6.6f, 9.4f, 3.6f), new Color(0.7f, 0.85f, 1f, 0.6f), 2, 5, 11, 14);
                    Shine(cv, 5.2f, 11.0f);
                    break;
                case 0x07:
                    cv.Fill((x, y) => Mathf.Min(Circle(x, y, 8, 5.4f, 4.4f), Box(x, y, 8, 10.6f, 1.6f, 2.6f)), new Color(0.75f, 0.85f, 1f, 0.9f), 3, 0, 13, 14);
                    cv.Fill((x, y) => Mathf.Max(Circle(x, y, 8, 5.4f, 3.6f), y - 6.4f), Blue, 4, 1, 12, 7);
                    cv.Fill((x, y) => Box(x, y, 8, 13.6f, 1.4f, 0.9f), Wood, 6, 12, 10, 15);
                    Shine(cv, 6.0f, 7.4f);
                    break;
                case 0x08:
                    cv.Fill((x, y) => Mathf.Max(Spike(x, y, 8, 1, 6.6f, 22), y - 13), Cloth, 1, 0, 15, 14);
                    cv.Fill((x, y) => Box(x, y, 8, 12.6f, 3.0f, 0.8f), Gold, 4.5f, 11.5f, 11.5f, 14);
                    cv.Fill((x, y) => Mathf.Max(Spike(x, y, 8, 1, 6.6f, 22), 3 - y), C("#5a3a8a"), 1, 0, 15, 3.5f);
                    break;
                case 0x09:
                    cv.Fill((x, y) => Segment(x, y, 4, 2, 11, 11, 0.7f), Wood, 3, 1, 12, 12);
                    cv.Fill((x, y) => Circle(x, y, 11.6f, 11.8f, 2.4f), C("#b06ae0"), 9, 9, 14.5f, 14.5f);
                    Shine(cv, 10.8f, 12.8f);
                    break;
                case 0x0A:
                    cv.Fill((x, y) => Box(x, y, 8, 8, 6.0f, 4.6f), C("#e8d9b0"), 1.5f, 3, 14.5f, 13);
                    foreach (float sx in new[] { 2.0f, 14.0f })
                        cv.Fill((x, y) => Box(x, y, sx, 8, 1.0f, 5.4f) - 0.3f, C("#c8b88a"), sx - 1.5f, 2, sx + 1.5f, 14);
                    cv.Fill((x, y) => Mathf.Min(Segment(x, y, 4.5f, 6, 7.5f, 10, 0.3f), Segment(x, y, 7.5f, 10, 11.5f, 7, 0.3f)), Red, 4, 5, 12, 11);
                    break;
                case 0x0B:
                    cv.Fill((x, y) => Mathf.Min(Box(x, y, 8, 7.5f, 1.3f, 6.8f), Box(x, y, 8, 10.2f, 4.8f, 1.3f)), Gold, 2.5f, 0.5f, 13.5f, 14.5f);
                    Shine(cv, 7.4f, 13.0f);
                    break;
                case 0x0C: KeyIcon(cv, Gold, 1.2f); break;
                case 0x0D:
                    for (int k = 0; k < 9; k++)
                    {
                        float a = Mathf.PI * (0.1f + 0.8f * k / 8f), bx = 8 + Mathf.Cos(a) * 5.4f, by = 9 - Mathf.Sin(a) * 5.4f + 5.4f * 0.0f;
                        float yy = 13 - Mathf.Sin(a) * 6;
                        cv.Fill((x, y) => Circle(x, y, bx, yy, 0.8f), Gold, bx - 1, yy - 1, bx + 1, yy + 1);
                    }
                    cv.Fill((x, y) => Spike(x, y, 8, 7.4f, 1.8f, 3.6f), C("#e05a7a"), 6, 3, 10, 8);
                    break;
                case 0x0E:
                    cv.Fill((x, y) => Mathf.Min(Box(x, y, 8, 5, 5.6f, 2.4f), Mathf.Min(Mathf.Min(Spike(x, y, 3.2f, 6, 1.6f, 12), Spike(x, y, 8, 6, 1.8f, 13.4f)), Spike(x, y, 12.8f, 6, 1.6f, 12))), Gold, 1.5f, 2, 14.5f, 14);
                    cv.Fill((x, y) => Circle(x, y, 8, 5, 1.0f), Red, 6.5f, 3.5f, 9.5f, 6.5f);
                    break;
                case 0x0F:
                    cv.Fill((x, y) => Mathf.Min(Circle(x, y, 8, 8, 5.4f), Box(x, y, 8, 4.4f, 5.4f, 2.6f)), Silver, 2, 1.5f, 14, 14);
                    cv.Fill((x, y) => Mathf.Min(Box(x, y, 9.6f, 7.4f, 3.0f, 0.5f), Box(x, y, 8.6f, 5.4f, 0.5f, 2.2f)), Hole, 6, 3, 13, 8.5f);
                    Shine(cv, 5.4f, 11.0f);
                    break;
                case 0x10:
                    cv.Fill((x, y) => Segment(x, y, 3, 13, 10.4f, 5.6f, 0.6f), Wood, 2, 4.5f, 11.5f, 14);
                    cv.Fill((x, y) => Ellipse(x - 0, y, 12.2f, 3.8f, 2.4f, 3.0f), Wood, 9.5f, 0.5f, 15, 7);
                    break;
                case 0x11:
                    cv.Fill((x, y) => Mathf.Min(Box(x, y, 7.4f, 3.2f, 5.0f, 1.4f) - 0.4f, Box(x, y, 4.4f, 6.0f, 2.0f, 3.0f) - 0.3f), C("#a0703a"), 1.5f, 1.5f, 13, 9.5f);
                    for (int k = 0; k < 3; k++)
                    {
                        float ey = 8 + k * 1.6f, ex = 1.2f - k * 0.4f;
                        cv.Fill((x, y) => Segment(x, y, 4, 7.4f, ex, ey, 0.6f), White, 0, 6.5f, 5, 12.5f);
                    }
                    break;
                case 0x12:
                    cv.Fill((x, y) => Mathf.Min(Ellipse(x, y, 8, 5, 3.6f, 4.0f), Circle(x, y, 8, 11, 2.6f)), C("#e8a0b4"), 4, 1, 12, 14);
                    cv.Fill((x, y) => Circle(x, y, 8.8f, 11.2f, 0.45f), Hole, 8, 10.5f, 9.5f, 12);
                    break;
                case 0x13:
                    cv.Fill((x, y) => Mathf.Max(Mathf.Abs(Circle(x, y, 9, 4, 7.4f)) - (0.6f + Mathf.Clamp01((x - 2) / 12f) * 1.6f), 4 - y), Gold, 1, 3, 15.5f, 12);
                    break;
                case 0x14:
                    cv.Fill((x, y) => Mathf.Min(Mathf.Max(Spike(x, y, 8, 3, 5.4f, 16), y - 12), Box(x, y, 8, 3.2f, 5.6f, 0.9f)), Gold, 2, 2, 14, 13);
                    cv.Fill((x, y) => Circle(x, y, 8, 1.6f, 1.2f), GoldDark, 6.5f, 0, 9.5f, 3);
                    Shine(cv, 6.4f, 9.4f);
                    break;
                case 0x15:
                    cv.Fill((x, y) => Mathf.Abs(Ellipse(x, y, 8, 11.6f, 4.0f, 1.6f)) - 0.5f, Gold, 3, 9, 13, 14);
                    cv.Fill((x, y) => Ellipse(x, y, 4.6f, 5.6f, 3.6f, 2.2f), White, 0.5f, 3, 8.5f, 8.5f);
                    cv.Fill((x, y) => Ellipse(x, y, 11.4f, 5.6f, 3.6f, 2.2f), White, 7.5f, 3, 15.5f, 8.5f);
                    break;
                case 0x16:
                    cv.Fill((x, y) => Box(x, y, 8, 5, 2.0f, 4.6f), White, 5.5f, 0, 10.5f, 10);
                    cv.Fill((x, y) => Mathf.Min(Circle(x, y, 8, 11.4f, 1.2f), Spike(x, y, 8, 11.4f, 1.1f, 15)), C("#ffb347"), 6.5f, 10, 9.5f, 15);
                    break;
                case 0x17:
                    cv.Fill((x, y) => Mathf.Min(Box(x, y, 8, 7, 4.4f, 5.0f) - 0.8f, Mathf.Min(Circle(x, y, 3.4f, 11.4f, 1.6f), Circle(x, y, 12.6f, 11.4f, 1.6f))), Silver, 1.5f, 1, 14.5f, 13.5f);
                    cv.Fill((x, y) => Box(x, y, 8, 7, 0.3f, 4.0f), SilverDark, 7.5f, 3, 8.5f, 11);
                    Shine(cv, 5.6f, 9.6f);
                    break;
                case 0x18:
                    cv.Fill((x, y) => Box(x, y, 8, 6, 6.2f, 2.6f) - 0.6f, Red, 1, 3, 15, 9);
                    for (int k = 0; k < 3; k++)
                    {
                        float bx = 4 + k * 4;
                        cv.Fill((x, y) => Box(x, y, bx, 6, 0.5f, 2.6f), Gold, bx - 1, 3, bx + 1, 9);
                    }
                    cv.Fill((x, y) => Circle(x, y, 2.2f, 6, 2.4f), C("#a0302a"), 0, 3.5f, 4.6f, 8.5f);
                    break;
                case 0x19:
                    cv.Fill((x, y) => Mathf.Min(Ellipse(x, y, 8, 4.0f, 6.8f, 1.6f), Mathf.Max(Circle(x, y, 8, 4.4f, 4.2f), 4.4f - y)), C("#7a5a3a"), 1, 2, 15, 9);
                    cv.Fill((x, y) => Box(x, y, 8, 5.4f, 4.0f, 0.5f), Red, 4, 4.5f, 12, 6.5f);
                    break;
                case 0x1A:
                    cv.Fill((x, y) => Mathf.Min(Ellipse(x, y, 7, 4.4f, 5.4f, 2.6f), Segment(x, y, 11.6f, 4.6f, 14.6f, 6.4f, 0.8f)), Gold, 1.5f, 1.5f, 15.5f, 8);
                    cv.Fill((x, y) => Mathf.Min(Circle(x, y, 14.6f, 8.0f, 1.0f), Spike(x, y, 14.6f, 8.0f, 0.9f, 11.6f)), C("#ffb347"), 13.4f, 6.8f, 15.8f, 12);
                    break;
                case 0x1B:
                case 0x24:
                    cv.Fill((x, y) => Mathf.Min(Ellipse(x, y, 8, 6, 4.6f, 5.0f), Box(x, y, 8, 12.6f, 1.8f, 1.6f)), Bronze, 3, 0.5f, 13, 14.5f);
                    cv.Fill((x, y) => Box(x, y, 8, 6.4f, 4.4f, 0.6f), Hole, 3.5f, 5.5f, 12.5f, 7.5f);
                    if (type == 0x24) cv.Fill((x, y) => Mathf.Abs(Circle(x, y, 12.4f, 8.4f, 1.8f)) - 0.45f, Bronze, 10, 6, 15, 11);
                    else foreach (float hx in new[] { 4.6f, 11.4f }) cv.Fill((x, y) => Mathf.Abs(Circle(x, y, hx, 11.6f, 1.2f)) - 0.35f, Bronze, hx - 2, 10, hx + 2, 13.5f);
                    Shine(cv, 6.0f, 8.8f);
                    break;
                case 0x1C:
                    cv.Fill((x, y) => Mathf.Max(Mathf.Abs(Circle(x, y, 8, 12, 4.4f)) - 0.3f, 12 - y + 0.0f) , Gold, 3, 11, 13, 16);
                    cv.Fill((x, y) => Mathf.Max(Mathf.Abs(Circle(x, y, 8, 12, 4.4f)) - 0.3f, y - 12), Gold, 3, 7, 13, 12);
                    cv.Fill((x, y) => Mathf.Min(Spike(x, y, 8, 7.6f, 2.4f, 2.0f), Spike(x, y, 8, 7.6f, 2.4f, 9.4f)), Green, 5.5f, 1.5f, 10.5f, 10);
                    break;
                case 0x1D:
                    foreach (float ex in new[] { 5.0f, 11.0f })
                    {
                        cv.Fill((x, y) => Mathf.Abs(Circle(x, y, ex, 12, 1.2f)) - 0.3f, Gold, ex - 2, 10, ex + 2, 14);
                        cv.Fill((x, y) => Mathf.Min(Circle(x, y, ex, 6.6f, 1.8f), Spike(x, y, ex, 6.6f, 1.6f, 10.6f)), C("#5ac8e8"), ex - 2.5f, 4, ex + 2.5f, 11);
                    }
                    break;
                case 0x1E:
                case 0x1F:
                    cv.Fill((x, y) => Mathf.Abs(Ellipse(x, y, 8, 6.4f, 5.0f, type == 0x1E ? 4.0f : 4.6f)) - (type == 0x1E ? 1.2f : 0.8f), Gold, 1.5f, 1, 14.5f, 12);
                    if (type == 0x1F) cv.Fill((x, y) => Circle(x, y, 8, 11.6f, 1.8f), C("#e05a7a"), 6, 9.5f, 10, 14);
                    Shine(cv, 5.0f, 8.6f);
                    break;
                case 0x20:
                    cv.Fill((x, y) => Box(x, y, 8, 7.4f, 5.4f, 6.0f) - 0.4f, C("#5a2a2a"), 2, 1, 14, 14);
                    cv.Fill((x, y) => Box(x, y, 13.0f, 7.4f, 0.6f, 5.6f), White, 12, 1.5f, 14, 13.5f);
                    cv.Fill((x, y) => Mathf.Min(Box(x, y, 7.4f, 8.4f, 0.5f, 3.0f), Box(x, y, 7.4f, 9.4f, 2.0f, 0.5f)), Gold, 5, 5, 10, 12);
                    break;
                case 0x21:
                    cv.Fill((x, y) => Mathf.Max(Mathf.Abs(Circle(x, y, 8, 9, 5.4f)) - 0.8f, 6 - y), Gold, 2, 5, 14, 15);
                    cv.Fill((x, y) => Box(x, y, 8, 3.6f, 4.6f, 1.0f), Gold, 3, 2, 13, 5);
                    for (int k = -1; k <= 1; k++)
                    {
                        float sx = 8 + k * 1.8f;
                        cv.Fill((x, y) => Box(x, y, sx, 8.6f, 0.15f, 4.6f), White, sx - 0.5f, 4, sx + 0.5f, 13.5f);
                    }
                    break;
                case 0x22:
                    cv.Fill((x, y) => Mathf.Abs(Spike(x, y, 8, 2, 6.4f, 14)) - 0.6f, Silver, 1, 1, 15, 15);
                    break;
                case 0x23:
                    cv.Fill((x, y) => Mathf.Min(Ellipse(x, y, 8, 7, 5.6f, 4.6f), Segment(x, y, 12.0f, 7, 15.0f, 7.6f, 1.2f)), C("#f2d2c0"), 1.5f, 2, 16, 12);
                    cv.Fill((x, y) => Mathf.Abs(Circle(x, y, 7.4f, 7.2f, 2.6f)) - 0.35f, C("#d09a8a"), 4, 4, 11, 11);
                    cv.Fill((x, y) => Mathf.Abs(Circle(x, y, 7.0f, 7.4f, 1.2f)) - 0.3f, C("#d09a8a"), 5, 5.5f, 9, 9.5f);
                    break;
                case 0x25:
                    cv.Fill((x, y) => Mathf.Max(Mathf.Abs(Circle(x, y, 0.5f, 1.0f, 13.4f)) - 0.9f, Mathf.Max(5.5f - x, y - 14.5f)), Silver, 4, 1, 15, 15);
                    cv.Fill((x, y) => Segment(x, y, 2.4f, 2.4f, 5.4f, 5.4f, 0.8f), Wood, 1, 1, 7, 7);
                    cv.Fill((x, y) => Segment(x, y, 3.6f, 6.6f, 6.8f, 3.4f, 0.55f), Gold, 2.5f, 2.5f, 8, 8);
                    break;
                case 0x26:
                    cv.Fill((x, y) => Mathf.Max(Mathf.Max(Mathf.Abs(x - y) * 0.7f - (14.8f - (x + y) / 2) * 0.22f, (x + y) / 2 - 14.8f), 7 - (x + y) / 2), Silver, 5, 5, 15.5f, 15.5f);
                    cv.Fill((x, y) => Segment(x, y, 2.6f, 2.6f, 6.2f, 6.2f, 0.8f), Wood, 1, 1, 7.5f, 7.5f);
                    cv.Fill((x, y) => Segment(x, y, 4.6f, 8.4f, 8.4f, 4.6f, 0.55f), Gold, 3.5f, 3.5f, 9.5f, 9.5f);
                    break;
                case 0x27:
                    for (int k = 0; k < 4; k++)
                    {
                        float ex = 2.0f + k * 0.4f, ey = 13 - k * 3.0f;
                        cv.Fill((x, y) => Segment(x, y, 12.4f, 5.0f, ex, ey, 1.4f - k * 0.15f), White, 0, 1, 15, 15);
                    }
                    break;
                case 0x28: ShieldIcon(cv, Bronze, BronzeDark); break;
                case 0x2B: ShieldIcon(cv, Silver, SilverDark); break;
                case 0x2C: ShieldIcon(cv, Gold, GoldDark); break;
                case 0x29:
                    cv.Fill((x, y) => Ellipse(x, y, 5.6f, 4.0f, 4.6f, 2.8f), C("#d6a05a"), 0.5f, 1, 10.5f, 7);
                    cv.Fill((x, y) => Mathf.Max(Box(x, y, 11.6f, 6.6f, 2.6f, 4.0f), y - 10.6f), new Color(0.75f, 0.85f, 1f, 0.9f), 8.5f, 2, 14.5f, 11);
                    cv.Fill((x, y) => Mathf.Max(Box(x, y, 11.6f, 5.2f, 2.2f, 2.4f), 0), Blue, 9, 2.5f, 14, 8);
                    break;
                case 0x2A:
                    cv.Fill((x, y) => Mathf.Min(Ellipse(x, y, 8, 5.6f, 4.6f, 4.6f), Box(x, y, 8, 11.0f, 1.8f, 1.4f)), White, 3, 0.5f, 13, 13);
                    cv.Fill((x, y) => Box(x, y, 8, 10.0f, 2.6f, 0.5f), Wood, 5, 9, 11, 11);
                    break;
                case 0x2D:
                    cv.Fill((x, y) => Box(x, y, 8, 5.6f, 3.0f, 5.0f) - 0.6f, C("#7a4a2a"), 4, 0, 12, 11);
                    for (int k = -1; k <= 1; k++)
                    {
                        float ax = 8 + k * 1.6f;
                        cv.Fill((x, y) => Segment(x, y, ax, 10, ax + k * 0.6f, 14.4f, 0.35f), Wood, ax - 1.5f, 9, ax + 1.5f, 15);
                        cv.Fill((x, y) => Spike(x, y, ax + k * 0.6f, 13.6f, 0.9f, 15.8f), Red, ax - 2, 13, ax + 2, 16);
                    }
                    break;
                case 0x2E:
                    foreach (var (cx, cy) in new[] { (5.6f, 4.0f), (10.4f, 4.4f), (8.0f, 8.4f) })
                    {
                        cv.Fill((x, y) => Ellipse(x, y, cx, cy, 3.4f, 3.0f), GoldDark, cx - 4, cy - 4, cx + 4, cy + 4);
                        cv.Fill((x, y) => Ellipse(x, y, cx, cy + 0.3f, 2.7f, 2.3f), Gold, cx - 3, cy - 3, cx + 3, cy + 3);
                    }
                    Shine(cv, 7.0f, 9.6f);
                    break;
                case 0x2F: KeyIcon(cv, C("#7fe0ff"), 1f); break;
                default:
                    cv.Fill((x, y) => (Mathf.Abs(x - 8) + Mathf.Abs(y - 8)) * 0.7f - 4.4f, Gold, 1, 1, 15, 15);
                    break;
            }
        }
    }
}
