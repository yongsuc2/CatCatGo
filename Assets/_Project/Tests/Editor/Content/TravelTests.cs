using NUnit.Framework;
using CatCatGo.Domain.Content;

namespace CatCatGo.Tests.Content
{
    [TestFixture]
    public class TravelTests
    {
        [Test]
        public void CalculatesGoldBasedOnChapterAndMultiplier()
        {
            var travel = new Travel(5);
            travel.SetMultiplier(3);

            var gold = travel.CalculateGold(10);
            Assert.Greater(gold, 0);

            travel.SetMultiplier(10);
            var gold10x = travel.CalculateGold(10);
            Assert.Greater(gold10x, gold);
        }

        [Test]
        public void RunsAndReturnsReward()
        {
            var travel = new Travel(3);
            var result = travel.Run(20, 50);

            Assert.IsTrue(result.IsOk());
            Assert.AreEqual(20, result.Data.StaminaSpent);
            Assert.Greater(result.Data.Reward.Resources.Count, 0);
        }

        [Test]
        public void FailsWithInsufficientStamina()
        {
            var travel = new Travel(3);
            var result = travel.Run(20, 10);

            Assert.IsTrue(result.IsFail());
        }

        [Test]
        public void RejectsInvalidMultipliers()
        {
            var travel = new Travel(1);
            var result = travel.SetMultiplier(7);
            Assert.IsTrue(result.IsFail());
        }

        [Test]
        public void AcceptsValidMultipliers()
        {
            var travel = new Travel(1);
            Assert.IsTrue(travel.SetMultiplier(3).IsOk());
            Assert.IsTrue(travel.SetMultiplier(50).IsOk());
        }

        [Test]
        public void PreviewMatchesActualCalculation()
        {
            var travel = new Travel(5);
            travel.SetMultiplier(10);
            Assert.AreEqual(travel.CalculateGold(30), travel.GetGoldPreview(30));
        }
    }
}
