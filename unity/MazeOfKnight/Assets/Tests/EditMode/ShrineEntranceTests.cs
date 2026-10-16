using System.Linq;
using MazeOfKnight.Data;
using MazeOfKnight.Game;
using NUnit.Framework;
using UnityEngine;

namespace MazeOfKnight.Tests
{
    // Todas las entradas a tiendas y santuarios del original se pueden usar con los mapas reales: en el castillo, el
    // sitio de pie delante del dios tiene suelo y hueco, la piedra que la tapa está ahí y ni un banco ni Cornifer se
    // quedan con el ↓; en los mundos, la escalera sube hasta la fila de arriba de la sala de debajo.
    public class ShrineEntranceTests
    {
        [Test]
        public void EveryShopAndShrineCanBeEntered()
        {
            var maps = Resources.LoadAll<MapData>("Maps");
            var shrinesJson = Resources.Load<TextAsset>("Shrines");
            var bossesJson = Resources.Load<TextAsset>("Bosses");
            if (maps.Length == 0 || shrinesJson == null || bossesJson == null) Assert.Ignore("Sin datos importados");
            var shrines = JsonUtility.FromJson<ShrinesData>(shrinesJson.text);
            var bosses = JsonUtility.FromJson<BossesData>(bossesJson.text);
            var worlds = maps.ToDictionary(m => m.mapName, m => new RoomWorld(m.rooms.Select(r => r.room)));
            var benches = new Benches(worlds, 1, w => bosses.ForWorld(w)?.room ?? -1);
            var cornifers = new Cornifers(worlds, w => bosses.ForWorld(w)?.room ?? -1, benches);
            var problems = new System.Collections.Generic.List<string>();
            foreach (var s in shrines.shrines)
            {
                string where = $"{s.kind} {s.god} {s.map}:{s.room}";
                if (!worlds.TryGetValue(s.map, out var world)) { problems.Add($"{where}: sin mapa"); continue; }
                var e = s.entry;
                if (e.how == "down")
                {
                    var room = world.Room(s.room);
                    if (room == null) { problems.Add($"{where}: sin sala"); continue; }
                    bool rock = room.rocks.Any(r => r.x == e.godX && r.y == e.godY);
                    if (e.rockOnTop && !rock) problems.Add($"{where}: sin la piedra que la tapa en ({e.godX}, {e.godY}); hay {string.Join(" ", room.rocks.Select(r => $"({r.x},{r.y})"))}");
                    // Con la piedra ya rota (es la que tapa la entrada: el cuerpo, delante del dios, queda dentro de ella).
                    if (rock) world.Clear(room, e.godX >> 3, (e.godY - OriginalPhysics.RoomTopPx) >> 3, 2, 2);
                    bool stand = false;
                    for (int x = e.xMin; x <= e.xMax && !stand; x++)
                        for (int y = e.yMin; y <= e.yMax && !stand; y++)
                        {
                            // Como PlayerMotor: de pie con y en la rejilla de 8 y sólido en y bajo los pies, con el cuerpo libre.
                            if (((y - OriginalPhysics.RoomTopPx) & 7) != 0 || world.Solid(room, x, y - 1)) continue;
                            for (int px = x + OriginalPhysics.SupportLeft; px <= x + OriginalPhysics.SupportRight; px += 4)
                                stand |= world.Solid(room, px, y);
                        }
                    if (!stand) problems.Add($"{where}: no hay suelo delante del dios ({e.xMin}…{e.xMax}, {e.yMin}…{e.yMax})");
                    for (int x = e.xMin; x <= e.xMax; x++)
                    {
                        if (benches.At(s.map, s.room, x, e.yMin) != null) { problems.Add($"{where}: un banco se queda con el ↓"); break; }
                        int w = s.map == "castle" ? 0 : int.Parse(s.map.Substring(5));
                        if (w > 0 && cornifers.At(w, s.room, x, e.yMin) != null) { problems.Add($"{where}: Cornifer se queda con el ↓"); break; }
                    }
                }
                else
                {
                    var from = world.Room(e.fromRoom);
                    if (from == null) { problems.Add($"{where}: sin la sala de debajo {e.fromRoom}"); continue; }
                    bool ladder = world.Ladder(from, e.x, OriginalPhysics.RoomTopPx + 2);
                    if (!ladder && !e.hiddenLadder) problems.Add($"{where}: sin escalera en x = {e.x} arriba de la sala {e.fromRoom}");
                }
            }
            Assert.IsEmpty(problems, string.Join("\n", problems));
        }
    }
}
