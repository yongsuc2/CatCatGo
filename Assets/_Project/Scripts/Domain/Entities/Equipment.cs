using System.Collections.Generic;
using System.Linq;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Domain.Data;

namespace CatCatGo.Domain.Entities
{
    public struct UniqueEffect
    {
        public string Description;
        public Stats StatBonus;

        public UniqueEffect(string description, Stats statBonus)
        {
            Description = description;
            StatBonus = statBonus;
        }
    }

    public class Equipment
    {
        public string Id { get; }
        public string Name { get; }
        public SlotType Slot { get; }
        public EquipmentGrade Grade { get; set; }
        public bool IsS { get; }
        public int Level { get; set; }
        public int PromoteCount { get; set; }
        public UniqueEffect? UniqueEffectValue { get; }
        public WeaponSubType? WeaponSubTypeValue { get; }
        public int MergeLevel { get; set; }
        public List<SubStat> SubStats { get; }

        public Equipment(
            string id,
            string name,
            SlotType slot,
            EquipmentGrade grade,
            bool isS,
            int level = 0,
            int promoteCount = 0,
            UniqueEffect? uniqueEffect = null,
            WeaponSubType? weaponSubType = null,
            int mergeLevel = 0,
            List<SubStat> subStats = null)
        {
            Id = id;
            Name = name;
            Slot = slot;
            Grade = grade;
            IsS = isS;
            Level = level;
            PromoteCount = promoteCount;
            UniqueEffectValue = uniqueEffect;
            WeaponSubTypeValue = weaponSubType;
            MergeLevel = mergeLevel;
            SubStats = subStats ?? new List<SubStat>();
        }

        public Stats GetStats()
        {
            var baseStats = EquipmentTable.GetBaseStats(Slot, Grade);
            int flat = EquipmentTable.GetUpgradeFlatPerLevel();
            var bonus = Stats.Create(
                atk: baseStats.Atk > 0 ? Level * flat : 0,
                maxHp: baseStats.MaxHp > 0 ? Level * flat : 0
            );
            var stats = baseStats.Add(bonus);
            if (UniqueEffectValue.HasValue)
            {
                stats = stats.Add(UniqueEffectValue.Value.StatBonus);
            }
            foreach (var sub in SubStats)
            {
                stats = stats.Add(SubStatToStats(sub));
            }
            return stats;
        }

        private static Stats SubStatToStats(SubStat sub)
        {
            switch (sub.Stat)
            {
                case "ATK": return Stats.Create(atk: (int)sub.Value);
                case "DEF": return Stats.Create(def: (int)sub.Value);
                case "HP": return Stats.Create(hp: (int)sub.Value);
                case "MAXHP": return Stats.Create(maxHp: (int)sub.Value);
                case "CRIT": return Stats.Create(crit: sub.Value);
                default: return Stats.Zero;
            }
        }

        public int GetUpgradeCost()
        {
            return EquipmentTable.GetUpgradeCost(Level);
        }

        public Result<UpgradeResult> Upgrade(int availableStones)
        {
            int cost = GetUpgradeCost();
            if (availableStones < cost)
            {
                return Result.Fail<UpgradeResult>("Not enough equipment stones");
            }

            if (NeedsPromote())
            {
                return Result.Fail<UpgradeResult>("Promotion required before further upgrade");
            }

            Level += 1;
            return Result.Ok(new UpgradeResult { Cost = cost, NewLevel = Level });
        }

        public bool NeedsPromote()
        {
            return EquipmentTable.CanPromoteAtLevel(Level)
                && PromoteCount <= EquipmentTable.GetPromoteLevels().ToList().IndexOf(Level);
        }

        public bool CanPromote()
        {
            return EquipmentTable.CanPromoteAtLevel(Level);
        }

        public Result<PromoteResult> Promote(int availablePowerStones)
        {
            if (!CanPromote())
            {
                return Result.Fail<PromoteResult>("Cannot promote at current level");
            }

            int cost = 1;
            if (availablePowerStones < cost)
            {
                return Result.Fail<PromoteResult>("Not enough power stones");
            }

            PromoteCount += 1;
            return Result.Ok(new PromoteResult { Cost = cost });
        }

        public int GetTotalUpgradeCost()
        {
            return EquipmentTable.GetTotalUpgradeCost(Level);
        }

        public Result<DemoteResult> Demote()
        {
            if (Level == 0)
            {
                return Result.Fail<DemoteResult>("Already at level 0");
            }
            int refund = GetTotalUpgradeCost();
            Level = 0;
            PromoteCount = 0;
            return Result.Ok(new DemoteResult { Refund = refund });
        }

        public void TransferLevelTo(Equipment target)
        {
            target.Level = Level;
            target.PromoteCount = PromoteCount;
        }

        public int GetGradeIndex()
        {
            return EquipmentTable.GetGradeIndex(Grade);
        }

        public bool IsBetterThan(Equipment other)
        {
            if (GetGradeIndex() != other.GetGradeIndex())
            {
                return GetGradeIndex() > other.GetGradeIndex();
            }
            if (IsS != other.IsS)
            {
                return IsS;
            }
            return Level > other.Level;
        }
    }

    public struct UpgradeResult
    {
        public int Cost;
        public int NewLevel;
    }

    public struct PromoteResult
    {
        public int Cost;
    }

    public struct DemoteResult
    {
        public int Refund;
    }
}
