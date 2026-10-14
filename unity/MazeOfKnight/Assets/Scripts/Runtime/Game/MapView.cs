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

        Grid grid;
        Tilemap front, rocks;

        public void Build(MapData map, RoomWorld world)
        {
            if (grid != null)
                DestroyImmediate(grid.gameObject);
            grid = new GameObject($"{map.mapName} (salas)").AddComponent<Grid>();
            grid.transform.SetParent(transform, false);
            grid.cellSize = new Vector3(RoomCoords.TileSize, RoomCoords.TileSize, 0);
            var back = Layer("Fondo", -10);
            var doors = Layer("Puertas", -8);
            front = Layer("Colisión", -5);
            rocks = Layer("Piedras", -4);

            foreach (var data in map.rooms)
            {
                var room = data.room;
                var bg = room.shrine ? shrine : ((room.gridX + room.gridY) & 1) == 0 ? background : backgroundAlt;
                for (int row = 0; row < RoomCoords.Rows; row++)
                    for (int col = 0; col < RoomCoords.Cols; col++)
                    {
                        var cell = RoomCoords.Cell(room, col, row);
                        back.SetTile(cell, bg);
                        var tile = TileFor(world.TileAt(room, col * 8, OriginalPhysics.RoomTopPx + row * 8));
                        if (tile != null && !room.shrine)
                            front.SetTile(cell, tile);
                    }
                if (room.doorWorld > 0)
                    Fill(doors, room, room.door.x, room.door.y, 4, 3, door);
            }
        }

        public void ShowRock(RoomModel room, int x, int y, bool visible) =>
            Fill(rocks, room, x, y, 2, 2, visible ? rock : null);

        // Vacía la colisión dibujada de un hueco (al romper una piedra).
        public void ClearSolid(RoomModel room, int x, int y, int width, int height) =>
            Fill(front, room, x, y, width, height, null);

        static void Fill(Tilemap tm, RoomModel room, int x, int y, int width, int height, TileBase tile)
        {
            int col0 = x >> 3, row0 = (y - OriginalPhysics.RoomTopPx) >> 3;
            for (int r = row0; r < row0 + height; r++)
                for (int c = col0; c < col0 + width; c++)
                    if (r >= 0 && r < RoomCoords.Rows && c >= 0 && c < RoomCoords.Cols)
                        tm.SetTile(RoomCoords.Cell(room, c, r), tile);
        }

        TileBase TileFor(TileClass cls)
        {
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
