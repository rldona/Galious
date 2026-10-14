using UnityEngine;

namespace MazeOfKnight.Data
{
    // Una sala importada. La crea el importador (Maze of Knight > Importar salas) en Assets/Generated.
    public class RoomData : ScriptableObject
    {
        public string mapName;
        public RoomModel room;

        public TileClass GetTile(int col, int row, bool bossFight = false)
        {
            var rows = bossFight && room.collisionBoss.Length > 0 ? room.collisionBoss : room.collision;
            if (row < 0 || row >= rows.Length || col < 0 || col >= rows[row].Length)
                return TileClass.Empty;
            return TileClasses.FromChar(rows[row][col]);
        }
    }
}
