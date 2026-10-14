using System.Collections.Generic;
using System.Linq;
using MazeOfKnight.Data;
using MazeOfKnight.Game;
using NUnit.Framework;
using UnityEngine;

namespace MazeOfKnight.Tests
{
    // Lo que se ha sacado del backlog: Muerte, objetos con condición del castillo y partida guardada.
    public class BacklogTests
    {
        static ShrinesData Shrines()
        {
            var json = Resources.Load<TextAsset>("Shrines");
            if (json == null) Assert.Ignore("Sin Resources/Shrines.json importado");
            return JsonUtility.FromJson<ShrinesData>(json.text);
        }

        static Inventory NewInventory() =>
            new Inventory(JsonUtility.FromJson<ItemsData>(Resources.Load<TextAsset>("Items")?.text ?? "{}"), new List<int>());

        [Test]
        public void DeathRevivesTheOtherCharacterOnce()
        {
            var data = Shrines();
            var death = data.shrines.First(s => s.kind == "resurrection");
            int[] lives = { 0, 0 }, credits = { 1, 1 };
            // Popolon juega; Aphrodite sin vidas. Con 100/100/20 lo ofrece.
            var v = new ShrineVisit(death, data, NewInventory(), 100, 120, 20, lives, credits, Character.Popolon);
            Assert.IsTrue(v.AskYesNo);
            v.Confirm();
            Assert.IsTrue(v.Revived);
            Assert.AreEqual(1, lives[0]);
            Assert.AreEqual(0, credits[0]);
            Assert.AreEqual((0, 20, 0), (v.Arrows, v.Coins, v.Keys));
            Assert.AreEqual(180, v.AutoExitFrames);
            // Sin crédito, solo el aviso.
            lives[0] = 0;
            var again = new ShrineVisit(death, data, NewInventory(), 100, 100, 20, lives, credits, Character.Popolon);
            Assert.IsFalse(again.AskYesNo);
            // Con el otro vivo, tampoco.
            var alive = new ShrineVisit(death, data, NewInventory(), 100, 100, 20, new[] { 1, 0 }, new[] { 1, 1 }, Character.Popolon);
            Assert.IsFalse(alive.AskYesNo);
        }

        [Test]
        public void DemeterSavesInsteadOfThePassword()
        {
            var data = Shrines();
            var demeter = data.shrines.First(s => s.kind == "password");
            var v = new ShrineVisit(demeter, data, NewInventory(), 0, 0, 0);
            Assert.IsTrue(v.AskYesNo);
            v.Confirm();
            Assert.IsTrue(v.SaveRequested);
        }

        [Test]
        public void SavePacksAndUnpacksSets()
        {
            var set = new HashSet<(string, int, int, int)> { ("castle", 3, 16, 120), ("world05", 6, 0, 32) };
            var back = new HashSet<(string, int, int, int)>();
            SaveData.Unpack(SaveData.Pack(set), back);
            CollectionAssert.AreEquivalent(set, back);
            var json = JsonUtility.ToJson(new SaveData { lives = new[] { 1, 0 }, items = new List<int> { 0x13 } });
            var d = JsonUtility.FromJson<SaveData>(json);
            Assert.AreEqual(0x13, d.items[0]);
            Assert.AreEqual(1, d.lives[0]);
        }

        static (PlayerMotor, RoomWorld) Castle(int room, int x, int y)
        {
            var map = Resources.LoadAll<MapData>("Maps").FirstOrDefault(m => m.mapName == "castle");
            if (map == null) Assert.Ignore("Sin mapas");
            var world = new RoomWorld(map.rooms.Select(r => r.room));
            return (new PlayerMotor(world, world.Room(room), x, y), world);
        }

        [Test]
        public void VaseNeedsThreeRightThreeLeftThreeJumps()
        {
            var (m, _) = Castle(40, 128, 120);
            var c = new CastleItems();
            CastleItems.Spot? got = null;
            void Do(MotorInput i, int ticks = 1)
            {
                for (int t = 0; t < ticks && got == null; t++)
                {
                    m.Tick(i);
                    got = c.Tick(m, false, w => w == 4, _ => false);
                }
            }
            var none = new MotorInput();
            Do(none, 2);
            for (int k = 0; k < 3; k++) { Do(new MotorInput { Right = true, Attack = true }); Do(none, 6); }
            Do(new MotorInput { Left = true });   // girarse (el golpe en el suelo no mueve)
            for (int k = 0; k < 3; k++) { Do(new MotorInput { Left = true, Attack = true }); Do(none, 6); }
            Assert.AreEqual(6, c.VaseSteps);
            for (int k = 0; k < 3 && got == null; k++) { Do(new MotorInput { Up = true, UpPressed = true }, 3); Do(none, 40); }
            Assert.IsNotNull(got);
            Assert.AreEqual(0x1B, got.Value.Type);
        }

        [Test]
        public void PsgPlaysTheRightPitch()
        {
            // Periodo 254 en el canal A: 1789772,5 / (16 · 254) = 440 Hz (la nota A4 de la tabla del driver).
            var psg = new Psg(44100);
            psg.Write(0, 254); psg.Write(1, 0);
            psg.Write(7, 0x3E);          // solo el tono del canal A
            psg.Write(8, 15);
            int crossings = 0;
            float prev = 0;
            for (int i = 0; i < 44100; i++)
            {
                float v = psg.Sample();
                if (i > 2000 && prev < 0 && v >= 0) crossings++;
                prev = v;
            }
            Assert.That(crossings, Is.InRange(415, 445));
        }

        [Test]
        public void SteppingStonesAppearOneByOne()
        {
            var st = new SteppingStones();
            var seen = new List<int>();
            st.Appeared += seen.Add;
            for (int t = 0; t < 299; t++) st.Tick(0x1C, 0x78);
            Assert.IsNull(st.Current, "aún no");
            st.Tick(0x1C, 0x78);
            Assert.AreEqual(0x38, st.Current);
            // Encima de cada piedra 128 pasos: sale la siguiente.
            for (int k = 0; k < 3; k++)
                for (int t = 0; t < 129; t++) st.Tick(st.Current.Value + 8, 0x78);
            CollectionAssert.AreEqual(new[] { 0x38, 0x60, 0x90, 0xB8 }, seen);
            for (int t = 0; t < 129; t++) st.Tick(0xB8 + 8, 0x78);
            Assert.IsNull(st.Current, "la última también se va");
        }

        [Test]
        public void VaseNeedsTheWorld4Boss()
        {
            var (m, _) = Castle(40, 128, 120);
            var c = new CastleItems();
            m.Tick(new MotorInput { Right = true, Attack = true });
            c.Tick(m, false, _ => false, _ => false);
            Assert.AreEqual(0, c.VaseSteps);
        }
    }
}
