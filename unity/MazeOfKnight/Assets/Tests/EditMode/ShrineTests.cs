using System.Collections.Generic;
using System.Linq;
using MazeOfKnight.Data;
using MazeOfKnight.Game;
using MazeOfKnight.Game.Enemies;
using NUnit.Framework;
using UnityEngine;

namespace MazeOfKnight.Tests
{
    // Santuarios, tiendas y eventos de sala contra docs/design_bible/08_santuarios.md y docs/entities.md (datos reales).
    public class ShrineTests
    {
        static ShrinesData Shrines()
        {
            var json = Resources.Load<TextAsset>("Shrines");
            if (json == null) Assert.Ignore("Sin Resources/Shrines.json importado");
            return JsonUtility.FromJson<ShrinesData>(json.text);
        }

        static Inventory NewInventory()
        {
            var json = Resources.Load<TextAsset>("Items");
            if (json == null) Assert.Ignore("Sin Resources/Items.json importado");
            return new Inventory(JsonUtility.FromJson<ItemsData>(json.text), new List<int>());
        }

        static MapData Map(string name) =>
            Resources.LoadAll<MapData>("Maps").FirstOrDefault(m => m.mapName == name) ?? throw new IgnoreException("Sin mapas");

        [Test]
        public void AresShopSellsQuiverAndShieldsInOrder()
        {
            var data = Shrines();
            var inv = NewInventory();
            var ares65 = data.shrines.First(s => s.map == "castle" && s.room == 65);   // carcaj 20, Silver Shield 40, llave 5
            var visit = new ShrineVisit(ares65, data, inv, 0, 100, 0);
            Assert.AreEqual(0, visit.Cursor);
            Assert.IsTrue(visit.Confirm(), "carcaj");
            Assert.AreEqual((100, 80), (visit.Arrows, visit.Coins));
            Assert.AreEqual(1, visit.Cursor, "la mano salta al siguiente libre");
            Assert.IsFalse(visit.Confirm(), "la plata pide el cobre");
            Assert.IsTrue(visit.Refused);
            Assert.AreEqual(80, visit.Coins, "no cobra");
            inv.Add(0x28);
            Assert.IsTrue(visit.Confirm());
            Assert.IsTrue(inv.Has(0x2B));
            Assert.AreEqual(2, inv.ShieldLevel);
            Assert.AreEqual(40, visit.Coins);
        }

        [Test]
        public void NotEnoughCoinsShowsTheMessage()
        {
            var data = Shrines();
            var shop = data.shrines.First(s => s.map == "castle" && s.room == 115);   // Mine 30
            var visit = new ShrineVisit(shop, data, NewInventory(), 0, 10, 0);
            Assert.IsFalse(visit.Confirm());
            StringAssert.Contains("COINS ARE NOT", visit.Text);
            StringAssert.Contains("ENOUGH", visit.Text);
        }

        [Test]
        public void MarkedEnemiesOpenTheHiddenLadderToTheWorldShrine()
        {
            // Mundo 2: matar a los marcados de la sala 7 hace aparecer la escalera de la fila 0, columna 15, que lleva al
            // santuario (sala 15).
            var map = Map("world02");
            var room7 = map.rooms.Select(r => r.room).First(r => r.id == 7);
            Assert.IsTrue(room7.markedRoom);
            Assert.AreEqual((0, 15, 3, 7), (room7.eventLadder.row, room7.eventLadder.col, room7.eventLadder.height, room7.eventLadder.openedBy));
            var world = new RoomWorld(map.rooms.Select(r => r.room));
            var catalog = Resources.Load<EnemyCatalog>("Enemies");
            var sys = new EnemySystem(catalog.enemies, 1);
            RoomModel cleared = null;
            sys.RoomCleared += r => cleared = r;
            sys.EnterRoom(world, room7, new PlayerMotor(world, room7, 128, 152), eventPending: true);
            var marked = sys.Enemies.Where(e => e.Marked).ToList();
            Assert.Greater(marked.Count, 0);
            foreach (var e in marked)
                sys.Damage(e, 99);
            Assert.AreSame(room7, cleared);
            var shrine = Shrines().shrines.First(s => s.map == "world02");
            Assert.AreEqual((7, 15), (shrine.entry.fromRoom, shrine.room));
        }

        [Test]
        public void ClimbingTheShrineLadderReachesTheTopOfTheRoom()
        {
            // Mundo 1, sala 14: la escalera de x = 128 llega a la fila 0; el santuario empieza con y < 40.
            var map = Map("world01");
            var world = new RoomWorld(map.rooms.Select(r => r.room));
            var room = world.Room(14);
            int y = 0;
            for (int py = 184; py > 40; py -= 8)
                if (world.Ladder(room, 128, py - 1) && !world.Ladder(room, 128, py)) { y = py; break; }
            var m = new PlayerMotor(world, room, 128, 40);
            m.PlaceOnLadder(room, 128, 48);
            for (int t = 0; t < 20 && m.Y >= 40; t++) m.Tick(new MotorInput { Up = true });
            Assert.Less(m.Y, 40, "sube por encima de y = 40 (entrada al santuario)");
            Assert.AreEqual(14, m.Room.id);
        }
    }
}
