using System.Collections.Generic;
using System.Linq;
using MazeOfKnight.Data;
using MazeOfKnight.Game;
using NUnit.Framework;
using UnityEngine;

namespace MazeOfKnight.Tests
{
    // Con las salas importadas (Resources/Maps): en cada piedra a la que se llega andando por el suelo, pegarse a ella y
    // dar una estocada tiene que dañarla, con los dos personajes y desde los dos lados.
    public class RockReachTests
    {
        static readonly string[] Maps = new[] { "castle" }.Concat(Enumerable.Range(1, 10).Select(i => $"world{i}")).ToArray();

        [Test]
        public void PushingAgainstEveryRockTheSwordHitsIt()
        {
            int tried = 0;
            var failures = new List<string>();
            foreach (var name in Maps)
            {
                var map = Resources.Load<MapData>($"Maps/{name}");
                if (map == null) continue;
                foreach (var entry in map.rooms)
                    foreach (var r in entry.room.rocks)
                        foreach (var character in new[] { Character.Popolon, Character.Aphrodite })
                            foreach (bool fromLeft in new[] { true, false })
                            {
                                var world = new RoomWorld(map.rooms.Select(e => e.room));
                                var room = world.Room(entry.room.id);
                                int y = r.y + 16;                       // de pie a la altura de la base de la piedra
                                int x = fromLeft ? r.x - 8 - 12 : r.x + 16 + 8 + 12;
                                if (x < 8 || x > 248) continue;
                                var motor = new PlayerMotor(world, room, x, y) { Character = character };
                                // Solo si ahí se está de pie (suelo debajo y sitio para el cuerpo).
                                motor.Tick(default);
                                if (motor.State != MotorState.Ground || motor.Room != room || motor.Y != y) continue;
                                var rocks = new Rocks(name, world, new HashSet<(string, int, int, int)>());
                                var walk = new MotorInput { Right = fromLeft, Left = !fromLeft };
                                for (int i = 0; i < 30; i++) motor.Tick(walk);
                                if (motor.State != MotorState.Ground || motor.Room != room) continue;
                                // Solo cuenta si se ha quedado pegado a esta piedra (a 3 px como mucho), no a otra cosa.
                                int gap = fromLeft ? r.x - (motor.X + OriginalPhysics.FootRight) - 1 : motor.X + OriginalPhysics.FootLeft - (r.x + 16);
                                if (gap < 0 || gap > 3 || motor.Y != y) continue;
                                var press = walk;
                                press.Attack = true;
                                motor.Tick(press);
                                if (!motor.AttackStartedThisTick) continue;
                                tried++;
                                rocks.Strike(motor, world);
                                var rock = rocks.All.First(k => k.room == room && k.x == r.x && k.y == r.y);
                                if (rock.hp == OriginalPhysics.RockHp)
                                {
                                    var (x0, x1, y0, y1) = motor.SwordRect();
                                    failures.Add($"{name} sala {room.id} piedra ({r.x},{r.y}) {character} {(fromLeft ? "→" : "←")}: " +
                                                 $"parado en x = {motor.X}, y = {motor.Y} (a {gap} px); espada x {x0}…{x1}, y {y0}…{y1}");
                                }
                            }
            }
            if (tried == 0) Assert.Ignore("Sin salas importadas");
            Assert.IsEmpty(failures, $"{failures.Count} de {tried} fallan:\n" + string.Join("\n", failures.Take(40)));
        }
    }
}
