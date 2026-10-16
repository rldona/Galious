using MazeOfKnight.Game;
using NUnit.Framework;

namespace MazeOfKnight.Tests
{
    // RemakeRules.HollowJournal: el diario del cazador cuenta derrotas por tipo y se guarda.
    public class JournalTests
    {
        [Test]
        public void CountsKillsOfEveryTypeOfAnEntryAndWarnsOnlyOnce()
        {
            var j = new Journal();
            int news = 0;
            j.NewType += _ => news++;
            j.Killed(0x19);
            j.Killed(0x19);
            j.Killed(0x37);
            j.Seen(0x31);
            Assert.AreEqual(3, j.Kills(new[] { 0x19, 0x37 }));
            Assert.AreEqual(3, news);
            Assert.IsTrue(j.Known(new[] { 0x31 }));
            Assert.AreEqual(0, j.Kills(new[] { 0x31 }));
            Assert.IsFalse(j.Known(new[] { 0x0E }));
        }

        [Test]
        public void SurvivesASaveAndLoad()
        {
            var j = new Journal();
            for (int i = 0; i < 12; i++) j.Killed(0x0E);
            j.Killed(0x101);
            j.Seen(0x31);
            var k = new Journal();
            k.Restore(j.Pack());
            Assert.AreEqual(12, k.Kills(new[] { 0x0E }));
            Assert.AreEqual(1, k.Kills(new[] { 0x101 }));
            Assert.IsTrue(k.Known(new[] { 0x31 }));
        }
    }
}
