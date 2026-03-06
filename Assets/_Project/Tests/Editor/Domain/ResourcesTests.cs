using NUnit.Framework;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.Enums;

namespace CatCatGo.Tests.Domain
{
    [TestFixture]
    public class ResourcesTests
    {
        private Resources _resources;

        [SetUp]
        public void SetUp()
        {
            _resources = new Resources();
        }

        [Test]
        public void StartsWithFullStamina()
        {
            Assert.AreEqual(100, _resources.Stamina);
            Assert.AreEqual(100, _resources.GetStaminaMax());
        }

        [Test]
        public void StartsWithZeroGoldAndGems()
        {
            Assert.AreEqual(0, _resources.Gold);
            Assert.AreEqual(0, _resources.Gems);
        }

        [Test]
        public void AddsGoldCorrectly()
        {
            _resources.Add(ResourceType.GOLD, 500);
            Assert.AreEqual(500, _resources.Gold);

            _resources.Add(ResourceType.GOLD, 300);
            Assert.AreEqual(800, _resources.Gold);
        }

        [Test]
        public void StaminaIsCappedAtMax()
        {
            _resources.Add(ResourceType.STAMINA, 50);
            Assert.AreEqual(100, _resources.Stamina);
        }

        [Test]
        public void SpendsResourceSuccessfully()
        {
            _resources.Add(ResourceType.GOLD, 1000);
            var result = _resources.Spend(ResourceType.GOLD, 400);

            Assert.IsTrue(result.IsOk());
            Assert.AreEqual(600, _resources.Gold);
        }

        [Test]
        public void SpendFailsWithInsufficientFunds()
        {
            _resources.Add(ResourceType.GOLD, 100);
            var result = _resources.Spend(ResourceType.GOLD, 200);

            Assert.IsTrue(result.IsFail());
            Assert.AreEqual(100, _resources.Gold);
        }

        [Test]
        public void CanAffordReturnsTrueWhenSufficient()
        {
            _resources.Add(ResourceType.GEMS, 500);
            Assert.IsTrue(_resources.CanAfford(ResourceType.GEMS, 500));
            Assert.IsTrue(_resources.CanAfford(ResourceType.GEMS, 499));
        }

        [Test]
        public void CanAffordReturnsFalseWhenInsufficient()
        {
            _resources.Add(ResourceType.GEMS, 100);
            Assert.IsFalse(_resources.CanAfford(ResourceType.GEMS, 101));
        }

        [Test]
        public void SpendMultipleIsAtomic()
        {
            _resources.Add(ResourceType.GOLD, 500);
            _resources.Add(ResourceType.GEMS, 10);

            var entries = new[]
            {
                (ResourceType.GOLD, 300f),
                (ResourceType.GEMS, 20f)
            };

            var result = _resources.SpendMultiple(entries);
            Assert.IsTrue(result.IsFail());
            Assert.AreEqual(500, _resources.Gold);
            Assert.AreEqual(10, _resources.Gems);
        }

        [Test]
        public void SpendMultipleDeductsAllWhenAffordable()
        {
            _resources.Add(ResourceType.GOLD, 500);
            _resources.Add(ResourceType.GEMS, 100);

            var entries = new[]
            {
                (ResourceType.GOLD, 200f),
                (ResourceType.GEMS, 50f)
            };

            var result = _resources.SpendMultiple(entries);
            Assert.IsTrue(result.IsOk());
            Assert.AreEqual(300, _resources.Gold);
            Assert.AreEqual(50, _resources.Gems);
        }

        [Test]
        public void TickRegeneratesStamina()
        {
            _resources.Spend(ResourceType.STAMINA, 50);
            Assert.AreEqual(50, _resources.Stamina);

            _resources.Tick(60000);
            Assert.AreEqual(51, _resources.Stamina);
        }

        [Test]
        public void TickDoesNotExceedStaminaMax()
        {
            float before = _resources.Stamina;
            _resources.Tick(600000);
            Assert.AreEqual(100, _resources.Stamina);
        }

        [Test]
        public void SetAmountBypassesStaminaCap()
        {
            _resources.SetAmount(ResourceType.STAMINA, 200);
            Assert.AreEqual(200, _resources.Stamina);
        }

        [Test]
        public void ToJsonAndFromJsonRoundTrip()
        {
            _resources.Add(ResourceType.GOLD, 1234);
            _resources.Add(ResourceType.GEMS, 567);
            _resources.Spend(ResourceType.STAMINA, 30);

            var json = _resources.ToJSON();
            var restored = Resources.FromJSON(json);

            Assert.AreEqual(1234, restored.Gold);
            Assert.AreEqual(567, restored.Gems);
            Assert.AreEqual(70, restored.Stamina);
        }

        [Test]
        public void GetReturnsZeroForUnsetResource()
        {
            Assert.AreEqual(0, _resources.Get(ResourceType.EQUIPMENT_STONE));
            Assert.AreEqual(0, _resources.Get(ResourceType.POWER_STONE));
        }
    }
}
