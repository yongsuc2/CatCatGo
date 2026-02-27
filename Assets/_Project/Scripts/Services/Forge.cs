using System;
using System.Collections.Generic;
using System.Linq;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.Data;
using CatCatGo.Infrastructure;

namespace CatCatGo.Services
{
    public class Forge
    {
        public bool CanMerge(List<Equipment> equipments)
        {
            if (equipments.Count < 2) return false;

            var firstGrade = equipments[0].Grade;
            var firstSlot = equipments[0].Slot;
            var firstSubType = equipments[0].WeaponSubTypeValue;

            if (!equipments.All(e => e.Grade == firstGrade && e.Slot == firstSlot && e.WeaponSubTypeValue == firstSubType))
                return false;

            if (EquipmentTable.IsHighGradeMerge(firstGrade))
            {
                int firstMergeLevel = equipments[0].MergeLevel;
                if (!equipments.All(e => e.MergeLevel == firstMergeLevel)) return false;
            }

            int required = EquipmentTable.GetMergeCount(firstGrade);
            if (required == 0) return false;

            return equipments.Count >= required;
        }

        public Result<ForgeResult> Merge(List<Equipment> equipments, SeededRandom rng = null)
        {
            if (!CanMerge(equipments))
                return Result.Fail<ForgeResult>("Cannot merge these equipments");

            var source = equipments[0];

            if (source.IsS || equipments.Any(e => e.IsS))
                return Result.Fail<ForgeResult>("S-grade equipment cannot be used as merge material");

            if (EquipmentTable.IsHighGradeMerge(source.Grade))
            {
                int maxEnhance = EquipmentTable.GetMergeEnhanceMax();
                if (source.MergeLevel < maxEnhance)
                {
                    var resultEquipment = new Equipment(
                        $"merged_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                        $"{source.Grade} {source.Slot}",
                        source.Slot,
                        source.Grade,
                        false,
                        source.Level,
                        source.PromoteCount,
                        null,
                        source.WeaponSubTypeValue,
                        source.MergeLevel + 1,
                        new List<SubStat>(source.SubStats));
                    return Result.Ok(new ForgeResult { Result = resultEquipment });
                }

                var nextGrade = EquipmentTable.GetNextGrade(source.Grade);
                if (!nextGrade.HasValue)
                    return Result.Fail<ForgeResult>("Already at max grade");

                var newSubStats = new List<SubStat>(source.SubStats);
                if (rng != null)
                    newSubStats.Add(EquipmentSubStatTable.RollSubStat(source.Slot, nextGrade.Value, rng));

                var mergedEquipment = new Equipment(
                    $"merged_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                    $"{nextGrade.Value} {source.Slot}",
                    source.Slot,
                    nextGrade.Value,
                    false,
                    source.Level,
                    source.PromoteCount,
                    null,
                    source.WeaponSubTypeValue,
                    0,
                    newSubStats);
                return Result.Ok(new ForgeResult { Result = mergedEquipment });
            }

            var nextGradeSimple = EquipmentTable.GetNextGrade(source.Grade);
            if (!nextGradeSimple.HasValue)
                return Result.Fail<ForgeResult>("Already at max grade");

            var newSubStatsSimple = new List<SubStat>(source.SubStats);
            if (rng != null)
                newSubStatsSimple.Add(EquipmentSubStatTable.RollSubStat(source.Slot, nextGradeSimple.Value, rng));

            var resultSimple = new Equipment(
                $"merged_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                $"{nextGradeSimple.Value} {source.Slot}",
                source.Slot,
                nextGradeSimple.Value,
                false,
                source.Level,
                source.PromoteCount,
                null,
                source.WeaponSubTypeValue,
                0,
                newSubStatsSimple);

            return Result.Ok(new ForgeResult { Result = resultSimple });
        }

        public int GetMergeRequirement(EquipmentGrade grade)
        {
            return EquipmentTable.GetMergeCount(grade);
        }

        public List<List<Equipment>> FindMergeCandidates(List<Equipment> inventory)
        {
            var groups = new Dictionary<string, List<Equipment>>();

            foreach (var eq in inventory)
            {
                if (eq.IsS) continue;
                string subKey = eq.Slot == SlotType.WEAPON ? $"_{eq.WeaponSubTypeValue}" : "";
                string mlKey = EquipmentTable.IsHighGradeMerge(eq.Grade) ? $"_ml{eq.MergeLevel}" : "";
                string key = $"{eq.Slot}{subKey}_{eq.Grade}{mlKey}";
                if (!groups.ContainsKey(key)) groups[key] = new List<Equipment>();
                groups[key].Add(eq);
            }

            var result = new List<List<Equipment>>();
            foreach (var group in groups.Values)
            {
                int required = EquipmentTable.GetMergeCount(group[0].Grade);
                if (required <= 0) continue;
                for (int i = 0; i + required <= group.Count; i += required)
                {
                    result.Add(group.GetRange(i, required));
                }
            }

            return result;
        }
    }

    public class ForgeResult
    {
        public Equipment Result;
    }
}
