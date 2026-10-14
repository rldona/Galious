using System.Collections.Generic;
using System.Linq;
using MazeOfKnight.Data;
using MazeOfKnight.Game;
using MazeOfKnight.Game.Enemies;
using NUnit.Framework;
using UnityEngine;

namespace MazeOfKnight.Tests
{
    // Armas secundarias contra docs/player.md ("Armas secundarias", medido en openMSX). Datos reales de weapons.json.
    public class WeaponTests
    {
        static WeaponsData Data()
        {
            var json = Resources.Load<TextAsset>("Weapons");
            if (json == null) Assert.Ignore("Sin Resources/Weapons.json importado");
            return JsonUtility.FromJson<WeaponsData>(json.text);
        }

        // Suelo en la fila 11 (y = 120); 'extra' añade sólidos por fila: (fila, desde columna, hasta columna).
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
            return new RoomModel { id = 1, collision = rows };
        }

        class Rig
        {
            public RoomWorld World;
            public PlayerMotor Motor;
            public EnemySystem Enemies;
            public WeaponSystem Weapons;
            public List<int> Items = new List<int>();

            public Projectile Fire(int id) =>
                Weapons.CanFire(Weapons.Data.Get(id), Motor, Items, 99) == null
                    ? Weapons.Fire(Weapons.Data.Get(id), Motor, World, Enemies, null, Items) : null;

            public void Tick(int n = 1)
            {
                for (int i = 0; i < n; i++) { Enemies.Tick(); Weapons.Tick(Motor, World, Enemies, null, Items); }
            }
        }

        static Rig Setup(RoomModel room, int x = 64, int y = 120, Character c = Character.Popolon, params EntityJson[] spawns)
        {
            room.spawns = spawns;
            var world = new RoomWorld(new[] { room });
            var rig = new Rig { World = world, Motor = new PlayerMotor(world, room, x, y) { Character = c } };
            rig.Enemies = new EnemySystem(new[] { new EnemyInfo { type = 0x0E, hp = 1, damage = 1 }, new EnemyInfo { type = 0x25, hp = 8, damage = 3 } }, 3);
            rig.Enemies.EnterRoom(world, room, rig.Motor);
            rig.Weapons = new WeaponSystem(Data());
            return rig;
        }

        [Test]
        public void ArrowFliesEightPixelsPerTickAndStopsAtAWall()
        {
            var rig = Setup(Room((10, 20, 20)));       // pared en x = 160, a la altura de la flecha
            var arrow = rig.Fire(1);
            Assert.AreEqual(64 + 8, arrow.X, "primera posición visible x + 8");
            Assert.AreEqual(112, arrow.Y, "(y − 8) redondeada a 8");
            rig.Tick();
            Assert.AreEqual(80, arrow.X);
            for (int t = 0; t < 20 && arrow.Alive; t++) rig.Tick();
            Assert.IsFalse(arrow.Alive);
            Assert.Less(arrow.X, 160);
        }

        [Test]
        public void PopolonFiresTwoArrowsAphroditeThreeEarringsThree()
        {
            int Count(Character c, int? item)
            {
                var rig = Setup(Room(), c: c);
                if (item.HasValue) rig.Items.Add(item.Value);
                return Enumerable.Range(0, 5).Count(_ => rig.Fire(1) != null);
            }
            Assert.AreEqual(2, Count(Character.Popolon, null));
            Assert.AreEqual(3, Count(Character.Aphrodite, null));
            Assert.AreEqual(3, Count(Character.Popolon, 0x1D), "Earrings");
        }

        [Test]
        public void ArrowKillsABatAndDisappears()
        {
            var rig = Setup(Room(), spawns: new EntityJson { type = 0x0E, x = 100, y = 104 });
            var bat = rig.Enemies.Enemies[0];
            var arrow = rig.Fire(1);
            for (int t = 0; t < 6 && arrow.Alive; t++) { bat.X = 100; bat.Y = 104; rig.Weapons.Tick(rig.Motor, rig.World, rig.Enemies, null, rig.Items); }
            Assert.IsFalse(bat.Alive);
            Assert.IsFalse(arrow.Alive);
        }

        [Test]
        public void CeramicArrowGoesThroughEnemies()
        {
            var rig = Setup(Room(), spawns: new EntityJson { type = 0x25, x = 100, y = 104 });
            var skel = rig.Enemies.Enemies[0];
            var arrow = rig.Fire(2);
            for (int t = 0; t < 30 && arrow.Alive; t++) { skel.X = 100; skel.Y = 104; rig.Weapons.Tick(rig.Motor, rig.World, rig.Enemies, null, rig.Items); }
            Assert.Less(skel.Hp, 8, "le ha dado");
            Assert.Greater(arrow.X, 130, "y ha seguido");
        }

        [Test]
        public void FireOnlyFromTheGroundAndFallsOffLedges()
        {
            // Plataforma de x 0 a 95 en la fila 7 (y = 88) sobre el suelo de y = 120.
            var rig = Setup(Room((7, 0, 11)), x: 80, y: 88);
            var fire = rig.Fire(4);
            Assert.IsNotNull(fire);
            rig.Tick();
            Assert.AreEqual(88 - 4, fire.Y, "va a 4 px del suelo");
            for (int t = 0; t < 30 && fire.Alive && fire.Y < 116; t++) rig.Tick();
            Assert.AreEqual(120 - 4, fire.Y, "ha caído al suelo de abajo");
            var air = Setup(Room(), y: 100);
            air.Motor.Tick(new MotorInput());
            Assert.IsNull(air.Fire(4), "en el aire no");
            Assert.IsNotNull(air.Fire(1), "la flecha sí");
        }

        [Test]
        public void RollingFireGoesRoundThePlatformOnce()
        {
            // Plataforma de x 64 a 127 en la fila 7 (y = 88 a 95).
            var rig = Setup(Room((7, 8, 15)), x: 96, y: 88);
            var fire = rig.Fire(3);
            float maxY = 0;
            int t;
            for (t = 0; t < 200 && fire.Alive; t++) { rig.Tick(); maxY = Mathf.Max(maxY, fire.Y); }
            Assert.IsFalse(fire.Alive, "muere tras una vuelta");
            Assert.AreEqual(96, maxY, "pasa por debajo (y = 96)");
            Assert.That(fire.X, Is.InRange(88f, 100f), "cerca de donde nació");
        }

        [Test]
        public void MineArmsWhenThePlayerLeavesAndExplodesOnAnEnemy()
        {
            var rig = Setup(Room(), spawns: new EntityJson { type = 0x25, x = 200, y = 104 });
            var skel = rig.Enemies.Enemies[0];
            var mine = rig.Fire(5);
            Assert.IsNotNull(mine);
            rig.Weapons.Tick(rig.Motor, rig.World, rig.Enemies, null, rig.Items);
            Assert.IsFalse(mine.Armed, "con el jugador encima no se arma");
            rig.Motor.Place(rig.Motor.Room, 140, 120);
            rig.Weapons.Tick(rig.Motor, rig.World, rig.Enemies, null, rig.Items);
            Assert.IsTrue(mine.Armed);
            skel.X = mine.X; skel.Y = 104;
            rig.Weapons.Tick(rig.Motor, rig.World, rig.Enemies, null, rig.Items);
            Assert.IsTrue(mine.Exploding);
            int ticks = 0;
            while (mine.Alive && ticks++ < 100) { skel.X = mine.X + 8; skel.Y = 104; rig.Enemies.Context.Tick++; rig.Weapons.Tick(rig.Motor, rig.World, rig.Enemies, null, rig.Items); }
            Assert.AreEqual(70, ticks, "la explosión dura 70 ticks");
            Assert.AreEqual(8 - 2 * 2, skel.Hp, "2 por golpe cada 32 ticks: 2 golpes en 70");
        }
    }
}
