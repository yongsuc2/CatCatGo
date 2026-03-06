using System.Collections.Generic;
using NUnit.Framework;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Infrastructure;
using CatCatGo.Services;

namespace CatCatGo.Tests.Services
{
    [TestFixture]
    public class PetManagerTests
    {
        private PetManager _manager;

        [SetUp]
        public void SetUp()
        {
            _manager = new PetManager();
        }

        private Pet CreatePet(string id, string name, PetTier tier, PetGrade grade, int level = 1)
        {
            return new Pet(id, name, tier, grade, PetGrade.IMMORTAL, level,
                Stats.Create(atk: 5, maxHp: 10));
        }

        [Test]
        public void HatchEggCreatesCommonPet()
        {
            var rng = new SeededRandom(42);
            var pet = _manager.HatchEgg(rng);

            Assert.IsNotNull(pet);
            Assert.AreEqual(PetGrade.COMMON, pet.Grade);
            Assert.AreEqual(1, pet.Level);
            Assert.IsTrue(pet.Id.StartsWith("pet_"));
        }

        [Test]
        public void HatchEggProducesDifferentPetsWithDifferentSeeds()
        {
            var pet1 = _manager.HatchEgg(new SeededRandom(1));
            var pet2 = _manager.HatchEgg(new SeededRandom(2));

            Assert.AreNotEqual(pet1.Id, pet2.Id);
        }

        [Test]
        public void FeedPetDelegatesToPet()
        {
            var pet = CreatePet("p1", "Cat", PetTier.A, PetGrade.COMMON);
            var result = _manager.FeedPet(pet, 5);

            Assert.IsTrue(result.IsOk());
            Assert.AreEqual(50, pet.Exp);
        }

        [Test]
        public void TryUpgradeGradeFailsWithoutDuplicate()
        {
            var pet = CreatePet("p1", "Cat", PetTier.A, PetGrade.COMMON);
            var noDuplicates = new List<Pet>();

            var result = _manager.TryUpgradeGrade(pet, noDuplicates);
            Assert.IsTrue(result.IsFail());
        }

        [Test]
        public void TryUpgradeGradeFailsWhenOnlySelfInList()
        {
            var pet = CreatePet("p1", "Cat", PetTier.A, PetGrade.COMMON);
            var result = _manager.TryUpgradeGrade(pet, new List<Pet> { pet });

            Assert.IsTrue(result.IsFail());
        }

        [Test]
        public void TryUpgradeGradeSucceedsWithDuplicate()
        {
            var pet = CreatePet("p1", "Cat", PetTier.A, PetGrade.COMMON);
            var duplicate = CreatePet("p2", "Cat", PetTier.A, PetGrade.COMMON);

            var result = _manager.TryUpgradeGrade(pet, new List<Pet> { pet, duplicate });
            Assert.IsTrue(result.IsOk());
            Assert.AreEqual(PetGrade.RARE, pet.Grade);
        }

        [Test]
        public void SelectBestPetReturnsNullWhenNoPets()
        {
            var player = new Player();
            var result = _manager.SelectBestPet(player);
            Assert.IsNull(result);
        }

        [Test]
        public void SelectBestPetPrefersSTier()
        {
            var petA = CreatePet("p1", "CatA", PetTier.A, PetGrade.LEGENDARY, 10);
            var petS = CreatePet("p2", "CatS", PetTier.S, PetGrade.COMMON, 1);

            var player = new Player();
            player.AddPet(petA);
            player.AddPet(petS);

            var best = _manager.SelectBestPet(player);
            Assert.AreEqual("p2", best.Id);
        }

        [Test]
        public void SelectBestPetPrefersHigherGradeWithinSameTier()
        {
            var petCommon = CreatePet("p1", "Cat1", PetTier.A, PetGrade.COMMON, 10);
            var petEpic = CreatePet("p2", "Cat2", PetTier.A, PetGrade.EPIC, 1);

            var player = new Player();
            player.AddPet(petCommon);
            player.AddPet(petEpic);

            var best = _manager.SelectBestPet(player);
            Assert.AreEqual("p2", best.Id);
        }

        [Test]
        public void GetTotalPassiveBonusSumsAllPets()
        {
            var pets = new List<Pet>
            {
                CreatePet("p1", "Cat1", PetTier.A, PetGrade.COMMON, 5),
                CreatePet("p2", "Cat2", PetTier.B, PetGrade.COMMON, 3)
            };

            var bonus = _manager.GetTotalPassiveBonus(pets);
            Assert.Greater(bonus.Atk, 0);
            Assert.Greater(bonus.MaxHp, 0);
        }

        [Test]
        public void GetTotalPassiveBonusReturnsZeroForEmptyList()
        {
            var bonus = _manager.GetTotalPassiveBonus(new List<Pet>());
            Assert.AreEqual(0, bonus.Atk);
            Assert.AreEqual(0, bonus.MaxHp);
        }
    }
}
