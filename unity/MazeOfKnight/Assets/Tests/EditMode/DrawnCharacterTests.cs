using System.Collections.Generic;
using System.Linq;
using MazeOfKnight.Data;
using MazeOfKnight.Game;
using MazeOfKnight.Game.Visuals;
using NUnit.Framework;
using UnityEngine;

namespace MazeOfKnight.Tests
{
    // Elección del clip dibujado del jugador (CharacterArt con ArtLibrary), como con los protagonistas de Hollow Knight.
    public class DrawnCharacterTests
    {
        static Sprite S(string name)
        {
            var s = Sprite.Create(new Texture2D(2, 2), new Rect(0, 0, 2, 2), Vector2.zero);
            s.name = name;
            return s;
        }

        static CharacterArt Art()
        {
            var files = new List<(string, Sprite)>();
            for (int i = 0; i < 2; i++) files.Add(($"popolon_quieto_{i:00}", S($"popolon_quieto_{i:00}")));
            for (int i = 0; i < 8; i++) files.Add(($"popolon_correr_{i:00}", S($"popolon_correr_{i:00}")));
            return new CharacterArt { Drawn = new ArtLibrary(files), DrawnOnly = true };
        }

        [Test]
        public void RunningAtSixtyFpsNeverFlashesTheIdlePose()
        {
            // El motor va a 30 ticks/s y la pantalla a 60: en la mitad de los fotogramas el jugador no se ha movido desde
            // el anterior, y aun así sigue corriendo (antes salía un fotograma de quieto sí y otro no: un "fantasma").
            var room = new RoomModel { id = 1, collision = Enumerable.Range(0, 20).Select(r => new string(r >= 11 ? '#' : '.', 32)).ToArray() };
            var m = new PlayerMotor(new RoomWorld(new[] { room }), room, 40, 120);
            var art = Art();
            var shown = new List<string>();
            for (int f = 0; f < 60; f++)
            {
                if (f % 2 == 0) m.Tick(new MotorInput { Right = true });
                shown.Add(art.Frame(m, 1f / 60f).name);
            }
            var running = shown.Skip(4).ToList();
            Assert.IsTrue(running.All(n => n.StartsWith("popolon_correr")), string.Join(" ", shown));
        }

        [Test]
        public void StoppingGoesBackToIdle()
        {
            var room = new RoomModel { id = 1, collision = Enumerable.Range(0, 20).Select(r => new string(r >= 11 ? '#' : '.', 32)).ToArray() };
            var m = new PlayerMotor(new RoomWorld(new[] { room }), room, 40, 120);
            var art = Art();
            for (int f = 0; f < 20; f++)
            {
                if (f % 2 == 0) m.Tick(new MotorInput { Right = true });
                art.Frame(m, 1f / 60f);
            }
            string last = null;
            for (int f = 0; f < 12; f++)
            {
                if (f % 2 == 0) m.Tick(default);
                last = art.Frame(m, 1f / 60f).name;
            }
            StringAssert.StartsWith("popolon_quieto", last);
        }
    }
}
