using System.Linq;
using MazeOfKnight.Data;
using MazeOfKnight.Game;
using NUnit.Framework;
using UnityEngine;

namespace MazeOfKnight.Tests
{
    // RemakeRules.HollowMaps: un Cornifer en cada mundo con mapa, lejos de la entrada y de los enemigos, con los mapas reales.
    public class CorniferTests
    {
        [Test]
        public void EveryWorldWithAMapHasACorniferFarFromTheEntrance()
        {
            var maps = Resources.LoadAll<MapData>("Maps");
            var bossesJson = Resources.Load<TextAsset>("Bosses");
            if (maps.Length == 0 || bossesJson == null) Assert.Ignore("Sin mapas importados");
            var bosses = JsonUtility.FromJson<BossesData>(bossesJson.text);
            var worlds = maps.ToDictionary(m => m.mapName, m => new RoomWorld(m.rooms.Select(r => r.room)));
            var benches = new Benches(worlds, 1, w => bosses.ForWorld(w)?.room ?? -1);
            var cornifers = new Cornifers(worlds, w => bosses.ForWorld(w)?.room ?? -1, benches);
            for (int w = 1; w <= 9; w++)
            {
                if (!worlds.ContainsKey(Benches.MapName(w))) continue;
                var c = cornifers.All.Single(s => s.World == w);
                var room = worlds[Benches.MapName(w)].Room(c.Room);
                Assert.GreaterOrEqual(c.Distance, Cornifers.MinDistance, $"mundo {w}");
                foreach (var e in room.spawns)
                    Assert.IsFalse(System.Math.Abs(e.x + 8 - c.X) < Benches.ClearX + Cornifers.Width * 4 && System.Math.Abs(e.y + 8 - (c.Y - 8)) < Benches.ClearY,
                        $"mundo {w}: lejos de donde aparecen enemigos");
                Assert.AreNotEqual(bosses.ForWorld(w).room, c.Room, $"mundo {w}: no en la del jefe");
                Assert.IsNull(benches.Find(Benches.MapName(w), c.Room), $"mundo {w}: no con un banco");
            }
            Assert.IsFalse(cornifers.All.Any(c => c.World == 10), "el mundo 10 no tiene mapa");
        }

        [Test]
        public void TheMapCostsMoreInLaterWorlds()
        {
            Assert.AreEqual(30, Cornifers.Price(1));
            Assert.AreEqual(110, Cornifers.Price(9));
        }
    }
}
