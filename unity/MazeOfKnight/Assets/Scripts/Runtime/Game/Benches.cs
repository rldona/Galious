using System.Collections.Generic;
using MazeOfKnight.Data;
using P = MazeOfKnight.Game.OriginalPhysics;

namespace MazeOfKnight.Game
{
    // RemakeRules.HollowBenches: los bancos de Galious: The Hollow Kingdom, como los de Hollow Knight. Uno en la sala donde
    // se empieza en el castillo, uno en la sala de entrada de cada mundo y otro en la sala de al lado de la del jefe (la de
    // la izquierda, la de la derecha, la de arriba o la de abajo, la primera con sitio). Su sitio en la sala sale de la
    // colisión: un suelo de 4 casillas con 3 filas de aire encima (sin escaleras ni líquidos), lo más cerca del centro.
    // Como en Hollow Knight, en un sitio tranquilo (lo pidió el usuario): solo en salas sin enemigos (ni colocados ni de
    // generador) y lejos de escaleras, puertas, objetos y paredes que se abren (Margin casillas a cada lado). Si la sala
    // elegida no vale, la más cercana que sí, hasta MaxDistance salas.
    // Sentarse (↓ encima) rellena todas las máscaras, guarda la partida y lo marca como sitio de reaparición.
    public class Benches
    {
        public const int Width = 32;                 // px: 4 casillas
        public const int Reach = 12;                 // px del centro del banco a los que se puede sentar
        public const int Margin = 3;                 // casillas libres de escaleras y objetos a cada lado
        public const int MaxDistance = 5;
        public const int ClearX = 72, ClearY = 40;   // px alrededor del banco donde no aparecen enemigos

        // Sala sin enemigos: ni colocados ni de generador.
        public static bool Quiet(RoomModel room) => room.spawns.Length == 0 && room.periodic.Length == 0;

        public struct Bench
        {
            public string Map;
            public int Room, X, Y;                   // centro del asiento y altura del suelo (los pies)
        }

        readonly List<Bench> all = new List<Bench>();
        public IReadOnlyList<Bench> All => all;

        // Mapa de cada mundo (0 = castillo).
        public static string MapName(int world) => world == 0 ? "castle" : OriginalWorlds.MapName(world);
        public static int WorldOf(string map) => map == "castle" ? 0 : int.TryParse(map.Substring(5), out int w) ? w : 0;

        // worlds: el mundo de cada mapa cargado; bossRoom(w): sala del jefe de ese mundo.
        public Benches(IDictionary<string, RoomWorld> worlds, int castleStartRoom, System.Func<int, int> bossRoom)
        {
            // enemies: también en una sala con enemigos (el de antes del jefe): lejos de donde aparecen (Spot) y GameBootstrap
            // quita los que aparezcan cerca (salvo los que cuentan para abrir una pared al matarlos a todos).
            bool Add(string map, int roomId, bool enemies = false)
            {
                if (!worlds.TryGetValue(map, out var w)) return false;
                var room = w.Room(roomId);
                if (room == null || room.shrine || all.Exists(b => b.Map == map && b.Room == roomId)) return false;
                if (roomId == bossRoom(WorldOf(map))) return false;          // nunca en la sala del jefe
                if (!enemies && !Quiet(room)) return false;
                // Con Margin casillas libres a cada lado o, si no cabe, con menos (nunca pegado: 1 como poco).
                (int, int)? at = null;
                for (int m = Margin; m >= 1 && at == null; m--) at = Spot(w, room, m);
                if (!(at is (int x, int y))) return false;
                all.Add(new Bench { Map = map, Room = roomId, X = x, Y = y });
                return true;
            }
            // En esa sala o, si no tiene sitio, en la más cercana que lo tenga, hasta 3 salas de distancia (la entrada del
            // mundo 10 es lava con piedras para saltar).
            void AddNear(string map, int roomId)
            {
                if (!worlds.TryGetValue(map, out var w)) return;
                var seen = new HashSet<int> { roomId };
                var frontier = new List<int> { roomId };
                for (int depth = 0; depth <= MaxDistance && frontier.Count > 0; depth++)
                {
                    foreach (int id in frontier) if (Add(map, id)) return;
                    var next = new List<int>();
                    foreach (int id in frontier)
                        if (w.Room(id) is RoomModel r)
                            foreach (int n in new[] { r.right, r.left, r.up, r.down })
                                if (n > 0 && w.Room(n) != null && seen.Add(n)) next.Add(n);
                    frontier = next;
                }
            }
            AddNear("castle", castleStartRoom);
            for (int world = 1; world <= 10; world++)
            {
                string map = MapName(world);
                if (!worlds.TryGetValue(map, out var w)) continue;
                AddNear(map, 1);
                // Antes del jefe: una sala vecina de la suya (lo eligió el usuario), aunque tenga enemigos; primero las
                // tranquilas.
                if (w.Room(bossRoom(world)) is RoomModel boss)
                {
                    var near = new[] { boss.left, boss.right, boss.up, boss.down };
                    bool done = false;
                    foreach (int n in near) if (!done && n > 0 && w.Room(n) != null) done = Add(map, n);
                    foreach (int n in near) if (!done && n > 0 && w.Room(n) != null) done = Add(map, n, true);
                }
            }
        }

        public IEnumerable<Bench> In(string map, int room)
        {
            foreach (var b in all)
                if (b.Map == map && b.Room == room) yield return b;
        }

        public Bench? Find(string map, int room) { foreach (var b in In(map, room)) return b; return null; }

        // El banco de esa sala al alcance de los pies (x, y), o null.
        public Bench? At(string map, int room, int x, int y)
        {
            foreach (var b in In(map, room))
                if (y == b.Y && System.Math.Abs(x - b.X) <= Reach) return b;
            return null;
        }

        // Un suelo de 'tiles' casillas (4, un banco) con 'air' filas de aire encima (3), lo más cerca del centro de la sala.
        public static (int x, int y)? Spot(RoomWorld world, RoomModel room, int margin = Margin, int tiles = 4, int air = 3)
        {
            bool Air(int c, int r) => world.TileAt(room, c * 8, P.RoomTopPx + r * 8) == TileClass.Empty;
            bool Floor(int c, int r) => world.TileAt(room, c * 8, P.RoomTopPx + r * 8) == TileClass.Solid && !Rock(c, r);
            bool Rock(int c, int r)
            {
                foreach (var k in room.rocks)
                {
                    int kc = k.x >> 3, kr = (k.y - P.RoomTopPx) >> 3;
                    if (c >= kc && c < kc + 2 && r >= kr && r < kr + 2) return true;
                }
                return false;
            }
            // Casillas con algo con lo que no debe estar pegado: escaleras, puertas, objetos de sala y la pared que se abre.
            var busy = new HashSet<(int, int)>();
            for (int r = 0; r < RoomCoords.Rows; r++)
                for (int c = 0; c < RoomCoords.Cols; c++)
                    if (world.TileAt(room, c * 8, P.RoomTopPx + r * 8) == TileClass.Ladder) busy.Add((c, r));
            void Mark(int x, int y, int w, int h)
            {
                for (int r = 0; r < h; r++)
                    for (int c = 0; c < w; c++) busy.Add(((x >> 3) + c, ((y - P.RoomTopPx) >> 3) + r));
            }
            foreach (var o in room.objects) Mark(o.x, o.y, 4, 3);
            foreach (var l in room.ladders) Mark(l.x, l.y, 2, l.height);
            if (room.doorWorld > 0 && room.door != null) Mark(room.door.x, room.door.y, 4, 3);
            if (room.hasItem) Mark(room.item.x, room.item.y, 2, 2);
            if (room.wallBlock != null && room.wallBlock.openedBy != 0) Mark(room.wallBlock.col * 8, P.RoomTopPx + room.wallBlock.row * 8, 4, 3);
            // Encima del suelo, nada a menos de 'margin' casillas; debajo, nada justo bajo el banco (una escalera que baja
            // desde el suelo a unas casillas no estorba al sentarse).
            bool Clear(int c0, int r)
            {
                // Lejos de donde aparecen los enemigos de la sala.
                foreach (var e in room.spawns)
                    if (System.Math.Abs(e.x + 8 - (c0 * 8 + tiles * 4)) < ClearX + tiles * 4 && System.Math.Abs(e.y + 8 - (P.RoomTopPx + r * 8 - 8)) < ClearY)
                        return false;
                for (int c = c0 - margin; c < c0 + tiles + margin; c++)
                    for (int rr = r - 4; rr < r; rr++)
                        if (busy.Contains((c, rr))) return false;
                for (int c = c0; c < c0 + tiles; c++)
                    if (busy.Contains((c, r)) || busy.Contains((c, r + 1))) return false;
                return true;
            }
            (int, int)? best = null;
            int bestD = int.MaxValue;
            for (int r = air; r < RoomCoords.Rows; r++)
                for (int c = 1; c + tiles <= RoomCoords.Cols - 1; c++)
                {
                    bool ok = true;
                    for (int i = 0; i < tiles && ok; i++)
                    {
                        ok = Floor(c + i, r);
                        for (int a = 1; a <= air && ok; a++) ok = Air(c + i, r - a);
                    }
                    if (!ok || !Clear(c, r)) continue;
                    int cx = c * 8 + tiles * 4, d = System.Math.Abs(cx - 128) + System.Math.Abs(r - 12) * 4;
                    if (d < bestD) { bestD = d; best = (cx, P.RoomTopPx + r * 8); }
                }
            return best;
        }
    }
}
