using System.Collections.Generic;
using System.Linq;
using MazeOfKnight.Data;
using MazeOfKnight.Game;
using MazeOfKnight.Game.Enemies;
using NUnit.Framework;

namespace MazeOfKnight.Tests
{
    // Enemigos contra sus fichas (docs/enemy_ai.md) y contra lo medido (combate, docs/player.md). Salas sintéticas.
    public class EnemyTests
    {
        static readonly EnemyInfo[] Catalog =
        {
            new EnemyInfo { type = 0x0E, hp = 1, damage = 1, lootRule = 0, lootKills = new[] { 1, 3 } },
            new EnemyInfo { type = 0x25, hp = 8, damage = 3, lootRule = 2, lootKills = new[] { 4, 6 }, lootLevel = 1 },
            new EnemyInfo { type = 0x31, hp = 0, damage = 2, invulnerable = true, immuneItem = 0x22 },
            new EnemyInfo { type = 0x18, hp = 1, damage = 2, lootRule = 0, lootKills = new[] { 1, 3 } },
            new EnemyInfo { type = 0x34, hp = 4, damage = 6 },
            new EnemyInfo { type = 0x2E, hp = 2, damage = 2, immuneItem = 0x17 },
            new EnemyInfo { type = 0x0B, hp = 1, damage = 10 },
            new EnemyInfo { type = 0x11, hp = 2, damage = 2 },
            new EnemyInfo { type = 0x1D, invulnerable = true, damage = 1 },
            new EnemyInfo { type = 0x19, hp = 1, damage = 1, lootRule = 3, lootWhenLastAlive = true },
            new EnemyInfo { type = 0x2A, hp = 3, damage = 1 },
            new EnemyInfo { type = 0x2C, hp = 4, damage = 2 },
            new EnemyInfo { type = 0x64, hp = 4, damage = 2 },
            new EnemyInfo { type = 0x36, invulnerable = true, damage = 6 },
            new EnemyInfo { type = 0x47, invulnerable = true, damage = 2 },
            new EnemyInfo { type = 0x49, hp = 5, damage = 8 },
            new EnemyInfo { type = 0x52, hp = 3, damage = 2 },
            new EnemyInfo { type = 0x53, hp = 1, damage = 1 },
            new EnemyInfo { type = 0x09, invulnerable = true, damage = 3 },
            new EnemyInfo { type = 0x3E, hp = 2, damage = 3 },
        };

        // Suelo en la fila 11 (y = 120); techo opcional en la fila 'ceiling'.
        static RoomModel Room(int ceiling = -1, params EntityJson[] spawns)
        {
            var rows = new string[20];
            for (int r = 0; r < 20; r++)
                rows[r] = new string(r >= 11 || r == ceiling ? '#' : '.', 32);
            return new RoomModel { id = 1, collision = rows, spawns = spawns };
        }

        static (EnemySystem, PlayerMotor, RoomWorld) Setup(RoomModel room, int px = 200, int py = 120)
        {
            var world = new RoomWorld(new[] { room });
            var motor = new PlayerMotor(world, room, px, py);
            var sys = new EnemySystem(Catalog, 7);
            sys.EnterRoom(world, room, motor);
            return (sys, motor, world);
        }

        static void Ticks(EnemySystem s, int n) { for (int i = 0; i < n; i++) s.Tick(); }

        [Test]
        public void BatHangsEightyTicksThenDropsTenPixels()
        {
            // Techo en la fila 3 (y 56..63): el murciélago cuelga justo debajo, en y = 64.
            var (sys, _, _) = Setup(Room(3, new EntityJson { type = 0x0E, x = 64, y = 64 }));
            var bat = sys.Enemies[0];
            Ticks(sys, 79);
            Assert.AreEqual(64, bat.Y, "colgado 80 ticks");
            Ticks(sys, 11);
            Assert.AreEqual(74, bat.Y, 0.01, "cae 10 ticks a 1 px/tick");
        }

        [Test]
        public void SpikesAreInvulnerableAndHarmlessWhenSmall()
        {
            var (sys, motor, _) = Setup(Room(-1, new EntityJson { type = 0x31, x = 64, y = 104 }), 64 - 18, 120);
            var spikes = sys.Enemies[0];
            int harmful = 0, visibleHarmless = 0;
            for (int t = 0; t < 600; t++)
            {
                sys.Tick();
                if (spikes.Visible && spikes.Harmful) harmful++;
                if (spikes.Visible && !spikes.Harmful) visibleHarmless++;
                if (t % 4 == 0)
                {
                    motor.Tick(new MotorInput { Attack = true });
                    sys.Strike(motor, new List<int>());
                    motor.Tick(new MotorInput());
                }
            }
            Assert.IsTrue(spikes.Alive, "invulnerables");
            Assert.Greater(harmful, 0);
            Assert.Greater(visibleHarmless, 0, "la etapa C4 se ve pero no daña");
            Assert.IsFalse(spikes.GrowsDown, "con suelo debajo crecen desde el suelo");
        }

        [Test]
        public void RollerNeverWalksOffItsPlatform()
        {
            // Plataforma de 6 tiles (x 64..111) en la fila 11; el resto es hueco.
            var room = Room();
            for (int r = 11; r < 20; r++)
                room.collision[r] = new string('.', 8) + new string('#', 6) + new string('.', 18);
            room.spawns = new[] { new EntityJson { type = 0x18, x = 80, y = 104 } };
            var (sys, _, _) = Setup(room);
            var roller = sys.Enemies[0];
            float min = roller.X, max = roller.X;
            for (int t = 0; t < 2000; t++)
            {
                sys.Tick();
                min = System.Math.Min(min, roller.X);
                max = System.Math.Max(max, roller.X + 15);
            }
            Assert.GreaterOrEqual(min, 64 - 1);
            Assert.LessOrEqual(max, 111 + 1);
            Assert.Greater(max - min, 30, "se mueve de un lado a otro");
        }

        [Test]
        public void BlockIsFasterWhenThePlayerIsClose()
        {
            var room = Room();
            room.spawns = new[] { new EntityJson { type = 0x34, x = 64, y = 40 } };
            var (sysFar, _, _) = Setup(room, 220, 120);     // |dx| ≥ 64 y |dy| 64..95 → 1 + 2 = 3
            var (sysNear, _, _) = Setup(room, 70, 60);      // |dx| < 24 y |dy| < 32 → 2,5 + 3 = 5,5
            float far0 = sysFar.Enemies[0].Y, near0 = sysNear.Enemies[0].Y;
            sysFar.Tick();
            sysNear.Tick();
            Assert.AreEqual(3f, sysFar.Enemies[0].Y - far0, 0.01);
            Assert.AreEqual(5.5f, sysNear.Enemies[0].Y - near0, 0.01);
        }

        [Test]
        public void SproutCycleIsSeventyTwoTicksVisible()
        {
            var (sys, _, _) = Setup(Room(-1, new EntityJson { type = 0x2E, x = 64, y = 104 }));
            var sprout = sys.Enemies[0];
            int t = 0;
            while (!sprout.Visible && t++ < 400) sys.Tick();
            int visible = 0, harmful = 0;
            while (sprout.Visible && visible < 400) { if (sprout.Harmful) harmful++; visible++; sys.Tick(); }
            Assert.AreEqual(72, visible, "16 + 4·3 + 16 + 4·3 + 16");
            Assert.AreEqual(4 * 3 + 16 + 4 * 3, harmful, "los pasos 0 y 8 no dañan");
        }

        [Test]
        public void SkeletonCurlsAndDiesAfterEightQuickHits()
        {
            var (sys, motor, _) = Setup(Room(-1, new EntityJson { type = 0x25, x = 120, y = 104 }), 102, 120);
            var skel = sys.Enemies[0];
            int hits = 0;
            for (int k = 0; k < 20 && skel.Alive; k++)
            {
                // El jugador lo sigue: a 18 px de su borde, mirando hacia él.
                motor.Place(sys.Room, (int)skel.X - 18, 120);
                motor.Tick(new MotorInput { Right = true });
                motor.Place(sys.Room, (int)skel.X - 18, 120);
                motor.Tick(new MotorInput { Attack = true });
                if (motor.AttackStartedThisTick) { sys.Strike(motor, new List<int>()); hits++; }
                if (hits == 1) Assert.IsFalse(skel.Harmful, "enroscado no hace daño");
                for (int i = 0; i < 4; i++) { motor.Tick(new MotorInput()); sys.Tick(); }
            }
            Assert.IsFalse(skel.Alive);
            Assert.AreEqual(8, hits);
        }

        [Test]
        public void SwordReachesEighteenPixels()
        {
            foreach (var (dx, expected) in new[] { (18, true), (20, false) })
            {
                var (sys, motor, _) = Setup(Room(-1, new EntityJson { type = 0x0E, x = 120, y = 104 }), 120 - dx, 120);
                var bat = sys.Enemies[0];
                bat.X = 120; bat.Y = 104;
                motor.Tick(new MotorInput { Attack = true });
                sys.Strike(motor, new List<int>());
                Assert.AreEqual(!expected, bat.Alive, $"a {dx} px");
            }
        }

        [Test]
        public void SkeletonBoxIsTwentyFourWideOnTheSideItFaces()
        {
            // Tabla b14:9006: 4E = (1, 0, 15, 24) mirando a la derecha y 4F = (1, −8, 15, 24) a la izquierda; bordes
            // incluidos, así que cubren 25 × 16 px.
            var boxes = Enumerable.Range(0, 0x50).Select(_ => new BoxRect { h = 16, w = 16 }).ToArray();
            boxes[0x4E] = new BoxRect { dy = 1, dx = 0, h = 15, w = 24 };
            boxes[0x4F] = new BoxRect { dy = 1, dx = -8, h = 15, w = 24 };
            var catalog = Catalog.Select(i => i.type == 0x25 ? new EnemyInfo { type = 0x25, hp = 8, damage = 3, box = 0x4E } : i);
            var room = Room(-1, new EntityJson { type = 0x25, x = 120, y = 104 });
            var world = new RoomWorld(new[] { room });
            var sys = new EnemySystem(catalog, 7, null, boxes);
            sys.EnterRoom(world, room, new PlayerMotor(world, room, 40, 120));
            var skel = sys.Enemies[0];
            Assert.IsTrue(skel.FacingLeft, "mira al jugador");
            var (x0, y0, x1, y1) = skel.Box();
            Assert.AreEqual((112f, 105f, 137f, 121f), (x0, y0, x1, y1));
            skel.FacingLeft = false;
            (x0, _, x1, _) = skel.Box();
            Assert.AreEqual((120f, 145f), (x0, x1));
        }

        [Test]
        public void ContactBoxMatchesTheMeasurement()
        {
            // Medido con el esqueleto: daña cuando el borde derecho del jugador toca su borde izquierdo (x + 7 en la medida,
            // x + 6 en el código; se sigue el código).
            var (sys, motor, _) = Setup(Room(-1, new EntityJson { type = 0x18, x = 100, y = 104 }), 92, 120);
            sys.Enemies[0].X = 100;
            // Caja del jugador b03:B8A0: x − 6 … x + 6 (bordes incluidos).
            motor.Place(sys.Room, 93, 120);
            Assert.IsNull(sys.Touching(motor, new List<int>()), "x + 6 = 99: aún no toca");
            motor.Place(sys.Room, 94, 120);
            Assert.IsNotNull(sys.Touching(motor, new List<int>()), "x + 6 = 100: toca");
        }

        [Test]
        public void ImmunityItemMakesAnEnemyHarmless()
        {
            var (sys, motor, _) = Setup(Room(-1, new EntityJson { type = 0x31, x = 100, y = 104 }), 108, 120);
            var spikes = sys.Enemies[0];
            spikes.Visible = spikes.Harmful = true; spikes.Grow = 1;
            Assert.IsNotNull(sys.Touching(motor, new List<int>()));
            Assert.IsNull(sys.Touching(motor, new List<int> { 0x22 }), "el objeto 0x22 protege de los pinchos");
        }

        [Test]
        public void KnockbackFollowsTheMeasuredArc()
        {
            // Medido (castillo, sala 104): x 81 -> 85 -> 89 -> 93 -> 97 -> 101 -> 105 e y 56 -> 55 -> 54 -> 54 -> 54
            // -> 55 -> 56, y de pie. Aquí el suelo está en y = 120.
            var room = Room();
            var motor = new PlayerMotor(new RoomWorld(new[] { room }), room, 81, 120);
            motor.Knockback(1);
            var xs = new List<int>(); var ys = new List<int>();
            for (int t = 0; t < 6; t++) { motor.Tick(new MotorInput()); xs.Add(motor.X); ys.Add(motor.Y); }
            CollectionAssert.AreEqual(new[] { 85, 89, 93, 97, 101, 105 }, xs);
            CollectionAssert.AreEqual(new[] { 119, 118, 118, 118, 119, 120 }, ys);
            Assert.AreEqual(MotorState.Ground, motor.State);
        }
    
        // Espadazo desde el suelo (y = 120), con el enemigo bajado a esa altura.
        static void Hit(EnemySystem sys, PlayerMotor motor, Enemy e)
        {
            e.X = 120; e.Y = 104;
            motor.Place(sys.Room, 102, 120);
            motor.Tick(new MotorInput { Right = true });
            motor.Place(sys.Room, 102, 120);
            motor.Tick(new MotorInput { Attack = true });
            sys.Strike(motor, new List<int>());
            for (int i = 0; i < 4; i++) motor.Tick(new MotorInput());
        }

        [Test]
        public void DartRunsAtFourPixelsPerTick()
        {
            var (sys, _, _) = Setup(Room(-1, new EntityJson { type = 0x0B, x = 120, y = 104 }));
            float x0 = sys.Enemies[0].X;
            sys.Tick();
            Assert.AreEqual(4f, System.Math.Abs(sys.Enemies[0].X - x0), 0.01);
        }

        [Test]
        public void HedgehogShootsASpineAtThePlayer()
        {
            var (sys, _, _) = Setup(Room(-1, new EntityJson { type = 0x11, x = 120, y = 104 }), 90, 120);
            Enemy spine = null;
            for (int t = 0; t < 80 && spine == null; t++)
            {
                sys.Tick();
                spine = sys.Enemies.FirstOrDefault(e => e.Type == 0x1D);
            }
            Assert.IsNotNull(spine, "dispara antes de 60 ticks si el jugador está a menos de 65 px");
            Assert.AreEqual(-4f, spine.Vx, 0.01, "a 4 px/tick hacia el jugador");
        }

        [Test]
        public void SplitFlyBecomesTwoThenFour()
        {
            var (sys, motor, _) = Setup(Room(-1, new EntityJson { type = 0x49, x = 120, y = 80 }));
            sys.Tick();
            Hit(sys, motor, sys.Enemies[0]);
            Assert.AreEqual(2, sys.Enemies.Count(e => e.Alive && e.Type == 0x52));
            for (int i = 0; i < 8; i++) sys.Tick();
            foreach (var fly in sys.Enemies.Where(e => e.Alive && e.Type == 0x52).ToList())
                Hit(sys, motor, fly);
            Assert.AreEqual(4, sys.Enemies.Count(e => e.Alive && e.Type == 0x53));
        }

        [Test]
        public void FusingBatsMergeIntoABiggerOne()
        {
            var (sys, _, _) = Setup(Room(-1, new EntityJson { type = 0x19, x = 100, y = 80 }, new EntityJson { type = 0x19, x = 140, y = 80 }));
            bool merged = false;
            for (int t = 0; t < 400 && !merged; t++)
            {
                sys.Tick();
                merged = sys.Enemies.OfType<FusingBat>().Any(b => b.Alive && b.Level == 1);
            }
            Assert.IsTrue(merged);
            Assert.AreEqual(1, sys.Enemies.Count(e => e.Alive && e.Type == 0x19));
            Assert.AreEqual(2, sys.Enemies.First(e => e.Alive).Hp);
        }

        [Test]
        public void TeleporterAlwaysAppearsOnAPlatform()
        {
            var (sys, _, world) = Setup(Room(-1, new EntityJson { type = 0x2A, x = 48, y = 104 }));
            var ghost = sys.Enemies[0];
            int seen = 0;
            for (int t = 0; t < 600; t++)
            {
                sys.Tick();
                if (!ghost.Visible) continue;
                seen++;
                Assert.IsTrue(world.Solid(sys.Room, (int)ghost.X, (int)ghost.Y + 16), $"suelo bajo ({ghost.X},{ghost.Y})");
                Assert.IsFalse(world.Solid(sys.Room, (int)ghost.X, (int)ghost.Y + 15));
            }
            Assert.Greater(seen, 0);
        }

        [Test]
        public void InchWormHeadAndTailTakeTurns()
        {
            var (sys, _, _) = Setup(Room(-1, new EntityJson { type = 0x2C, x = 80, y = 104 }, new EntityJson { type = 0x64, x = 64, y = 104 }));
            var head = sys.Enemies[0];
            var tail = sys.Enemies[1];
            int both = 0, moves = 0;
            for (int t = 0; t < 400; t++)
            {
                float h = head.X, tl = tail.X;
                sys.Tick();
                bool hm = head.X != h, tm = tail.X != tl;
                if (hm && tm) both++;
                if (hm || tm) moves++;
            }
            Assert.AreEqual(0, both, "nunca se mueven a la vez");
            Assert.Greater(moves, 100);
            head.Alive = false;
            float run = 0;
            for (int t = 0; t < 40; t++) { float x = tail.X; sys.Tick(); run += System.Math.Abs(tail.X - x); }
            Assert.Greater(run, 40, "sola, corre a 2 px/tick");
        }

        [Test]
        public void ChainOrbsRelayOneAtATime()
        {
            var spawns = Enumerable.Range(0, 9).Select(i => new EntityJson { type = 0x36, x = 24 + 24 * i, y = 72 }).ToArray();
            var (sys, _, _) = Setup(Room(-1, spawns));
            int landings = 0;
            int before = sys.Enemies.OfType<ChainOrb>().Count(o => o.State == 2 || o.State == 1);
            for (int t = 0; t < 600; t++)
            {
                var flying = sys.Enemies.OfType<ChainOrb>().Where(o => o.State != 0).ToList();
                sys.Tick();
                Assert.LessOrEqual(sys.Enemies.OfType<ChainOrb>().Count(o => o.State == 2), 1, "solo una en vuelo");
                landings += flying.Count(o => o.State == 0);
            }
            Assert.AreEqual(1, before);
            Assert.Greater(landings, 3);
        }

        [Test]
        public void RollingBombStopsAtThePlayersXAndExplodes()
        {
            var (sys, _, _) = Setup(Room(-1, new EntityJson { type = 0x47, x = 40, y = 104 }), 160, 120);
            var bomb = sys.Enemies[0];
            for (int t = 0; t < 200 && bomb.Width == 16; t++) sys.Tick();
            Assert.AreEqual(32, bomb.Width, "explota (32 × 32)");
            Assert.AreEqual(152 - 8, bomb.X, 4, "se paró en la x del jugador");
            for (int t = 0; t < 17; t++) sys.Tick();
            Assert.IsFalse(bomb.Alive);
        }
    
        // ---------- botín (docs/items_enemies.md, "Muerte y botín") ----------

        [Test]
        public void LootRulesCountKillsPerTypeWithTwoTypesTracked()
        {
            var (sys, _, _) = Setup(Room());
            // Regla 0: 1.ª y 3.ª muerte; regla 2: 4.ª y 6.ª.
            CollectionAssert.AreEqual(new[] { true, false, true, false, false }, Enumerable.Range(0, 5).Select(_ => sys.LootRoll(0x0E)));
            CollectionAssert.AreEqual(new[] { false, false, false, true, false, true }, Enumerable.Range(0, 6).Select(_ => sys.LootRoll(0x25)));
            Assert.IsFalse(sys.LootRoll(0x18), "un tercer tipo no se sigue: nunca suelta nada");
        }

        [Test]
        public void LastAliveRuleWaitsForTheLastOne()
        {
            var (sys, _, _) = Setup(Room(-1, new EntityJson { type = 0x19, x = 100, y = 80 }, new EntityJson { type = 0x19, x = 140, y = 80 }));
            Assert.IsFalse(sys.LootRoll(0x19), "queda otro vivo");
            sys.Enemies[0].Alive = false;
            sys.Enemies[1].Alive = false;
            Assert.IsTrue(sys.LootRoll(0x19));
        }

        [Test]
        public void KilledBatLeavesCoinsAfterTheExplosion()
        {
            var room = Room(3, new EntityJson { type = 0x0E, x = 123, y = 105 });
            var world = new RoomWorld(new[] { room });
            var motor = new PlayerMotor(world, room, 200, 120);
            var loot = new LootTable { coinBelow = new[] { 16, 16, 16, 16 } };   // siempre monedas
            var sys = new EnemySystem(Catalog, 7, loot);
            sys.EnterRoom(world, room, motor);
            var bat = sys.Enemies[0];
            Hit(sys, motor, bat);
            Assert.IsFalse(bat.Alive);
            Assert.IsTrue(sys.Enemies.Any(e => e.Type == EnemySystem.ExplosionType));
            for (int t = 0; t < EnemySystem.ExplosionTicks - 1; t++) sys.Tick();
            Assert.IsFalse(sys.Enemies.Any(e => e is LootDrop), "aún explotando");
            sys.Tick();
            var drop = sys.Enemies.OfType<LootDrop>().Single();
            Assert.AreEqual(0x5E, drop.Type);
            Assert.AreEqual(1, drop.Amount, "nivel 0: 1 moneda");
            Assert.AreEqual(120, drop.X, "x alineada a 8");
            Assert.AreEqual(104, drop.Y, "y alineada a 8");
            for (int t = 0; t < 300; t++) sys.Tick();
            Assert.IsTrue(drop.Alive, "no caduca");
            motor.Place(sys.Room, 120 - 7, 120);
            Assert.IsNull(sys.PickUp(motor), "x − 7: fuera");
            motor.Place(sys.Room, 120 - 6, 120);
            Assert.AreSame(drop, sys.PickUp(motor));
        }
    
        // ---------- generadores periódicos (docs/entities.md) ----------

        static List<int> SpawnTicks(RoomModel room, int ticks, int type, ICollection<int> items = null)
        {
            var (sys, _, _) = Setup(room);
            if (items != null) sys.HasItem = items.Contains;
            sys.EnterRoom(sys.Context.World, room, sys.Context.Player);
            var seen = new HashSet<Enemy>();
            var at = new List<int>();
            for (int t = 1; t <= ticks; t++)
            {
                sys.Tick();
                foreach (var e in sys.Enemies.Where(e => e.Type == type && seen.Add(e)))
                    at.Add(t);
            }
            return at;
        }

        [Test]
        public void BatchGeneratorThrowsThreeEveryHundredSeventySevenTicks()
        {
            // E650 (castillo, sala 58): intentos en 113, 145, 177, 290, 322, 354…, en x 0x60, 0xC0, 0x40.
            var room = Room();
            room.periodic = new[] { new PeriodicJson { gen = "E650", type = 0x09, firstDelay = 113, period = 32, batch = 3, batchGap = 113,
                                                       y = 0, xRule = "sequence", xSequence = new[] { 0x60, 0xC0, 0x40 } } };
            CollectionAssert.AreEqual(new[] { 113, 145, 177, 290, 322, 354 }, SpawnTicks(room, 360, 0x09));
        }

        [Test]
        public void LimitedGeneratorStopsAndCarpetDisablesOne()
        {
            var room = Room();
            room.periodic = new[] { new PeriodicJson { gen = "E668", type = 0x3E, firstDelay = 16, period = 16, maxTotal = 5,
                                                       x = 0x40, y = 0x28, xRule = "sequence", xSequence = new[] { 0x40 } } };
            Assert.AreEqual(5, SpawnTicks(room, 400, 0x3E).Count, "el E668 se para tras 5 intentos");
            room.periodic[0].disabledByItem = 0x18;
            Assert.AreEqual(0, SpawnTicks(room, 400, 0x3E, new List<int> { 0x18 }).Count, "con el Carpet no se arma");
        }

        [Test]
        public void BouncingBallBouncesOnceThenFallsThrough()
        {
            // Suelo en y = 120; nace arriba y rebota en él una vez (vy −4) y luego lo atraviesa.
            var (sys, _, _) = Setup(Room(-1, new EntityJson { type = 0x09, x = 64, y = 40 }));
            var ball = sys.Enemies[0];
            float lowest = 0;
            bool bounced = false;
            for (int t = 0; t < 120 && ball.Alive; t++)
            {
                sys.Tick();
                if (ball.Vy == -4) { bounced = true; Assert.AreEqual(104, ball.Y, "rebota sobre el suelo"); }
                lowest = System.Math.Max(lowest, ball.Y);
            }
            Assert.IsTrue(bounced);
            Assert.IsFalse(ball.Alive, "después cae y se borra al llegar a y = 0xB8");
        }
    }
}
