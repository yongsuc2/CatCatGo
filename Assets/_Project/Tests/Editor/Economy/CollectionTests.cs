using NUnit.Framework;
using CatCatGo.Domain.Economy;

namespace CatCatGo.Tests.Economy
{
    [TestFixture]
    public class CollectionTests
    {
        [Test]
        public void StartsWithNoAcquiredEntries()
        {
            var col = new Collection();
            Assert.AreEqual(0, col.GetAcquiredCount());
            Assert.Greater(col.GetTotalCount(), 0);
        }

        [Test]
        public void AcquiresEntryAndUpdatesCount()
        {
            var col = new Collection();
            var success = col.Acquire("col_sword_1");

            Assert.IsTrue(success);
            Assert.AreEqual(1, col.GetAcquiredCount());
            Assert.IsTrue(col.IsAcquired("col_sword_1"));
        }

        [Test]
        public void CannotAcquireSameEntryTwice()
        {
            var col = new Collection();
            col.Acquire("col_sword_1");
            var second = col.Acquire("col_sword_1");

            Assert.IsFalse(second);
            Assert.AreEqual(1, col.GetAcquiredCount());
        }

        [Test]
        public void TotalBonusIncreasesWithAcquisitions()
        {
            var col = new Collection();
            var before = col.GetTotalBonus();
            Assert.AreEqual(0, before.Atk);

            col.Acquire("col_sword_1");
            var after = col.GetTotalBonus();
            Assert.Greater(after.Atk, 0);
        }

        [Test]
        public void TracksProgressPercentage()
        {
            var col = new Collection();
            Assert.AreEqual(0f, col.GetProgress());

            var total = col.GetTotalCount();
            var allEntries = col.GetAllEntries();
            foreach (var entry in allEntries)
            {
                col.Acquire(entry.Id);
            }

            Assert.AreEqual(1f, col.GetProgress());
            Assert.AreEqual(total, col.GetAcquiredCount());
        }
    }
}
