using System;
using System.Collections.Generic;
using System.Linq;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Domain.Data;
using CatCatGo.Infrastructure;

namespace CatCatGo.Domain.Economy
{
    public class PullResult
    {
        public Equipment Equipment;
        public List<ResourceReward> Resources;
        public bool IsPity;
    }

    public class TreasureChest
    {
        private static readonly SlotType[] Slots =
        {
            SlotType.WEAPON, SlotType.ARMOR, SlotType.RING,
            SlotType.NECKLACE, SlotType.SHOES, SlotType.GLOVES, SlotType.HAT,
        };

        private static readonly WeaponSubType[] WeaponSubTypes =
        {
            WeaponSubType.SWORD, WeaponSubType.STAFF, WeaponSubType.BOW,
        };

        public ChestType Type;
        public int PityCount;

        public TreasureChest(ChestType type)
        {
            Type = type;
            PityCount = 0;
        }

        public int GetCostPerPull()
        {
            switch (Type)
            {
                case ChestType.EQUIPMENT: return GachaDataTable.Equipment.CostPerPull;
                case ChestType.PET: return GachaDataTable.Pet.CostPerPull;
                default: return 0;
            }
        }

        public int GetPull10Cost()
        {
            return GetCostPerPull() * 9;
        }

        public int GetPityThreshold()
        {
            switch (Type)
            {
                case ChestType.EQUIPMENT: return GachaDataTable.Equipment.PityThreshold;
                case ChestType.PET: return GachaDataTable.Pet.PityThreshold;
                default: return 0;
            }
        }

        public PullResult Pull(SeededRandom rng)
        {
            if (Type == ChestType.PET)
                return PullSpecial(rng);

            PityCount += 1;

            int pityThreshold = GetPityThreshold();
            if (pityThreshold > 0 && PityCount >= pityThreshold)
            {
                PityCount = 0;
                return CreatePityResult(rng);
            }

            var config = GachaDataTable.Equipment;
            var entries = config.GradeWeights.Select(w => (item: w.Grade, weight: w.Weight)).ToList();
            var grade = rng.WeightedPick(entries);

            var slot = rng.Pick(Slots);
            bool isS = GachaDataTable.SEligibleGrades.Contains(grade) && rng.Chance(GachaDataTable.SRate);
            WeaponSubType? subType = slot == SlotType.WEAPON ? (WeaponSubType?)rng.Pick(WeaponSubTypes) : null;
            string name = slot == SlotType.WEAPON && subType.HasValue
                ? $"{EquipmentDataTable.GetGradeLabel(grade)} {EquipmentDataTable.GetWeaponSubTypeLabel(subType.Value)}"
                : $"{EquipmentDataTable.GetGradeLabel(grade)} {EquipmentDataTable.GetSlotLabel(slot)}";

            var subStats = EquipmentSubStatTable.RollNewSubStats(slot, grade, rng);
            var equipment = new Equipment(
                $"chest_{DateTime.UtcNow.Ticks}_{rng.NextInt(0, 9999)}",
                name,
                slot,
                grade,
                isS,
                0, 0, null,
                subType,
                0,
                subStats);

            return new PullResult { Equipment = equipment, Resources = new List<ResourceReward>(), IsPity = false };
        }

        public List<PullResult> Pull10(SeededRandom rng)
        {
            var results = new List<PullResult>();
            for (int i = 0; i < 10; i++)
                results.Add(Pull(rng));
            return results;
        }

        private PullResult CreatePityResult(SeededRandom rng)
        {
            var slot = rng.Pick(Slots);
            WeaponSubType? subType = slot == SlotType.WEAPON ? (WeaponSubType?)rng.Pick(WeaponSubTypes) : null;
            string slotLabel = slot == SlotType.WEAPON && subType.HasValue
                ? EquipmentDataTable.GetWeaponSubTypeLabel(subType.Value)
                : EquipmentDataTable.GetSlotLabel(slot);
            var subStats = EquipmentSubStatTable.RollNewSubStats(slot, EquipmentGrade.MYTHIC, rng);
            var equipment = new Equipment(
                $"pity_{DateTime.UtcNow.Ticks}",
                $"\uc2e0\ud654 {slotLabel}",
                slot,
                EquipmentGrade.MYTHIC,
                false,
                0, 0, null,
                subType,
                0,
                subStats);
            return new PullResult { Equipment = equipment, Resources = new List<ResourceReward>(), IsPity = true };
        }

        private PullResult PullSpecial(SeededRandom rng)
        {
            return new PullResult
            {
                Equipment = null,
                Resources = new List<ResourceReward>
                {
                    new ResourceReward(ResourceType.PET_EGG, GachaDataTable.Pet.EggAmount),
                    new ResourceReward(ResourceType.PET_FOOD, rng.NextInt(GachaDataTable.Pet.FoodMin, GachaDataTable.Pet.FoodMax)),
                },
                IsPity = false,
            };
        }

        public float GetPityProgress()
        {
            int threshold = GetPityThreshold();
            if (threshold == 0) return 0;
            return (float)PityCount / threshold;
        }

        public int GetRemainingToPity()
        {
            int threshold = GetPityThreshold();
            if (threshold == 0) return -1;
            return threshold - PityCount;
        }
    }
}
