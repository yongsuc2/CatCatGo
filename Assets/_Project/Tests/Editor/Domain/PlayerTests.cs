using NUnit.Framework;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.ValueObjects;

namespace CatCatGo.Tests.Domain
{
    [TestFixture]
    public class PlayerTests
    {
        [Test]
        public void StartsWithBaseStats()
        {
            var player = new Player();
            var stats = player.ComputeStats();

            Assert.Greater(stats.MaxHp, 0);
            Assert.Greater(stats.Atk, 0);
            Assert.Greater(stats.Def, 0);
        }

        [Test]
        public void TalentUpgradeIncreasesComputedStats()
        {
            var player = new Player();
            var baseStat = player.ComputeStats();

            player.Resources.SetAmount(ResourceType.GOLD, 100000);
            player.Talent.Upgrade(StatType.ATK, 100000);
            player.Talent.Upgrade(StatType.ATK, 100000);
            player.Talent.Upgrade(StatType.ATK, 100000);

            var newStats = player.ComputeStats();
            Assert.Greater(newStats.Atk, baseStat.Atk);
        }

        [Test]
        public void EquippingWeaponIncreasesAtk()
        {
            var player = new Player();
            var baseStat = player.ComputeStats();

            var weapon = new Equipment("w1", "Sword", SlotType.WEAPON, EquipmentGrade.EPIC, false);
            player.GetEquipmentSlot(SlotType.WEAPON).Equip(weapon);

            var newStats = player.ComputeStats();
            Assert.Greater(newStats.Atk, baseStat.Atk);
        }

        [Test]
        public void ActivePetAddsStats()
        {
            var player = new Player();
            var baseStat = player.ComputeStats();

            var pet = new Pet("p1", "Elsa", PetTier.S, PetGrade.LEGENDARY, PetGrade.IMMORTAL, 5, Stats.Create(atk: 10, maxHp: 50));
            player.AddPet(pet);
            player.SetActivePet(pet);

            var newStats = player.ComputeStats();
            Assert.Greater(newStats.Atk, baseStat.Atk);
            Assert.Greater(newStats.MaxHp, baseStat.MaxHp);
        }

        [Test]
        public void InactivePetsProvideReducedPassiveBonus()
        {
            var player = new Player();

            var activePet = new Pet("p1", "Active", PetTier.S, PetGrade.LEGENDARY, PetGrade.IMMORTAL, 5, Stats.Create(atk: 10));
            var inactivePet = new Pet("p2", "Inactive", PetTier.A, PetGrade.EPIC, PetGrade.LEGENDARY, 5, Stats.Create(atk: 10));

            player.AddPet(activePet);
            player.AddPet(inactivePet);
            player.SetActivePet(activePet);

            var statsWithInactive = player.ComputeStats();

            var playerNoInactive = new Player();
            playerNoInactive.AddPet(activePet);
            playerNoInactive.SetActivePet(activePet);
            var statsWithoutInactive = playerNoInactive.ComputeStats();

            Assert.Greater(statsWithInactive.Atk, statsWithoutInactive.Atk);
        }

        [Test]
        public void HpEqualsMaxHpAfterComputeStats()
        {
            var player = new Player();
            var stats = player.ComputeStats();
            Assert.AreEqual(stats.MaxHp, stats.Hp);
        }
    }
}
