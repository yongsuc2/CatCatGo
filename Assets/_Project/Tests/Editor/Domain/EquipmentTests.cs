using NUnit.Framework;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.Enums;

namespace CatCatGo.Tests.Domain
{
    [TestFixture]
    public class EquipmentTests
    {
        [Test]
        public void ComputesStatsBasedOnGradeAndSlot()
        {
            var weapon = new Equipment("w1", "Iron Sword", SlotType.WEAPON, EquipmentGrade.COMMON, false);
            var stats = weapon.GetStats();
            Assert.Greater(stats.Atk, 0);
        }

        [Test]
        public void StatsIncreaseWithLevelUpgrade()
        {
            var weapon = new Equipment("w1", "Iron Sword", SlotType.WEAPON, EquipmentGrade.EPIC, false);
            var baseStats = weapon.GetStats();

            for (int i = 0; i < 5; i++)
            {
                weapon.Upgrade(10);
            }
            var upgradedStats = weapon.GetStats();
            Assert.Greater(upgradedStats.Atk, baseStats.Atk);
        }

        [Test]
        public void SGradeIsBetterThanNonSAtSameGrade()
        {
            var normal = new Equipment("w1", "Sword", SlotType.WEAPON, EquipmentGrade.EPIC, false);
            var sGrade = new Equipment("w2", "S-Sword", SlotType.WEAPON, EquipmentGrade.EPIC, true);
            Assert.IsTrue(sGrade.IsBetterThan(normal));
        }

        [Test]
        public void HigherGradeIsAlwaysBetter()
        {
            var epic = new Equipment("w1", "Epic Sword", SlotType.WEAPON, EquipmentGrade.EPIC, false);
            var legendary = new Equipment("w2", "Legend Sword", SlotType.WEAPON, EquipmentGrade.LEGENDARY, false);
            Assert.IsTrue(legendary.IsBetterThan(epic));
        }

        [Test]
        public void TransfersLevelToAnotherEquipment()
        {
            var old = new Equipment("w1", "Old", SlotType.WEAPON, EquipmentGrade.COMMON, false, 15, 1);
            var newer = new Equipment("w2", "New", SlotType.WEAPON, EquipmentGrade.RARE, false);

            old.TransferLevelTo(newer);
            Assert.AreEqual(15, newer.Level);
            Assert.AreEqual(1, newer.PromoteCount);
        }
    }

    [TestFixture]
    public class EquipmentSlotTests
    {
        [Test]
        public void EquipsAndUnequipsCorrectly()
        {
            var slot = new EquipmentSlot(SlotType.WEAPON);
            var weapon = new Equipment("w1", "Sword", SlotType.WEAPON, EquipmentGrade.COMMON, false);

            var result = slot.Equip(weapon);
            Assert.IsTrue(result.IsOk());
            Assert.AreEqual(1, slot.GetEquipped().Count);

            var unequipResult = slot.Unequip(0);
            Assert.IsTrue(unequipResult.IsOk());
            Assert.AreEqual(0, slot.GetEquipped().Count);
        }

        [Test]
        public void RingSlotAllows2Equipment()
        {
            var slot = new EquipmentSlot(SlotType.RING);
            var ring1 = new Equipment("r1", "Ring1", SlotType.RING, EquipmentGrade.COMMON, false);
            var ring2 = new Equipment("r2", "Ring2", SlotType.RING, EquipmentGrade.COMMON, false);

            slot.Equip(ring1, 0);
            slot.Equip(ring2, 1);
            Assert.AreEqual(2, slot.GetEquipped().Count);
        }

        [Test]
        public void RejectsEquipmentWithWrongSlotType()
        {
            var slot = new EquipmentSlot(SlotType.WEAPON);
            var ring = new Equipment("r1", "Ring", SlotType.RING, EquipmentGrade.COMMON, false);

            var result = slot.Equip(ring);
            Assert.IsTrue(result.IsFail());
        }

        [Test]
        public void ComputesTotalStatsFromAllEquipped()
        {
            var slot = new EquipmentSlot(SlotType.RING);
            var ring1 = new Equipment("r1", "Ring1", SlotType.RING, EquipmentGrade.COMMON, false);
            var ring2 = new Equipment("r2", "Ring2", SlotType.RING, EquipmentGrade.RARE, false);

            slot.Equip(ring1, 0);
            slot.Equip(ring2, 1);

            var stats = slot.GetTotalStats();
            Assert.Greater(stats.Atk, 0);
        }
    }
}
