using System.Collections.Generic;
using System.Linq;
using MazeOfKnight.Data;
using MazeOfKnight.Game;
using NUnit.Framework;

namespace MazeOfKnight.Tests
{
    // Compara el motor con las curvas medidas en el original (docs/player.md, "Física del movimiento";
    // tools/openmsx/jump_measure.py). Las salas son sintéticas: no usan datos de la ROM.
    public class PlayerMotorTests
    {
        // Sala con suelo en la fila 11 (y = 120) y aire libre encima, como la sala 1 del castillo.
        static RoomModel FlatRoom(int id = 1, int floorRow = 11, int ceilingRow = -1, int ladderCol = -1)
        {
            var rows = new string[20];
            for (int r = 0; r < 20; r++)
            {
                char[] row = Enumerable.Repeat(r >= floorRow ? '#' : r == ceilingRow ? '#' : '.', 32).ToArray();
                if (ladderCol >= 0) { row[ladderCol] = 'H'; row[ladderCol + 1] = 'H'; }
                rows[r] = new string(row);
            }
            return new RoomModel { id = id, collision = rows };
        }

        static (PlayerMotor motor, List<int> dy, List<int> dx) Run(RoomModel room, Character c, int x, int y,
            int ticks, System.Func<int, MotorInput> input)
        {
            var motor = new PlayerMotor(new RoomWorld(new[] { room }), room, x, y) { Character = c };
            var dy = new List<int>();
            var dx = new List<int>();
            for (int t = 0; t < ticks; t++)
            {
                int y0 = motor.Y, x0 = motor.X;
                motor.Tick(input(t));
                dy.Add(motor.Y - y0);
                dx.Add(motor.X - x0);
            }
            return (motor, dy, dx);
        }

        static readonly int[] PopolonFull =
        {
            0, 0,                                          // pulsación y tick quieto
            -6, -6, -6, -6, -6, -5, -5, -4, -4, -3, -2, -1, // subida: 54 px
            0, 0, 0, 0,                                    // arriba
            1, 2, 3, 4, 4, 5, 5, 6, 6, 6, 6, 6,            // bajada hasta el suelo
            0, 0,
        };

        static readonly int[] AphroditeFull =
        {
            0, 0,
            -6, -6, -6, -5, -5, -4, -4, -3, -2, -1,        // 42 px
            0, 0, 0, 0,
            1, 2, 3, 4, 4, 5, 5, 6, 6, 6,
            0, 0,
        };

        [Test]
        public void PopolonFullJumpMatchesOriginal()
        {
            var (motor, dy, _) = Run(FlatRoom(), Character.Popolon, 64, 120, PopolonFull.Length,
                t => new MotorInput { Up = t < 40 && t < 26 });
            CollectionAssert.AreEqual(PopolonFull, dy);
            Assert.AreEqual(120, motor.Y);
            Assert.AreEqual(54, -dy.Where(v => v < 0).Sum());
        }

        [Test]
        public void AphroditeJumpIsFixedHeight()
        {
            // Aunque suelte ↑ enseguida, Aphrodite hace el salto entero.
            var (_, dy, _) = Run(FlatRoom(), Character.Aphrodite, 64, 120, AphroditeFull.Length,
                t => new MotorInput { Up = t < 2 });
            CollectionAssert.AreEqual(AphroditeFull, dy);
        }

        [Test]
        public void PopolonReleaseCutsTheRise()
        {
            // Medido: 5 subidas de 6, un tick quieto, la entrada de 1, 4 ticks arriba y a bajar.
            var (_, dy, _) = Run(FlatRoom(), Character.Popolon, 64, 120, 30, t => new MotorInput { Up = t < 7 });
            CollectionAssert.AreEqual(new[] { 0, 0, -6, -6, -6, -6, -6, 0, -1, 0, 0, 0, 0, 1, 2, 3, 4, 4, 5, 5, 6, 1 },
                dy.Take(22).ToArray());
        }

        [Test]
        public void PopolonTapBeforeTakeoffDoesNotJump()
        {
            var (_, dy, _) = Run(FlatRoom(), Character.Popolon, 64, 120, 20, t => new MotorInput { Up = t < 1 });
            Assert.IsTrue(dy.All(v => v == 0));
        }

        [Test]
        public void CeilingCutsTheRiseLikeRelease()
        {
            // Sala 97 del castillo: suelo en la fila 12 (y = 128) y techo en la fila 7.
            var room = FlatRoom(floorRow: 12, ceilingRow: 7);
            var (motor, dy, _) = Run(room, Character.Popolon, 88, 128, 12, t => new MotorInput { Up = true });
            CollectionAssert.AreEqual(new[] { 0, 0, -6, -6, -6, 0, -1, 0, 0, 0, 0, 1 }, dy.ToArray());
        }

        [Test]
        public void WalkIsTwoPixelsPerTick()
        {
            var (_, _, dx) = Run(FlatRoom(), Character.Popolon, 64, 120, 10, t => new MotorInput { Right = true });
            Assert.IsTrue(dx.All(v => v == 2));
        }

        [Test]
        public void AirControlIsTwoRisingThenThree()
        {
            var (_, _, dx) = Run(FlatRoom(), Character.Popolon, 64, 120, 30,
                t => new MotorInput { Up = true, Right = true });
            // pulsación (2), tick quieto (0), 12 de subida (2), 1.º arriba (2), resto arriba y bajada (3)
            var expected = new List<int> { 2, 0 };
            expected.AddRange(Enumerable.Repeat(2, 12));
            expected.Add(2);
            expected.AddRange(Enumerable.Repeat(3, 3 + 12));
            CollectionAssert.AreEqual(expected, dx.Take(expected.Count).ToList());
        }

        [Test]
        public void LedgeFallHasNoAirControlAndStartsAtThree()
        {
            var room = FlatRoom(floorRow: 15);
            var motor = new PlayerMotor(new RoomWorld(new[] { room }), room, 64, 40);
            motor.Tick(new MotorInput { Right = true });     // sin suelo: empieza a caer
            var dy = new List<int>();
            for (int t = 0; t < 8; t++)
            {
                int y0 = motor.Y, x0 = motor.X;
                motor.Tick(new MotorInput { Right = true });
                dy.Add(motor.Y - y0);
                Assert.AreEqual(x0, motor.X, "caer por un borde no deja mover en horizontal");
            }
            CollectionAssert.AreEqual(new[] { 3, 4, 4, 5, 5, 6, 6, 6 }, dy);
        }

        [Test]
        public void LadderClimbsOnePixelPerTickAndAligns()
        {
            // Escalera en las columnas 14-15 (x = 112): el jugador se alinea a x = 120.
            var room = FlatRoom(ladderCol: 14);
            var (motor, dy, _) = Run(room, Character.Popolon, 118, 120, 10, t => new MotorInput { Up = true });
            Assert.AreEqual(120, motor.X);
            Assert.AreEqual(MotorState.Ladder, motor.State);
            CollectionAssert.AreEqual(Enumerable.Repeat(-1, 9).ToArray(), dy.Skip(1).ToArray());
        }

        // Sala 1 del castillo: la escalera atraviesa el suelo (fila 11 sólida, escalera en 0-10 y 12-19).
        static RoomModel LadderThroughFloor(bool ladderAbove = true)
        {
            var room = FlatRoom(floorRow: 11);
            for (int r = 0; r < 20; r++)
                if (r != 11 && (r > 11 || ladderAbove))
                    room.collision[r] = room.collision[r].Substring(0, 14) + "HH" + room.collision[r].Substring(16);
            return room;
        }

        [Test]
        public void LadderGoesDownThroughTheFloor()
        {
            var (motor, dy, _) = Run(LadderThroughFloor(), Character.Popolon, 121, 120, 20, t => new MotorInput { Down = true });
            Assert.AreEqual(120, motor.X);
            Assert.AreEqual(MotorState.Ladder, motor.State);
            Assert.AreEqual(120 + 19, motor.Y);
        }

        [Test]
        public void LadderClimbsBackUpThroughTheFloor()
        {
            var room = LadderThroughFloor();
            var motor = new PlayerMotor(new RoomWorld(new[] { room }), room, 120, 150);
            for (int t = 0; t < 40; t++)
                motor.Tick(new MotorInput { Up = true });
            Assert.AreEqual(MotorState.Ladder, motor.State);
            Assert.AreEqual(150 - 39, motor.Y);   // el primer tick solo se engancha
        }

        [Test]
        public void LadderEndingUnderAFloorExitsOnTop()
        {
            var room = LadderThroughFloor(ladderAbove: false);
            var motor = new PlayerMotor(new RoomWorld(new[] { room }), room, 120, 150);
            motor.Tick(new MotorInput { Up = true });
            for (int t = 0; t < 60 && motor.State == MotorState.Ladder; t++)
                motor.Tick(new MotorInput { Up = true });
            Assert.AreEqual(MotorState.Ground, motor.State);
            Assert.AreEqual(120, motor.Y);
        }

        [Test]
        public void WalkingOffTheRightEdgeEntersTheNeighbour()
        {
            var a = FlatRoom(1);
            var b = FlatRoom(2);
            a.right = 2;
            b.left = 1;
            var motor = new PlayerMotor(new RoomWorld(new[] { a, b }), a, 250, 120);
            for (int t = 0; t < 5; t++)
                motor.Tick(new MotorInput { Right = true });
            Assert.AreEqual(2, motor.Room.id);
            Assert.AreEqual(4, motor.X);
        }
    }
}
