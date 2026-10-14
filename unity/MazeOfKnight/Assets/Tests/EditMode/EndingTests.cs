using System.Collections.Generic;
using System.Linq;
using MazeOfKnight.Data;
using MazeOfKnight.Game;
using MazeOfKnight.Game.Enemies;
using NUnit.Framework;
using UnityEngine;

namespace MazeOfKnight.Tests
{
    // Final del juego contra docs/ending.md (datos reales del mundo 10).
    public class EndingTests
    {
        static (EnemySystem sys, RoomWorld world, RoomModel room) RockRoom(int seed)
        {
            var map = Resources.LoadAll<MapData>("Maps").First(m => m.mapName == "world10");
            var world = new RoomWorld(map.rooms.Select(r => r.room));
            var room = world.Room(6);
            Assert.IsNotNull(room, "la sala 6 del mundo 10 está en los datos");
            var catalog = Resources.Load<EnemyCatalog>("Enemies");
            var sys = new EnemySystem(catalog.enemies, seed, catalog.loot, catalog.boxRects);
            sys.EnterRoom(world, room, new PlayerMotor(world, room, 24, 184));
            return (sys, world, room);
        }

        [Test]
        public void TenRocksAndTheBabyUnderOneOrNone()
        {
            var counts = new int[11];
            for (int seed = 1; seed <= 640; seed++)
            {
                var (sys, _, _) = RockRoom(seed);
                var rocks = sys.Enemies.OfType<BabyRock>().ToList();
                Assert.AreEqual(10, rocks.Count);
                int i = rocks.FindIndex(r => r.HasBaby);
                counts[i < 0 ? 10 : i]++;
                Assert.LessOrEqual(rocks.Count(r => r.HasBaby), 1);
            }
            // (0..3) + (0..7): 3–7 salen 4 de cada 32; 0 y 10 (sin bebé), 1 de cada 32.
            Assert.Greater(counts[5], counts[0] * 2);
            Assert.Greater(counts[10], 0, "a veces no hay bebé (1/32), como en el original");
        }

        [Test]
        public void BrokenRocksLeaveTheBabyOrASpiralGhost()
        {
            var (sys, _, _) = RockRoom(5);
            var rocks = sys.Enemies.OfType<BabyRock>().ToList();
            foreach (var r in rocks) r.HasBaby = false;
            rocks[3].HasBaby = true;
            foreach (var r in rocks) sys.Damage(r, 5);
            Assert.AreEqual(1, sys.Enemies.Count(e => e.Alive && e is Baby));
            Assert.AreEqual(9, sys.Enemies.Count(e => e.Alive && e is SpiralGhost));
            Assert.IsFalse(sys.Enemies.Any(e => e is LootDrop || e is DeathExplosion), "sin botín ni explosión");
            for (int t = 0; t < 400; t++) sys.Tick();
            Assert.AreEqual(0, sys.Enemies.Count(e => e.Alive && e is SpiralGhost), "los fantasmas salen de la pantalla");
            Assert.AreEqual(1, sys.Enemies.Count(e => e.Alive && e is Baby));
        }

        [Test]
        public void AfterGaliousOnlyTheRightExitOpensToRoomSix()
        {
            var json = Resources.Load<TextAsset>("Bosses");
            if (json == null) Assert.Ignore("Sin Bosses.json");
            var data = JsonUtility.FromJson<BossesData>(json.text);
            var map = Resources.LoadAll<MapData>("Maps").First(m => m.mapName == "world10");
            var world = new RoomWorld(map.rooms.Select(r => r.room));
            var room = world.Room(data.ForWorld(10).room);
            Assert.AreEqual(6, room.afterBossRight);
            var catalog = Resources.Load<EnemyCatalog>("Enemies");
            var motor = new PlayerMotor(world, room, 40, 184);
            var sys = new EnemySystem(catalog.enemies, 3, catalog.loot, catalog.boxRects);
            sys.EnterRoom(world, room, motor);
            var fight = new BossFight(data);
            fight.EnterRoom(10, room);
            sys.BossDefeated += _ => fight.OnBossDefeated();
            foreach (var c in data.ForWorld(10).word) fight.Type(c);
            for (int t = 0; t < 600 && fight.Current != BossFight.Stage.Fighting; t++) { sys.Tick(); fight.Tick(sys, () => sys.SpawnBoss(fight.Info, false, false, true)); }
            sys.HitBoss(fight.Boss, 999);
            for (int t = 0; t < 70; t++) fight.Tick(sys, () => null);
            Assert.AreEqual(BossFight.Stage.RightExitOnly, fight.Current, "sin llave");
            Assert.AreEqual(6, room.right);
            fight.EnterRoom(10, world.Room(6));
            Assert.AreEqual(BossFight.Stage.Idle, fight.Current);
        }
    }
}
