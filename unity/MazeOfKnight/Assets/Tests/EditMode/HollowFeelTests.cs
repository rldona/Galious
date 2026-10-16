using System.Linq;
using MazeOfKnight.Data;
using MazeOfKnight.Game;
using NUnit.Framework;

namespace MazeOfKnight.Tests
{
    // Reglas del remake con la sensación de Hollow Knight (RemakeRules.CoyoteJump y JumpBuffer): desactivadas, el motor
    // hace lo medido en el original; activadas, perdonan saltar un poco tarde o un poco pronto.
    public class HollowFeelTests
    {
        bool coyote, buffer;

        [SetUp]
        public void SetUp() { coyote = RemakeRules.CoyoteJump; buffer = RemakeRules.JumpBuffer; }

        [TearDown]
        public void TearDown() { RemakeRules.CoyoteJump = coyote; RemakeRules.JumpBuffer = buffer; }

        // Plataforma en la fila 11 de las columnas 0-9 y suelo abajo en la fila 15: se sale andando por x = 80.
        static PlayerMotor OffTheLedge()
        {
            var rows = Enumerable.Range(0, 20).Select(r => r == 11 ? new string('#', 10) + new string('.', 22) : r >= 15 ? new string('#', 32) : new string('.', 32)).ToArray();
            var room = new RoomModel { id = 1, collision = rows };
            var motor = new PlayerMotor(new RoomWorld(new[] { room }), room, 70, 120);
            while (motor.State == MotorState.Ground) motor.Tick(new MotorInput { Right = true });
            return motor;
        }

        // ↑ pulsado k ticks después de quedarse sin suelo: ¿salta?
        static bool JumpsAfterLeavingLedge(int k)
        {
            var m = OffTheLedge();
            for (int t = 0; t < k; t++) m.Tick(default);
            m.Tick(new MotorInput { Up = true, UpPressed = true });
            return m.State == MotorState.JumpPrep;
        }

        [Test]
        public void CoyoteJumpOnlyInTheFirstTwoTicksOffTheLedge()
        {
            RemakeRules.CoyoteJump = false;
            Assert.IsFalse(JumpsAfterLeavingLedge(0), "original: sin suelo no se salta");
            RemakeRules.CoyoteJump = true;
            Assert.IsTrue(JumpsAfterLeavingLedge(0));
            Assert.IsTrue(JumpsAfterLeavingLedge(1));
            Assert.IsFalse(JumpsAfterLeavingLedge(2));
        }

        [Test]
        public void CoyoteJumpIsTheSameJumpAsFromTheGround()
        {
            RemakeRules.CoyoteJump = true;
            var m = OffTheLedge();
            int y0 = m.Y;
            m.Tick(new MotorInput { Up = true, UpPressed = true });
            int top = m.Y;
            for (int t = 0; t < 16; t++) { m.Tick(new MotorInput { Up = true }); top = System.Math.Min(top, m.Y); }
            Assert.AreEqual(54, y0 - top, "Popolon con ↑ mantenido sube 54 px, como desde el suelo");
        }

        // Aphrodite salta con una pulsación en el tick 0; ¿en qué tick vuelve a estar en el suelo?
        static int LandingTick()
        {
            var room = new RoomModel { id = 1, collision = Enumerable.Range(0, 20).Select(r => new string(r >= 11 ? '#' : '.', 32)).ToArray() };
            var m = new PlayerMotor(new RoomWorld(new[] { room }), room, 64, 120) { Character = Character.Aphrodite };
            bool left = false;
            for (int t = 0; t < 60; t++)
            {
                m.Tick(new MotorInput { Up = t == 0, UpPressed = t == 0 });
                if (m.State != MotorState.Ground) left = true;
                else if (left) return t;
            }
            return -1;
        }

        // Salta, y vuelve a pulsar ↑ (y lo mantiene) en el tick 'press': ¿vuelve a saltar al tocar el suelo?
        static bool JumpsAgainOnLanding(int press, int landing)
        {
            var room = new RoomModel { id = 1, collision = Enumerable.Range(0, 20).Select(r => new string(r >= 11 ? '#' : '.', 32)).ToArray() };
            var m = new PlayerMotor(new RoomWorld(new[] { room }), room, 64, 120) { Character = Character.Aphrodite };
            for (int t = 0; t <= landing + 1; t++)
            {
                bool up = t == 0 || t >= press;
                m.Tick(new MotorInput { Up = up, UpPressed = t == 0 || t == press });
            }
            return m.State == MotorState.JumpPrep;
        }

        [Test]
        public void JumpBufferRemembersAPressInTheLastTwoAirTicks()
        {
            int landing = LandingTick();
            Assert.Greater(landing, 10);
            RemakeRules.JumpBuffer = false;
            Assert.IsFalse(JumpsAgainOnLanding(landing - 1, landing), "original: hace falta pulsar en el suelo");
            RemakeRules.JumpBuffer = true;
            Assert.IsTrue(JumpsAgainOnLanding(landing - 1, landing));
            Assert.IsTrue(JumpsAgainOnLanding(landing, landing));
            Assert.IsFalse(JumpsAgainOnLanding(landing - 2, landing));
        }

        [Test]
        public void HollowFeelSwitchesTheThreeRules()
        {
            bool hit = RemakeRules.HitStop;
            RemakeRules.HollowFeel = true;
            Assert.IsTrue(RemakeRules.HitStop && RemakeRules.CoyoteJump && RemakeRules.JumpBuffer);
            RemakeRules.HollowFeel = false;
            Assert.IsFalse(RemakeRules.HitStop || RemakeRules.CoyoteJump || RemakeRules.JumpBuffer);
            RemakeRules.HitStop = hit;
        }
    }
}
