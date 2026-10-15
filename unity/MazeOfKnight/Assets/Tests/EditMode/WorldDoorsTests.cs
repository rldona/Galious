using System.Collections.Generic;
using System.Linq;
using MazeOfKnight.Data;
using MazeOfKnight.Game;
using NUnit.Framework;

namespace MazeOfKnight.Tests
{
    // Puertas de mundo del castillo comparadas con lo medido en openMSX (sala 104, tools/openmsx/door_open_measure.py).
    // Sala sintética: suelo en la fila 11 (y = 120) y la puerta en (64, 96), así que se entra desde y = 120.
    public class WorldDoorsTests
    {
        static (WorldDoors doors, PlayerMotor motor) Setup(int x)
        {
            var rows = new string[20];
            for (int r = 0; r < 20; r++) rows[r] = new string(r >= 11 ? '#' : '.', 32);
            var room = new RoomModel { id = 104, collision = rows, doorWorld = 1, door = new PointJson { x = 64, y = 96 } };
            var motor = new PlayerMotor(new RoomWorld(new[] { room }), room, x, 120);
            return (new WorldDoors(new HashSet<int>(), new HashSet<int>()), motor);
        }

        [Test]
        public void ClosedWithoutKey()
        {
            var (doors, motor) = Setup(80);
            for (int t = 0; t < 40; t++) doors.Tick(motor, true);
            Assert.IsFalse(doors.IsOpen(1));
            Assert.AreEqual(0, doors.OpeningWorld);
        }

        [Test]
        public void KeysUnlockNextWorld()
        {
            var (doors, _) = Setup(80);
            doors.KeyTaken(0);
            doors.KeyTaken(4);
            CollectionAssert.AreEquivalent(new[] { 1, 5 }, doors.Unlocked.ToArray());
            doors.KeyTaken(9);
            Assert.IsTrue(doors.Unlocked.Contains(10));
        }

        [Test]
        public void OpensInThreeStepsOfSixTicks()
        {
            var (doors, motor) = Setup(80);
            doors.KeyTaken(0);
            var stages = new List<(int tick, int rows)>();
            int opened = -1, t = 0;
            doors.Stage += (w, rows) => stages.Add((t, rows));
            doors.DoorOpened += w => opened = t;
            for (t = 0; t < 30; t++) doors.Tick(motor, true);
            // Empieza en cuanto está delante; fila de abajo a los 6 ticks, la de arriba a los 12, abierta a los 18.
            CollectionAssert.AreEqual(new[] { (0, 0), (6, 1), (12, 2) }, stages);
            Assert.AreEqual(18, opened);
            Assert.IsTrue(doors.IsOpen(1));
            Assert.IsFalse(doors.Unlocked.Contains(1));
        }

        [Test]
        public void OnlyFromTheEntrySpot()
        {
            foreach (int x in new[] { 76, 83, 64, 100 })
            {
                var (doors, motor) = Setup(x);
                doors.KeyTaken(0);
                doors.Tick(motor, true);
                Assert.AreEqual(0, doors.OpeningWorld, $"x = {x}");
            }
            foreach (int x in new[] { 78, 82 })
            {
                var (doors, motor) = Setup(x);
                doors.KeyTaken(0);
                doors.Tick(motor, true);
                Assert.AreEqual(1, doors.OpeningWorld, $"x = {x}");
            }
        }

        [Test]
        public void KeepsOpeningIfThePlayerWalksOn()
        {
            var (doors, motor) = Setup(80);
            doors.KeyTaken(0);
            doors.Tick(motor, true);
            motor.Place(motor.Room, 40, 120);
            for (int t = 0; t < 20; t++) doors.Tick(motor, true);
            Assert.IsTrue(doors.IsOpen(1));
        }

        [Test]
        public void NotInWorlds()
        {
            var (doors, motor) = Setup(80);
            doors.KeyTaken(0);
            for (int t = 0; t < 30; t++) doors.Tick(motor, false);
            Assert.IsFalse(doors.IsOpen(1));
        }
    }
}
