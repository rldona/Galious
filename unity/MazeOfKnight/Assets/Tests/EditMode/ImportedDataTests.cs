using System.Linq;
using MazeOfKnight.Data;
using MazeOfKnight.Game;
using MazeOfKnight.Game.Enemies;
using NUnit.Framework;
using UnityEngine;

namespace MazeOfKnight.Tests
{
    // Comprobaciones con las salas importadas (Resources/Maps, generadas desde la ROM del usuario). Si no se ha
    // importado nada, se saltan.
    public class ImportedDataTests
    {
        internal static RoomWorld WorldFor(string name) => World(Load(name));

        static MapData Load(string name)
        {
            var map = Resources.Load<MapData>($"Maps/{name}");
            if (map == null)
                Assert.Ignore("Sin salas importadas (Maze of Knight > Importar salas)");
            return map;
        }

        static RoomWorld World(MapData map) => new RoomWorld(map.rooms.Select(r => r.room));

        static void AssertStands(RoomWorld world, RoomModel room, int x, int y, string what)
        {
            var motor = new PlayerMotor(world, room, x, y);
            for (int t = 0; t < 20; t++)
                motor.Tick(new MotorInput());
            Assert.AreEqual(MotorState.Ground, motor.State, what);
            Assert.AreEqual(y, motor.Y, what);
            Assert.AreEqual(room.id, motor.Room.id, what);
        }

        [Test]
        public void WorldEntriesStandOnTheFloor()
        {
            for (int k = 1; k <= 9; k++)
            {
                var world = World(Load(OriginalWorlds.MapName(k)));
                var (x, y) = OriginalWorlds.Entry[k];
                AssertStands(world, world.Room(1), x, y, $"entrada del mundo {k}");
            }
        }

        [Test]
        public void CastleDoorsAreReachableAndReturnStandsOnTheFloor()
        {
            var castle = World(Load("castle"));
            for (int k = 1; k <= 9; k++)
            {
                var room = castle.Rooms.Single(r => r.doorWorld == k);
                int x = room.door.x + OriginalWorlds.DoorExitDx, y = room.door.y + OriginalWorlds.DoorExitDy;
                AssertStands(castle, room, x, y, $"vuelta del mundo {k}");
                var motor = new PlayerMotor(castle, room, x, y);
                int entered = 0;
                motor.DoorEntered += w => entered = w;
                motor.Tick(new MotorInput { Down = true });
                Assert.AreEqual(k, entered, $"puerta del mundo {k}");
            }
        }

        [Test]
        public void KeyDoorsAreSolidColumns()
        {
            int n = 0;
            foreach (var name in new[] { "castle" }.Concat(Enumerable.Range(1, 10).Select(OriginalWorlds.MapName)))
            {
                var world = World(Load(name));
                foreach (var room in world.Rooms)
                    foreach (var o in room.objects.Where(o => (o.type & 0x0F) == 1 || (o.type & 0x0F) == 2))
                    {
                        n++;
                        for (int dy = 0; dy < 24; dy += 8)
                            Assert.IsTrue(world.Solid(room, o.x, o.y + dy), $"{name} sala {room.id} puerta ({o.x},{o.y})");
                    }
            }
            Assert.AreEqual(15 + 34, n);
        }

        [Test]
        public void LaddersAtTheTopEdgeLeadIntoTheRoomAbove()
        {
            int n = 0;
            var stuck = new System.Collections.Generic.List<string>();
            foreach (var name in new[] { "castle" }.Concat(Enumerable.Range(1, 10).Select(OriginalWorlds.MapName)))
            {
                var world = World(Load(name));
                foreach (var room in world.Rooms)
                {
                    var above = world.Room(room.up);
                    if (above == null || room.shrine || above.shrine)
                        continue;
                    for (int c = 0; c < 31; c++)
                    {
                        if (room.collision[0][c] != 'H' || room.collision[0][c + 1] != 'H' || (c > 0 && room.collision[0][c - 1] == 'H'))
                            continue;
                        n++;
                        var motor = new PlayerMotor(world, room, c * 8 + 8, 48);
                        motor.Tick(new MotorInput { Up = true });
                        for (int t = 0; t < 60 && motor.Room == room; t++)
                            motor.Tick(new MotorInput { Up = true });
                        if (motor.Room != above)
                            stuck.Add($"{name} sala {room.id} col {c} (y {motor.Y}, {motor.State})");
                    }
                }
            }
            Assert.Greater(n, 0);
            Assert.IsEmpty(stuck, string.Join("; ", stuck));
        }

        // Toda la caja (cada 7 px, con 1 px de margen) sobre algún sólido que no sea una piedra: los generadores
        // ponen a veces al enemigo encima de una piedra, como en el original (castillo, salas 12, 58 y 117).
        static bool InsideWall(RoomWorld world, RoomModel room, Enemy e)
        {
            for (int dy = 1; dy < e.Height - 1; dy += 7)
                for (int dx = 1; dx < e.Width - 1; dx += 7)
                {
                    int px = (int)e.X + dx, py = (int)e.Y + dy;
                    bool rock = room.rocks.Any(r => px >= r.x && px < r.x + 16 && py >= r.y && py < r.y + 16);
                    if (!rock && world.Solid(room, px, py))
                        return true;
                }
            return false;
        }

        [Test]
        public void JumpAlongTheTopEdgeKeepsMovingSideways()
        {
            // Castillo, sala 73: plataformas en y = 56 pegadas al marcador, hueco de x 64 a 95. Al saltar la cabeza
            // choca arriba (y = 37) pero el salto sigue avanzando y llega a la plataforma de la derecha.
            var world = WorldFor("castle");
            var room = world.Room(73);
            var m = new PlayerMotor(world, room, 60, 56);
            for (int t = 0; t < 30 && !(t > 3 && m.State == MotorState.Ground); t++)
                m.Tick(new MotorInput { Right = true, Up = true });
            Assert.AreEqual(MotorState.Ground, m.State);
            Assert.AreEqual(56, m.Y, "en la plataforma de la derecha, no en el piso de abajo");
            Assert.GreaterOrEqual(m.X, 92);
        }

        // Los que andan por las plataformas: nunca deben meterse en un muro ni salir de la sala.
        static readonly System.Type[] Walkers =
        {
            typeof(Skeleton), typeof(Roller), typeof(Charger), typeof(Snake), typeof(Hopper), typeof(Knight),
            typeof(Runner), typeof(Soldier), typeof(Dart), typeof(Lurker), typeof(Hedgehog), typeof(Armadillo),
            typeof(Emerger), typeof(Sentinel), typeof(SlowCharger), typeof(LevelJumper), typeof(InchWorm),
            typeof(WideWalker), typeof(GrowingBall),
        };

        [Test]
        public void ImplementedEnemiesRunInEveryRoom()
        {
            var catalog = Resources.Load<EnemyCatalog>("Enemies");
            if (catalog == null)
                Assert.Ignore("Sin catálogo de enemigos importado");
            int rooms = 0, enemies = 0;
            var problems = new System.Collections.Generic.List<string>();
            foreach (var name in new[] { "castle" }.Concat(Enumerable.Range(1, 10).Select(OriginalWorlds.MapName)))
            {
                var world = World(Load(name));
                foreach (var room in world.Rooms.Where(r => r.spawns.Any(s => EnemySystem.Implemented(s.type))))
                {
                    rooms++;
                    var motor = new PlayerMotor(world, room, 128, 120);
                    var sys = new EnemySystem(catalog.enemies, room.id);
                    sys.EnterRoom(world, room, motor);
                    if (sys.Missing > 0) problems.Add($"{name} {room.id}: {sys.Missing} sin implementar");
                    enemies += sys.Enemies.Count;
                    int before = problems.Count;
                    for (int t = 0; t < 600; t++)
                    {
                        sys.Tick();
                        // El jugador va y viene por la sala para que reaccionen.
                        motor.Place(room, 24 + (t * 2) % 208, 120);
                        foreach (var e in sys.Enemies.Where(e => e.Alive && !e.ThroughFloors && Walkers.Contains(e.GetType())))
                        {
                            if (e.X < -1 || e.X + e.Width > 257 || e.Y < 31 || e.Y >= 0xB8)   // b06:B452 los borra desde y = 0xB8
                                problems.Add($"{name} {room.id} 0x{e.Type:X2} t{t} fuera de la sala ({e.X:0},{e.Y:0})");
                            else if (InsideWall(world, room, e))
                                problems.Add($"{name} {room.id} 0x{e.Type:X2} t{t} dentro de un muro ({e.X:0},{e.Y:0}) {e}");
                        }
                        if (problems.Count > before) break;
                    }
                }
            }
            Assert.Greater(enemies, 400);
            Assert.IsEmpty(problems, string.Join("; ", problems.Take(15)));
            TestContext.WriteLine($"{rooms} salas, {enemies} enemigos simulados 600 ticks");
        }

        [Test]
        public void RocksSitOnSolidCells()
        {
            var castle = World(Load("castle"));
            int n = 0;
            foreach (var room in castle.Rooms)
                foreach (var r in room.rocks)
                {
                    n++;
                    for (int dy = 0; dy < 16; dy += 8)
                        for (int dx = 0; dx < 16; dx += 8)
                            Assert.IsTrue(castle.Solid(room, r.x + dx, r.y + dy), $"piedra sala {room.id} ({r.x},{r.y})");
                }
            Assert.AreEqual(245, n);
        }
    }
}
