using System.Collections.Generic;
using System.Linq;
using MazeOfKnight.Data;
using MazeOfKnight.Game;
using MazeOfKnight.Game.Enemies;
using NUnit.Framework;
using UnityEngine;

namespace MazeOfKnight.Tests
{
    // Jefes contra docs/boss_ai.md (datos reales de bosses.json y de las salas).
    public class BossTests
    {
        static BossesData Data()
        {
            var json = Resources.Load<TextAsset>("Bosses");
            if (json == null) Assert.Ignore("Sin Resources/Bosses.json importado");
            return JsonUtility.FromJson<BossesData>(json.text);
        }

        static (BossFight fight, EnemySystem sys, RoomWorld world, RoomModel room, PlayerMotor motor) Setup(int worldNumber)
        {
            var data = Data();
            var info = data.ForWorld(worldNumber);
            var map = Resources.LoadAll<MapData>("Maps").First(m => m.mapName == OriginalWorlds.MapName(worldNumber));
            var world = new RoomWorld(map.rooms.Select(r => r.room));
            var room = world.Room(info.room);
            var motor = new PlayerMotor(world, room, 40, 184);
            var catalog = Resources.Load<EnemyCatalog>("Enemies");
            var sys = new EnemySystem(catalog.enemies, 3, catalog.loot, catalog.boxRects);
            sys.EnterRoom(world, room, motor);
            var fight = new BossFight(data);
            fight.EnterRoom(worldNumber, room);
            return (fight, sys, world, room, motor);
        }

        static void Type(BossFight f, string text) { foreach (var c in text) f.Type(c); }

        static Boss Summon(BossFight fight, EnemySystem sys, bool cross = false)
        {
            Type(fight, "xx" + fight.Info.word.ToLowerInvariant());
            Assert.AreEqual(BossFight.Stage.Invoking, fight.Current);
            for (int t = 0; t < 600 && fight.Current != BossFight.Stage.Fighting; t++)
            {
                sys.Tick();
                fight.Tick(sys, () => sys.SpawnBoss(fight.Info, false, false, cross));
            }
            Assert.AreEqual(BossFight.Stage.Fighting, fight.Current);
            return fight.Boss;
        }

        [Test]
        public void TheWordSummonsTheBossAfterTheSparks()
        {
            var (fight, sys, _, _, _) = Setup(1);
            Assert.IsTrue(fight.Listening);
            Type(fight, "YOMA");
            Assert.AreEqual(BossFight.Stage.Idle, fight.Current, "aún no");
            int ticks = 0;
            Type(fight, "R");
            while (fight.Current != BossFight.Stage.Fighting && ticks++ < 600)
            {
                sys.Tick();
                fight.Tick(sys, () => sys.SpawnBoss(fight.Info, false, false, false));
            }
            Assert.IsInstanceOf<BonyDragon>(fight.Boss);
            Assert.AreEqual(24, fight.Boss.Hp);
            // Medido: unos 408 frames desde la última letra; 97 + 65 ticks y las chispas viven 80.
            Assert.That(ticks, Is.InRange(150, 260));
        }

        [Test]
        public void HolyWaterHalvesAndCloakHalvesContact()
        {
            var (fight, sys, _, _, _) = Setup(1);
            var b = sys.SpawnBoss(fight.Info, true, true, false);
            Assert.AreEqual((12, 4), (b.Hp, b.Damage));
        }

        [Test]
        public void BonyDragonOnlyTakesSwordAndArrows()
        {
            var (fight, sys, _, _, _) = Setup(1);
            var b = sys.SpawnBoss(fight.Info, false, false, false);
            Assert.IsTrue(b.Vulnerable(0), "espada");
            Assert.IsTrue(b.Vulnerable(1), "flecha");
            Assert.IsFalse(b.Vulnerable(4), "fuego no");
            Assert.IsFalse(b.Vulnerable(5), "mina no");
        }

        // Remake (RemakeRules.FalseKnightMace): el False Knight da con la maza en vez de lanzar huesos; la bola hace daño
        // solo cuando golpea el suelo, delante de él. Sin la regla, los huesos del original.
        [Test]
        public void BonyDragonSwingsTheMaceInsteadOfThrowingBones()
        {
            bool before = RemakeRules.FalseKnightMace;
            try
            {
                foreach (bool mace in new[] { true, false })
                {
                    RemakeRules.FalseKnightMace = mace;
                    var (fight, sys, _, _, motor) = Setup(1);
                    var boss = Summon(fight, sys);
                    bool bones = false, maceSeen = false, maceHurts = false, inFront = true;
                    for (int t = 0; t < 300; t++)
                    {
                        sys.Tick();
                        foreach (var e in sys.Enemies.Where(e => e.Alive))
                        {
                            if (e is Bone) bones = true;
                            if (e is MaceHead m)
                            {
                                maceSeen = true;
                                if (m.Harmful) maceHurts = true;
                                float cx = (boss.ContactRects().Min(r => r.x0) + boss.ContactRects().Max(r => r.x1)) / 2;
                                if (boss.FacingLeft ? m.X + m.Width > cx : m.X < cx) inFront = false;
                            }
                        }
                    }
                    if (mace)
                    {
                        Assert.IsFalse(bones, "sin huesos");
                        Assert.IsTrue(maceSeen && maceHurts, "la maza golpea");
                        Assert.IsTrue(inFront, "por delante");
                    }
                    else
                    {
                        Assert.IsTrue(bones, "los huesos del original");
                        Assert.IsFalse(maceSeen);
                    }
                }
            }
            finally { RemakeRules.FalseKnightMace = before; }
        }

        [Test]
        public void SeaAnemoneOnlyWhenOpenAndGaliousOnlyWithTheCross()
        {
            var (fight, sys, _, _, _) = Setup(2);
            var anemone = (SeaAnemone)sys.SpawnBoss(fight.Info, false, false, false);
            anemone.Sprite = 0x10;
            Assert.IsFalse(anemone.CanBeHitNow);
            anemone.Sprite = 0x12;
            Assert.IsTrue(anemone.CanBeHitNow);
            var (g, gsys, _, _, _) = Setup(10);
            Assert.IsFalse(gsys.SpawnBoss(g.Info, false, false, false).CanBeHitNow, "sin la Cross");
            Assert.IsTrue(gsys.SpawnBoss(g.Info, false, false, true).CanBeHitNow);
        }

        [Test]
        public void DefeatDropsTheGreatKeyAndTheFanfareReopensTheRoom()
        {
            var (fight, sys, _, _, _) = Setup(1);
            sys.BossDefeated += _ => fight.OnBossDefeated();
            var boss = Summon(fight, sys);
            sys.HitBoss(boss, boss.Hp);
            Assert.AreEqual(BossFight.Stage.Dissolving, fight.Current);
            for (int t = 0; t < 70; t++) fight.Tick(sys, () => null);
            Assert.AreEqual(BossFight.Stage.KeyOnFloor, fight.Current);
            Assert.AreEqual((128, 136), fight.KeyPos);
            Assert.IsTrue(fight.LocksExits, "las salidas siguen cerradas hasta coger la llave");
            fight.KeyTaken();
            for (int t = 0; t < 241; t++) fight.Tick(sys, () => null);
            Assert.AreEqual(BossFight.Stage.Idle, fight.Current);
            Assert.IsFalse(fight.Listening, "no se puede volver a luchar");
        }

        [Test]
        public void EveryBossFightsForAWhileWithoutBreaking()
        {
            foreach (int w in new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 })
            {
                var (fight, sys, _, _, motor) = Setup(w);
                var boss = Summon(fight, sys, cross: true);
                for (int t = 0; t < 1500; t++)
                {
                    motor.Place(motor.Room, 24 + (t * 3) % 208, 184);
                    sys.Tick();
                    Assert.That(boss.X, Is.InRange(-48f, 300f), $"{fight.Info.name} t{t} x");
                    Assert.That(boss.Y, Is.InRange(0f, 200f), $"{fight.Info.name} t{t} y");
                    Assert.IsTrue(boss.ContactRects().Any() || !boss.Visible, $"{fight.Info.name} sin caja");
                }
                Assert.IsTrue(boss.Alive, fight.Info.name);
            }
        }
    }
}
