using NUnit.Framework;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.ValueObjects;

namespace CatCatGo.Tests.Domain
{
    [TestFixture]
    public class PetTests
    {
        private Pet CreatePet(PetGrade grade = PetGrade.COMMON, int level = 1, int exp = 0)
        {
            return new Pet("pet_1", "TestCat", PetTier.A, grade, PetGrade.IMMORTAL, level,
                Stats.Create(atk: 5, maxHp: 10), exp);
        }

        [Test]
        public void FeedIncreasesExp()
        {
            var pet = CreatePet();
            var result = pet.Feed(5);

            Assert.IsTrue(result.IsOk());
            Assert.AreEqual(50, pet.Exp);
        }

        [Test]
        public void FeedWithZeroFoodFails()
        {
            var pet = CreatePet();
            var result = pet.Feed(0);

            Assert.IsTrue(result.IsFail());
        }

        [Test]
        public void FeedWithNegativeFoodFails()
        {
            var pet = CreatePet();
            var result = pet.Feed(-1);

            Assert.IsTrue(result.IsFail());
        }

        [Test]
        public void FeedLevelsUpWhenExpReachesThreshold()
        {
            var pet = CreatePet();
            var result = pet.Feed(10);

            Assert.IsTrue(result.IsOk());
            Assert.AreEqual(2, pet.Level);
            Assert.AreEqual(1, result.Data.LevelsGained);
        }

        [Test]
        public void FeedCanCauseMultipleLevelUps()
        {
            var pet = CreatePet();
            var result = pet.Feed(50);

            Assert.IsTrue(result.IsOk());
            Assert.Greater(result.Data.LevelsGained, 1);
            Assert.Greater(pet.Level, 2);
        }

        [Test]
        public void ExpToNextLevelIncreasesWithLevel()
        {
            var pet1 = CreatePet(level: 1);
            var pet5 = CreatePet(level: 5);

            Assert.AreEqual(100, pet1.GetExpToNextLevel());
            Assert.AreEqual(180, pet5.GetExpToNextLevel());
        }

        [Test]
        public void UpgradeGradeAdvancesToNextGrade()
        {
            var pet = CreatePet(PetGrade.COMMON);
            var result = pet.UpgradeGrade();

            Assert.IsTrue(result.IsOk());
            Assert.AreEqual(PetGrade.RARE, result.Data.NewGrade);
            Assert.AreEqual(PetGrade.RARE, pet.Grade);
        }

        [Test]
        public void UpgradeGradeFailsAtMaxGrade()
        {
            var pet = CreatePet(PetGrade.IMMORTAL);
            var result = pet.UpgradeGrade();

            Assert.IsTrue(result.IsFail());
            Assert.AreEqual(PetGrade.IMMORTAL, pet.Grade);
        }

        [Test]
        public void IsMaxGradeReturnsTrueAtMaxGrade()
        {
            var pet = CreatePet(PetGrade.IMMORTAL);
            Assert.IsTrue(pet.IsMaxGrade());
        }

        [Test]
        public void IsMaxGradeReturnsFalseBeforeMaxGrade()
        {
            var pet = CreatePet(PetGrade.COMMON);
            Assert.IsFalse(pet.IsMaxGrade());
        }

        [Test]
        public void GetGlobalBonusIncludesLevelScaling()
        {
            var pet = CreatePet(level: 5);
            var bonus = pet.GetGlobalBonus();

            Assert.AreEqual(5 + 5 * 2, bonus.Atk);
            Assert.AreEqual(10 + 5 * 4, bonus.MaxHp);
        }

        [Test]
        public void GetGradeIndexReturnsCorrectOrder()
        {
            Assert.AreEqual(0, CreatePet(PetGrade.COMMON).GetGradeIndex());
            Assert.AreEqual(1, CreatePet(PetGrade.RARE).GetGradeIndex());
            Assert.AreEqual(2, CreatePet(PetGrade.EPIC).GetGradeIndex());
            Assert.AreEqual(3, CreatePet(PetGrade.LEGENDARY).GetGradeIndex());
            Assert.AreEqual(4, CreatePet(PetGrade.IMMORTAL).GetGradeIndex());
        }

        [Test]
        public void UpgradeGradeFailsWhenCustomMaxGradeReached()
        {
            var pet = new Pet("pet_1", "TestCat", PetTier.B, PetGrade.RARE, PetGrade.RARE, 1);
            var result = pet.UpgradeGrade();

            Assert.IsTrue(result.IsFail());
        }
    }
}
