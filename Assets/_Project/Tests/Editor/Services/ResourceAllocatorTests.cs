using NUnit.Framework;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.Enums;
using CatCatGo.Services;

namespace CatCatGo.Tests.Services
{
    [TestFixture]
    public class ResourceAllocatorTests
    {
        private ResourceAllocator _allocator;

        [SetUp]
        public void SetUp()
        {
            _allocator = new ResourceAllocator();
        }

        [Test]
        public void AllocatesMajorityToAtk()
        {
            var player = new Player();
            player.Resources.SetAmount(ResourceType.GOLD, 10000);

            var plan = _allocator.AllocateGold(player);
            Assert.Greater(plan.AtkAmount, plan.HpAmount);
            Assert.Greater(plan.HpAmount, plan.DefAmount);
        }

        [Test]
        public void TotalAllocationDoesNotExceedAvailableGold()
        {
            var player = new Player();
            player.Resources.SetAmount(ResourceType.GOLD, 10000);

            var plan = _allocator.AllocateGold(player);
            var total = plan.AtkAmount + plan.HpAmount + plan.DefAmount + plan.HeritageAmount;
            Assert.LessOrEqual(total, 10000);
        }

        [Test]
        public void AutoUpgradesTalentWithGold()
        {
            var player = new Player();
            player.Resources.SetAmount(ResourceType.GOLD, 50000);

            var results = _allocator.AutoUpgradeTalent(player);
            Assert.Greater(results.Count, 0);
            Assert.Greater(player.Talent.AtkLevel, 0);
            Assert.Less(player.Resources.Gold, 50000);
        }

        [Test]
        public void AdvisesAgainstSpendingGemsOnStamina()
        {
            var player = new Player();
            player.Resources.SetAmount(ResourceType.GEMS, 10000);

            Assert.IsFalse(_allocator.ShouldSpendGems(player, "stamina"));
        }

        [Test]
        public void AdvisesGachaWhenEnoughGemsFor10Pull()
        {
            var player = new Player();
            player.Resources.SetAmount(ResourceType.GEMS, 3000);

            Assert.IsTrue(_allocator.ShouldSpendGems(player, "gacha"));
        }
    }
}
