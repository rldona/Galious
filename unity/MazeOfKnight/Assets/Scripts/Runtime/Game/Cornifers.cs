using MazeOfKnight.Data;
using System.Collections.Generic;
using System.Linq;

namespace MazeOfKnight.Game
{
    // RemakeRules.HollowMaps: Cornifer, el cartógrafo de Hollow Knight, uno en cada mundo con mapa (1–9; el 10 no tiene
    // mapa en el original) y otro en el castillo (0; lo pidió el usuario: sin comprarle el mapa solo se ve lo visitado).
    // Conseguir el mapa tiene que costar (lo pidió el usuario): en los mundos está en la sala más alejada de la de entrada,
    // contando salas; en el castillo (146 salas: en la más alejada ya se habría recorrido todo), como el de los Cruces
    // Olvidados de Hollow Knight, a media distancia: la más alejada a no más de CastleMaxDistance salas de la de inicio, sin banco, sin santuario y que no sea la del jefe, con suelo para él (Width casillas,
    // Benches.Spot, lejos de donde aparecen enemigos). Vende el mapa del mundo (el objeto 0x0A de ese
    // mundo) por Price(mundo) geo.
    public class Cornifers
    {
        public const int Width = 6;               // casillas de suelo bajo él
        public const int Air = 5;                 // filas de aire encima que se prefieren (mide 31 px); como poco 3
        public const int Reach = 28;              // px de su centro a los que se le puede hablar
        public const int MinDistance = 2;
        public const int CastleMinDistance = 6, CastleMaxDistance = 10;

        public static int Price(int world) => 20 + 10 * world;   // de 30 a 110 geo: tiene que costar

        public struct Spot
        {
            public int World, Room, X, Y, Distance;   // Distance: salas desde la entrada
        }

        readonly List<Spot> all = new List<Spot>();
        public IReadOnlyList<Spot> All => all;

        public Cornifers(IDictionary<string, RoomWorld> worlds, System.Func<int, int> bossRoom, Benches benches, int castleStartRoom = 1)
        {
            for (int world = 0; world <= 9; world++)
            {
                int start = world == 0 ? castleStartRoom : 1;
                if (!worlds.TryGetValue(Benches.MapName(world), out var w) || w.Room(start) == null) continue;
                // Distancia en salas desde la entrada.
                var dist = new Dictionary<int, int> { [start] = 0 };
                var queue = new Queue<int>();
                queue.Enqueue(start);
                while (queue.Count > 0)
                {
                    int id = queue.Dequeue();
                    if (!(w.Room(id) is RoomModel r)) continue;
                    foreach (int n in new[] { r.right, r.left, r.up, r.down })
                        if (n > 0 && w.Room(n) != null && !dist.ContainsKey(n)) { dist[n] = dist[id] + 1; queue.Enqueue(n); }
                }
                // La sala válida más lejana; a igual distancia, mejor sin enemigos (Benches.Quiet), luego con más aire
                // encima (de Air filas a 3, que le deja pegado al techo) y luego la de número menor. En una sala con enemigos, lejos de donde aparecen (GameBootstrap quita los que salgan a su lado).
                Spot? best = null;
                int bestScore = -1;
                foreach (var kv in dist.OrderBy(kv => kv.Key))
                {
                    var room = w.Room(kv.Key);
                    if (kv.Value < MinDistance || room.shrine || kv.Key == bossRoom(world)) continue;
                    if (world == 0 && (kv.Value < CastleMinDistance || kv.Value > CastleMaxDistance || room.doorWorld > 0)) continue;
                    if (benches != null && benches.Find(Benches.MapName(world), kv.Key) != null) continue;
                    for (int air = Air; air >= 3; air--)
                    {
                        (int, int)? at = null;
                        for (int m = 2; m >= 0 && at == null; m--) at = Benches.Spot(w, room, m, Width, air);
                        if (!(at is (int x, int y))) continue;
                        int score = kv.Value * 8 + (Benches.Quiet(room) ? 4 : 0) + (air - 3);
                        if (score > bestScore) { bestScore = score; best = new Spot { World = world, Room = kv.Key, X = x, Y = y, Distance = kv.Value }; }
                        break;
                    }
                }
                if (best is Spot b) all.Add(b);
            }
        }

        public Spot? In(int world, int room)
        {
            foreach (var c in all) if (c.World == world && c.Room == room) return c;
            return null;
        }

        public Spot? At(int world, int room, int x, int y)
        {
            if (In(world, room) is Spot c && y == c.Y && System.Math.Abs(x - c.X) <= Reach) return c;
            return null;
        }
    }
}
