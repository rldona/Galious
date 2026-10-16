using System.Collections.Generic;
using MazeOfKnight.Data;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MazeOfKnight.Game
{
    // Dibuja todas las salas de un mapa con los tiles provisionales: fondo, una capa de colisión, y encima
    // puertas de mundo y piedras. Las salas se colocan en su posición de la rejilla (RoomCoords), así que
    // pasar de sala es solo mover la cámara.
    public class MapView : MonoBehaviour
    {
        public TileBase background;
        public TileBase backgroundAlt;
        public TileBase shrine;
        public TileBase solid;
        public TileBase ladder;
        public TileBase water;
        public TileBase lava;
        public TileBase bossPassable;
        public TileBase rock;
        public TileBase door;
        public TileBase keyDoor;
        public TileBase revolvingDoor;

        // Tiles del estilo del remake (Visuals/Atmosphere): si están puestos, sustituyen a los provisionales, se quita el
        // fondo en damero y se añade el borde iluminado encima de cada suelo. La colisión no cambia.
        public class StyleTiles
        {
            public TileBase Solid, SolidInner, SolidDeep, Rim, Ladder, LadderRight, Water, Lava, BossPassable;
            // Variantes del borde y del interior (arte dibujado) para romper la repetición; se eligen por celda.
            public TileBase[] SolidVariants, InnerVariants;
            public TileBase[,] Rock, Door, KeyDoor, Revolving;    // dibujos de varias celdas: [fila desde arriba, columna]
            public TileBase[,] DoorClosed, DoorHalf;              // puerta de mundo cerrada y con la fila de abajo abierta
        }

        public StyleTiles Style;

        // Lo que se ha puesto encima de la colisión, para volver a dibujarlo al cambiar de estilo.
        readonly Dictionary<(RoomModel, int, int), bool> shownRocks = new Dictionary<(RoomModel, int, int), bool>();
        readonly Dictionary<(RoomModel, int, int), int> shownKeyDoors = new Dictionary<(RoomModel, int, int), int>();
        readonly HashSet<(RoomModel, int, int)> shownRevolving = new HashSet<(RoomModel, int, int)>();
        readonly Dictionary<(RoomModel, int, int), int> shownDoors = new Dictionary<(RoomModel, int, int), int>();

        Grid grid;
        Tilemap back, front, rim, rocks, keyDoors, doorsLayer;
        // Escenario de Hollow Knight (Visuals/HollowScenery), con los dos estilos; null si no se ha importado.
        Visuals.HollowScenery scenery;
        bool sceneryLoaded;
        Visuals.HollowScenery Scenery
        {
            get
            {
                if (!sceneryLoaded) { sceneryLoaded = true; scenery = Visuals.HollowScenery.Load(); }
                return scenery;
            }
        }
        MapData lastMap;
        RoomWorld lastWorld;

        // Tiles del estilo con otro color (cambio de paleta): los tilemaps copian el color al poner el tile.
        public void RefreshStyle()
        {
            if (front != null) front.RefreshAllTiles();
            if (rim != null) rim.RefreshAllTiles();
        }

        // Pone o quita el estilo sin rehacer el mapa (se conservan piedras, puertas y paredes abiertas).
        public void SetStyle(StyleTiles style, StyleTiles placeholders)
        {
            Style = style;
            if (front == null) return;
            foreach (var ((room, x, y), visible) in new Dictionary<(RoomModel, int, int), bool>(shownRocks)) ShowRock(room, x, y, visible);
            foreach (var ((room, x, y), open) in new Dictionary<(RoomModel, int, int), int>(shownDoors)) ShowDoor(room, x, y, open);
            foreach (var (room, x, y) in new List<(RoomModel, int, int)>(shownRevolving)) ShowRevolvingDoor(room, x, y);
            back.GetComponent<TilemapRenderer>().enabled = style == null || Scenery != null;
            rim.ClearAllTiles();
            if (lastMap != null)
                foreach (var data in lastMap.rooms)
                    if (!data.room.shrine)
                        RedrawCells(lastWorld, data.room, 0, 0, RoomCoords.Cols, RoomCoords.Rows);
            // Las puertas con llave tapan su columna de la colisión: después de redibujarla.
            foreach (var ((room, x, y), h) in new Dictionary<(RoomModel, int, int), int>(shownKeyDoors)) ShowKeyDoor(room, x, y, h);
        }

        public void Build(MapData map, RoomWorld world)
        {
            lastMap = map;
            lastWorld = world;
            shownRocks.Clear();
            shownKeyDoors.Clear();
            shownRevolving.Clear();
            shownDoors.Clear();
            if (grid != null)
                DestroyImmediate(grid.gameObject);
            Scenery?.Clear();
            Scenery?.UseMap(map.mapName);
            grid = new GameObject($"{map.mapName} (salas)").AddComponent<Grid>();
            grid.transform.SetParent(transform, false);
            grid.cellSize = new Vector3(RoomCoords.TileSize, RoomCoords.TileSize, 0);
            back = Layer("Fondo", -10);
            back.GetComponent<TilemapRenderer>().enabled = Style == null || Scenery != null;
            var doors = doorsLayer = Layer("Puertas", -8);
            front = Layer("Colisión", -6);
            rim = Layer("Bordes", -5);
            rocks = Layer("Piedras", -4);
            keyDoors = Layer("Puertas con llave", -3);

            foreach (var data in map.rooms)
            {
                var room = data.room;
                var bg = room.shrine ? shrine : Scenery != null ? null : ((room.gridX + room.gridY) & 1) == 0 ? background : backgroundAlt;
                for (int row = 0; row < RoomCoords.Rows; row++)
                    for (int col = 0; col < RoomCoords.Cols; col++)
                    {
                        var cell = RoomCoords.Cell(room, col, row);
                        back.SetTile(cell, bg);
                        var tile = TileAtCell(world, room, col, row);
                        if (tile != null && !room.shrine)
                            front.SetTile(cell, tile);
                        if (!room.shrine) UpdateRim(world, room, col, row);
                    }
                if (!room.shrine) Scenery?.BuildRoom(world, room, grid.transform);
            }
        }

        // Puerta con llave: columna de 1 x 3 tiles, cerrada hasta la altura dada (desde arriba).
        public void ShowKeyDoor(RoomModel room, int x, int y, int height)
        {
            shownKeyDoors[(room, x, y)] = height;
            Fill(front, room, x, y, 1, 3, null);
            Fill(keyDoors, room, x, y, 1, 3, null);
            // Con la verja de Hollow Knight: lo que se ve de ella mientras sube (sus 'height' casillas de abajo, arriba).
            if (Visuals.HollowItems.Piece("puerta_llave_3") != null)
            {
                Piece(("llave", room, x, y), room, x + 4, y + 8 * height, height > 0 ? Visuals.HollowItems.Piece($"puerta_llave_{height}") : null, -3);
                return;
            }
            if (Style?.KeyDoor != null) FillPattern(keyDoors, room, x, y, Style.KeyDoor, height);
            else Fill(keyDoors, room, x, y, 1, height, keyDoor);
        }

        // Vuelve a dibujar la capa de colisión de un rectángulo (en tiles) a partir del estado actual del mundo.
        public void RedrawCells(RoomWorld world, RoomModel room, int col, int row, int width, int height)
        {
            // Con el estilo, el tono del macizo depende de los vecinos: se redibuja también un margen de 2.
            int m = Style != null ? 2 : 0;
            for (int r = row - m; r < row + height + m; r++)
                for (int c = col - m; c < col + width + m; c++)
                    if (r >= 0 && r < RoomCoords.Rows && c >= 0 && c < RoomCoords.Cols)
                        front.SetTile(RoomCoords.Cell(room, c, r), TileAtCell(world, room, c, r));
            for (int r = row; r <= row + height; r++)
                for (int c = col; c < col + width; c++)
                    UpdateRim(world, room, c, r);
            if (!room.shrine && grid != null) Scenery?.BuildRoom(world, room, grid.transform);
        }

        // Borde de arriba: un sólido con aire (o escalera, o agua) encima.
        void UpdateRim(RoomWorld world, RoomModel room, int col, int row)
        {
            if (rim == null || row < 0 || row >= RoomCoords.Rows || col < 0 || col >= RoomCoords.Cols) return;
            // Sin borde sobre una piedra (es sólida, pero no es suelo de la sala).
            bool on = Style != null && Scenery == null && !RockAt(room, col, row) &&
                      world.TileAt(room, col * 8, OriginalPhysics.RoomTopPx + row * 8) == TileClass.Solid &&
                      world.TileAt(room, col * 8, OriginalPhysics.RoomTopPx + row * 8 - 8) != TileClass.Solid;
            rim.SetTile(RoomCoords.Cell(room, col, row), on ? Style.Rim : null);
        }

        // Puerta de mundo (4 × 3 tiles: dintel y un hueco de 2 × 2 entre dos jambas), con 'openRows' filas del hueco
        // abiertas desde abajo: 0 cerrada, 1 a medio abrir, 2 abierta (WorldDoors).
        public void ShowDoor(RoomModel room, int x, int y, int openRows)
        {
            shownDoors[(room, x, y)] = openRows;
            // Con la puerta de jefe de Hollow Knight en el hueco (2 × 2 entre las jambas): entera cerrada, rompiéndose a
            // medio abrir y nada abierta. El marco, el de siempre (abierto).
            if (Visuals.HollowItems.Piece("puerta_mundo_cerrada") != null)
            {
                // El marco: con los decorados de Hollow Knight (GameBootstrap.ShowDecor: arco y columnas) no se pinta.
                if (Resources.Load<Sprite>("Hollow/decorados/arco") != null) { }
                else if (Style?.Door != null) FillPattern(doorsLayer, room, x, y, Style.Door, 3);
                else Fill(doorsLayer, room, x, y, 4, 3, door);
                // Con la fachada de Hollow Knight (decorados/puerta_mundo) el hueco se deja abierto: el sello parecía una piedra
                // y no se veía que es una entrada (lo pidió el usuario). Se sigue sin poder pasar hasta que se abre.
                bool facade = Resources.Load<Sprite>("Hollow/decorados/puerta_mundo") != null;
                var gate = openRows >= 2 || facade ? null : Visuals.HollowItems.Piece(openRows == 1 ? "puerta_mundo_media" : "puerta_mundo_cerrada");
                Piece(("mundo", room, x, y), room, x + 16, y + 24, gate, -7);
                return;
            }
            if (Style?.Door != null)
            {
                var pattern = openRows >= 2 ? Style.Door : openRows == 1 ? Style.DoorHalf : Style.DoorClosed;
                FillPattern(doorsLayer, room, x, y, pattern ?? Style.Door, 3);
                return;
            }
            Fill(doorsLayer, room, x, y, 4, 3, door);
            // Sin estilo: el hueco cerrado con el tile de las puertas con llave.
            for (int r = 1; r <= 2 - openRows; r++)
                Fill(doorsLayer, room, x + 8, y + 8 * r, 2, 1, keyDoor);
        }

        // Sala oscura (mundo 5, sala 6, sin la Candle): los muros no se ven, pero siguen ahí.
        public void SetDark(RoomWorld world, RoomModel room, bool dark)
        {
            Scenery?.SetVisible(room, !dark);
            if (dark)
            {
                for (int r = 0; r < RoomCoords.Rows; r++)
                    for (int c = 0; c < RoomCoords.Cols; c++)
                        if (world.TileAt(room, c * 8, OriginalPhysics.RoomTopPx + r * 8) == TileClass.Solid)
                        {
                            front.SetTile(RoomCoords.Cell(room, c, r), null);
                            rim.SetTile(RoomCoords.Cell(room, c, r), null);
                        }
            }
            else
                RedrawCells(world, room, 0, 0, RoomCoords.Cols, RoomCoords.Rows);
        }

        // Puerta giratoria: columna de 1 x 3 tiles con su propio color, encima de la colisión.
        public void ShowRevolvingDoor(RoomModel room, int x, int y)
        {
            shownRevolving.Add((room, x, y));
            if (Visuals.HollowItems.Piece("puerta_giratoria") is Sprite rev)
            {
                Piece(("giratoria", room, x, y), room, x + 4, y + 24, rev, -3);
                return;
            }
            if (Style?.Revolving != null) FillPattern(keyDoors, room, x, y, Style.Revolving, 3);
            else Fill(keyDoors, room, x, y, 1, 3, revolvingDoor);
        }

        bool RockAt(RoomModel room, int col, int row)
        {
            foreach (var r in room.rocks)
            {
                int rc = r.x >> 3, rr = (r.y - OriginalPhysics.RoomTopPx) >> 3;
                if (col >= rc && col < rc + 2 && row >= rr && row < rr + 2 && shownRocks.TryGetValue((room, r.x, r.y), out bool v) && v) return true;
            }
            return false;
        }

        public void ShowRock(RoomModel room, int x, int y, bool visible)
        {
            shownRocks[(room, x, y)] = visible;
            int col0 = x >> 3, row0 = (y - OriginalPhysics.RoomTopPx) >> 3;
            if (lastWorld != null && Style != null)
                RedrawCells(lastWorld, room, col0, row0, 2, 2);
            // Con la roca de geo de Hollow Knight.
            if (Visuals.HollowItems.Piece("roca") is Sprite geo)
            {
                Fill(rocks, room, x, y, 2, 2, null);
                Fill(front, room, x, y, 2, 2, null);   // sin la baldosa sólida debajo (la piedra es la roca)
                Piece(("roca", room, x, y), room, x + 8, y + 16, visible ? geo : null, -4);
                return;
            }
            if (visible && Style?.Rock != null) FillPattern(rocks, room, x, y, Style.Rock, 2);
            else Fill(rocks, room, x, y, 2, 2, visible ? rock : null);
        }

        // Piezas de Hollow Knight (Visuals/HollowItems.Piece) como sprites encima del mapa: una por sitio, con el pivote abajo
        // en el centro en (px, py), píxeles del original. Sin sprite, se esconde. Cuelgan de la rejilla del mapa: al
        // reconstruirlo desaparecen y se vuelven a crear.
        readonly Dictionary<(string, RoomModel, int, int), SpriteRenderer> pieces = new Dictionary<(string, RoomModel, int, int), SpriteRenderer>();

        void Piece((string, RoomModel, int, int) key, RoomModel room, float px, float py, Sprite sprite, int order)
        {
            if (!pieces.TryGetValue(key, out var sr) || sr == null)
            {
                if (sprite == null) return;
                sr = new GameObject($"Pieza {key.Item1} ({room.id}: {key.Item3}, {key.Item4})").AddComponent<SpriteRenderer>();
                sr.transform.SetParent(grid != null ? grid.transform : transform, false);
                pieces[key] = sr;
            }
            sr.enabled = sprite != null;
            if (sprite == null) return;
            sr.sprite = sprite;
            sr.sortingOrder = order;
            var at = RoomCoords.PixelToWorld(room, 0, 0) + new Vector2(px, -py) / RoomCoords.PixelsPerUnit;
            sr.transform.position = new Vector3(at.x, at.y, 0);
        }

        // Dibujo de varias celdas: las 'rowsShown' filas de arriba del patrón.
        static void FillPattern(Tilemap tm, RoomModel room, int x, int y, TileBase[,] pattern, int rowsShown)
        {
            int col0 = x >> 3, row0 = (y - OriginalPhysics.RoomTopPx) >> 3;
            for (int r = 0; r < pattern.GetLength(0); r++)
                for (int c = 0; c < pattern.GetLength(1); c++)
                {
                    int rr = row0 + r, cc = col0 + c;
                    if (rr >= 0 && rr < RoomCoords.Rows && cc >= 0 && cc < RoomCoords.Cols)
                        tm.SetTile(RoomCoords.Cell(room, cc, rr), r < rowsShown ? pattern[r, c] : null);
                }
        }

        // Vacía la colisión dibujada de un hueco (al romper una piedra).
        public void ClearSolid(RoomModel room, int x, int y, int width, int height)
        {
            Fill(front, room, x, y, width, height, null);
            if (lastWorld != null)
                RedrawCells(lastWorld, room, x >> 3, (y - OriginalPhysics.RoomTopPx) >> 3, width, height + 1);
        }

        static void Fill(Tilemap tm, RoomModel room, int x, int y, int width, int height, TileBase tile)
        {
            int col0 = x >> 3, row0 = (y - OriginalPhysics.RoomTopPx) >> 3;
            for (int r = row0; r < row0 + height; r++)
                for (int c = col0; c < col0 + width; c++)
                    if (r >= 0 && r < RoomCoords.Rows && c >= 0 && c < RoomCoords.Cols)
                        tm.SetTile(RoomCoords.Cell(room, c, r), tile);
        }

        // Escalera de Hollow Knight (Assets/Art/Resources/Hollow/escalera, tools/hollow/objetos.py) en 4 baldosas, [fila
        // par/impar, columna izquierda/derecha], de las tres que eligió el usuario: en el castillo, barras de hierro; en
        // los mundos construidos (Canales, Ciudad, Confines, Palacio: 2, 5, 9 y 10), cadenas de campanas; en las cuevas,
        // cadenas con travesaños. Null si no se ha importado.
        public static string LadderVariant(string map) =>
            map == "castle" ? "hierro" : map is "world02" or "world05" or "world09" or "world10" ? "adorno" : "cadenas";
        readonly Dictionary<string, TileBase[,]> hollowLadders = new Dictionary<string, TileBase[,]>();
        TileBase[,] HollowLadder()
        {
            string variant = LadderVariant(lastMap != null ? lastMap.mapName : null);
            if (hollowLadders.TryGetValue(variant, out var cached)) return cached;
            var t = new TileBase[2, 2];
            string[] rows = { "arriba", "abajo" }, cols = { "izq", "der" };
            for (int r = 0; r < 2 && t != null; r++)
                for (int c = 0; c < 2; c++)
                {
                    var sp = Resources.Load<Sprite>($"Hollow/escalera/escalera_{variant}_{rows[r]}_{cols[c]}");
                    if (sp == null) { t = null; break; }
                    var tile = ScriptableObject.CreateInstance<Tile>();
                    tile.sprite = sp;
                    tile.colliderType = Tile.ColliderType.None;
                    t[r, c] = tile;
                }
            return hollowLadders[variant] = t;
        }

        TileBase TileAtCell(RoomWorld world, RoomModel room, int col, int row)
        {
            var cls = world.TileAt(room, col * 8, OriginalPhysics.RoomTopPx + row * 8);
            if (cls == TileClass.Ladder && HollowLadder() is TileBase[,] hl)
            {
                // Columna derecha: la que tiene escalera a su izquierda (y no dos). Filas alternas: arriba y abajo del trozo.
                bool right = world.TileAt(room, (col - 1) * 8, OriginalPhysics.RoomTopPx + row * 8) == TileClass.Ladder &&
                             world.TileAt(room, (col - 2) * 8, OriginalPhysics.RoomTopPx + row * 8) != TileClass.Ladder;
                return hl[row & 1, right ? 1 : 0];
            }
            // Con el escenario de Hollow Knight, el agua y la lava con su color (la superficie la pone HollowLiquids).
            if (Scenery != null && (cls == TileClass.Water || cls == TileClass.Lava))
            {
                int depth = 0;
                while (depth < Visuals.HollowScenery.LiquidDepths && row - depth - 1 >= 0 &&
                       world.TileAt(room, col * 8, OriginalPhysics.RoomTopPx + (row - depth - 1) * 8) == cls) depth++;
                return Scenery.Liquid(cls == TileClass.Lava, depth);
            }
            // Con el escenario de Hollow Knight, el sólido es negro (sus piezas visten los bordes); la piedra, su dibujo.
            if (Scenery != null && (cls == TileClass.Solid || cls == TileClass.BossPassable))
                return RockAt(room, col, row) ? null : Scenery.FillTile;
            // Escalera de 2 tiles: la mitad derecha es la que tiene escalera a su izquierda.
            if (Style?.LadderRight != null && cls == TileClass.Ladder &&
                world.TileAt(room, (col - 1) * 8, OriginalPhysics.RoomTopPx + row * 8) == TileClass.Ladder &&
                world.TileAt(room, (col - 2) * 8, OriginalPhysics.RoomTopPx + row * 8) != TileClass.Ladder)
                return Style.LadderRight;
            if (Style == null || cls != TileClass.Solid)
                return TileFor(cls);
            // La piedra es sólida, pero se ve la piedra y no el macizo de la sala.
            if (RockAt(room, col, row))
                return null;
            // Más oscuro cuanto más lejos del aire: el macizo se lee como masa y su borde como el sitio donde se pisa.
            for (int d = 1; d <= 2; d++)
                for (int dr = -d; dr <= d; dr++)
                    for (int dc = -d; dc <= d; dc++)
                    {
                        if (System.Math.Max(System.Math.Abs(dr), System.Math.Abs(dc)) != d) continue;
                        var t = world.TileAt(room, (col + dc) * 8, OriginalPhysics.RoomTopPx + (row + dr) * 8);
                        if (t != TileClass.Solid && t != TileClass.BossPassable)
                            return d == 1 ? Variant(Style.SolidVariants, Style.Solid, room, col, row) : Variant(Style.InnerVariants, Style.SolidInner, room, col, row);
                    }
            return Style.SolidDeep;
        }

        // Una variante por celda, siempre la misma para la misma celda (el tile base es la más frecuente).
        static TileBase Variant(TileBase[] variants, TileBase fallback, RoomModel room, int col, int row)
        {
            if (variants == null || variants.Length == 0) return fallback;
            unchecked
            {
                int h = (room.id * 73856093) ^ (col * 19349663) ^ (row * 83492791);
                h = (h >> 3) & 0x7fffffff;
                int k = h % (variants.Length * 3);
                return k < variants.Length ? variants[k] : fallback;
            }
        }

        TileBase TileFor(TileClass cls)
        {
            if (Style != null)
                switch (cls)
                {
                    case TileClass.Solid: return Style.Solid;
                    case TileClass.Ladder: return Style.Ladder;
                    case TileClass.Water: return Style.Water;
                    case TileClass.Lava: return Style.Lava;
                    case TileClass.BossPassable: return Style.BossPassable;
                    default: return null;
                }
            switch (cls)
            {
                case TileClass.Solid: return solid;
                case TileClass.Ladder: return ladder;
                case TileClass.Water: return water;
                case TileClass.Lava: return lava;
                case TileClass.BossPassable: return bossPassable;
                default: return null;
            }
        }

        Tilemap Layer(string name, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(grid.transform, false);
            var tm = go.AddComponent<Tilemap>();
            var r = go.AddComponent<TilemapRenderer>();
            r.sortingOrder = order;
            // Material iluminado (Sprite-Lit-Default de URP 2D, Resources/Materials/SpriteLit, lo crea RenderSetup): las luces 2D de Visuals/Atmosphere
            // alumbran la sala. Sin luces 2D (estilo quitado) la luz global es blanca y se ve igual que sin luz.
            var lit = Resources.Load<Material>("Materials/SpriteLit");
            if (lit != null) r.sharedMaterial = lit;
            return tm;
        }
    }
}
