using System.Collections.Generic;
using NUnit.Framework;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.Enums;
using CatCatGo.Services;

namespace CatCatGo.Tests.Services
{
    [TestFixture]
    public class EquipmentManagerTests
    {
        private EquipmentManager _manager;

        [SetUp]
        public void SetUp()
        {
            _manager = new EquipmentManager();
        }

        [Test]
        public void CompareEquipmentPrefersHigherGrade()
        {
            var epic = new Equipment("w1", "Epic", SlotType.WEAPON, EquipmentGrade.EPIC, false);
            var legendary = new Equipment("w2", "Legend", SlotType.WEAPON, EquipmentGrade.LEGENDARY, false);

            int result = _manager.CompareEquipment(legendary, epic);
            Assert.Greater(result, 0);
        }

        [Test]
        public void CompareEquipmentPrefersSGradeAtSameGrade()
        {
            var normal = new Equipment("w1", "Normal", SlotType.WEAPON, EquipmentGrade.EPIC, false);
            var sGrade = new Equipment("w2", "S-Grade", SlotType.WEAPON, EquipmentGrade.EPIC, true);

            int result = _manager.CompareEquipment(sGrade, normal);
            Assert.Greater(result, 0);
        }

        [Test]
        public void CompareEquipmentUsesStatTotalAsTiebreaker()
        {
            var weapon1 = new Equipment("w1", "Weak", SlotType.WEAPON, EquipmentGrade.COMMON, false);
            var weapon2 = new Equipment("w2", "Strong", SlotType.WEAPON, EquipmentGrade.COMMON, false, 5);

            int result = _manager.CompareEquipment(weapon2, weapon1);
            Assert.Greater(result, 0);
        }

        [Test]
        public void AutoEquipBestEquipsIntoEmptySlots()
        {
            var player = new Player();
            var weapon = new Equipment("w1", "Sword", SlotType.WEAPON, EquipmentGrade.COMMON, false);
            var armor = new Equipment("a1", "Armor", SlotType.ARMOR, EquipmentGrade.COMMON, false);
            var inventory = new List<Equipment> { weapon, armor };

            var replaced = _manager.AutoEquipBest(player, inventory);
            Assert.AreEqual(0, replaced.Count);
        }

        [Test]
        public void AutoEquipBestReplacesWeakerEquipment()
        {
            var player = new Player();
            var weakWeapon = new Equipment("w1", "Weak", SlotType.WEAPON, EquipmentGrade.COMMON, false);
            var strongWeapon = new Equipment("w2", "Strong", SlotType.WEAPON, EquipmentGrade.EPIC, false);

            _manager.AutoEquipBest(player, new List<Equipment> { weakWeapon });

            var replaced = _manager.AutoEquipBest(player, new List<Equipment> { strongWeapon });
            Assert.AreEqual(1, replaced.Count);
            Assert.AreEqual("w1", replaced[0].Id);
        }

        [Test]
        public void GetUpgradePriorityReturnsNullForEmptyLoadout()
        {
            var player = new Player();
            var result = _manager.GetUpgradePriority(player);
            Assert.IsNull(result);
        }

        [Test]
        public void GetUpgradePriorityReturnsSlotForEquippedItem()
        {
            var player = new Player();
            var weapon = new Equipment("w1", "Sword", SlotType.WEAPON, EquipmentGrade.COMMON, false);
            player.AddToInventory(weapon);
            player.EquipFromInventory("w1");

            if (!weapon.NeedsPromote())
            {
                var result = _manager.GetUpgradePriority(player);
                Assert.IsNotNull(result);
                Assert.AreEqual(SlotType.WEAPON, result.Value.Slot);
            }
        }

        [Test]
        public void CompareEquipmentReturnsZeroForIdentical()
        {
            var weapon = new Equipment("w1", "Sword", SlotType.WEAPON, EquipmentGrade.COMMON, false);
            int result = _manager.CompareEquipment(weapon, weapon);
            Assert.AreEqual(0, result);
        }
    }
}
