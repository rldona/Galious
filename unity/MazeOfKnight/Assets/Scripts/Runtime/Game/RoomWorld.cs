using System.Collections.Generic;
using MazeOfKnight.Data;

namespace MazeOfKnight.Game
{
    // Consulta de tiles en coordenadas del original (píxeles de pantalla dentro de una sala), siguiendo los
    // enlaces a las salas vecinas cuando el punto cae fuera. Fuera de una sala sin vecino, todo es sólido.
    // Guarda una copia de la colisión de cada sala para poder cambiarla en juego (piedras rotas) sin tocar
    // los assets importados.
    public class RoomWorld
    {
        readonly Dictionary<int, RoomModel> rooms = new Dictionary<int, RoomModel>();
        readonly Dictionary<int, char[][]> grids = new Dictionary<int, char[][]>();
        public bool BossFight;

        public RoomWorld(IEnumerable<RoomModel> models)
        {
            foreach (var m in models)
            {
                rooms[m.id] = m;
                var grid = new char[m.collision.Length][];
                for (int r = 0; r < grid.Length; r++)
                    grid[r] = m.collision[r].ToCharArray();
                grids[m.id] = grid;
            }
        }

        public IEnumerable<RoomModel> Rooms => rooms.Values;

        public RoomModel Room(int id) => rooms.TryGetValue(id, out var r) ? r : null;

        // Salida al castillo (0xF5-0xFE) en una dirección de la sala, o 0.
        public static int CastleExit(RoomModel room, int dx, int dy)
        {
            int link = Link(room, dx, dy);
            return link >= 0xF5 && link <= 0xFE ? link : 0;
        }

        static int Link(RoomModel room, int dx, int dy) =>
            dx < 0 ? room.left : dx > 0 ? room.right : dy < 0 ? room.up : room.down;

        // Enlace a otra sala de este mapa (las salidas al castillo no cuentan aquí).
        public RoomModel Neighbour(RoomModel room, int dx, int dy) => Room(Link(room, dx, dy));

        public TileClass TileAt(RoomModel room, int px, int py)
        {
            // Salta a la sala vecina si el punto está fuera (una sala de distancia como mucho). Más allá de una
            // salida al castillo el paso está libre.
            int dx = px < 0 ? -1 : px >= OriginalPhysics.RoomWidthPx ? 1 : 0;
            if (dx != 0)
            {
                if (CastleExit(room, dx, 0) != 0) return TileClass.Empty;
                room = Neighbour(room, dx, 0);
                px -= dx * OriginalPhysics.RoomWidthPx;
            }
            int dy = py < OriginalPhysics.RoomTopPx ? -1 : py >= OriginalPhysics.RoomBottomPx ? 1 : 0;
            if (room != null && dy != 0)
            {
                if (CastleExit(room, 0, dy) != 0) return TileClass.Empty;
                room = Neighbour(room, 0, dy);
                py -= dy * 160;
            }
            // Sala de dios de un mundo: sin disposición propia; de momento no se entra (docs/world_maps.md).
            if (room == null || room.shrine)
                return TileClass.Solid;

            int row = (py - OriginalPhysics.RoomTopPx) >> 3, col = px >> 3;
            var grid = grids[room.id];
            if (row < 0 || row >= grid.Length || col < 0 || col >= 32)
                return TileClass.Solid;
            return TileClasses.FromChar(grid[row][col]);
        }

        public bool Solid(RoomModel room, int px, int py)
        {
            var t = TileAt(room, px, py);
            return t == TileClass.Solid || (t == TileClass.BossPassable && !BossFight);
        }

        public bool Ladder(RoomModel room, int px, int py) => TileAt(room, px, py) == TileClass.Ladder;

        // Vacía un rectángulo de tiles de una sala (por ejemplo, una piedra rota).
        public void Clear(RoomModel room, int col, int row, int width, int height) =>
            Fill(room, col, row, width, height, '.');

        // Copia un rectángulo de una cuadrícula de la sala importada (por ejemplo, la colisión sin escaleras extra).
        public void CopyFrom(RoomModel room, string[] source, int col, int row, int width, int height)
        {
            var grid = grids[room.id];
            for (int r = row; r < row + height && r < grid.Length && r < source.Length; r++)
                for (int c = col; c < col + width && c < 32; c++)
                    if (r >= 0 && c >= 0)
                        grid[r][c] = source[r][c];
        }

        public void SetSolid(RoomModel room, int col, int row, int width, int height) =>
            Fill(room, col, row, width, height, '#');

        public void SetLadder(RoomModel room, int col, int row, int width, int height) =>
            Fill(room, col, row, width, height, 'H');

        void Fill(RoomModel room, int col, int row, int width, int height, char value)
        {
            var grid = grids[room.id];
            for (int r = row; r < row + height && r < grid.Length; r++)
                for (int c = col; c < col + width && c < 32; c++)
                    if (r >= 0 && c >= 0)
                        grid[r][c] = value;
        }
    }
}
