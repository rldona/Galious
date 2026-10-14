using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MazeOfKnight.Data;
using MazeOfKnight.Game;
using UnityEngine;
using P = MazeOfKnight.Game.OriginalPhysics;

namespace MazeOfKnight.EditorTools.Walkthrough
{
    // Prueba de partida completa: ¿se puede acabar el juego del remake? Recorre los mapas con ReachSearch (el motor del
    // jugador con todas las teclas posibles) y encima lleva la progresión del original (docs/design_bible/01_mundo.md,
    // "Progresión"): objetos, tiendas, piedras, paredes, eventos de sala, lápidas, jefes, Grandes Llaves y puertas de
    // mundo, hasta tocar al bebé en la sala 6 del mundo 10. Repite hasta que no se consigue nada nuevo.
    // Supuestos optimistas [I], porque no hay enemigos en la búsqueda:
    //   - los enemigos marcados de una sala se pueden matar si se llega a la sala, y los jefes si se llega a su sala con
    //     la palabra leída (Galious, además, con la Cross);
    //   - llaves, monedas y flechas no faltan (se sacan de los enemigos y de las tiendas);
    //   - las piedras y paredes que se alcanzan con la espada se rompen, y lo que se alcanzó antes de romperlas sigue
    //     valiendo;
    //   - los muros que se cierran a la espalda (clase 5) no se cierran; las escaleras que se retiran están puestas.
    public class Walkthrough
    {
        public class MapRun
        {
            public string Name;
            public int World;                    // 0 castillo
            public MapData Map;
            public Dictionary<int, RoomModel> Rooms = new Dictionary<int, RoomModel>();
            public readonly HashSet<(int, int, int)> BrokenRocks = new HashSet<(int, int, int)>();
            public readonly HashSet<(int, int, int)> BrokenWalls = new HashSet<(int, int, int)>();
            public readonly HashSet<int> FiredEvents = new HashSet<int>();
            public readonly HashSet<int> Reached = new HashSet<int>();
            public readonly HashSet<(int, int, int)> TakenItems = new HashSet<(int, int, int)>();
            public bool Stones;
            public string LastSignature;
            public int Runs, States;
            public long Millis;
            public bool Truncated;
            public ReachSearch LastSearch;
            // Lo visto en la última búsqueda.
            public readonly HashSet<(int, int, int)> HitRocks = new HashSet<(int, int, int)>();
            public readonly HashSet<(int, int, int)> HitWalls = new HashSet<(int, int, int)>();
            public readonly HashSet<int> MarkedReached = new HashSet<int>();
            public bool StonesShore, BossRoomReached;
        }

        readonly Dictionary<string, MapRun> runs = new Dictionary<string, MapRun>();
        readonly ItemsData items;
        readonly ShrinesData shrines;
        readonly WeaponsData weapons;
        readonly LiftsData lifts;

        public readonly HashSet<int> Have = new HashSet<int>();
        public readonly HashSet<int> Bosses = new HashSet<int>();
        public readonly HashSet<int> Words = new HashSet<int>();
        public readonly HashSet<int> DoorsReached = new HashSet<int>();
        public readonly HashSet<int> WorldsLeft = new HashSet<int>();      // se ha llegado a una salida al castillo
        public readonly HashSet<string> Shrines = new HashSet<string>();
        public readonly HashSet<int> World10Doors = new HashSet<int>();
        public bool CastleGreatKey, Ending;
        public readonly List<string> Log = new List<string>();
        int progress;

        public Walkthrough()
        {
            items = Load<ItemsData>("Items") ?? new ItemsData();
            shrines = Load<ShrinesData>("Shrines") ?? new ShrinesData();
            weapons = Load<WeaponsData>("Weapons") ?? new WeaponsData();
            lifts = Load<LiftsData>("Lifts") ?? new LiftsData();
            foreach (var m in Resources.LoadAll<MapData>("Maps"))
            {
                var r = new MapRun { Name = m.mapName, Map = m, World = m.mapName == "castle" ? 0 : m.e041 - 1 };
                foreach (var rd in m.rooms)
                    r.Rooms[rd.room.id] = JsonUtility.FromJson<RoomModel>(JsonUtility.ToJson(rd.room));
                runs[m.mapName] = r;
            }
        }

        static T Load<T>(string name) where T : class
        {
            var t = Resources.Load<TextAsset>(name);
            return t != null ? JsonUtility.FromJson<T>(t.text) : null;
        }

        public bool HasData => runs.ContainsKey("castle");
        public IEnumerable<MapRun> Runs => runs.Values.OrderBy(r => r.World);

        void Gain(string what)
        {
            progress++;
            Log.Add(what);
        }

        void GainItem(int type, string where)
        {
            if (Have.Add(type)) Gain($"{ItemName(type)} (0x{type:X2}) — {where}");
        }

        public string ItemName(int type) => items.Get(type)?.name ?? $"objeto 0x{type:X2}";

        static string MapLabel(int world) => world == 0 ? "castillo" : $"mundo {world}";

        // ---------- progresión ----------

        public bool DoorUnlocked(int w) => w == 1 ? CastleGreatKey : Bosses.Contains(w - 1);

        bool Allowed(int world, Character c)
        {
            if (world == 0) return true;
            var inv = new Inventory(items, Have.ToList());
            return inv.WorldEntryBlocked(world, c) == null;
        }

        public bool CanEnter(int w) =>
            DoorUnlocked(w) && DoorsReached.Contains(w) && runs.ContainsKey(OriginalWorlds.MapName(w)) &&
            (Allowed(w, Character.Popolon) || Allowed(w, Character.Aphrodite));

        public void Run(int maxPasses = 40)
        {
            for (int pass = 0; pass < maxPasses; pass++)
            {
                int before = progress;
                Explore(runs["castle"]);
                for (int w = 1; w <= 10; w++)
                    if (CanEnter(w))
                        Explore(runs[OriginalWorlds.MapName(w)]);
                if (progress == before) break;
            }
        }

        string Signature(MapRun r) =>
            string.Join(",", Have.OrderBy(i => i)) + "|" + string.Join(",", Bosses.OrderBy(i => i)) + "|" +
            string.Join(",", Words.OrderBy(i => i)) + "|" + string.Join(",", WorldsLeft.OrderBy(i => i)) + "|" +
            string.Join(",", DoorsReached.OrderBy(i => i)) + "|" + r.BrokenRocks.Count + "|" + r.BrokenWalls.Count + "|" +
            string.Join(",", r.FiredEvents.OrderBy(i => i)) + "|" + r.Stones + "|" + string.Join(",", Shrines.OrderBy(s => s));

        void Explore(MapRun r)
        {
            for (int round = 0; round < 30; round++)
            {
                string sig = Signature(r);
                if (sig == r.LastSignature) return;
                r.LastSignature = sig;
                Search(r);
                bool changed = false;
                foreach (var k in r.HitRocks)
                    if (r.BrokenRocks.Add(k)) changed = true;
                foreach (var k in r.HitWalls)
                    if (r.BrokenWalls.Add(k)) changed = true;
                foreach (int room in r.MarkedReached)
                    if (r.FiredEvents.Add(room)) changed = true;
                // Mundo 3, sala 4: subir por el pozo, que enlaza hacia arriba consigo mismo.
                if (r.World == 3 && r.LastSearch.Transitions.Contains((4, 4, 1)) && r.FiredEvents.Add(-1))
                {
                    changed = true;
                    Gain("mundo 3: el pozo de la sala 4 abre su muro");
                }
                if (r.StonesShore && !r.Stones) { r.Stones = true; changed = true; }
                if (r.World > 0 && r.LastSearch.CastleExits.Count > 0 && WorldsLeft.Add(r.World))
                    Gain($"{MapLabel(r.World)}: se puede volver al castillo");
                foreach (int w in r.LastSearch.DoorsEntered)
                    if (DoorsReached.Add(w)) Gain($"castillo: se llega a la puerta del mundo {w}");
                if (r.World > 0 && r.BossRoomReached && Words.Contains(r.World) && !Bosses.Contains(r.World) &&
                    (r.World != 10 || Have.Contains(0x0B)))
                {
                    Bosses.Add(r.World);
                    Gain($"{MapLabel(r.World)}: jefe vencido ({r.Map.bossWord})" + (r.World < 10 ? $"; su Gran Llave abre la puerta del mundo {r.World + 1}" : ""));
                    if (r.World == 10)
                        foreach (var room in r.Rooms.Values.Where(x => x.afterBossRight >= 0))
                            room.right = room.afterBossRight;
                    changed = true;
                }
                if (!changed && sig == Signature(r)) return;
            }
        }

        // ---------- una búsqueda ----------

        class DoorInfo { public int X, Y, Cls, Type; }

        void Search(MapRun r)
        {
            var sw = Stopwatch.StartNew();
            var world = BuildWorld(r);
            r.HitRocks.Clear();
            r.HitWalls.Clear();
            r.MarkedReached.Clear();
            r.StonesShore = r.BossRoomReached = false;

            var doors = new Dictionary<int, List<DoorInfo>>();
            foreach (var room in r.Rooms.Values)
                foreach (var o in room.objects)
                {
                    int cls = o.type & 0x0F;
                    if (cls < 1 || cls > 4) continue;
                    if (!doors.TryGetValue(room.id, out var l)) doors[room.id] = l = new List<DoorInfo>();
                    l.Add(new DoorInfo { X = o.x, Y = o.y, Cls = cls, Type = o.type });
                }

            var search = new ReachSearch { CharacterAllowed = c => Allowed(r.World, c) };
            search.Macros = m => DoorMacros(m, doors);
            var inspector = new Inspector(this, r, world);
            search.Visit = inspector.Visit;
            search.Run(Starts(r, world));
            r.LastSearch = search;
            r.Runs++;
            r.States = search.States;
            r.Truncated |= search.Truncated;
            r.Millis += sw.ElapsedMilliseconds;
            UnityEngine.Debug.Log($"[Walkthrough] {r.Name} búsqueda {r.Runs}: {search.States:N0} estados, {sw.ElapsedMilliseconds} ms, {r.Reached.Count} salas");
        }

        IEnumerable<PlayerMotor> DoorMacros(PlayerMotor m, Dictionary<int, List<DoorInfo>> doors)
        {
            if (m.State != MotorState.Ground || !doors.TryGetValue(m.Room.id, out var list)) yield break;
            foreach (var d in list)
            {
                if (m.Y != d.Y + 0x18) continue;
                int dir = m.X == d.X - 8 ? 1 : m.X == d.X + 16 ? -1 : 0;
                if (dir == 0) continue;
                if (d.Cls == 3 && dir != ((d.Type & 0x10) != 0 ? 1 : -1)) continue;
                if (d.Cls == 4 && m.Character == Character.Aphrodite && !Have.Contains(RoomObjects.ItemBreadAndWater)) continue;
                var c = ReachSearch.Copy(m);
                if (d.Cls <= 2) c.Carry(dir, P.KeyDoorOpenTicks, P.KeyDoorCarryTicks, P.KeyDoorCarrySpeed);
                else c.Carry(dir, 0, RoomObjects.RevolvingCarryTicks, 1);
                yield return c;
            }
        }

        RoomWorld BuildWorld(MapRun r)
        {
            // Mundo 10: la puerta sale en una de 4 salas al azar; se ponen las 4 para ver que se llega a todas.
            if (r.World == 0 && Bosses.Contains(9))
                for (int i = 0; i < OriginalWorlds.World10DoorRoom.Length; i++)
                {
                    var room = r.Rooms[OriginalWorlds.World10DoorRoom[i]];
                    room.doorWorld = 10;
                    room.door = new PointJson { x = OriginalWorlds.World10DoorPos[i].x, y = OriginalWorlds.World10DoorPos[i].y };
                }
            var world = new RoomWorld(r.Rooms.Values);
            foreach (var (room, x, y) in r.BrokenRocks)
                world.Clear(r.Rooms[room], x >> 3, (y - P.RoomTopPx) >> 3, 2, 2);
            foreach (var (room, x, y) in r.BrokenWalls)
                world.Clear(r.Rooms[room], x >> 3, (y - P.RoomTopPx) >> 3, 4, 3);
            foreach (var room in r.Rooms.Values)
            {
                var l = room.eventLadder;
                if (l != null && l.height > 0 && r.FiredEvents.Contains(l.openedBy))
                    world.SetLadder(room, l.col, l.row, 2, l.height);
                var w = room.wallBlock;
                if (w != null && w.openedBy != 0 && r.FiredEvents.Contains(w.openedBy))
                    world.Clear(room, w.col, w.row, 4, 3);
            }
            if (r.World == 5 && Have.Contains(Inventory.Oar))
                foreach (var id in new[] { 12, 13 })
                    if (r.Rooms.TryGetValue(id, out var room))
                        for (int row = 0; row < room.collision.Length; row++)
                            for (int col = 0; col < 32; col++)
                                if (room.collision[row][col] == '~' && (row == 0 || room.collision[row - 1][col] != '~'))
                                    world.SetSolid(room, col, row, 1, 1);
            if (r.World == 8 && Have.Contains(Inventory.Carpet))
                foreach (var room in r.Rooms.Values.Where(x => x.collision.Any(row => row.Contains('L'))))
                    world.SetSolid(room, 0, 12, 32, 8);
            // Piedras del agua (mundo 5, sala 11): salen de una en una; aquí, las cuatro a la vez [I].
            if (r.Stones)
                foreach (var room in r.Rooms.Values.Where(x => x.objects.Any(o => (o.type & 0x0F) == 6)))
                    foreach (int x in new[] { 0x38, 0x60, 0x90, 0xB8 })
                        world.SetSolid(room, x / 8, (0x78 - P.RoomTopPx) / 8, 2, 2);
            return world;
        }

        List<PlayerMotor> Starts(MapRun r, RoomWorld world)
        {
            var list = new List<PlayerMotor>();
            LiftSystem NewLifts() => r.World == 0 ? new LiftSystem(lifts) { InCastle = true } : null;
            void Add(int room, int x, int y)
            {
                var model = world.Room(room);
                if (model == null) return;
                foreach (var c in new[] { Character.Popolon, Character.Aphrodite })
                    list.Add(new PlayerMotor(world, model, x, y) { Character = c, Lifts = NewLifts() });
            }
            if (r.World == 0)
            {
                Add(1, 192, 120);
                // Vuelta de un mundo (y el Ala): delante de su puerta.
                foreach (int w in WorldsLeft)
                {
                    var room = r.Rooms.Values.FirstOrDefault(x => x.doorWorld == w);
                    if (room != null) Add(room.id, room.door.x + OriginalWorlds.DoorExitDx, room.door.y + OriginalWorlds.DoorExitDy);
                }
                // Angel Ring: el santuario de Demeter (sala 72).
                var demeter = shrines.shrines.FirstOrDefault(s => s.map == "castle" && s.kind == "password");
                if (Have.Contains(Inventory.AngelRing) && demeter != null) Add(demeter.exit.toRoom, demeter.exit.x, demeter.exit.y);
            }
            else
            {
                var (x, y) = OriginalWorlds.Entry[r.World];
                Add(1, x, y);
            }
            return list;
        }

        // Lo que se mira en cada estado de la búsqueda (tiene que ser rápido: se llama millones de veces).
        class Inspector
        {
            readonly Walkthrough w;
            readonly MapRun r;
            readonly RoomWorld world;
            readonly Dictionary<int, List<(int x, int y)>> rocks = new Dictionary<int, List<(int, int)>>();
            readonly Dictionary<int, List<(int x, int y)>> walls = new Dictionary<int, List<(int, int)>>();
            readonly Dictionary<int, List<Tombstone>> tombs = new Dictionary<int, List<Tombstone>>();
            readonly Dictionary<int, List<Shrine>> shrineByRoom = new Dictionary<int, List<Shrine>>();
            readonly MagnifierInfo glass;
            readonly bool pendant, harp, glassOwned;

            public Inspector(Walkthrough w, MapRun r, RoomWorld world)
            {
                this.w = w;
                this.r = r;
                this.world = world;
                foreach (var room in r.Rooms.Values)
                {
                    foreach (var k in room.rocks)
                        if (!r.BrokenRocks.Contains((room.id, k.x, k.y))) Add(rocks, room.id, (k.x, k.y));
                    foreach (var o in room.objects)
                        if ((o.type & 0x0F) == 7 && !r.BrokenWalls.Contains((room.id, o.x, o.y))) Add(walls, room.id, (o.x, o.y));
                }
                glass = w.weapons.magnifier;
                foreach (var t in glass.tombstones)
                    if (t.kind != 0 && t.e041 == r.Map.e041) Add(tombs, t.room, t);
                foreach (var s in w.shrines.shrines)
                    if (s.map == r.Name) Add(shrineByRoom, s.state == 16 ? s.room : s.entry.fromRoom, s);
                pendant = w.Have.Contains(RoomObjects.ItemPendant);
                harp = w.Have.Contains(Inventory.Harp);
                glassOwned = w.Have.Contains(6);
            }

            static void Add<T>(Dictionary<int, List<T>> d, int k, T v)
            {
                if (!d.TryGetValue(k, out var l)) d[k] = l = new List<T>();
                l.Add(v);
            }

            int lastRoom = -1;

            public void Visit(PlayerMotor m)
            {
                var room = m.Room;
                if (room.id != lastRoom)
                {
                    lastRoom = room.id;
                    if (r.Reached.Add(room.id)) { }
                    if (room.markedRoom) r.MarkedReached.Add(room.id);
                    if (r.World > 0 && room.id == r.Map.bossRoom) r.BossRoomReached = true;
                    if (r.World == 10 && room.id == 6 && !w.Ending) { w.Ending = true; w.Gain("mundo 10: sala del bebé (final)"); }
                }
                else if (room.markedRoom) r.MarkedReached.Add(room.id);
                bool ground = m.State == MotorState.Ground;
                bool canSwing = m.State != MotorState.Ladder && m.State != MotorState.Carried;

                // Piedras: la espada (o el arpa, al primer espadazo de la sala).
                if (canSwing && rocks.TryGetValue(room.id, out var rl))
                {
                    var (x0, x1, y0, y1) = m.SwordRect();
                    foreach (var (x, y) in rl)
                        if (harp || !(x1 < x || x0 > x + 15 || y1 < y || y0 > y + 15))
                            r.HitRocks.Add((room.id, x, y));
                }
                // Paredes que se rompen (clase 7): Popolon, o Aphrodite con el Pendant.
                if (canSwing && walls.TryGetValue(room.id, out var wl) && (m.Character == Character.Popolon || pendant))
                    foreach (var (x, y) in wl)
                    {
                        if (m.Y <= y || m.Y > y + 24) continue;
                        int gap = m.FacingLeft ? (m.X + P.FootLeft) - (x + 31) : x - (m.X + P.FootRight);
                        if (gap >= 0 && gap < RoomObjects.BreakableReach) r.HitWalls.Add((room.id, x, y));
                    }
                // Objeto de la sala (a la vista o tras romper su piedra).
                if (room.hasItem && (room.item.x != 0 || room.item.y != 0) && Touches(m, room.item.x, room.item.y) &&
                    !(rocks.TryGetValue(room.id, out var under) && under.Contains((room.item.x, room.item.y))))
                    w.GainItem(room.item.type, $"{MapLabel(r.World)}, sala {room.id}");
                if (r.World == 0 && room.hasItem && room.item.type == Inventory.GreatKey && !w.CastleGreatKey &&
                    w.Have.Contains(Inventory.GreatKey))
                {
                    w.CastleGreatKey = true;
                    w.Gain("castillo: la Gran Llave abre la puerta del mundo 1");
                }
                if (r.World == 0) CastleSpots(m, room, ground);
                // Santuarios y tiendas.
                if (shrineByRoom.TryGetValue(room.id, out var sl))
                    foreach (var s in sl)
                        if (InShrine(m, s)) w.VisitShrine(s);
                // Lápida con la palabra (lupa).
                if (glassOwned && tombs.TryGetValue(room.id, out var tl))
                    foreach (var t in tl)
                    {
                        int dy = m.Y + glass.playerDy - t.y, dx = m.X + glass.playerDx - t.x;
                        if (dy >= glass.dyMin && dy <= glass.dyMax && dx >= glass.dxMin && dx <= glass.dxMax && w.Words.Add(r.World))
                            w.Gain($"{MapLabel(r.World)}: palabra leída en la lápida de la sala {room.id} ({r.Map.bossWord})");
                    }
                // Piedras del agua: esperar en una orilla (pies en y = 0x78).
                if (r.World == 5 && ground && m.Y == 0x78 && room.objects.Any(o => (o.type & 0x0F) == 6) &&
                    (m.X >= 0x18 && m.X < 0x20 || m.X >= 0xE0 && m.X < 0xE8))
                    r.StonesShore = true;
                if (m.AtDoor() && room.doorWorld == 10) w.World10Doors.Add(room.id);
            }

            static bool Touches(PlayerMotor m, int ix, int iy) =>
                !(m.X + P.FootRight < ix || m.X + P.FootLeft > ix + 15 || m.Y - P.HeadHeight > iy + 15 || m.Y <= iy);

            bool InShrine(PlayerMotor m, Shrine s)
            {
                var e = s.entry;
                if (s.state == 16)
                    return m.State == MotorState.Ground && m.Room.id == s.room && m.X >= e.xMin && m.X <= e.xMax &&
                           m.Y >= e.yMin && m.Y <= e.yMax && (e.needsItem == 0 || w.Have.Contains(e.needsItem)) &&
                           !(rocks.TryGetValue(s.room, out var l) && l.Contains((e.godX, e.godY)));
                return m.Room.id == e.fromRoom && m.Y < e.exitBelowY && Math.Abs(m.X - e.x) <= 8 &&
                       world.Ladder(m.Room, e.x, P.RoomTopPx + 2);
            }

            // Objetos con condición del castillo (CastleItems): la condición y luego tocar el objeto donde sale.
            void CastleSpots(PlayerMotor m, RoomModel room, bool ground)
            {
                foreach (var s in CastleItems.Spots)
                {
                    if (s.Room != room.id || w.Have.Contains(s.Type)) continue;
                    bool ok;
                    switch (s.Room)
                    {
                        case 48:
                        case 75: ok = r.FiredEvents.Contains(s.Room); break;
                        case 24: ok = r.FiredEvents.Contains(s.Room) && w.Bosses.Contains(6); break;
                        case 40: ok = w.Bosses.Contains(4); break;          // 3 + 3 espadazos y 3 saltos donde sea
                        case 43: ok = r.Reached.Contains(-43); if (m.OnLift) r.Reached.Add(-43); break;   // → ↓ ← ↑ sobre el ascensor
                        default: ok = false; break;
                    }
                    if (ok && Touches(m, s.X, s.Y)) w.GainItem(s.Type, $"castillo, sala {s.Room} (con condición)");
                }
            }
        }

        void VisitShrine(Shrine s)
        {
            string key = $"{s.map}:{s.room}";
            if (!Shrines.Add(key)) return;
            Gain($"{(s.map == "castle" ? "castillo" : s.map)}: santuario de {s.god} (sala {s.room}, {s.kind})");
            foreach (var i in s.items)
                if (i.rule == "flag" || i.rule == "shield")
                    GainItem(i.type, $"tienda de {s.god} ({s.map}, sala {s.room})");
        }

        // ---------- informe ----------

        public string Report()
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Prueba de partida completa");
            sb.AppendLine();
            sb.AppendLine("Generado por `MazeOfKnight.EditorTools.Walkthrough` (menú **Maze of Knight → Prueba de partida completa**). " +
                          "Busca con el motor del jugador todos los estados alcanzables de cada mapa y lleva la progresión del original. " +
                          "Sin enemigos: los supone vencibles. Ver los supuestos en `Walkthrough.cs`.");
            sb.AppendLine();
            sb.AppendLine(Ending ? "**Resultado: se puede acabar el juego.**" : "**Resultado: NO se llega al final.**");
            sb.AppendLine();
            sb.AppendLine("## Progresión");
            sb.AppendLine();
            for (int i = 0; i < Log.Count; i++) sb.AppendLine($"{i + 1}. {Log[i]}");
            sb.AppendLine();
            sb.AppendLine("## Mapas");
            sb.AppendLine();
            sb.AppendLine("| Mapa | Salas alcanzadas | Sin alcanzar | Búsquedas | Estados (última) | Tiempo |");
            sb.AppendLine("|---|---|---|---|---|---|");
            foreach (var r in Runs)
            {
                var all = r.Rooms.Values.Where(x => !x.shrine).Select(x => x.id).ToList();
                var missing = all.Where(id => !r.Reached.Contains(id)).OrderBy(i => i).ToList();
                sb.AppendLine($"| {MapLabel(r.World)} | {all.Count - missing.Count}/{all.Count} | {(missing.Count == 0 ? "—" : string.Join(", ", missing))} | " +
                              $"{r.Runs} | {r.States:N0}{(r.Truncated ? " (cortada)" : "")} | {r.Millis / 1000.0:F1} s |");
            }
            sb.AppendLine();
            sb.AppendLine("## Lo que no se consigue");
            sb.AppendLine();
            foreach (var r in Runs)
            {
                if (r.Runs == 0) { sb.AppendLine($"- {MapLabel(r.World)}: no se entra."); continue; }
                foreach (var room in r.Rooms.Values.OrderBy(x => x.id))
                {
                    if (room.hasItem && (room.item.x != 0 || room.item.y != 0) && !Have.Contains(room.item.type) && room.item.type < 0x2D)
                        sb.AppendLine($"- {MapLabel(r.World)}, sala {room.id}: {ItemName(room.item.type)}");
                    foreach (var k in room.rocks)
                        if (!r.BrokenRocks.Contains((room.id, k.x, k.y)) && r.Reached.Contains(room.id))
                            sb.AppendLine($"- {MapLabel(r.World)}, sala {room.id}: piedra en ({k.x}, {k.y}) sin alcanzar con la espada");
                    foreach (var o in room.objects.Where(o => (o.type & 0x0F) == 7))
                        if (!r.BrokenWalls.Contains((room.id, o.x, o.y)))
                            sb.AppendLine($"- {MapLabel(r.World)}, sala {room.id}: pared que se rompe en ({o.x}, {o.y}) sin alcanzar");
                }
                if (r.World > 0 && !Bosses.Contains(r.World))
                    sb.AppendLine($"- {MapLabel(r.World)}: jefe sin vencer (sala del jefe {(r.BossRoomReached ? "alcanzada" : "sin alcanzar")}, palabra {(Words.Contains(r.World) ? "leída" : "sin leer")})");
                if (r.World > 0 && !WorldsLeft.Contains(r.World))
                    sb.AppendLine($"- {MapLabel(r.World)}: no se llega a ninguna salida al castillo");
            }
            foreach (var s in shrines.shrines)
                if (!Shrines.Contains($"{s.map}:{s.room}"))
                    sb.AppendLine($"- {(s.map == "castle" ? "castillo" : s.map)}: santuario de {s.god} (sala {s.room}, {s.kind}) sin alcanzar");
            for (int i = 0; i < OriginalWorlds.World10DoorRoom.Length; i++)
                if (Bosses.Contains(9) && !World10Doors.Contains(OriginalWorlds.World10DoorRoom[i]))
                    sb.AppendLine($"- castillo: la puerta del mundo 10 en la sala {OriginalWorlds.World10DoorRoom[i]} (E06E = {i}) no se alcanza");
            sb.AppendLine();
            sb.AppendLine("## No comprobado");
            sb.AppendLine();
            int closing = runs.Values.Sum(r => r.Rooms.Values.Sum(x => x.objects.Count(o => (o.type & 0x0F) == 5)));
            sb.AppendLine($"- Muros que se cierran a la espalda (clase 5, {closing} en total): la búsqueda los deja abiertos.");
            sb.AppendLine("- Enemigos, daño (salvo líquidos), llaves, monedas y flechas.");
            return sb.ToString();
        }
    }
}
