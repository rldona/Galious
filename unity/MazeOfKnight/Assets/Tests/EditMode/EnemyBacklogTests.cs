using System.Collections.Generic;
using System.Linq;
using MazeOfKnight.Data;
using MazeOfKnight.Game;
using MazeOfKnight.Game.Enemies;
using NUnit.Framework;
using UnityEngine;

namespace MazeOfKnight.Tests
{
    // Escudos, cajas por fotograma, experiencia del 0x19 y sondeos de salto (docs/backlog.md, "Enemigos y combate").
    public class EnemyBacklogTests
    {
        static readonly EnemyInfo[] Catalog =
        {
            new EnemyInfo { type = 0x0A, hp = 6, damage = 4, box = 0x4E },
            new EnemyInfo { type = 0x0E, hp = 1, damage = 1, box = 0x69 },
            new EnemyInfo { type = 0x0F, hp = 6, damage = 2, box = 0x32 },
            new EnemyInfo { type = 0x19, hp = 1, damage = 1, box = 0x4A },
            new EnemyInfo { type = 0x25, hp = 8, damage = 3, box = 0x4E },
            new EnemyInfo { type = 0x31, invulnerable = true, damage = 2, box = 0x1B },
            new EnemyInfo { type = 0x36, invulnerable = true, damage = 6, box = 0x12 },
            new EnemyInfo { type = 0x04, hp = 1, damage = 2, box = 0x22 },
        };

        // Tabla de cajas sintética: la i tiene alto i & 15 para distinguirlas; las que importan, como en b14:9006.
        static BoxRect[] Boxes()
        {
            var t = Enumerable.Range(0, 0x7A).Select(i => new BoxRect { h = i & 15, w = 15 }).ToArray();
            t[0x22] = new BoxRect { dy = 0, dx = 0, h = 7, w = 8 };
            t[0x11] = t[0x12] = new BoxRect { dy = 0, dx = 0, h = 16, w = 15 };
            t[0x4E] = new BoxRect { dy = 1, dx = 0, h = 15, w = 24 };
            t[0x4F] = new BoxRect { dy = 1, dx = -8, h = 15, w = 24 };
            return t;
        }

        // Suelo en la fila 11 (y = 120); 'extra' añade sólidos (fila, desde columna, hasta columna).
        static RoomModel Room(params (int row, int c0, int c1)[] extra)
        {
            var rows = new string[20];
            for (int r = 0; r < 20; r++)
            {
                var row = Enumerable.Repeat(r >= 11 ? '#' : '.', 32).ToArray();
                foreach (var (er, c0, c1) in extra)
                    if (er == r) for (int c = c0; c <= c1; c++) row[c] = '#';
                rows[r] = new string(row);
            }
            return new RoomModel { id = 1, collision = rows, spawns = new EntityJson[0] };
        }

        static (EnemySystem, PlayerMotor, RoomWorld) Setup(RoomModel room, int px = 200, bool boxes = true)
        {
            var world = new RoomWorld(new[] { room });
            var motor = new PlayerMotor(world, room, px, 120);
            var sys = new EnemySystem(Catalog, 7, null, boxes ? Boxes() : null);
            sys.EnterRoom(world, room, motor);
            return (sys, motor, world);
        }

        // El jugador en x mirando a la izquierda o a la derecha da un espadazo.
        static void Slash(EnemySystem sys, PlayerMotor motor, int x, bool left)
        {
            motor.Place(sys.Room, x, 120);
            motor.Tick(new MotorInput { Left = left, Right = !left });
            motor.Place(sys.Room, x, 120);
            motor.Tick(new MotorInput { Attack = true });
            sys.Strike(motor, new List<int>());
        }

        static WeaponsData Weapons()
        {
            var json = Resources.Load<TextAsset>("Weapons");
            if (json == null) Assert.Ignore("Sin Resources/Weapons.json importado");
            return JsonUtility.FromJson<WeaponsData>(json.text);
        }

        // ---------- escudos ----------

        [Test]
        public void SwordClinksOnAnOrbWithoutHurtingIt()
        {
            var (sys, motor, _) = Setup(Room());
            var orb = sys.Spawn(0x36, 120, 104);
            int clinks = 0;
            sys.Clinked += _ => clinks++;
            Slash(sys, motor, 102, false);
            Assert.AreEqual(1, clinks, "IX+2 bit 2: «clinc»");
            Assert.IsTrue(orb.Alive);
        }

        [Test]
        public void KnightIsOnlyHurtFromBehind()
        {
            var (sys, motor, _) = Setup(Room());
            var knight = sys.Spawn(0x0A, 120, 104);
            Assert.IsTrue(knight.ShieldSide);
            int clinks = 0;
            sys.Clinked += _ => clinks++;
            knight.FacingLeft = true;                 // mira al jugador, que está a la izquierda
            Slash(sys, motor, 102, false);
            Assert.AreEqual(6, knight.Hp, "de frente el golpe se pierde");
            Assert.AreEqual(0, clinks, "sin «clinc» (solo el bit 2 suena)");
            knight.FacingLeft = false;                // de espaldas al jugador
            Slash(sys, motor, 102, false);
            Assert.AreEqual(5, knight.Hp, "por la espalda vale");
        }

        [Test]
        public void ArrowBouncesOffAShield()
        {
            var room = Room();
            var (sys, motor, world) = Setup(room, 64);
            var orb = sys.Spawn(0x36, 100, 104);
            var weapons = new WeaponSystem(Weapons());
            Enemy bounced = null;
            weapons.Deflected += (p, e) => bounced = e;
            var arrow = weapons.Fire(weapons.Data.Get(1), motor, world, sys, null, new List<int>());
            for (int t = 0; t < 10 && !arrow.Deflected; t++)
            {
                orb.X = 100; orb.Y = 104;
                weapons.Tick(motor, world, sys, null, new List<int>());
            }
            Assert.IsTrue(arrow.Deflected, "rebota");
            Assert.AreSame(orb, bounced);
            Assert.IsTrue(orb.Alive);
            float x = arrow.X, y = arrow.Y;
            weapons.Tick(motor, world, sys, null, new List<int>());
            Assert.AreEqual(-8, arrow.Y - y, 0.01, "sube a 8 px/tick");
            Assert.Less(arrow.X, x, "con vx invertida");
        }

        [Test]
        public void ArrowHurtsTheSkeletonOnlyFromBehind()
        {
            foreach (bool facingArrow in new[] { true, false })
            {
                var (sys, motor, world) = Setup(Room(), 64);
                var skel = sys.Spawn(0x25, 100, 104);
                var weapons = new WeaponSystem(Weapons());
                var arrow = weapons.Fire(weapons.Data.Get(1), motor, world, sys, null, new List<int>());
                for (int t = 0; t < 10 && arrow.Alive && !arrow.Deflected; t++)
                {
                    skel.X = 100; skel.Y = 104; skel.FacingLeft = facingArrow;
                    weapons.Tick(motor, world, sys, null, new List<int>());
                }
                Assert.AreEqual(facingArrow, arrow.Deflected);
                Assert.AreEqual(facingArrow ? 8 : 7, skel.Hp);
            }
        }

        // ---------- cajas por fotograma ----------

        [Test]
        public void BatChangesBoxWhenItLetsGo()
        {
            // Techo en la fila 3 (y 56..63): cuelga justo debajo.
            var (sys, _, _) = Setup(Room((3, 0, 31)));
            var bat = sys.Spawn(0x0E, 64, 64);
            Assert.AreEqual(0x67, bat.BoxIndex, "colgado");
            for (int t = 0; t < 81; t++) sys.Tick();
            Assert.That(bat.BoxIndex, Is.EqualTo(0x69).Or.EqualTo(0x0F), "se suelta y revolotea");
        }

        [Test]
        public void SkeletonCurlBoxesFollowTable9A61()
        {
            var (sys, motor, _) = Setup(Room(), 40);
            var skel = sys.Spawn(0x25, 120, 104);
            Assert.AreEqual(0x4E, skel.BoxIndex);
            Assert.IsNotNull(skel.LeftBox, "24 de ancho hacia donde mira");
            skel.FacingLeft = false;
            Slash(sys, motor, (int)skel.X - 18, false);
            Assert.AreEqual(0x4D, skel.BoxIndex, "enroscado un paso");
        }

        [Test]
        public void GrowingBallBoxGrowsWithEachHit()
        {
            var (sys, _, _) = Setup(Room());
            var ball = (GrowingBall)sys.Spawn(0x0F, 120, 104);
            int[] right = { 0x33, 0x35, 0x36, 0x00 }, left = { 0x32, 0x34, 0x36, 0x00 };
            for (int stage = 0; stage < 4; stage++)
            {
                sys.Tick();
                Assert.AreEqual((ball.FacingLeft ? left : right)[stage], ball.BoxIndex, $"etapa {stage}");
                sys.Damage(ball, 1);
            }
        }

        [Test]
        public void SpikeBoxesComeFromTable8C17()
        {
            var (sys, _, _) = Setup(Room());
            var spikes = sys.Spawn(0x31, 64, 104);
            var seen = new HashSet<int>();
            for (int t = 0; t < 600; t++)
            {
                sys.Tick();
                if (spikes.Visible) seen.Add(spikes.BoxIndex);
            }
            CollectionAssert.IsSubsetOf(new[] { 0x72, 0x73, 0x74, 0x75 }, seen);
            CollectionAssert.IsSubsetOf(seen, new[] { 0x72, 0x73, 0x74, 0x75 }, "desde el suelo, sin las del techo");
        }

        [Test]
        public void PistonOnlyHurtsWithItsHead()
        {
            var (sys, motor, _) = Setup(Room(), 200);
            var piston = sys.Spawn(0x04, 64, 40);
            for (int t = 0; t < 15 * 5; t++) sys.Tick();
            Assert.AreEqual(40 + 8 * 5, piston.Y, 0.01, "la y del enemigo es la cabeza");
            var (x0, y0, x1, y1) = piston.Box();
            Assert.AreEqual((64f, 80f, 73f, 88f), (x0, y0, x1, y1), "caja 22 en la cabeza");
            Assert.AreEqual(40f, piston.DrawRect().y0, "se dibuja desde el techo");
        }

        // ---------- experiencia y daño del 0x19 por tamaño ----------

        [Test]
        public void FusingBatExperienceAndDamageDependOnSize()
        {
            var helmet = new Inventory(new ItemsData(), new List<int> { Inventory.HatHelmet });
            int[] exp = { 1, 2, 4 }, damage = { 1, 2, 4 }, helmeted = { 1, 1, 2 };
            for (int level = 0; level < 3; level++)
            {
                var (sys, _, _) = Setup(Room());
                sys.Context.FusionLevel = level;
                var bat = (FusingBat)sys.Spawn(0x19, 100, 80);
                Assert.AreEqual(level, bat.Level);
                Assert.AreEqual(exp[level], bat.ExperienceOverride, $"experiencia del nivel {level}");
                Assert.AreEqual(damage[level], bat.Damage);
                Assert.AreEqual(helmeted[level], helmet.ReduceDamage(bat.Damage), "con el Hat Helmet");
            }
        }

        [Test]
        public void OtherEnemiesUseTheCatalogExperience()
        {
            var (sys, _, _) = Setup(Room());
            Assert.IsNull(sys.Spawn(0x0E, 100, 80).ExperienceOverride);
        }

        // ---------- simplificaciones de las IA ----------

        class Prober : Enemy
        {
            public override void Tick(EnemyContext ctx) { }
            public bool Probe(EnemyContext ctx, int kind) => ProbeLevelJump(ctx, kind);
        }

        [Test]
        public void LevelJumpProbeNeedsAGapAndAPlatform()
        {
            // Plataforma de arriba en la fila 7 (y = 88), columnas 14..20 (x 112..167); el enemigo, en x 64 y = 104.
            var room = Room((7, 14, 20));
            var (sys, _, _) = Setup(room);
            var e = new Prober { Room = room, X = 64, Y = 104 };
            Assert.IsTrue(e.Probe(sys.Context, 1), "hueco libre en x + 32 y suelo en x + 64, 16 px más arriba");
            Assert.IsFalse(e.Probe(sys.Context, 2), "abajo no hay hueco: el suelo sigue");
            var blocked = Room((7, 12, 20));
            var (sys2, _, _) = Setup(blocked);
            Assert.IsFalse(new Prober { Room = blocked, X = 64, Y = 104 }.Probe(sys2.Context, 1), "sin hueco en x + 32");
            e.FacingLeft = true;
            Assert.IsFalse(e.Probe(sys.Context, 1), "a la izquierda no hay plataforma");
        }

        [Test]
        public void HeadsInWorld9Room19StayInAimedMode()
        {
            foreach (bool forced in new[] { true, false })
            {
                var room = Room();
                room.id = forced ? 0x19 : 0x18;
                var world = new RoomWorld(new[] { room });
                var motor = new PlayerMotor(world, room, 120, 120);
                var sys = new EnemySystem(Catalog, 5);
                sys.Context.E041 = 10;
                room.spawns = new[] { new EntityJson { type = 0x69, x = 64, y = 64 }, new EntityJson { type = 0x69, x = 128, y = 64 } };
                sys.EnterRoom(world, room, motor);
                var leader = sys.Enemies.OfType<FloatingHead>().First();
                bool sawAimed = false, backToRandom = false;
                for (int t = 0; t < 6000; t++)
                {
                    sys.Tick();
                    if (leader.Mode == 1) sawAimed = true;
                    else if (sawAimed) backToRandom = true;
                }
                Assert.IsTrue(sawAimed);
                Assert.AreEqual(!forced, backToRandom, forced ? "forzado: no vuelve al azar" : "fuera de la sala alterna");
            }
        }
    }
}
