using MazeOfKnight.Data;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MazeOfKnight.Game
{
    // Dibuja todas las salas de un mapa con los tiles provisionales: una capa de fondo y una por clase de
    // colisión. Las salas se colocan en su posición de la rejilla (RoomCoords), así que pasar de sala es
    // solo mover la cámara.
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

        Grid grid;

        public void Build(MapData map)
        {
            if (grid != null)
                Destroy(grid.gameObject);
            grid = new GameObject($"{map.mapName} (salas)").AddComponent<Grid>();
            grid.transform.SetParent(transform, false);
            grid.cellSize = new Vector3(RoomCoords.TileSize, RoomCoords.TileSize, 0);
            var back = Layer("Fondo", -10);
            var front = Layer("Colisión", -5);

            foreach (var data in map.rooms)
            {
                var room = data.room;
                var bg = room.shrine ? shrine : ((room.gridX + room.gridY) & 1) == 0 ? background : backgroundAlt;
                for (int row = 0; row < RoomCoords.Rows; row++)
                    for (int col = 0; col < RoomCoords.Cols; col++)
                    {
                        var cell = RoomCoords.Cell(room, col, row);
                        back.SetTile(cell, bg);
                        var tile = TileFor(data.GetTile(col, row));
                        if (tile != null)
                            front.SetTile(cell, tile);
                    }
            }
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
