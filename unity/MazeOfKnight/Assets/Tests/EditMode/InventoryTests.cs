using System.Collections.Generic;
using System.Linq;
using MazeOfKnight.Data;
using MazeOfKnight.Game;
using NUnit.Framework;
using UnityEngine;

namespace MazeOfKnight.Tests
{
    // Inventario y efectos de los objetos contra docs/items_enemies.md (datos reales de items.json).
    public class InventoryTests
    {
        static Inventory New()
        {
            var json = Resources.Load<TextAsset>("Items");
            if (json == null) Assert.Ignore("Sin Resources/Items.json importado");
            return new Inventory(JsonUtility.FromJson<ItemsData>(json.text), new List<int>());
        }

        [Test]
        public void WorldItemsOnlyCountInsideTheirWorld()
        {
            var inv = New();
            inv.EnterWorld(3);
            inv.Add(Inventory.Map);
            Assert.IsTrue(inv.Has(Inventory.Map));
            inv.EnterWorld(0);
            Assert.IsFalse(inv.Has(Inventory.Map), "en el castillo no");
            inv.EnterWorld(4);
            Assert.IsFalse(inv.Has(Inventory.Map), "el del mundo 3 no vale en el 4");
            inv.EnterWorld(3);
            Assert.IsTrue(inv.Has(Inventory.Map), "al volver al 3, sigue");
            inv.ForgetWorldItems(3);
            inv.EnterWorld(3);
            Assert.IsFalse(inv.Has(Inventory.Map), "el Angel Ring desde el mundo los borra");
        }

        [Test]
        public void HatHelmetHalvesDamageButOneStaysOne()
        {
            var inv = New();
            Assert.AreEqual(3, inv.ReduceDamage(3));
            inv.Add(Inventory.HatHelmet);
            CollectionAssert.AreEqual(new[] { 1, 1, 2, 4 }, new[] { 1, 3, 5, 8 }.Select(inv.ReduceDamage));
        }

        [Test]
        public void ImmunityItemsAndShields()
        {
            var inv = New();
            Assert.IsFalse(inv.ImmuneTo(0x08));
            inv.Add(0x24);   // Pitcher
            Assert.IsTrue(inv.ImmuneTo(0x08), "la jarra protege de 0x08");
            Assert.IsFalse(inv.ShieldBlocks(0x3D));
            inv.Add(0x28);   // Copper Shield
            Assert.IsTrue(inv.ShieldBlocks(0x1D));
            Assert.IsFalse(inv.ShieldBlocks(0x3D), "nivel 1 no para 0x3D");
            inv.Add(0x2B);   // Silver Shield
            Assert.IsTrue(inv.ShieldBlocks(0x3D));
            Assert.AreEqual(2, inv.ShieldLevel);
        }

        [Test]
        public void ExperienceRefillsVitalityAtFortyEight()
        {
            // Medido: 60 de experiencia con 3 de vitalidad → 12 de experiencia y vitalidad llena.
            var inv = New();
            Assert.IsFalse(inv.AddExperience(Character.Popolon, 40));
            Assert.IsTrue(inv.AddExperience(Character.Popolon, 20));
            Assert.AreEqual(12, inv.Experience[(int)Character.Popolon]);
            inv.Add(Inventory.Vase);
            inv.AddExperience(Character.Popolon, 5);
            Assert.AreEqual(22, inv.Experience[(int)Character.Popolon], "el Vase la duplica");
        }

        [Test]
        public void GreatKeyRaisesMaxVitalityUpToSeventyTwo()
        {
            var inv = New();
            int[] vit = { 8, 3 }, max = { 8, 8 };
            Assert.IsTrue(inv.GreatKeyBoost(vit, max, Character.Popolon));
            Assert.AreEqual((16, 16), (max[1], vit[1]));
            Assert.AreEqual(8, max[0], "solo el personaje activo");
            max[1] = 72;
            Assert.IsFalse(inv.GreatKeyBoost(vit, max, Character.Popolon));
        }

        [Test]
        public void NecklaceAndConchGateWorldsSevenAndFive()
        {
            var inv = New();
            Assert.IsNotNull(inv.WorldEntryBlocked(7, Character.Popolon));
            Assert.IsNull(inv.WorldEntryBlocked(7, Character.Aphrodite));
            Assert.IsNotNull(inv.WorldEntryBlocked(5, Character.Aphrodite));
            inv.Add(Inventory.Necklace);
            inv.Add(Inventory.ConchShell);
            Assert.IsNull(inv.WorldEntryBlocked(7, Character.Popolon));
            Assert.IsNull(inv.WorldEntryBlocked(5, Character.Aphrodite));
        }

        [Test]
        public void BibleHasSixteenUsesAndHoiFixesWorldEight()
        {
            var inv = New();
            Assert.IsFalse(inv.UseBible(), "sin la Biblia");
            inv.Add(Inventory.Bible);
            Assert.AreEqual(16, Enumerable.Range(0, 20).Count(_ => inv.UseBible()));
            Assert.AreEqual(150, inv.BibleTicks);
            inv.EnterWorld(8);
            Assert.IsTrue(inv.ReversedControls);
            inv.Add(Inventory.Hoi);
            Assert.IsFalse(inv.ReversedControls);
            Assert.AreEqual(104, inv.WingRoom(1));
        }

        [Test]
        public void ShoesWalkTwoAndAHalfPixelsPerTick()
        {
            // Medido: 30 → 37,5 px en medio segundo (15 ticks).
            var rows = Enumerable.Range(0, 20).Select(r => new string(r >= 11 ? '#' : '.', 32)).ToArray();
            var room = new RoomModel { id = 1, collision = rows };
            var m = new PlayerMotor(new RoomWorld(new[] { room }), room, 40, 120) { Shoes = true };
            for (int t = 0; t < 16; t++) m.Tick(new MotorInput { Right = true });
            Assert.AreEqual(40 + 40, m.X);
        }
    }
}
