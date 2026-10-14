using System.Collections.Generic;
using MazeOfKnight.Data;

namespace MazeOfKnight.Game
{
    // Consulta de tiles en coordenadas del original (píxeles de pantalla dentro de una sala), siguiendo los
    // enlaces a las salas vecinas cuando el punto cae fuera. Fuera de una sala sin vecino, todo es sólido.
    public class RoomWorld
    {
        readonly Dictionary<int, RoomModel> rooms = new Dictionary<int, RoomModel>();
        public bool BossFight;

        public RoomWorld(IEnumerable<RoomModel> models)
        {
            foreach (var m in models)
                rooms[m.id] = m;
        }

        public RoomModel Room(int id) => rooms.TryGetValue(id, out var r) ? r : null;

        // Enlace válido a otra sala de este mapa (las salidas al castillo, 0xF5-0xFE, no cuentan aquí).
        public RoomModel Neighbour(RoomModel room, int dx, int dy)
        {
            int link = dx < 0 ? room.left : dx > 0 ? room.right : dy < 0 ? room.up : room.down;
            return Room(link);
        }

        public TileClass TileAt(RoomModel room, int px, int py)
        {
            // Salta a la sala vecina si el punto está fuera (una sala de distancia como mucho).
            if (px < 0) { room = Neighbour(room, -1, 0); px += OriginalPhysics.RoomWidthPx; }
            else if (px >= OriginalPhysics.RoomWidthPx) { room = Neighbour(room, 1, 0); px -= OriginalPhysics.RoomWidthPx; }
            if (room != null && py < OriginalPhysics.RoomTopPx) { room = Neighbour(room, 0, -1); py += 160; }
            else if (room != null && py >= OriginalPhysics.RoomBottomPx) { room = Neighbour(room, 0, 1); py -= 160; }
            if (room == null || room.shrine)
                return TileClass.Solid;

            int row = (py - OriginalPhysics.RoomTopPx) >> 3, col = px >> 3;
            if (row < 0 || row >= room.collision.Length || col < 0 || col >= 32)
                return TileClass.Solid;
            return TileClasses.FromChar(room.collision[row][col]);
        }

        public bool Solid(RoomModel room, int px, int py)
        {
            var t = TileAt(room, px, py);
            return t == TileClass.Solid || (t == TileClass.BossPassable && !BossFight);
        }

        public bool Ladder(RoomModel room, int px, int py) => TileAt(room, px, py) == TileClass.Ladder;
    }
}
