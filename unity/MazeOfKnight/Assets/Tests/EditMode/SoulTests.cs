using MazeOfKnight.Game;
using NUnit.Framework;

namespace MazeOfKnight.Tests
{
    // RemakeRules.HollowSoul: el alma y la concentración.
    public class SoulTests
    {
        [Test]
        public void HitsFillTheOrbAndFocusHealsTwoMasksForAThird()
        {
            var soul = new Soul();
            for (int i = 0; i < 20; i++) soul.Gain(false);
            Assert.AreEqual(Soul.Max, soul.Value, "hasta 99");
            soul.Value = 0;
            soul.Gain(true);
            Assert.AreEqual(Soul.HitGain * 2, soul.Value, "el doble con el Vase");
            soul.Value = 40;
            int healed = 0, ticks = 0;
            while (healed == 0 && ticks < 100) { healed = soul.Tick(true, true); ticks++; }
            Assert.AreEqual(Soul.FocusTicks, ticks, "0,9 s mantenido");
            Assert.AreEqual(Soul.FocusHeal, healed);
            Assert.AreEqual(40 - Soul.FocusCost, soul.Value);
            Assert.AreEqual(0, soul.Tick(true, true), "sin alma suficiente no empieza otra");
            Assert.IsFalse(soul.Focusing);
            soul.Value = 99;
            for (int i = 0; i < 10; i++) soul.Tick(true, true);
            soul.Tick(false, true);
            Assert.IsFalse(soul.Focusing, "soltar lo corta");
            Assert.AreEqual(99, soul.Value, "y no gasta");
            for (int i = 0; i < 10; i++) soul.Tick(true, true);
            soul.Tick(true, false);
            Assert.IsFalse(soul.Focusing, "un golpe o moverse lo corta");
        }
    }
}
