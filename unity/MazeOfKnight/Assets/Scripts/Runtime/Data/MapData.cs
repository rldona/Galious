using System.Collections.Generic;
using UnityEngine;

namespace MazeOfKnight.Data
{
    // El castillo o un mundo: sus salas y los datos del jefe.
    public class MapData : ScriptableObject
    {
        public string mapName;
        public int e041;
        public int bossRoom;
        public string bossWord;
        public List<RoomData> rooms = new List<RoomData>();

        public RoomData Find(int id) => rooms.Find(r => r.room.id == id);
    }
}
