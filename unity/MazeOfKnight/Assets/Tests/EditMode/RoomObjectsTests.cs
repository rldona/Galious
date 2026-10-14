using System.Collections.Generic;
using System.Linq;
using MazeOfKnight.Data;
using MazeOfKnight.Game;
using NUnit.Framework;

namespace MazeOfKnight.Tests
{
    // Mecanismos de sala contra lo medido en openMSX (tools/openmsx/room_object_measure.py). Salas sintéticas.
    public class RoomObjectsTests
    {
        // Pasillo entre el techo (fila 3) y el suelo (fila 7): el jugador anda en y = 88.
        static RoomModel Corridor(params EntityJson[] objects)
        {
            var rows = new string[20];
            for (int r = 0; r < 20; r++)
                rows[r] = new string(r == 3 || r >= 7 ? '#' : '.', 32);
            return new RoomModel { id = 1, collision = rows, objects = objects };
        }

        static void Column(RoomModel room, int x, int y, int width = 1, int height = 3)
        {
            for (int r = (y - 32) / 8; r < (y - 32) / 8 + height; r++)
                room.collision[r] = room.collision[r].Substring(0, x / 8) + new string('#', width) + room.collision[r].Substring(x / 8 + width);
        }

        static (PlayerMotor, RoomObjects, RoomWorld) Setup(RoomModel room, int x, Character c = Character.Popolon, int y = 88)
        {
            var world = new RoomWorld(new[] { room });
            var objects = new RoomObjects("test", world, new HashSet<(string, int, int, int)>());
            return (new PlayerMotor(world, room, x, y) { Character = c }, objects, world);
        }

        static void Run(PlayerMotor m, RoomObjects o, int ticks, MotorInput input, ICollection<int> items = null)
        {
            for (int t = 0; t < ticks; t++)
            {
                m.Tick(input);
                o.Tick(m, items ?? new List<int>());
            }
        }

        // ---------- puertas giratorias ----------

        [Test]
        public void RevolvingDoorCarriesPopolonThrough()
        {
            // Mundo 1, sala 7: clase 4 en (104, 64). Medido: 30 ticks empujando y 24 px a 1 px/tick.
            var room = Corridor(new EntityJson { x = 104, y = 64, type = 4 });
            Column(room, 104, 64);
            var (m, o, _) = Setup(room, 96);
            Run(m, o, 30, new MotorInput { Right = true });
            Assert.AreEqual(MotorState.Carried, m.State);
            Run(m, o, 24, new MotorInput());
            Assert.AreEqual(120, m.X, "al otro lado: x = 104 + 16");
            Assert.AreEqual(MotorState.Ground, m.State);
        }

        [Test]
        public void AphroditeNeedsBreadAndWaterForRevolvingDoors()
        {
            var room = Corridor(new EntityJson { x = 104, y = 64, type = 4 });
            Column(room, 104, 64);
            var (m, o, _) = Setup(room, 96, Character.Aphrodite);
            Run(m, o, 60, new MotorInput { Right = true });
            Assert.AreEqual(96, m.X, "sin Bread & Water no gira");
            var (m2, o2, _) = Setup(room, 96, Character.Aphrodite);
            Run(m2, o2, 30 + 24, new MotorInput { Right = true }, new List<int> { RoomObjects.ItemBreadAndWater });
            Assert.AreEqual(120, m2.X);
        }

        [Test]
        public void OneWayDoorOnlyTurnsItsWay()
        {
            // Mundo 2, sala 6: tipo 0x03 en (120, 64): solo hacia la izquierda.
            var room = Corridor(new EntityJson { x = 120, y = 64, type = 0x03 });
            Column(room, 120, 64);
            var (m, o, _) = Setup(room, 112);
            Run(m, o, 80, new MotorInput { Right = true });
            Assert.AreEqual(112, m.X, "hacia la derecha no gira");
            var (m2, o2, _) = Setup(room, 136);
            Run(m2, o2, 30 + 24, new MotorInput { Left = true });
            Assert.AreEqual(112, m2.X, "hacia la izquierda pasa: x = 120 - 8");
        }

        // ---------- muro que se cierra ----------

        [Test]
        public void ClosingWallGrowsBehindYou()
        {
            // Castillo, sala 48: clase 5 en (160, 64). Medido: empieza a cerrarse con el jugador en x = 176 y crece
            // un tile cada 15 ticks hasta 3.
            var room = Corridor(new EntityJson { x = 160, y = 64, type = 5 });
            var (m, o, world) = Setup(room, 136);
            int closedAt = -1;
            for (int t = 0; t < 40 && closedAt < 0; t++)
            {
                m.Tick(new MotorInput { Right = true });
                o.Tick(m, new List<int>());
                if (world.Solid(room, 160, 64)) closedAt = m.X;
            }
            Assert.That(closedAt, Is.InRange(176, 180), "se cierra con el jugador entero al otro lado");
            Assert.IsFalse(world.Solid(room, 160, 80), "aún solo el tile de arriba");
            Run(m, o, 31, new MotorInput());
            Assert.IsTrue(world.Solid(room, 160, 64) && world.Solid(room, 160, 72) && world.Solid(room, 160, 80));
        }

        [Test]
        public void ClosingWallWaitsUntilYouAreFullyPast()
        {
            var room = Corridor(new EntityJson { x = 160, y = 64, type = 5 });
            var (m, o, world) = Setup(room, 136);
            Run(m, o, 17, new MotorInput { Right = true });   // x = 170: anotado, pero aún encima de la columna
            Run(m, o, 60, new MotorInput());
            Assert.IsFalse(world.Solid(room, 160, 64));
        }

        // ---------- pared que se rompe ----------

        static (PlayerMotor, RoomObjects, RoomWorld) WallRoom(int type, Character c)
        {
            // Castillo, sala 97: bloque de 4 x 3 tiles en (128, 96); el jugador golpea desde x = 118, y = 112.
            var rows = new string[20];
            for (int r = 0; r < 20; r++)
                rows[r] = new string(r >= 10 ? '#' : '.', 32);
            var room = new RoomModel { id = 1, collision = rows, objects = new[] { new EntityJson { x = 128, y = 96, type = type } } };
            Column(room, 128, 96, 4, 3);
            return Setup(room, 118, c, 112);
        }

        static int StrikesToBreak(PlayerMotor m, RoomObjects o, RoomWorld w, ICollection<int> items, int max = 40)
        {
            for (int k = 1; k <= max; k++)
            {
                m.Tick(new MotorInput { Attack = true });
                if (m.AttackStartedThisTick) o.Strike(m, items);
                for (int i = 0; i < 4; i++) m.Tick(new MotorInput());
                if (!w.Solid(m.Room, 128, 100)) return k;
            }
            return -1;
        }

        [Test]
        public void BreakableWallHitsByType()
        {
            foreach (var (type, hits) in new[] { (0x07, 30), (0x17, 20), (0x27, 10), (0x37, 3) })
            {
                var (m, o, w) = WallRoom(type, Character.Popolon);
                Assert.AreEqual(hits, StrikesToBreak(m, o, w, new List<int>()), $"tipo 0x{type:X2}");
            }
        }

        [Test]
        public void AphroditeNeedsThePendantForWalls()
        {
            var (m, o, w) = WallRoom(0x37, Character.Aphrodite);
            Assert.AreEqual(-1, StrikesToBreak(m, o, w, new List<int>(), 10));
            var (m2, o2, w2) = WallRoom(0x37, Character.Aphrodite);
            Assert.AreEqual(3, StrikesToBreak(m2, o2, w2, new List<int> { RoomObjects.ItemPendant }));
        }

        // ---------- escaleras que se retiran ----------

        [Test]
        public void ExtraLadderRetractsAfterUseAndComesBackInAMinute()
        {
            // Castillo, sala 13 (modo 3): escalera extra de 7 tiles en (176, 64), sobre el suelo de la fila 11.
            var rows = new string[20];
            var bare = new string[20];
            for (int r = 0; r < 20; r++)
            {
                rows[r] = bare[r] = new string(r == 3 || r >= 11 ? '#' : '.', 32);
                if (r >= 4 && r <= 10)
                    rows[r] = rows[r].Substring(0, 22) + "HH" + rows[r].Substring(24);
            }
            var room = new RoomModel
            {
                id = 1, collision = rows, collisionNoLadders = bare,
                ladders = new[] { new LadderJson { x = 176, y = 64, height = 7, mode = 3 } },
            };
            var (m, o, world) = Setup(room, 184, Character.Popolon, 120);
            m.Tick(new MotorInput { Up = true });
            o.Tick(m, new List<int>());
            for (int t = 0; t < 120 && m.State == MotorState.Ladder; t++)
            {
                m.Tick(new MotorInput { Up = true });
                o.Tick(m, new List<int>());
            }
            Assert.AreEqual(MotorState.Ground, m.State, "arriba de la escalera, sobre el techo");
            Assert.IsFalse(world.Ladder(room, 176, 80), "la escalera ya no está");
            Run(m, o, RoomObjects.LadderRetractTicks, new MotorInput());
            Assert.IsTrue(world.Ladder(room, 176, 80), "vuelve al minuto");
        }
    }
}
