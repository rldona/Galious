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
            // Solo informa: el remake no es idéntico (sin plataformas que suben, azar sin sembrar), así que la ruta puede
            // torcerse. Se compara con la medida en openMSX.
            var map = Resources.LoadAll<MapData>("Maps").First(m => m.mapName == "castle");
            foreach (var demo in Data().demos)
            {
                var world = new RoomWorld(map.rooms.Select(r => r.room));
                var motor = new PlayerMotor(world, world.Room(demo.room), demo.x, demo.y);
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
                    trace.AppendLine($"{tick++}\t{motor.Room.id}\t{motor.X}\t{motor.Y}\t{motor.State}\t{(input.Up ? "U" : "")}{(input.Down ? "D" : "")}{(input.Left ? "L" : "")}{(input.Right ? "R" : "")}{(input.Attack ? "S" : "")}");
                    if (motor.Room.id != rooms[rooms.Count - 1]) rooms.Add(motor.Room.id);
                }
                System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath, $"../../../logs/unity/demo{demo.index}_remake.tsv"), trace.ToString());
                TestContext.WriteLine($"demo {demo.index}: {string.Join("→", rooms)}");
                Assert.Greater(rooms.Count, 1, $"demo {demo.index} sale de su sala");
                // La demo 1 (sin plataformas que suben) recorre exactamente las salas del original (y, comparada tick a
                // tick con la traza de openMSX, las mismas posiciones).
                if (demo.index == 1)
                    CollectionAssert.AreEqual(new[] { 1, 107, 120, 121, 132, 133, 143, 133 }, rooms);
            }
        }
    }
}
