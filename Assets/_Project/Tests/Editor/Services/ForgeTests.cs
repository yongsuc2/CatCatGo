using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.Enums;
using CatCatGo.Services;

namespace CatCatGo.Tests.Services
{
    [TestFixture]
    public class ForgeTests
    {
        private Forge _forge;

        [SetUp]
        public void SetUp()
        {
            _forge = new Forge();
        }

        private static Equipment MakeEquipment(
            SlotType slot,
            EquipmentGrade grade,
            bool isS = false,
            int level = 0,
            int promoteCount = 0,
            int mergeLevel = 0)
        {
            return new Equipment(
                $"eq_{Guid.NewGuid():N}",
                $"{grade} {slot}",
                slot,
                grade,
                isS,
                level,
                promoteCount,
                null,
                null,
                mergeLevel);
        }

        private static List<Equipment> MakeMany(
            int count,
            SlotType slot,
            EquipmentGrade grade,
            bool isS = false,
            int level = 0,
            int mergeLevel = 0)
        {
            var list = new List<Equipment>();
            for (int i = 0; i < count; i++)
                list.Add(MakeEquipment(slot, grade, isS, level, 0, mergeLevel));
            return list;
        }

        [Test]
        public void CanMerge_ReturnsFalseForEmptyList()
        {
            Assert.IsFalse(_forge.CanMerge(new List<Equipment>()));
        }

        [Test]
        public void CanMerge_ReturnsFalseForSingleItem()
        {
            Assert.IsFalse(_forge.CanMerge(new List<Equipment> { MakeEquipment(SlotType.WEAPON, EquipmentGrade.COMMON) }));
        }

        [Test]
        public void CanMerge_ReturnsFalseForMixedGrades()
        {
            var items = new List<Equipment>
            {
                MakeEquipment(SlotType.WEAPON, EquipmentGrade.COMMON),
                MakeEquipment(SlotType.WEAPON, EquipmentGrade.RARE),
                MakeEquipment(SlotType.WEAPON, EquipmentGrade.COMMON),
            };
            Assert.IsFalse(_forge.CanMerge(items));
        }

        [Test]
        public void CanMerge_ReturnsFalseForMixedSlots()
        {
            var items = new List<Equipment>
            {
                MakeEquipment(SlotType.WEAPON, EquipmentGrade.COMMON),
                MakeEquipment(SlotType.ARMOR, EquipmentGrade.COMMON),
                MakeEquipment(SlotType.WEAPON, EquipmentGrade.COMMON),
            };
            Assert.IsFalse(_forge.CanMerge(items));
        }

        [Test]
        public void CanMerge_ReturnsFalseForInsufficientCommonCount()
        {
            Assert.IsFalse(_forge.CanMerge(MakeMany(2, SlotType.WEAPON, EquipmentGrade.COMMON)));
        }

        [Test]
        public void CanMerge_ReturnsTrueFor3CommonSameSlot()
        {
            Assert.IsTrue(_forge.CanMerge(MakeMany(3, SlotType.WEAPON, EquipmentGrade.COMMON)));
        }

        [Test]
        public void CanMerge_ReturnsTrueFor3UncommonSameSlot()
        {
            Assert.IsTrue(_forge.CanMerge(MakeMany(3, SlotType.RING, EquipmentGrade.UNCOMMON)));
        }

        [Test]
        public void CanMerge_ReturnsTrueFor3RareSameSlot()
        {
            Assert.IsTrue(_forge.CanMerge(MakeMany(3, SlotType.ARMOR, EquipmentGrade.RARE)));
        }

        [Test]
        public void CanMerge_ReturnsTrueFor2EpicSameSlotSameMergeLevel()
        {
            Assert.IsTrue(_forge.CanMerge(MakeMany(2, SlotType.WEAPON, EquipmentGrade.EPIC)));
        }

        [Test]
        public void CanMerge_ReturnsTrueFor2LegendarySameSlotSameMergeLevel()
        {
            Assert.IsTrue(_forge.CanMerge(MakeMany(2, SlotType.NECKLACE, EquipmentGrade.LEGENDARY)));
        }

        [Test]
        public void CanMerge_ReturnsFalseForEpicWithDifferentMergeLevels()
        {
            var items = new List<Equipment>
            {
                MakeEquipment(SlotType.WEAPON, EquipmentGrade.EPIC, mergeLevel: 0),
                MakeEquipment(SlotType.WEAPON, EquipmentGrade.EPIC, mergeLevel: 1),
            };
            Assert.IsFalse(_forge.CanMerge(items));
        }

        [Test]
        public void CanMerge_ReturnsFalseForMythic()
        {
            Assert.IsFalse(_forge.CanMerge(MakeMany(3, SlotType.WEAPON, EquipmentGrade.MYTHIC)));
        }

        [Test]
        public void CanMerge_ReturnsTrueWithExcessItems()
        {
            Assert.IsTrue(_forge.CanMerge(MakeMany(4, SlotType.WEAPON, EquipmentGrade.COMMON)));
        }

        [Test]
        public void Merge_CommonX3ToUncommon()
        {
            var items = MakeMany(3, SlotType.WEAPON, EquipmentGrade.COMMON);
            var result = _forge.Merge(items);
            Assert.IsTrue(result.IsOk());
            Assert.AreEqual(EquipmentGrade.UNCOMMON, result.Data.Result.Grade);
        }

        [Test]
        public void Merge_UncommonX3ToRare()
        {
            var items = MakeMany(3, SlotType.ARMOR, EquipmentGrade.UNCOMMON);
            var result = _forge.Merge(items);
            Assert.IsTrue(result.IsOk());
            Assert.AreEqual(EquipmentGrade.RARE, result.Data.Result.Grade);
        }

        [Test]
        public void Merge_RareX3ToEpic()
        {
            var items = MakeMany(3, SlotType.RING, EquipmentGrade.RARE);
            var result = _forge.Merge(items);
            Assert.IsTrue(result.IsOk());
            Assert.AreEqual(EquipmentGrade.EPIC, result.Data.Result.Grade);
            Assert.AreEqual(0, result.Data.Result.MergeLevel);
        }

        [Test]
        public void Merge_EpicPlus0X2ToEpicPlus1()
        {
            var items = MakeMany(2, SlotType.WEAPON, EquipmentGrade.EPIC, mergeLevel: 0);
            var result = _forge.Merge(items);
            Assert.IsTrue(result.IsOk());
            Assert.AreEqual(EquipmentGrade.EPIC, result.Data.Result.Grade);
            Assert.AreEqual(1, result.Data.Result.MergeLevel);
        }

        [Test]
        public void Merge_EpicPlus1X2ToEpicPlus2()
        {
            var items = MakeMany(2, SlotType.WEAPON, EquipmentGrade.EPIC, mergeLevel: 1);
            var result = _forge.Merge(items);
            Assert.IsTrue(result.IsOk());
            Assert.AreEqual(EquipmentGrade.EPIC, result.Data.Result.Grade);
            Assert.AreEqual(2, result.Data.Result.MergeLevel);
        }

        [Test]
        public void Merge_EpicPlus2X2ToLegendaryPlus0()
        {
            var items = MakeMany(2, SlotType.WEAPON, EquipmentGrade.EPIC, mergeLevel: 2);
            var result = _forge.Merge(items);
            Assert.IsTrue(result.IsOk());
            Assert.AreEqual(EquipmentGrade.LEGENDARY, result.Data.Result.Grade);
            Assert.AreEqual(0, result.Data.Result.MergeLevel);
        }

        [Test]
        public void Merge_LegendaryPlus0X2ToLegendaryPlus1()
        {
            var items = MakeMany(2, SlotType.WEAPON, EquipmentGrade.LEGENDARY, mergeLevel: 0);
            var result = _forge.Merge(items);
            Assert.IsTrue(result.IsOk());
            Assert.AreEqual(EquipmentGrade.LEGENDARY, result.Data.Result.Grade);
            Assert.AreEqual(1, result.Data.Result.MergeLevel);
        }

        [Test]
        public void Merge_LegendaryPlus1X2ToLegendaryPlus2()
        {
            var items = MakeMany(2, SlotType.WEAPON, EquipmentGrade.LEGENDARY, mergeLevel: 1);
            var result = _forge.Merge(items);
            Assert.IsTrue(result.IsOk());
            Assert.AreEqual(EquipmentGrade.LEGENDARY, result.Data.Result.Grade);
            Assert.AreEqual(2, result.Data.Result.MergeLevel);
        }

        [Test]
        public void Merge_LegendaryPlus2X2ToMythic()
        {
            var items = MakeMany(2, SlotType.WEAPON, EquipmentGrade.LEGENDARY, mergeLevel: 2);
            var result = _forge.Merge(items);
            Assert.IsTrue(result.IsOk());
            Assert.AreEqual(EquipmentGrade.MYTHIC, result.Data.Result.Grade);
            Assert.AreEqual(0, result.Data.Result.MergeLevel);
        }

        [Test]
        public void Merge_ResultPreservesSourceSlot()
        {
            var items = MakeMany(3, SlotType.RING, EquipmentGrade.COMMON);
            var result = _forge.Merge(items);
            Assert.AreEqual(SlotType.RING, result.Data.Result.Slot);
        }

        [Test]
        public void Merge_ResultPreservesSourceLevel()
        {
            var items = MakeMany(3, SlotType.WEAPON, EquipmentGrade.COMMON, level: 5);
            var result = _forge.Merge(items);
            Assert.AreEqual(5, result.Data.Result.Level);
        }

        [Test]
        public void Merge_ResultPreservesSourcePromoteCount()
        {
            var items = new List<Equipment>
            {
                MakeEquipment(SlotType.WEAPON, EquipmentGrade.COMMON, promoteCount: 2),
                MakeEquipment(SlotType.WEAPON, EquipmentGrade.COMMON),
                MakeEquipment(SlotType.WEAPON, EquipmentGrade.COMMON),
            };
            var result = _forge.Merge(items);
            Assert.AreEqual(2, result.Data.Result.PromoteCount);
        }

        [Test]
        public void Merge_ResultIsNotSGrade()
        {
            var items = MakeMany(3, SlotType.WEAPON, EquipmentGrade.COMMON);
            var result = _forge.Merge(items);
            Assert.IsFalse(result.Data.Result.IsS);
        }

        [Test]
        public void Merge_ResultHasUniqueId()
        {
            var items = MakeMany(3, SlotType.WEAPON, EquipmentGrade.COMMON);
            var result = _forge.Merge(items);
            Assert.IsTrue(result.Data.Result.Id.Contains("merged_"));
        }

        [Test]
        public void Merge_RejectsWhenFirstItemIsSGrade()
        {
            var items = new List<Equipment>
            {
                MakeEquipment(SlotType.WEAPON, EquipmentGrade.COMMON, isS: true),
                MakeEquipment(SlotType.WEAPON, EquipmentGrade.COMMON),
                MakeEquipment(SlotType.WEAPON, EquipmentGrade.COMMON),
            };
            Assert.IsTrue(_forge.Merge(items).IsFail());
        }

        [Test]
        public void Merge_RejectsWhenAnyItemIsSGrade()
        {
            var items = new List<Equipment>
            {
                MakeEquipment(SlotType.WEAPON, EquipmentGrade.COMMON),
                MakeEquipment(SlotType.WEAPON, EquipmentGrade.COMMON, isS: true),
                MakeEquipment(SlotType.WEAPON, EquipmentGrade.COMMON),
            };
            Assert.IsTrue(_forge.Merge(items).IsFail());
        }

        [Test]
        public void Merge_RejectsSGradeEpicMerge()
        {
            var items = new List<Equipment>
            {
                MakeEquipment(SlotType.WEAPON, EquipmentGrade.EPIC, isS: true),
                MakeEquipment(SlotType.WEAPON, EquipmentGrade.EPIC),
            };
            Assert.IsTrue(_forge.Merge(items).IsFail());
        }

        [Test]
        public void Merge_FailsForInsufficientItems()
        {
            var items = MakeMany(2, SlotType.WEAPON, EquipmentGrade.COMMON);
            Assert.IsTrue(_forge.Merge(items).IsFail());
        }

        [Test]
        public void Merge_FailsForMythicAlreadyMax()
        {
            var items = MakeMany(3, SlotType.WEAPON, EquipmentGrade.MYTHIC);
            Assert.IsTrue(_forge.Merge(items).IsFail());
        }

        [Test]
        public void Merge_FailsForMixedGrades()
        {
            var items = new List<Equipment>
            {
                MakeEquipment(SlotType.WEAPON, EquipmentGrade.COMMON),
                MakeEquipment(SlotType.WEAPON, EquipmentGrade.UNCOMMON),
                MakeEquipment(SlotType.WEAPON, EquipmentGrade.COMMON),
            };
            Assert.IsTrue(_forge.Merge(items).IsFail());
        }

        [Test]
        public void FindMergeCandidates_GroupsCommonBySlotAndGrade()
        {
            var inventory = MakeMany(3, SlotType.WEAPON, EquipmentGrade.COMMON);
            var candidates = _forge.FindMergeCandidates(inventory);
            Assert.AreEqual(1, candidates.Count);
            Assert.AreEqual(3, candidates[0].Count);
        }

        [Test]
        public void FindMergeCandidates_ReturnsEmptyForInsufficientItems()
        {
            var inventory = MakeMany(2, SlotType.WEAPON, EquipmentGrade.COMMON);
            var candidates = _forge.FindMergeCandidates(inventory);
            Assert.AreEqual(0, candidates.Count);
        }

        [Test]
        public void FindMergeCandidates_CreatesMultipleBatchesFromLargeGroup()
        {
            var inventory = MakeMany(6, SlotType.WEAPON, EquipmentGrade.COMMON);
            var candidates = _forge.FindMergeCandidates(inventory);
            Assert.AreEqual(2, candidates.Count);
            Assert.AreEqual(3, candidates[0].Count);
            Assert.AreEqual(3, candidates[1].Count);
        }

        [Test]
        public void FindMergeCandidates_LeftoverItemsNotIncluded()
        {
            var inventory = MakeMany(5, SlotType.WEAPON, EquipmentGrade.COMMON);
            var candidates = _forge.FindMergeCandidates(inventory);
            Assert.AreEqual(1, candidates.Count);
            Assert.AreEqual(3, candidates[0].Count);
        }

        [Test]
        public void FindMergeCandidates_SeparatesDifferentSlots()
        {
            var inventory = new List<Equipment>();
            inventory.AddRange(MakeMany(3, SlotType.WEAPON, EquipmentGrade.COMMON));
            inventory.AddRange(MakeMany(3, SlotType.ARMOR, EquipmentGrade.COMMON));
            var candidates = _forge.FindMergeCandidates(inventory);
            Assert.AreEqual(2, candidates.Count);
        }

        [Test]
        public void FindMergeCandidates_SeparatesDifferentGrades()
        {
            var inventory = new List<Equipment>();
            inventory.AddRange(MakeMany(3, SlotType.WEAPON, EquipmentGrade.COMMON));
            inventory.AddRange(MakeMany(3, SlotType.WEAPON, EquipmentGrade.UNCOMMON));
            var candidates = _forge.FindMergeCandidates(inventory);
            Assert.AreEqual(2, candidates.Count);
        }

        [Test]
        public void FindMergeCandidates_SkipsSGradeEquipment()
        {
            var inventory = new List<Equipment>
            {
                MakeEquipment(SlotType.WEAPON, EquipmentGrade.COMMON, isS: true),
            };
            inventory.AddRange(MakeMany(2, SlotType.WEAPON, EquipmentGrade.COMMON));
            var candidates = _forge.FindMergeCandidates(inventory);
            Assert.AreEqual(0, candidates.Count);
        }

        [Test]
        public void FindMergeCandidates_ExcludesMythic()
        {
            var inventory = MakeMany(3, SlotType.WEAPON, EquipmentGrade.MYTHIC);
            var candidates = _forge.FindMergeCandidates(inventory);
            Assert.AreEqual(0, candidates.Count);
        }

        [Test]
        public void FindMergeCandidates_EpicGroupsBySlotGradeAndMergeLevel()
        {
            var inventory = MakeMany(2, SlotType.WEAPON, EquipmentGrade.EPIC, mergeLevel: 0);
            var candidates = _forge.FindMergeCandidates(inventory);
            Assert.AreEqual(1, candidates.Count);
            Assert.AreEqual(2, candidates[0].Count);
        }

        [Test]
        public void FindMergeCandidates_EpicSeparatesDifferentMergeLevels()
        {
            var inventory = new List<Equipment>
            {
                MakeEquipment(SlotType.WEAPON, EquipmentGrade.EPIC, mergeLevel: 0),
                MakeEquipment(SlotType.WEAPON, EquipmentGrade.EPIC, mergeLevel: 1),
            };
            var candidates = _forge.FindMergeCandidates(inventory);
            Assert.AreEqual(0, candidates.Count);
        }

        [Test]
        public void FindMergeCandidates_LegendaryGroupsByMergeLevel()
        {
            var inventory = MakeMany(2, SlotType.WEAPON, EquipmentGrade.LEGENDARY, mergeLevel: 1);
            var candidates = _forge.FindMergeCandidates(inventory);
            Assert.AreEqual(1, candidates.Count);
            Assert.AreEqual(2, candidates[0].Count);
        }

        [Test]
        public void FindMergeCandidates_MixedScenario()
        {
            var inventory = new List<Equipment>();
            inventory.AddRange(MakeMany(3, SlotType.WEAPON, EquipmentGrade.COMMON));
            inventory.AddRange(MakeMany(2, SlotType.WEAPON, EquipmentGrade.COMMON));
            inventory.AddRange(MakeMany(3, SlotType.ARMOR, EquipmentGrade.RARE));
            inventory.AddRange(MakeMany(2, SlotType.RING, EquipmentGrade.EPIC));
            inventory.Add(MakeEquipment(SlotType.WEAPON, EquipmentGrade.MYTHIC));
            inventory.Add(MakeEquipment(SlotType.WEAPON, EquipmentGrade.COMMON, isS: true));
            var candidates = _forge.FindMergeCandidates(inventory);
            Assert.AreEqual(3, candidates.Count);
        }

        [Test]
        public void GetMergeRequirement_CommonRequires3()
        {
            Assert.AreEqual(3, _forge.GetMergeRequirement(EquipmentGrade.COMMON));
        }

        [Test]
        public void GetMergeRequirement_UncommonRequires3()
        {
            Assert.AreEqual(3, _forge.GetMergeRequirement(EquipmentGrade.UNCOMMON));
        }

        [Test]
        public void GetMergeRequirement_RareRequires3()
        {
            Assert.AreEqual(3, _forge.GetMergeRequirement(EquipmentGrade.RARE));
        }

        [Test]
        public void GetMergeRequirement_EpicRequires2()
        {
            Assert.AreEqual(2, _forge.GetMergeRequirement(EquipmentGrade.EPIC));
        }

        [Test]
        public void GetMergeRequirement_LegendaryRequires2()
        {
            Assert.AreEqual(2, _forge.GetMergeRequirement(EquipmentGrade.LEGENDARY));
        }

        [Test]
        public void GetMergeRequirement_MythicRequires0()
        {
            Assert.AreEqual(0, _forge.GetMergeRequirement(EquipmentGrade.MYTHIC));
        }

        [Test]
        public void FullSynthesisChain_CommonToMythic()
        {
            var commons = MakeMany(9, SlotType.WEAPON, EquipmentGrade.COMMON);

            var uncommons = new List<Equipment>();
            for (int i = 0; i < 9; i += 3)
            {
                var result = _forge.Merge(commons.GetRange(i, 3));
                Assert.IsTrue(result.IsOk());
                uncommons.Add(result.Data.Result);
            }
            Assert.AreEqual(3, uncommons.Count);
            Assert.IsTrue(uncommons.All(e => e.Grade == EquipmentGrade.UNCOMMON));

            var rareResult = _forge.Merge(uncommons);
            Assert.IsTrue(rareResult.IsOk());
            Assert.AreEqual(EquipmentGrade.RARE, rareResult.Data.Result.Grade);

            var rares = new List<Equipment>
            {
                rareResult.Data.Result,
                _forge.Merge(MakeMany(3, SlotType.WEAPON, EquipmentGrade.UNCOMMON)).Data.Result,
                _forge.Merge(MakeMany(3, SlotType.WEAPON, EquipmentGrade.UNCOMMON)).Data.Result,
            };
            var epicResult = _forge.Merge(rares);
            Assert.IsTrue(epicResult.IsOk());
            Assert.AreEqual(EquipmentGrade.EPIC, epicResult.Data.Result.Grade);
            Assert.AreEqual(0, epicResult.Data.Result.MergeLevel);

            var epic1Result = _forge.Merge(new List<Equipment>
            {
                epicResult.Data.Result,
                MakeEquipment(SlotType.WEAPON, EquipmentGrade.EPIC),
            });
            Assert.IsTrue(epic1Result.IsOk());
            Assert.AreEqual(EquipmentGrade.EPIC, epic1Result.Data.Result.Grade);
            Assert.AreEqual(1, epic1Result.Data.Result.MergeLevel);

            var epic2Result = _forge.Merge(new List<Equipment>
            {
                epic1Result.Data.Result,
                MakeEquipment(SlotType.WEAPON, EquipmentGrade.EPIC, mergeLevel: 1),
            });
            Assert.IsTrue(epic2Result.IsOk());
            Assert.AreEqual(EquipmentGrade.EPIC, epic2Result.Data.Result.Grade);
            Assert.AreEqual(2, epic2Result.Data.Result.MergeLevel);

            var legendaryResult = _forge.Merge(new List<Equipment>
            {
                epic2Result.Data.Result,
                MakeEquipment(SlotType.WEAPON, EquipmentGrade.EPIC, mergeLevel: 2),
            });
            Assert.IsTrue(legendaryResult.IsOk());
            Assert.AreEqual(EquipmentGrade.LEGENDARY, legendaryResult.Data.Result.Grade);
            Assert.AreEqual(0, legendaryResult.Data.Result.MergeLevel);

            var leg1Result = _forge.Merge(new List<Equipment>
            {
                legendaryResult.Data.Result,
                MakeEquipment(SlotType.WEAPON, EquipmentGrade.LEGENDARY),
            });
            Assert.IsTrue(leg1Result.IsOk());
            Assert.AreEqual(EquipmentGrade.LEGENDARY, leg1Result.Data.Result.Grade);
            Assert.AreEqual(1, leg1Result.Data.Result.MergeLevel);

            var leg2Result = _forge.Merge(new List<Equipment>
            {
                leg1Result.Data.Result,
                MakeEquipment(SlotType.WEAPON, EquipmentGrade.LEGENDARY, mergeLevel: 1),
            });
            Assert.IsTrue(leg2Result.IsOk());
            Assert.AreEqual(EquipmentGrade.LEGENDARY, leg2Result.Data.Result.Grade);
            Assert.AreEqual(2, leg2Result.Data.Result.MergeLevel);

            var mythicResult = _forge.Merge(new List<Equipment>
            {
                leg2Result.Data.Result,
                MakeEquipment(SlotType.WEAPON, EquipmentGrade.LEGENDARY, mergeLevel: 2),
            });
            Assert.IsTrue(mythicResult.IsOk());
            Assert.AreEqual(EquipmentGrade.MYTHIC, mythicResult.Data.Result.Grade);
            Assert.AreEqual(0, mythicResult.Data.Result.MergeLevel);
        }
    }
}
