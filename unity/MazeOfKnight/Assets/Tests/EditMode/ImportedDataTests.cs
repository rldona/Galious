using System.Linq;
using MazeOfKnight.Data;
using MazeOfKnight.Game;
using NUnit.Framework;
using UnityEngine;

namespace MazeOfKnight.Tests
{
    // Comprobaciones con las salas importadas (Resources/Maps, generadas desde la ROM del usuario). Si no se ha
    // importado nada, se saltan.
    public class ImportedDataTests
    {
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
