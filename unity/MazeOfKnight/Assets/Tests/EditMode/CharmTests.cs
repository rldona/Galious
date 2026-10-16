using System.Collections.Generic;
using MazeOfKnight.Data;
using MazeOfKnight.Game;
using NUnit.Framework;
using UnityEngine;

namespace MazeOfKnight.Tests
{
    // RemakeRules.HollowCharms: los objetos pasivos son amuletos con muescas.
    public class CharmTests
    {
        static Inventory NewInventory(int bosses = 0)
        {
            var json = Resources.Load<TextAsset>("Items");
            Assert.IsNotNull(json, "falta Resources/Items.json");
            return new Inventory(JsonUtility.FromJson<ItemsData>(json.text), new List<int>()) { BossesDefeated = () => bosses };
        }

        [Test]
        public void ACharmIsEquippedOnPickupOnlyIfItsNotchesFit()
        {
            var inv = NewInventory();
            Assert.AreEqual(3, inv.Notches);
            inv.Add(0x11);                       // botas, 2
            Assert.IsTrue(inv.Has(0x11));
            inv.Add(0x19);                       // casco de sombrero, 3: no cabe
            Assert.IsFalse(inv.Has(0x19));
            Assert.IsTrue(inv.Owns(0x19));
            Assert.AreEqual(2, inv.NotchesUsed);
            Assert.IsFalse(inv.Equip(0x19));
            Assert.IsTrue(inv.Unequip(0x11));
            Assert.IsTrue(inv.Equip(0x19));
            Assert.AreEqual(1, inv.ReduceDamage(2));
        }

        [Test]
        public void EveryBossDefeatedGivesOneMoreNotch() => Assert.AreEqual(6, NewInventory(3).Notches);

        [Test]
        public void KeysAndProgressItemsAreAlwaysActive()
        {
            var inv = NewInventory();
            foreach (int t in new[] { 0x0C, 0x0D, 0x13, 0x15, 0x16, 0x18, 0x27 })
            {
                Assert.IsFalse(Inventory.IsCharm(t), $"0x{t:X2}");
                inv.Add(t);
                Assert.IsTrue(inv.Has(t));
            }
            Assert.AreEqual(0, inv.NotchesUsed);
        }

        [Test]
        public void TheShieldsAreOneCharmOfTheBestLevel()
        {
            var inv = NewInventory();
            inv.Add(0x28);
            inv.Add(0x2B);
            Assert.AreEqual(2, inv.ShieldLevel);
            Assert.AreEqual(2, inv.NotchesUsed);
            inv.Unequip(0x2B);
            Assert.AreEqual(0, inv.ShieldLevel);
            Assert.AreEqual(2, inv.OwnedShieldLevel);
            inv.Equip(0x28);
            Assert.AreEqual(2, inv.ShieldLevel);
        }

        [Test]
        public void OldSavesKeepTheirItemsAsEquippedCharms()
        {
            var inv = NewInventory();
            inv.Active.AddRange(new[] { 0x0E, 0x11, 0x19, 0x0D });
            inv.AdoptCharms(new int[0]);
            Assert.IsTrue(inv.OwnsCharm(0x19) && inv.Equipped(0x19));
            Assert.IsFalse(inv.OwnedCharms.Contains(0x0D));
        }

        [Test]
        public void EveryItemHasASpanishText()
        {
            for (int t = 1; t <= 0x2F; t++)
                Assert.IsFalse(ItemTexts.Get(t).Name.StartsWith("Objeto"), $"0x{t:X2}");
        }
    }
}
