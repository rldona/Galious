using System.Collections.Generic;
using System.Linq;
using MazeOfKnight.Data;
using MazeOfKnight.Game;
using NUnit.Framework;
using UnityEngine;

namespace MazeOfKnight.Tests
{
    // Ciclo de presentación contra docs/attract.md (datos reales de attract.json).
    public class AttractTests
    {
        static AttractData Data()
        {
            var json = Resources.Load<TextAsset>("Attract");
            if (json == null) Assert.Ignore("Sin Resources/Attract.json importado");
            return JsonUtility.FromJson<AttractData>(json.text);
        }

        [Test]
        public void DemosLastTheirRecordedTicks()
        {
            foreach (var demo in Data().demos)
            {
                var p = new DemoPlayer(demo);
                int ticks = 0;
                while (!p.Finished && ticks < 5000) { p.Next(); ticks++; }
                // El tick que lee FF también cuenta.
                Assert.AreEqual(demo.durationTicks, ticks, $"demo {demo.index}");
            }
        }

        [Test]
        public void StoryScrollsForAboutTheMeasuredTime()
        {
            var d = Data();
            var story = new ScrollingText(d.storyLines, d.story.stepFramesMeasured, 2);
            Assert.AreEqual(1710, story.DurationFrames, 60);
        }

        [Test]
        public void DemoReplaysOnTheEngine()
        {
            // Las dos demos recorren las salas del original (sin enemigos: no hay ninguno en su camino que cambie la ruta).
            // Las posiciones tick a tick se comparan con las trazas de openMSX (logs/unity/demoN_remake.tsv).
            var map = Resources.LoadAll<MapData>("Maps").First(m => m.mapName == "castle");
            foreach (var demo in Data().demos)
            {
                var world = new RoomWorld(map.rooms.Select(r => r.room));
                var motor = new PlayerMotor(world, world.Room(demo.room), demo.x, demo.y);
                var ljson = Resources.Load<TextAsset>("Lifts");
                motor.Lifts = new LiftSystem(ljson != null ? JsonUtility.FromJson<LiftsData>(ljson.text) : null);
                motor.Lifts.PresetForStart(demo.room);
                var rocks = new Rocks("castle", world, new HashSet<(string, int, int, int)>());
                var wjson = Resources.Load<TextAsset>("Weapons");
                var weapons = new MazeOfKnight.Game.WeaponSystem(wjson != null ? JsonUtility.FromJson<WeaponsData>(wjson.text) : null);
                var catalog = Resources.Load<EnemyCatalog>("Enemies");
                var enemies = new MazeOfKnight.Game.Enemies.EnemySystem(new EnemyInfo[0], 1);
                enemies.EnterRoom(world, motor.Room, motor);
                var items = new List<int> { 1 };      // la demo lleva el Arrow elegido y 100 flechas
                bool fireDelayed = false;
                var rooms = new List<int> { demo.room };
                var p = new DemoPlayer(demo);
                var trace = new System.Text.StringBuilder();
                int tick = 0;
                var delayed = new MotorInput();
                while (!p.Finished)
                {
                    var input = p.Next();
                    motor.Tick(delayed);   // el tick de retraso del original (lo pone PlayerController)
                    delayed = input;
                    if (motor.AttackStartedThisTick) rocks.Strike(motor, world);
                    if (enemies.Room != motor.Room) enemies.EnterRoom(world, motor.Room, motor);
                    if (fireDelayed)
                    {
                        var w = weapons.Data.Get(1);
                        if (weapons.CanFire(w, motor, items, 99) == null) weapons.Fire(w, motor, world, enemies, rocks, items);
                    }
                    fireDelayed = p.FirePressed;
                    weapons.Tick(motor, world, enemies, rocks, items);
                    trace.AppendLine($"{tick++}\t{motor.Room.id}\t{motor.X}\t{motor.Y}\t{motor.State}\t{(input.Up ? "U" : "")}{(input.Down ? "D" : "")}{(input.Left ? "L" : "")}{(input.Right ? "R" : "")}{(input.Attack ? "S" : "")}\t{motor.Lifts.ShaftInRoom(motor.Room.id)?.Y}\t{motor.Lifts.SlideInRoom?.X}");
                    if (motor.Room.id != rooms[rooms.Count - 1]) rooms.Add(motor.Room.id);
                }
                // La traza, para compararla con openMSX (logs/ no está en el repo: en el CI se crea).
                string logs = System.IO.Path.Combine(Application.dataPath, "../../../logs/unity");
                System.IO.Directory.CreateDirectory(logs);
                System.IO.File.WriteAllText(System.IO.Path.Combine(logs, $"demo{demo.index}_remake.tsv"), trace.ToString());
                TestContext.WriteLine($"demo {demo.index}: {string.Join("→", rooms)}");
                Assert.Greater(rooms.Count, 1, $"demo {demo.index} sale de su sala");
                // Salas del original (trazas de openMSX: logs/omsx/attract y logs/omsx/lift/demo0).
                if (demo.index == 0)
                    CollectionAssert.AreEqual(new[] { 146, 135, 126, 113, 101, 100, 99, 112, 111, 110, 109 }, rooms);
                if (demo.index == 1)
                    CollectionAssert.AreEqual(new[] { 1, 107, 120, 121, 132, 133, 143, 133 }, rooms);
            }
        }
    }
}
