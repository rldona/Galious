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
            // Medido: 5 subidas de 6, un tick quieto, la entrada de 1, 4 ticks arriba y a bajar. Al llegar al suelo con
            // caída de sobra se hunde ese tick y al siguiente se corrige (popolon_up_tap20: 117 → 123 → 120).
            var (_, dy, _) = Run(FlatRoom(), Character.Popolon, 64, 120, 30, t => new MotorInput { Up = t < 7 });
            CollectionAssert.AreEqual(new[] { 0, 0, -6, -6, -6, -6, -6, 0, -1, 0, 0, 0, 0, 1, 2, 3, 4, 4, 5, 5, 6, 6, -5 },
                dy.Take(23).ToArray());
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
        public void HoldingUpJumpsOnlyOnce()
        {
            // Medido: con ↑ mantenido 300 frames solo hay un salto.
            var (_, dy, _) = Run(FlatRoom(), Character.Popolon, 64, 120, 100, t => new MotorInput { Up = true });
            int jumps = 0;
            for (int t = 1; t < dy.Count; t++)
                if (dy[t] < 0 && dy[t - 1] >= 0) jumps++;
            Assert.AreEqual(1, jumps);
        }

        [Test]
        public void ReleaseAndPressBetweenTicksStillJumpsAgain()
        {
            // El mando lee la entrada en cada fotograma: si ↑ se suelta y se vuelve a pulsar entre dos ticks, el motor
            // la ve mantenida, pero llega marcada como pulsación nueva (UpPressed) y vuelve a saltar.
            var (_, dy, _) = Run(FlatRoom(), Character.Popolon, 64, 120, 100,
                t => new MotorInput { Up = true, UpPressed = t == 0 || t == 50 });
            int jumps = 0;
            for (int t = 1; t < dy.Count; t++)
                if (dy[t] < 0 && dy[t - 1] >= 0) jumps++;
            Assert.AreEqual(2, jumps);
        }

        [Test]
        public void JumpsAfterFallingNextToAWall()
        {
            // Al caer por un borde (sin control en el aire) el cuerpo, de x − 8 a x + 7, puede quedar unos píxeles
            // metido en la pared de al lado: el salto no debe tomarla por techo.
            var room = FlatRoom();
            for (int r = 0; r < 11; r++)
                room.collision[r] = "########" + room.collision[r].Substring(8);   // pared hasta x = 63
            var (_, dy, _) = Run(room, Character.Popolon, 69, 120, 20, t => new MotorInput { Up = t < 15 });
            Assert.Less(dy.Sum(), -40, "salta entero");
        }

        [Test]
        public void DescendingStopsOnTheFloorUntilPressedAgain()
        {
            // Medido: bajando con ↓ mantenido se queda de pie en el suelo (y = 120) aunque la escalera siga debajo.
            var room = LadderThroughFloor();
            var motor = new PlayerMotor(new RoomWorld(new[] { room }), room, 120, 100);
            motor.Tick(new MotorInput { Down = true });
            for (int t = 0; t < 60; t++)
                motor.Tick(new MotorInput { Down = true });
            Assert.AreEqual(MotorState.Ground, motor.State);
            Assert.AreEqual(120, motor.Y);
            motor.Tick(new MotorInput());
            motor.Tick(new MotorInput { Down = true });
            Assert.AreEqual(MotorState.Ladder, motor.State);
            Assert.AreEqual(128, motor.Y);
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
            // pulsación (0: el tick en que se procesa ↑ ya no anda), tick quieto (0), 12 de subida (2), 1.º arriba (2),
            // resto arriba y bajada (3, con ↑ y la dirección pulsadas)
            var expected = new List<int> { 0, 0 };
            expected.AddRange(Enumerable.Repeat(2, 12));
            expected.Add(2);
            expected.AddRange(Enumerable.Repeat(3, 3 + 12));
            CollectionAssert.AreEqual(expected, dx.Take(expected.Count).ToList());
        }

        [Test]
        public void JumpKeepsItsDirectionAndOnlyUpAndDirectionGiveThree()
        {
            // Trazas de las demos (openMSX): soltando todo en el aire sigue a 2 px/tick hacia donde iba; con la dirección
            // sola, 2; con ↑ y la dirección, 3 (después del primer tick arriba).
            var (_, _, released) = Run(FlatRoom(), Character.Popolon, 64, 120, 30,
                t => new MotorInput { Up = t < 26, Right = t < 6 });
            Assert.IsTrue(released.Skip(6).Take(20).All(v => v == 2), string.Join(",", released));
            var (_, _, dirOnly) = Run(FlatRoom(), Character.Popolon, 64, 120, 30,
                t => new MotorInput { Up = t < 26 && t < 14, Right = true });
            Assert.IsTrue(dirOnly.Skip(16).Take(8).All(v => v == 2), string.Join(",", dirOnly));
        }

        [Test]
        public void LedgeFallHoversFourTicksWithoutAirControl()
        {
            // Trazas de las demos (openMSX): al salir andando de una plataforma, 4 ticks quieto en el aire y luego
            // 1, 2, 3, 4, 4, 5… sin poder moverse en horizontal.
            var rows = Enumerable.Range(0, 20).Select(r => r == 11 ? new string('#', 10) + new string('.', 22) : r >= 15 ? new string('#', 32) : new string('.', 32)).ToArray();
            var room = new RoomModel { id = 1, collision = rows };
            var motor = new PlayerMotor(new RoomWorld(new[] { room }), room, 80, 120);
            while (motor.State == MotorState.Ground) motor.Tick(new MotorInput { Right = true });
            int x = motor.X;
            var dy = new List<int>();
            for (int t = 0; t < 10; t++)
            {
                int y0 = motor.Y;
                motor.Tick(new MotorInput { Right = true });
                dy.Add(motor.Y - y0);
                Assert.AreEqual(x, motor.X, "caer por un borde no deja mover en horizontal");
            }
            CollectionAssert.AreEqual(new[] { 0, 0, 0, 0, 1, 2, 3, 4, 4, 5 }, dy);
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
            // Medido: al engancharse cruza la fila del suelo de golpe (y = 120 -> 128) y luego baja 1 px/tick.
            var (motor, dy, _) = Run(LadderThroughFloor(), Character.Popolon, 121, 120, 20, t => new MotorInput { Down = true });
            Assert.AreEqual(120, motor.X);
            Assert.AreEqual(MotorState.Ladder, motor.State);
            Assert.AreEqual(8, dy[0]);
            Assert.AreEqual(128 + 19, motor.Y);
        }

        [Test]
        public void LadderClimbsBackUpThroughTheFloor()
        {
            // Medido: subiendo, al entrar 1 px en la fila del suelo (y = 127) salta a y = 120 y queda de pie;
            // con ↑ mantenido no sigue por la escalera de arriba hasta volver a pulsar.
            var room = LadderThroughFloor();
            var motor = new PlayerMotor(new RoomWorld(new[] { room }), room, 120, 150);
            for (int t = 0; t < 40; t++)
                motor.Tick(new MotorInput { Up = true });
            Assert.AreEqual(MotorState.Ground, motor.State);
            Assert.AreEqual(120, motor.Y);
            motor.Tick(new MotorInput());
            motor.Tick(new MotorInput { Up = true });
            Assert.AreEqual(MotorState.Ladder, motor.State);
            motor.Tick(new MotorInput { Up = true });
            Assert.AreEqual(119, motor.Y);
        }

        [Test]
        public void StepOffTheLadderAtAFloorItCrosses()
        {
            // Escalera que atraviesa el suelo (fila 11): sube desde abajo hasta y = 120 y sale andando.
            var room = LadderThroughFloor();
            var motor = new PlayerMotor(new RoomWorld(new[] { room }), room, 120, 150);
            motor.Tick(new MotorInput { Up = true });
            while (motor.State == MotorState.Ladder)
                motor.Tick(new MotorInput { Up = true });
            Assert.AreEqual(MotorState.Ground, motor.State, "llega al suelo de pie");
            Assert.AreEqual(120, motor.Y);
            for (int t = 0; t < 5; t++)
                motor.Tick(new MotorInput { Right = true });
            Assert.AreEqual(MotorState.Ground, motor.State);
            Assert.AreEqual(130, motor.X);
            Assert.AreEqual(120, motor.Y);
        }

        [Test]
        public void NoSteppingOffMidLadderBetweenWalls()
        {
            // Escalera de abajo de la sala 1: paredes a los dos lados.
            var room = LadderThroughFloor();
            for (int r = 12; r < 20; r++)
                room.collision[r] = new string('#', 14) + "HH" + new string('#', 16);
            var motor = new PlayerMotor(new RoomWorld(new[] { room }), room, 120, 150);
            motor.Tick(new MotorInput { Up = true });
            motor.Tick(new MotorInput { Up = true });
            for (int t = 0; t < 5; t++)
                motor.Tick(new MotorInput { Right = true });
            Assert.AreEqual(MotorState.Ladder, motor.State);
            Assert.AreEqual(120, motor.X);
        }

        [Test]
        public void LadderSidewaysOnlyWithTheRemakeRule()
        {
            var room = LadderThroughFloor();
            PlayerMotor OnUpperLadder()
            {
                var m = new PlayerMotor(new RoomWorld(new[] { room }), room, 120, 120);
                m.Tick(new MotorInput { Up = true });      // se engancha a la escalera de arriba
                for (int t = 0; t < 20; t++)
                    m.Tick(new MotorInput { Up = true });
                return m;
            }
            var original = OnUpperLadder();
            original.Tick(new MotorInput { Right = true });
            Assert.AreEqual(MotorState.Ladder, original.State, "original: a media escalera, ← → no hacen nada");
            try
            {
                RemakeRules.LeaveLadderSideways = true;
                var remake = OnUpperLadder();
                remake.Tick(new MotorInput { Right = true });
                Assert.AreEqual(MotorState.Fall, remake.State);
                Assert.AreEqual(123, remake.X);
            }
            finally
            {
                RemakeRules.LeaveLadderSideways = false;
            }
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

        // ---------- espada (docs/player.md, "Espada") ----------

        [Test]
        public void SwordLastsFourTicksAndStopsWalking()
        {
            var (motor, _, dx) = Run(FlatRoom(), Character.Popolon, 64, 120, 10,
                t => new MotorInput { Right = true, Attack = t >= 2 });
            CollectionAssert.AreEqual(new[] { 2, 2, 0, 0, 0, 0, 2, 2, 2, 2 }, dx.ToArray(), "se para 4 ticks y no repite");
        }

        [Test]
        public void SwordNeedsANewPress()
        {
            var room = FlatRoom();
            var motor = new PlayerMotor(new RoomWorld(new[] { room }), room, 64, 120);
            var started = new List<int>();
            // Pulsado 0-5 (empieza en 0; pulsar durante el golpe no cuenta), suelta 6, pulsa 7.
            for (int t = 0; t < 12; t++)
            {
                motor.Tick(new MotorInput { Attack = t <= 5 || t >= 7 });
                if (motor.AttackStartedThisTick) started.Add(t);
            }
            CollectionAssert.AreEqual(new[] { 0, 7 }, started);
        }

        [Test]
        public void SwordInTheAirKeepsAirControl()
        {
            var (_, _, dx) = Run(FlatRoom(), Character.Popolon, 64, 120, 12,
                t => new MotorInput { Up = true, Right = true, Attack = t == 5 });
            Assert.IsTrue(dx.Skip(2).Take(10).All(v => v == 2));
        }

        [Test]
        public void NoSwordOnTheLadder()
        {
            var room = FlatRoom(ladderCol: 14);
            var motor = new PlayerMotor(new RoomWorld(new[] { room }), room, 120, 120);
            motor.Tick(new MotorInput { Up = true });
            Assert.AreEqual(MotorState.Ladder, motor.State);
            motor.Tick(new MotorInput { Up = true, Attack = true });
            Assert.AreEqual(0, motor.AttackTicks);
        }

        [Test]
        public void PopolonBreaksARockInThreeHitsAphroditeInFifteen()
        {
            foreach (var (character, ring, hits) in new[] { (Character.Popolon, false, 3), (Character.Aphrodite, false, 15), (Character.Aphrodite, true, 3) })
            {
                // Piedra de 2 x 2 tiles sobre el suelo (fila 11), con el borde a 18 px del jugador.
                var room = FlatRoom();
                room.rocks = new[] { new PointJson { x = 64 + 18, y = 104 } };
                for (int r = 9; r <= 10; r++)
                    room.collision[r] = room.collision[r].Substring(0, 10) + "##" + room.collision[r].Substring(12);
                var world = new RoomWorld(new[] { room });
                var rocks = new Rocks("test", world, new HashSet<(string, int, int, int)>());
                var motor = new PlayerMotor(world, room, 64, 120) { Character = character };
                int presses = 0;
                for (int i = 0; i < 40 && !rocks.All.First().broken; i++)
                {
                    motor.Tick(new MotorInput { Attack = true });
                    if (motor.AttackStartedThisTick) { presses++; rocks.Strike(motor, world, ring); }
                    for (int k = 0; k < 4; k++) motor.Tick(new MotorInput());
                }
                Assert.AreEqual(hits, presses, $"{character}{(ring ? " con el Ring" : "")}");
                Assert.IsFalse(world.Solid(room, 64 + 18, 110), "el hueco de la piedra queda libre");
            }
        }

        [Test]
        public void SwordHitsARockWhilePushingAgainstIt()
        {
            // Lo normal es pegarse a la piedra y golpear: andando contra ella, desde los dos lados y con los dos personajes,
            // cada estocada tiene que darle.
            foreach (var character in new[] { Character.Popolon, Character.Aphrodite })
                foreach (bool fromLeft in new[] { true, false })
                {
                    var room = FlatRoom();
                    room.rocks = new[] { new PointJson { x = 120, y = 104 } };
                    for (int r = 9; r <= 10; r++)
                        room.collision[r] = room.collision[r].Substring(0, 15) + "##" + room.collision[r].Substring(17);
                    var world = new RoomWorld(new[] { room });
                    var rocks = new Rocks("test", world, new HashSet<(string, int, int, int)>());
                    var motor = new PlayerMotor(world, room, fromLeft ? 80 : 170, 120) { Character = character };
                    var walk = new MotorInput { Right = fromLeft, Left = !fromLeft };
                    for (int i = 0; i < 60; i++) motor.Tick(walk);
                    int x = motor.X, presses = 0;
                    for (int i = 0; i < 5; i++)
                    {
                        var press = walk;
                        press.Attack = true;
                        motor.Tick(press);
                        if (motor.AttackStartedThisTick) { presses++; rocks.Strike(motor, world); }
                        for (int k = 0; k < 5; k++) motor.Tick(walk);
                    }
                    int damage = character == Character.Popolon ? OriginalPhysics.RockDamagePopolon : OriginalPhysics.RockDamageAphrodite;
                    string what = $"{character} {(fromLeft ? "desde la izquierda" : "desde la derecha")}, x = {x}, {presses} estocadas";
                    Assert.AreEqual(5, presses, what);
                    Assert.AreEqual(System.Math.Max(0, OriginalPhysics.RockHp - 5 * damage), System.Math.Max(0, rocks.All.First().hp), what);
                }
        }

        [Test]
        public void SwordDoesNotReachTwentyPixels()
        {
            var room = FlatRoom();
            room.rocks = new[] { new PointJson { x = 64 + 20, y = 104 } };
            var world = new RoomWorld(new[] { room });
            var rocks = new Rocks("test", world, new HashSet<(string, int, int, int)>());
            var motor = new PlayerMotor(world, room, 64, 120);
            motor.Tick(new MotorInput { Attack = true });
            rocks.Strike(motor, world);
            Assert.AreEqual(OriginalPhysics.RockHp, rocks.All.First().hp);
        }

        // ---------- puertas y salidas ----------

        [Test]
        public void DownAtAWorldDoorEntersTheWorld()
        {
            var room = FlatRoom();
            room.doorWorld = 3;
            room.door = new PointJson { x = 64, y = 96 };   // jugador en y = 96 + 0x18 = 120
            var motor = new PlayerMotor(new RoomWorld(new[] { room }), room, 64 + 16, 120);
            int entered = 0;
            motor.DoorEntered += w => entered = w;
            motor.Tick(new MotorInput { Down = true });
            Assert.AreEqual(3, entered);
        }

        [Test]
        public void WalkingThroughACastleExitRaisesTheEvent()
        {
            var room = FlatRoom();
            room.left = 0xFB;
            var motor = new PlayerMotor(new RoomWorld(new[] { room }), room, 4, 120);
            int exit = 0;
            motor.CastleExitReached += code => exit = code;
            for (int t = 0; t < 4; t++)
                motor.Tick(new MotorInput { Left = true });
            Assert.AreEqual(0xFB, exit);
        }

        [Test]
        public void ClimbingOffTheTopThroughACastleExitRaisesTheEvent()
        {
            var room = FlatRoom(ladderCol: 14);
            room.up = 0xFE;
            var motor = new PlayerMotor(new RoomWorld(new[] { room }), room, 120, 48);
            int exit = 0;
            motor.CastleExitReached += code => exit = code;
            for (int t = 0; t < 30 && exit == 0; t++)
                motor.Tick(new MotorInput { Up = true });
            Assert.AreEqual(0xFE, exit);
        }

        // ---------- agua y lava (docs/player.md) ----------

        // Castillo, sala 137: agua en las filas 12-18 y suelo en la 19 (y = 184).
        static RoomModel WaterRoom(char liquid = '~')
        {
            var room = FlatRoom(floorRow: 19);
            for (int r = 12; r <= 18; r++)
                room.collision[r] = new string(liquid, 32);
            return room;
        }

        [Test]
        public void SinkingInWaterIsOneAndAHalfPixelsPerTick()
        {
            var room = WaterRoom();
            var motor = new PlayerMotor(new RoomWorld(new[] { room }), room, 80, 104);
            int entered = -1;
            var ys = new List<int>();
            for (int t = 0; t < 60; t++)
            {
                motor.Tick(new MotorInput());
                ys.Add(motor.Y);
                if (entered < 0 && motor.InLiquid) entered = t;
            }
            Assert.GreaterOrEqual(entered, 0);
            // Ya a velocidad máxima de caída (6): en el agua, 6/4 = 1,5 px/tick (1 y 2 alternos).
            int a = ys[entered + 4], b = ys[entered + 12];
            Assert.AreEqual(12, b - a);
            Assert.AreEqual(184, ys.Last(), "se para en el fondo");
            Assert.AreEqual(MotorState.Ground, motor.State);
        }

        [Test]
        public void WalkingInWaterIsHalfSpeed()
        {
            var room = WaterRoom();
            var motor = new PlayerMotor(new RoomWorld(new[] { room }), room, 80, 184);
            for (int t = 0; t < 20; t++)
                motor.Tick(new MotorInput { Right = true });
            Assert.AreEqual(80 + 20, motor.X);
        }

        [Test]
        public void AphroditeJumpInWaterIsSlowButSameHeight()
        {
            var room = WaterRoom();
            var motor = new PlayerMotor(new RoomWorld(new[] { room }), room, 80, 184) { Character = Character.Aphrodite };
            int top = 184, ticksToTop = 0;
            for (int t = 0; t < 80; t++)
            {
                motor.Tick(new MotorInput { Up = t < 2 });
                if (motor.Y < top) { top = motor.Y; ticksToTop = t; }
            }
            Assert.AreEqual(184 - 42, top, "42 px, como fuera del agua");
            Assert.Greater(ticksToTop, 35, "medido: unos 40 ticks para subir");
        }

        [Test]
        public void LavaFloorStopsAt184()
        {
            var room = FlatRoom(floorRow: 20);
            for (int r = 12; r <= 19; r++)
                room.collision[r] = new string('L', 32);
            var motor = new PlayerMotor(new RoomWorld(new[] { room }), room, 64, 96);
            for (int t = 0; t < 120; t++)
                motor.Tick(new MotorInput());
            Assert.AreEqual(184, motor.Y);
            Assert.AreEqual(TileClass.Lava, motor.Liquid);
        }

        // ---------- puertas con llave (docs/entities.md) ----------

        // Castillo, sala 16: puerta de 1 x 3 tiles en (160, 96), entre el techo (fila 7) y el suelo (fila 11).
        static RoomModel KeyDoorRoom(int cls)
        {
            var room = FlatRoom(floorRow: 11, ceilingRow: 7);
            for (int r = 8; r <= 10; r++)
                room.collision[r] = room.collision[r].Substring(0, 20) + "#" + room.collision[r].Substring(21);
            room.objects = new[] { new EntityJson { x = 160, y = 96, type = cls } };
            return room;
        }

        static (PlayerMotor motor, KeyDoors doors, RoomWorld world) DoorSetup(int cls, int x)
        {
            var room = KeyDoorRoom(cls);
            var world = new RoomWorld(new[] { room });
            var doors = new KeyDoors("test", world, new HashSet<(string, int, int, int)>());
            return (new PlayerMotor(world, room, x, 120), doors, world);
        }

        [Test]
        public void KeyDoorNeedsThirtyTicksAndAKey()
        {
            var (motor, doors, _) = DoorSetup(1, 140);
            int keys = 1, ticks = 0;
            while (motor.State != MotorState.Carried && ticks < 100)
            {
                motor.Tick(new MotorInput { Right = true });
                keys = doors.Tick(motor, keys);
                ticks++;
            }
            Assert.AreEqual(0, keys);
            Assert.AreEqual(152, motor.X, "pegado a la puerta: x = 160 - 8");
            // 6 ticks andando hasta la puerta (140 -> 152); el tick en que llega ya empuja: 5 + 30.
            Assert.AreEqual(5 + 30, ticks);
        }

        [Test]
        public void KeyDoorWithoutKeysDoesNothing()
        {
            var (motor, doors, _) = DoorSetup(1, 152);
            for (int t = 0; t < 60; t++)
            {
                motor.Tick(new MotorInput { Right = true });
                doors.Tick(motor, 0);
            }
            Assert.AreEqual(152, motor.X);
            Assert.AreNotEqual(MotorState.Carried, motor.State);
        }

        [Test]
        public void CastleKeyDoorCarriesThroughAndClosesBehind()
        {
            var (motor, doors, world) = DoorSetup(1, 152);
            int keys = 2;
            for (int t = 0; t < 30 + 24 + 2; t++)
            {
                motor.Tick(new MotorInput { Right = t < 30 });
                keys = doors.Tick(motor, keys);
            }
            Assert.AreEqual(176, motor.X, "al otro lado: x = 160 + 16");
            Assert.AreEqual(1, keys);
            Assert.IsTrue(world.Solid(motor.Room, 160, 100), "se vuelve a cerrar");
        }

        [Test]
        public void WorldKeyDoorStaysOpen()
        {
            var (motor, doors, world) = DoorSetup(2, 152);
            int keys = 1;
            for (int t = 0; t < 30 + 24 + 2; t++)
            {
                motor.Tick(new MotorInput { Right = t < 30 });
                keys = doors.Tick(motor, keys);
            }
            Assert.AreEqual(176, motor.X);
            Assert.IsFalse(world.Solid(motor.Room, 160, 100), "queda abierta");
        }

        [Test]
        public void FallsIntoATwoTileHoleWhereTheOriginalDoes()
        {
            // Mundo 1, sala 2: hueco en las columnas 15-16 (x = 120..135) del suelo de la fila 7 (y = 88).
            var room = FlatRoom(floorRow: 7);
            for (int r = 7; r < 20; r++)
                room.collision[r] = room.collision[r].Substring(0, 15) + ".." + room.collision[r].Substring(17);
            foreach (var (x0, dir, expected) in new[] { (100, 1, 124), (101, 1, 125), (156, -1, 130), (157, -1, 131) })
            {
                var motor = new PlayerMotor(new RoomWorld(new[] { room }), room, x0, 88);
                int fellAt = -1;
                for (int t = 0; t < 40 && fellAt < 0; t++)
                {
                    int x = motor.X;
                    motor.Tick(new MotorInput { Right = dir > 0, Left = dir < 0 });
                    if (motor.State == MotorState.Fall) fellAt = motor.X;
                }
                Assert.AreEqual(expected, fellAt, $"desde x = {x0}");
            }
        }

        // Castillo, salas 95 y 85: la escalera de la 95 llega al borde de arriba y atraviesa el suelo de la 85.
        static (RoomModel lower, RoomModel upper) StackedRooms()
        {
            var lower = FlatRoom(1, floorRow: 11, ladderCol: 24);
            for (int r = 11; r < 20; r++)
                lower.collision[r] = new string('#', 32);
            var upper = FlatRoom(2, floorRow: 19);
            lower.up = 2;
            upper.down = 1;
            return (lower, upper);
        }

        [Test]
        public void ClimbingIntoTheRoomAboveStandsOnItsFloor()
        {
            var (lower, upper) = StackedRooms();
            var motor = new PlayerMotor(new RoomWorld(new[] { lower, upper }), lower, 200, 120);
            for (int t = 0; t < 120 && !(motor.Room == upper && motor.State == MotorState.Ground); t++)
                motor.Tick(new MotorInput { Up = true });
            Assert.AreEqual(2, motor.Room.id, "cambia de sala sin otra pulsación");
            Assert.AreEqual(MotorState.Ground, motor.State);
            Assert.AreEqual(184, motor.Y, "de pie sobre el suelo de la sala de arriba");
            for (int t = 0; t < 10; t++)
                motor.Tick(new MotorInput { Up = true });
            Assert.AreEqual(184, motor.Y, "con ↑ mantenido no salta");
        }

        [Test]
        public void DescendingThroughTheFloorIntoTheRoomBelow()
        {
            var (lower, upper) = StackedRooms();
            var motor = new PlayerMotor(new RoomWorld(new[] { lower, upper }), upper, 200, 184);
            motor.Tick(new MotorInput { Down = true });
            Assert.AreEqual(MotorState.Ladder, motor.State);
            for (int t = 0; t < 20; t++)
                motor.Tick(new MotorInput { Down = true });
            Assert.AreEqual(1, motor.Room.id);
            Assert.AreEqual(MotorState.Ladder, motor.State);
        }

        [Test]
        public void JumpingAtTheTopDoesNotLeaveTheRoom()
        {
            // Medido (mundo 1, sala 1, salida al castillo arriba): desde y = 56 el salto llega a y = 37 y no sale.
            var room = FlatRoom(floorRow: 3);
            room.up = 0xFE;
            var motor = new PlayerMotor(new RoomWorld(new[] { room }), room, 108, 56);
            int exit = 0, minY = 56;
            motor.CastleExitReached += c => exit = c;
            for (int t = 0; t < 60; t++)
            {
                motor.Tick(new MotorInput { Up = t < 30 });
                minY = System.Math.Min(minY, motor.Y);
            }
            Assert.AreEqual(0, exit, "no sale del mundo");
            Assert.AreEqual(37, minY);
            Assert.AreEqual(56, motor.Y);
        }

        [Test]
        public void CanWalkAwayFromAWallItOverlaps()
        {
            // Cayó pegado a una pared y la solapa por la derecha (x + 7 dentro del muro de la columna 30).
            var room = FlatRoom(floorRow: 19);
            for (int r = 0; r < 19; r++)
                room.collision[r] = room.collision[r].Substring(0, 30) + "##";
            var motor = new PlayerMotor(new RoomWorld(new[] { room }), room, 235, 184);
            for (int t = 0; t < 5; t++)
                motor.Tick(new MotorInput { Left = true });
            Assert.AreEqual(225, motor.X);
        }

        [Test]
        public void RoomEdgesSwitchAtTheMeasuredPositions()
        {
            var a = FlatRoom(1);
            var b = FlatRoom(2);
            a.right = 2;
            b.left = 1;
            // Medido (demos en openMSX): al llegar a x = 248 cambia de sala y aparece en x = 16; desde ahí sigue andando.
            var motor = new PlayerMotor(new RoomWorld(new[] { a, b }), a, 244, 120);
            motor.Tick(new MotorInput { Right = true });
            Assert.AreEqual(1, motor.Room.id, "x = 246");
            motor.Tick(new MotorInput { Right = true });
            Assert.AreEqual(2, motor.Room.id);
            Assert.AreEqual(16, motor.X);
            motor.Tick(new MotorInput { Left = true });   // 16 → 14: cruza
            Assert.AreEqual(1, motor.Room.id, "y vuelve al bajar de x = 16");
            Assert.AreEqual(247, motor.X);
        }
    }
}
