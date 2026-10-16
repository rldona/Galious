using System.Linq;
using MazeOfKnight.Data;
using MazeOfKnight.Game;
using NUnit.Framework;
using UnityEngine;

namespace MazeOfKnight.Tests
{
    // RemakeRules.HollowBenches: los bancos con los mapas reales (si están importados).
    public class BenchTests
    {
        [Test]
        public void CastleAndEveryWorldHaveTheirBenchesOnFirmGround()
        {
            var maps = Resources.LoadAll<MapData>("Maps");
            var bossesJson = Resources.Load<TextAsset>("Bosses");
            if (maps.Length == 0 || bossesJson == null) Assert.Ignore("Sin mapas importados");
            var bosses = JsonUtility.FromJson<BossesData>(bossesJson.text);
            var worlds = maps.ToDictionary(m => m.mapName, m => new RoomWorld(m.rooms.Select(r => r.room)));
            var castle = maps.First(m => m.mapName == "castle");
            int startRoom = Object.FindAnyObjectByType<GameBootstrap>()?.startRoom ?? 1;
            var benches = new Benches(worlds, startRoom, w => bosses.ForWorld(w)?.room ?? -1);
            Assert.IsTrue(benches.All.Any(b => b.Map == "castle"), "uno en el castillo");
            for (int w = 1; w <= 10; w++)
            {
                string map = Benches.MapName(w);
                if (!worlds.ContainsKey(map)) continue;
                var mine = benches.All.Where(b => b.Map == map).ToList();
                // En la entrada o en la sala más cercana con sitio (mundo 10: la entrada es lava).
                Assert.GreaterOrEqual(mine.Count, 1, $"mundo {w}: al menos uno");
                Assert.IsFalse(mine.Any(b => b.Room == bosses.ForWorld(w).room), $"mundo {w}: ninguno en la sala del jefe");
                // En una sala vecina de la del jefe, si alguna tiene sitio (mundo 4: su única vecina son pozos de escalera).
                var boss = worlds[map].Room(bosses.ForWorld(w).room);
                var near = new[] { boss.left, boss.right, boss.up, boss.down }.Where(n => n > 0 && worlds[map].Room(n) != null).ToList();
                if (near.Any(n => Benches.Spot(worlds[map], worlds[map].Room(n), 1) != null))
                    Assert.IsTrue(mine.Any(b => near.Contains(b.Room)), $"mundo {w}: en una sala vecina de la del jefe");
            }
            // Cada banco: suelo firme bajo sus 4 casillas y aire encima.
            foreach (var b in benches.All)
            {
                var world = worlds[b.Map];
                var room = world.Room(b.Room);
                // Lejos de donde aparecen enemigos (el de al lado del jefe puede estar en una sala con ellos).
                foreach (var e in room.spawns)
                    Assert.IsFalse(System.Math.Abs(e.x + 8 - b.X) < Benches.ClearX + 16 && System.Math.Abs(e.y + 8 - (b.Y - 8)) < Benches.ClearY,
                        $"{b.Map} sala {b.Room}: enemigo 0x{e.type:X2} en ({e.x}, {e.y}) junto al banco");
                // Sin escaleras pegadas por encima del suelo (al menos una casilla libre a cada lado).
                for (int x = b.X - 16 - 8; x < b.X + 16 + 8; x += 8)
                    for (int y = b.Y - 32; y < b.Y; y += 8)
                        if (x >= 0 && x < 256) Assert.AreNotEqual(TileClass.Ladder, world.TileAt(room, x, y), $"{b.Map} sala {b.Room}: escalera en ({x}, {y})");
                for (int x = b.X - 15; x <= b.X + 15; x += 5)
                {
                    Assert.AreEqual(TileClass.Solid, world.TileAt(room, x, b.Y), $"{b.Map} sala {b.Room}: suelo en x {x}");
                    Assert.AreEqual(TileClass.Empty, world.TileAt(room, x, b.Y - 1), $"{b.Map} sala {b.Room}: aire en x {x}");
                }
                Assert.IsNotNull(benches.At(b.Map, b.Room, b.X + Benches.Reach, b.Y), "se alcanza desde el borde");
                Assert.IsNull(benches.At(b.Map, b.Room, b.X + Benches.Reach + 1, b.Y));
            }
            TestContext.WriteLine($"{benches.All.Count} bancos: " + string.Join(", ", benches.All.Select(b => $"{b.Map}/{b.Room}")));
        }
    }
}
