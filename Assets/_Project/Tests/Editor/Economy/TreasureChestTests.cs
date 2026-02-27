using System.Collections.Generic;
using NUnit.Framework;
using CatCatGo.Domain.Economy;
using CatCatGo.Domain.Enums;
using CatCatGo.Infrastructure;

namespace CatCatGo.Tests.Economy
{
    [TestFixture]
    public class TreasureChestTests
    {
        [Test]
        public void EquipmentChestCosts150PerPull()
        {
            var chest = new TreasureChest(ChestType.EQUIPMENT);
            Assert.AreEqual(150, chest.GetCostPerPull());
        }

        [Test]
        public void Pull10Costs9xSinglePull()
        {
            var chest = new TreasureChest(ChestType.EQUIPMENT);
            Assert.AreEqual(150 * 9, chest.GetPull10Cost());
        }

        [Test]
        public void PullReturnsEquipment()
        {
            var chest = new TreasureChest(ChestType.EQUIPMENT);
            var rng = new SeededRandom(42);
            var result = chest.Pull(rng);

            Assert.IsNotNull(result.Equipment);
            Assert.IsNotNull(result.Equipment.Grade);
        }

        [Test]
        public void Pull10Returns10Results()
        {
            var chest = new TreasureChest(ChestType.EQUIPMENT);
            var rng = new SeededRandom(42);
            var results = chest.Pull10(rng);

            Assert.AreEqual(10, results.Count);
        }

        [Test]
        public void PityCounterIncrementsOnEachPull()
        {
            var chest = new TreasureChest(ChestType.EQUIPMENT);
            var rng = new SeededRandom(42);

            chest.Pull(rng);
            Assert.AreEqual(1, chest.PityCount);

            chest.Pull(rng);
            Assert.AreEqual(2, chest.PityCount);
        }

        [Test]
        public void PityTriggersAt180PullsWithMythicReward()
        {
            var chest = new TreasureChest(ChestType.EQUIPMENT);
            var rng = new SeededRandom(42);

            for (int i = 0; i < 179; i++)
            {
                chest.Pull(rng);
            }
            Assert.AreEqual(179, chest.PityCount);

            var pityResult = chest.Pull(rng);
            Assert.IsTrue(pityResult.IsPity);
            Assert.AreEqual(EquipmentGrade.MYTHIC, pityResult.Equipment.Grade);
            Assert.AreEqual(0, chest.PityCount);
        }

        [Test]
        public void PetChestReturnsPetResources()
        {
            var chest = new TreasureChest(ChestType.PET);
            var rng = new SeededRandom(42);
            var result = chest.Pull(rng);

            Assert.IsNull(result.Equipment);
            Assert.Greater(result.Resources.Count, 0);
        }

        [Test]
        public void PityProgressTracksCorrectly()
        {
            var chest = new TreasureChest(ChestType.EQUIPMENT);
            var rng = new SeededRandom(42);

            for (int i = 0; i < 90; i++)
            {
                chest.Pull(rng);
            }

            Assert.AreEqual(0.5f, chest.GetPityProgress(), 0.001f);
            Assert.AreEqual(90, chest.GetRemainingToPity());
        }

        [Test]
        public void GradeDistributionFollowsSynthesisRatioWeights()
        {
            var chest = new TreasureChest(ChestType.EQUIPMENT);
            var rng = new SeededRandom(12345);
            var counts = new Dictionary<EquipmentGrade, int>();

            for (int i = 0; i < 3640; i++)
            {
                var result = chest.Pull(rng);
                if (result.Equipment != null)
                {
                    var grade = result.Equipment.Grade;
                    counts.TryGetValue(grade, out int current);
                    counts[grade] = current + 1;
                }
            }

            counts.TryGetValue(EquipmentGrade.COMMON, out int commonCount);
            counts.TryGetValue(EquipmentGrade.UNCOMMON, out int uncommonCount);
            counts.TryGetValue(EquipmentGrade.RARE, out int rareCount);
            counts.TryGetValue(EquipmentGrade.EPIC, out int epicCount);

            Assert.Greater(commonCount, uncommonCount);
            Assert.Greater(uncommonCount, rareCount);
            Assert.Greater(rareCount, epicCount);
        }
    }
}
