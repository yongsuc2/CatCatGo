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

    }
}
