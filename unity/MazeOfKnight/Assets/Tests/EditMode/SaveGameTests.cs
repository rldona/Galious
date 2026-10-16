using System.IO;
using MazeOfKnight.Game;
using NUnit.Framework;

namespace MazeOfKnight.Tests
{
    // Guardado en dos pasos (SaveData.WriteAtomic): primero <fichero>.new y luego en su sitio.
    public class SaveGameTests
    {
        string dir, path;

        [SetUp]
        public void SetUp()
        {
            dir = Path.Combine(Path.GetTempPath(), "mok_guardado_" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            path = Path.Combine(dir, "partida.json");
        }

        [TearDown]
        public void TearDown() => Directory.Delete(dir, true);

        [Test]
        public void WritesAndReplacesWithoutLeavingTheTemporary()
        {
            SaveData.WriteAtomic(path, "uno");
            Assert.AreEqual("uno", File.ReadAllText(path));
            SaveData.WriteAtomic(path, "dos");
            Assert.AreEqual("dos", File.ReadAllText(path));
            Assert.IsFalse(File.Exists(path + ".new"));
        }

        [Test]
        public void CutBeforeReplacingKeepsThePreviousSave()
        {
            SaveData.WriteAtomic(path, "buena");
            File.WriteAllText(path + ".new", "a med");       // el juego se cerró escribiendo la nueva
            Assert.AreEqual("buena", SaveData.ReadAtomic(path));
        }

        [Test]
        public void CutAfterWritingTheTemporaryRecoversIt()
        {
            File.WriteAllText(path + ".new", "nueva");        // completa, pero no llegó a ponerse en su sitio
            Assert.AreEqual("nueva", SaveData.ReadAtomic(path));
            Assert.IsTrue(File.Exists(path));
            Assert.IsFalse(File.Exists(path + ".new"));
        }
    }
}
