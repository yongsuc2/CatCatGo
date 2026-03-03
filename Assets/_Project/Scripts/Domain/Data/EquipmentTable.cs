using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Infrastructure;

namespace CatCatGo.Domain.Data
{
    public static class EquipmentTable
    {
        private static List<(SlotType slot, EquipmentGrade grade, Stats stats)> _baseStats;
        private static Dictionary<EquipmentGrade, int> _mergeCount;
        private static EquipmentGrade[] _gradeOrder;
        private static int[] _promoteLevels;
        private static Dictionary<SlotType, int> _slotMaxCount;
        private static HashSet<EquipmentGrade> _highGradeMergeSet;
        private static int _upgradeFlatPerLevel;
        private static int _upgradeCostBase;
        private static int _mergeEnhanceMax;
        private static List<(int upToLevel, float rate)> _upgradeCostTiers;
        private static Dictionary<SlotType, float> _upgradePrioritySlotScores;
        private static float _upgradePrioritySGradeBonus;
        private static float _upgradePriorityLevelDecay;

        private static void EnsureLoaded()
        {
            if (_baseStats != null) return;

            var baseArr = JsonDataLoader.LoadJArray("equipment-base-stats.data.json");
            _baseStats = new List<(SlotType, EquipmentGrade, Stats)>();
            foreach (var row in baseArr)
            {
                var slot = (SlotType)Enum.Parse(typeof(SlotType), row["slot"].ToString());
                var grade = (EquipmentGrade)Enum.Parse(typeof(EquipmentGrade), row["grade"].ToString());
                var stats = Stats.Create(
                    atk: row["atk"]?.Value<int>() ?? 0,
                    maxHp: row["maxHp"]?.Value<int>() ?? 0,
                    def: row["def"]?.Value<int>() ?? 0,
                    crit: row["crit"]?.Value<float>() ?? 0f
                );
                _baseStats.Add((slot, grade, stats));
            }

            var constants = JsonDataLoader.LoadJObject("equipment-constants.data.json");
            _upgradeFlatPerLevel = constants["upgradeFlatPerLevel"].Value<int>();
            _upgradeCostBase = constants["upgradeCostBase"].Value<int>();
            _mergeEnhanceMax = constants["mergeEnhanceMax"].Value<int>();

            _mergeCount = new Dictionary<EquipmentGrade, int>();
            foreach (var kv in (JObject)constants["mergeCount"])
                _mergeCount[(EquipmentGrade)Enum.Parse(typeof(EquipmentGrade), kv.Key)] = kv.Value.Value<int>();

            _gradeOrder = constants["gradeOrder"].Select(t => (EquipmentGrade)Enum.Parse(typeof(EquipmentGrade), t.ToString())).ToArray();
            _promoteLevels = constants["promoteLevels"].Select(t => t.Value<int>()).ToArray();

            _slotMaxCount = new Dictionary<SlotType, int>();
            foreach (var kv in (JObject)constants["slotMaxCount"])
                _slotMaxCount[(SlotType)Enum.Parse(typeof(SlotType), kv.Key)] = kv.Value.Value<int>();

            _highGradeMergeSet = new HashSet<EquipmentGrade>();
            foreach (var g in constants["highGradeMergeGrades"])
                _highGradeMergeSet.Add((EquipmentGrade)Enum.Parse(typeof(EquipmentGrade), g.ToString()));

            _upgradeCostTiers = new List<(int, float)>();
            foreach (var t in constants["upgradeCostTiers"])
                _upgradeCostTiers.Add((t["upToLevel"].Value<int>(), t["rate"].Value<float>()));

            _upgradePrioritySlotScores = new Dictionary<SlotType, float>();
            var priority = constants["upgradePriority"];
            if (priority != null)
            {
                foreach (var kv in (JObject)priority["slotScores"])
                    _upgradePrioritySlotScores[(SlotType)Enum.Parse(typeof(SlotType), kv.Key)] = kv.Value.Value<float>();
                _upgradePrioritySGradeBonus = priority["sGradeBonus"].Value<float>();
                _upgradePriorityLevelDecay = priority["levelDecay"].Value<float>();
            }
        }

        public static Stats GetBaseStats(SlotType slot, EquipmentGrade grade)
        {
            EnsureLoaded();
            var entry = _baseStats.FirstOrDefault(e => e.slot == slot && e.grade == grade);
            return entry.stats;
        }

        public static int GetUpgradeFlatPerLevel()
        {
            EnsureLoaded();
            return _upgradeFlatPerLevel;
        }

        public static int GetMergeCount(EquipmentGrade grade)
        {
            EnsureLoaded();
            return _mergeCount.TryGetValue(grade, out var v) ? v : 0;
        }

        public static EquipmentGrade? GetNextGrade(EquipmentGrade grade)
        {
            EnsureLoaded();
            int idx = Array.IndexOf(_gradeOrder, grade);
            if (idx < 0 || idx >= _gradeOrder.Length - 1) return null;
            return _gradeOrder[idx + 1];
        }

        public static int GetGradeIndex(EquipmentGrade grade)
        {
            EnsureLoaded();
            return Array.IndexOf(_gradeOrder, grade);
        }

        public static int[] GetPromoteLevels()
        {
            EnsureLoaded();
            return _promoteLevels;
        }

        public static bool CanPromoteAtLevel(int level)
        {
            EnsureLoaded();
            return _promoteLevels.Contains(level);
        }

        public static int GetSlotMaxCount(SlotType slot)
        {
            EnsureLoaded();
            return _slotMaxCount.TryGetValue(slot, out var v) ? v : 1;
        }

        public static bool IsHighGradeMerge(EquipmentGrade grade)
        {
            EnsureLoaded();
            return _highGradeMergeSet.Contains(grade);
        }

        public static int GetMergeEnhanceMax()
        {
            EnsureLoaded();
            return _mergeEnhanceMax;
        }

        public static int GetUpgradeCost(int currentLevel)
        {
            EnsureLoaded();
            int cost = _upgradeCostBase;
            for (int lv = 1; lv <= currentLevel; lv++)
            {
                var tier = _upgradeCostTiers.FirstOrDefault(t => lv <= t.upToLevel);
                if (tier == default) tier = _upgradeCostTiers[_upgradeCostTiers.Count - 1];
                cost = Mathf.CeilToInt(cost * tier.rate);
            }
            return cost;
        }

        public static float GetUpgradePrioritySlotScore(SlotType slot)
        {
            EnsureLoaded();
            return _upgradePrioritySlotScores.TryGetValue(slot, out var v) ? v : 1f;
        }

        public static float GetUpgradePrioritySGradeBonus()
        {
            EnsureLoaded();
            return _upgradePrioritySGradeBonus;
        }

        public static float GetUpgradePriorityLevelDecay()
        {
            EnsureLoaded();
            return _upgradePriorityLevelDecay;
        }

        public static int GetTotalUpgradeCost(int level)
        {
            EnsureLoaded();
            if (level <= 0) return 0;
            int total = 0;
            int cost = _upgradeCostBase;
            total += cost;
            for (int lv = 1; lv < level; lv++)
            {
                var tier = _upgradeCostTiers.FirstOrDefault(t => lv <= t.upToLevel);
                if (tier == default) tier = _upgradeCostTiers[_upgradeCostTiers.Count - 1];
                cost = Mathf.CeilToInt(cost * tier.rate);
                total += cost;
            }
            return total;
        }
    }
}
