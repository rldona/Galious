using System.Collections.Generic;
using System.Linq;
using MazeOfKnight.Data;
using MazeOfKnight.Game;
using NUnit.Framework;
using UnityEngine;

namespace MazeOfKnight.Tests
{
    // Ascensores del castillo contra las medidas de openMSX (tools/openmsx/lift_measure.py, docs/entities.md).
    public class LiftTests
    {
        static (PlayerMotor motor, LiftSystem lifts) Setup(int room, int x, int y)
        {
            var json = Resources.Load<TextAsset>("Lifts");
            if (json == null) Assert.Ignore("Sin Resources/Lifts.json importado");
            var map = Resources.LoadAll<MapData>("Maps").First(m => m.mapName == "castle");
            var world = new RoomWorld(map.rooms.Select(r => r.room));
            var lifts = new LiftSystem(JsonUtility.FromJson<LiftsData>(json.text));
            var motor = new PlayerMotor(world, world.Room(room), x, y) { Lifts = lifts };
            return (motor, lifts);
        }

        [Test]
        public void ShaftsTurnAtTheirEnds()
        {
            // Medido (room1.tsv.gz): el 6 baja hasta 144 en la sala 146 y vuelve (… 142 144 142); arriba, en la 80, sube
            // hasta 117 (… 121 119 117 119): da la vuelta con y + 1 < 120. Las salas: 80 88 101 113 126 135 146.
            var (motor, lifts) = Setup(1, 192, 120);
            var six = lifts.Shafts.First(l => l.Id == 6);
            var seq = new List<(int room, int y)>();
            for (int t = 0; t < 2000; t++)
            {
                motor.Tick(default);
                seq.Add((six.RoomId, six.Y));
            }
            bool Has(params (int, int)[] w) =>
                Enumerable.Range(0, seq.Count - w.Length).Any(i => w.Select((p, k) => seq[i + k] == p).All(b => b));
            Assert.IsTrue(Has((146, 140), (146, 142), (146, 144), (146, 142)), "vuelta abajo");
            Assert.IsTrue(Has((80, 121), (80, 119), (80, 117), (80, 119)), "vuelta arriba");
            Assert.IsTrue(Has((88, 190), (101, 40), (101, 42)), "baja a la sala siguiente por arriba");
            var rooms = new List<int>();
            foreach (var (r, _) in seq)
                if (rooms.Count == 0 || rooms[rooms.Count - 1] != r) rooms.Add(r);
            CollectionAssert.AreEqual(new[] { 80, 88, 101, 113, 126, 135, 146, 135 }, rooms.Take(8));
        }

        [Test]
        public void SlideBouncesWithTheOldX()
        {
            // Sala 43, entrando por la izquierda (x = 16, y = 120): 48 50 52 … 176 178 176 … 48 46 48 (room43.tsv.gz).
            var (motor, lifts) = Setup(43, 16, 120);
            var xs = new List<int>();
            for (int t = 0; t < 300; t++)
            {
                motor.Tick(default);
                xs.Add(lifts.SlideInRoom.X);
            }
            Assert.AreEqual(50, xs[0], "empieza en 48 y el primer tick ya se mueve");
            string s = string.Join(",", xs);
            StringAssert.Contains("174,176,178,176,174", s);
            StringAssert.Contains("50,48,46,48,50", s);
        }

        [Test]
        public void StandingOnTheSlideCarriesThePlayer()
        {
            var (motor, lifts) = Setup(43, 64, 120);
            motor.Tick(default);                 // la colisión lo pone encima
            Assert.IsTrue(motor.OnLift);
            int x0 = motor.X, lx0 = lifts.SlideInRoom.X;
            for (int t = 0; t < 20; t++)
                motor.Tick(default);
            Assert.AreEqual(MotorState.Ground, motor.State);
            Assert.AreEqual(120, motor.Y);
            Assert.AreEqual(lifts.SlideInRoom.X - lx0, motor.X - x0, "va con el ascensor");
        }

        [Test]
        public void RidingAShaftChangesRoomWithIt()
        {
            // Demo 0: sobre el ascensor 6 en la sala 146, subiendo: llega a la 135 en y = 185 con el ascensor.
            var (motor, lifts) = Setup(146, 128, 56);
            lifts.PresetForStart(146);
            for (int t = 0; t < 30 && motor.Room.id == 146; t++)
                motor.Tick(default);
            Assert.AreEqual(135, motor.Room.id);
            Assert.AreEqual(185, motor.Y);
            Assert.IsTrue(motor.OnLift);
            motor.Tick(default);
            Assert.AreEqual(183, motor.Y);
        }

        [Test]
        public void FireWeaponsAreBlockedOnALift()
        {
            var (motor, _) = Setup(43, 64, 120);
            motor.Tick(default);
            var wjson = Resources.Load<TextAsset>("Weapons");
            if (wjson == null) Assert.Ignore("Sin Resources/Weapons.json importado");
            var weapons = new WeaponSystem(JsonUtility.FromJson<WeaponsData>(wjson.text));
            var items = new List<int> { 1, 3, 4, 5 };
            Assert.IsNull(weapons.CanFire(weapons.Data.Get(1), motor, items, 99), "flechas sí");
            Assert.AreEqual("aquí no", weapons.CanFire(weapons.Data.Get(4), motor, items, 99), "fuego no");
        }
    }
}
