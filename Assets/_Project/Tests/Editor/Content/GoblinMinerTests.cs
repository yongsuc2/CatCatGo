using NUnit.Framework;
using CatCatGo.Domain.Content;
using CatCatGo.Infrastructure;

namespace CatCatGo.Tests.Content
{
    [TestFixture]
    public class GoblinMinerTests
    {
        [Test]
        public void MinesAndGainsOre()
        {
            var miner = new GoblinMiner();
            var result = miner.Mine(1);
            Assert.IsTrue(result.IsOk());
            Assert.AreEqual(1, miner.OreCount);
        }

        [Test]
        public void FailsToMineWithoutPickaxes()
        {
            var miner = new GoblinMiner();
            var result = miner.Mine(0);
            Assert.IsTrue(result.IsFail());
        }

        [Test]
        public void CannotUseCartBefore30Ore()
        {
            var miner = new GoblinMiner(29);
            Assert.IsFalse(miner.CanUseCart());
        }

        [Test]
        public void CanUseCartAt30Ore()
        {
            var miner = new GoblinMiner(30);
            Assert.IsTrue(miner.CanUseCart());
        }

        [Test]
        public void UsesCartAndGetsReward()
        {
            var miner = new GoblinMiner(30);
            var rng = new SeededRandom(42);
            var result = miner.UseCart(rng);

            Assert.IsTrue(result.IsOk());
            Assert.AreEqual(0, miner.OreCount);
            Assert.Greater(result.Data.Reward.Resources.Count, 0);
        }

        [Test]
        public void TracksProgressCorrectly()
        {
            var miner = new GoblinMiner(15);
            Assert.AreEqual(0.5f, miner.GetProgress(), 0.001f);
            Assert.AreEqual(15, miner.GetOreNeeded());
        }
    }
}
