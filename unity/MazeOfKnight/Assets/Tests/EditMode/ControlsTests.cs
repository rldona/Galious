using MazeOfKnight.Game;
using NUnit.Framework;

namespace MazeOfKnight.Tests
{
    // Mando y opciones (paso 7): los botones por defecto y sus nombres, y el volumen por canal del chip de sonido.
    public class ControlsTests
    {
        [Test]
        public void DefaultButtonsAreTheOnesOfTheGame()
        {
            Assert.AreEqual("<Gamepad>/buttonSouth", Controls.Get("saltar").Pad);
            Assert.AreEqual("<Keyboard>/space", Controls.Get("espada").Key);
            Assert.AreEqual("<Keyboard>/leftCtrl", Controls.Get("biblia").Key);
            Assert.AreEqual("<Gamepad>/leftShoulder", Controls.Get("biblia").Pad);
            Assert.AreEqual("<Gamepad>/rightShoulder", Controls.Get("palabras").Pad);
        }

        [Test]
        public void ButtonsHaveShortNames()
        {
            Assert.AreEqual("A", Controls.Label("<Gamepad>/buttonSouth"));
            Assert.AreEqual("RB", Controls.Label("<Gamepad>/rightShoulder"));
            Assert.AreEqual("Espacio", Controls.Label("<Keyboard>/space"));
            Assert.AreEqual("M", Controls.Label("<Keyboard>/m"));
            Assert.AreEqual("—", Controls.Label(null));
        }

        [Test]
        public void FullChannelGainSoundsLikeTheOriginal()
        {
            var a = new Psg(44100);
            var b = new Psg(44100);
            foreach (var p in new[] { a, b })
            {
                p.Regs[0] = 0x40; p.Regs[2] = 0x80; p.Regs[4] = 0xC0;
                p.Regs[7] = 0x38; p.Regs[8] = 15; p.Regs[9] = 12; p.Regs[10] = 10;
            }
            b.Gain[0] = b.Gain[1] = b.Gain[2] = 1;
            for (int i = 0; i < 2000; i++) Assert.AreEqual(a.Sample(), b.Sample());
            var c = new Psg(44100);
            c.Regs[7] = 0x38; c.Regs[0] = 0x40; c.Regs[8] = 15;
            c.Gain[0] = 0;
            float sum = 0;
            for (int i = 0; i < 2000; i++) sum += System.Math.Abs(c.Sample());
            Assert.Less(sum, 0.01f, "un canal a 0 no suena");
        }
    }
}
