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
            public TileBase Solid, SolidInner, SolidDeep, Rim, Ladder, Water, Lava, BossPassable;
        }

        public StyleTiles Style;

        Grid grid;
        Tilemap back, front, rim, rocks, keyDoors, doorsLayer;
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
            back.GetComponent<TilemapRenderer>().enabled = style == null;
            rim.ClearAllTiles();
            if (lastMap != null)
                foreach (var data in lastMap.rooms)
                    if (!data.room.shrine)
                        RedrawCells(lastWorld, data.room, 0, 0, RoomCoords.Cols, RoomCoords.Rows);
        }

        public void Build(MapData map, RoomWorld world)
        {
            lastMap = map;
            lastWorld = world;
            if (grid != null)
                DestroyImmediate(grid.gameObject);
            grid = new GameObject($"{map.mapName} (salas)").AddComponent<Grid>();
            grid.transform.SetParent(transform, false);
            grid.cellSize = new Vector3(RoomCoords.TileSize, RoomCoords.TileSize, 0);
            back = Layer("Fondo", -10);
            back.GetComponent<TilemapRenderer>().enabled = Style == null;
            var doors = doorsLayer = Layer("Puertas", -8);
            front = Layer("Colisión", -6);
            rim = Layer("Bordes", -5);
            rocks = Layer("Piedras", -4);
            keyDoors = Layer("Puertas con llave", -3);

            foreach (var data in map.rooms)
            {
                var room = data.room;
                var bg = room.shrine ? shrine : ((room.gridX + room.gridY) & 1) == 0 ? background : backgroundAlt;
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
                if (room.doorWorld > 0)
                    Fill(doors, room, room.door.x, room.door.y, 4, 3, door);
            }
        }

        // Puerta con llave: columna de 1 x 3 tiles, cerrada hasta la altura dada (desde arriba).
        public void ShowKeyDoor(RoomModel room, int x, int y, int height)
        {
            Fill(front, room, x, y, 1, 3, null);
            Fill(keyDoors, room, x, y, 1, 3, null);
            Fill(keyDoors, room, x, y, 1, height, keyDoor);
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
        }

        // Borde de arriba: un sólido con aire (o escalera, o agua) encima.
        void UpdateRim(RoomWorld world, RoomModel room, int col, int row)
        {
            if (rim == null || row < 0 || row >= RoomCoords.Rows || col < 0 || col >= RoomCoords.Cols) return;
            bool on = Style != null &&
                      world.TileAt(room, col * 8, OriginalPhysics.RoomTopPx + row * 8) == TileClass.Solid &&
                      world.TileAt(room, col * 8, OriginalPhysics.RoomTopPx + row * 8 - 8) != TileClass.Solid;
            rim.SetTile(RoomCoords.Cell(room, col, row), on ? Style.Rim : null);
        }

        // Puerta de mundo que aparece en juego (la del mundo 10).
        public void ShowDoor(RoomModel room, int x, int y) => Fill(doorsLayer, room, x, y, 4, 3, door);

        // Sala oscura (mundo 5, sala 6, sin la Candle): los muros no se ven, pero siguen ahí.
        public void SetDark(RoomWorld world, RoomModel room, bool dark)
        {
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
        public void ShowRevolvingDoor(RoomModel room, int x, int y) => Fill(keyDoors, room, x, y, 1, 3, revolvingDoor);

        public void ShowRock(RoomModel room, int x, int y, bool visible) =>
            Fill(rocks, room, x, y, 2, 2, visible ? rock : null);

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

        TileBase TileAtCell(RoomWorld world, RoomModel room, int col, int row)
        {
            var cls = world.TileAt(room, col * 8, OriginalPhysics.RoomTopPx + row * 8);
            if (Style == null || cls != TileClass.Solid)
                return TileFor(cls);
            // Más oscuro cuanto más lejos del aire: el macizo se lee como masa y su borde como el sitio donde se pisa.
            for (int d = 1; d <= 2; d++)
                for (int dr = -d; dr <= d; dr++)
                    for (int dc = -d; dc <= d; dc++)
                    {
                        if (System.Math.Max(System.Math.Abs(dr), System.Math.Abs(dc)) != d) continue;
                        var t = world.TileAt(room, (col + dc) * 8, OriginalPhysics.RoomTopPx + (row + dr) * 8);
                        if (t != TileClass.Solid && t != TileClass.BossPassable)
                            return d == 1 ? Style.Solid : Style.SolidInner;
                    }
            return Style.SolidDeep;
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
            go.AddComponent<TilemapRenderer>().sortingOrder = order;
            return tm;
        }
    }
}
