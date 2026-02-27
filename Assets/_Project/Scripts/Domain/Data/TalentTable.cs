using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Infrastructure;
using UnityEngine;

namespace CatCatGo.Domain.Data
{
    public struct SubGradeRange
    {
        public TalentGrade Grade;
        public int Tier;
        public int Levels;
        public int Cost;
        public string BonusStat;
        public int BonusAmount;
        public bool HasBonus;
        public int StartLevel;
        public int EndLevel;
    }

    public struct SubGradeInfo
    {
        public TalentGrade Grade;
        public int Tier;
        public int TierCount;
        public int LevelInTier;
        public int TierLevels;
        public int Cost;
    }

    public struct TransitionInfo
    {
        public int Level;
        public TalentGrade Grade;
        public int Tier;
        public bool IsMainGrade;
        public string BonusStat;
        public int BonusAmount;
    }

    public struct TalentMilestone
    {
        public int Level;
        public string RewardType;
        public int RewardAmount;
    }

    public static class TalentTable
    {
        private static bool _loaded;
        private static Dictionary<StatType, int> _statPerLevel;
        private static TalentGrade[] _gradeOrder;
        private static int _levelsPerTier;
        private static int _levelsPerStat;
        private static List<GradeConfig> _gradeConfigs;
        private static List<MainGradeBonus> _mainGradeBonuses;
        private static int _milestoneInterval;
        private static int _goldCostMultiplier;
        private static int _goldBoostPercent;

        private static List<SubGradeRange> _subGradeRanges;
        private static int _totalMaxLevel;
        private static List<GradeThreshold> _gradeThresholds;
        private static List<TalentMilestone> _milestones;
        private static List<TransitionInfo> _mainTransitions;
        private static List<TransitionInfo> _allTransitions;

        private static readonly Dictionary<TalentGrade, string> GradeLabels = new Dictionary<TalentGrade, string>
        {
            { TalentGrade.DISCIPLE, "\uc218\ub828\uc0dd" },
            { TalentGrade.ADVENTURER, "\ubaa8\ud5d8\uac00" },
            { TalentGrade.ELITE, "\uc815\uc608" },
            { TalentGrade.MASTER, "\ub2ec\uc778" },
            { TalentGrade.WARRIOR, "\uc804\uc0ac" },
            { TalentGrade.HERO, "\uc601\uc6c5" },
        };

        private struct GradeConfig
        {
            public TalentGrade Grade;
            public int Tiers;
            public float BaseCost;
            public float CostGrowth;
            public int TierBonusBase;
            public int TierBonusGrowth;
        }

        private struct MainGradeBonus
        {
            public TalentGrade Grade;
            public string Stat;
            public int Amount;
        }

        private struct GradeThreshold
        {
            public TalentGrade Grade;
            public int TotalLevel;
        }

        private static void EnsureLoaded()
        {
            if (_loaded) return;
            _loaded = true;

            var data = JsonDataLoader.LoadJObject("talent.data.json");

            _statPerLevel = new Dictionary<StatType, int>();
            foreach (var kv in (JObject)data["statPerLevel"])
            {
                var st = (StatType)Enum.Parse(typeof(StatType), kv.Key);
                _statPerLevel[st] = kv.Value.Value<int>();
            }

            var gradeOrderArr = (JArray)data["gradeOrder"];
            _gradeOrder = gradeOrderArr.Select(t => (TalentGrade)Enum.Parse(typeof(TalentGrade), t.ToString())).ToArray();

            _levelsPerTier = data["levelsPerTier"].Value<int>();
            _levelsPerStat = data["levelsPerStat"].Value<int>();

            _gradeConfigs = new List<GradeConfig>();
            foreach (var gc in (JArray)data["gradeConfig"])
            {
                _gradeConfigs.Add(new GradeConfig
                {
                    Grade = (TalentGrade)Enum.Parse(typeof(TalentGrade), gc["grade"].ToString()),
                    Tiers = gc["tiers"].Value<int>(),
                    BaseCost = gc["baseCost"].Value<float>(),
                    CostGrowth = gc["costGrowth"].Value<float>(),
                    TierBonusBase = gc["tierBonusBase"].Value<int>(),
                    TierBonusGrowth = gc["tierBonusGrowth"].Value<int>(),
                });
            }

            _mainGradeBonuses = new List<MainGradeBonus>();
            foreach (var mb in (JArray)data["mainGradeBonuses"])
            {
                _mainGradeBonuses.Add(new MainGradeBonus
                {
                    Grade = (TalentGrade)Enum.Parse(typeof(TalentGrade), mb["grade"].ToString()),
                    Stat = mb["stat"].ToString(),
                    Amount = mb["amount"].Value<int>(),
                });
            }

            var mc = (JObject)data["milestoneConfig"];
            _milestoneInterval = mc["interval"].Value<int>();
            _goldCostMultiplier = mc["goldCostMultiplier"].Value<int>();
            _goldBoostPercent = mc["goldBoostPercent"].Value<int>();

            _subGradeRanges = BuildSubGradeRanges();
            _totalMaxLevel = _subGradeRanges[_subGradeRanges.Count - 1].EndLevel;
            _gradeThresholds = BuildGradeThresholds();
            _milestones = GenerateMilestones();
            _mainTransitions = BuildMainTransitions();
            _allTransitions = BuildAllTransitions();
        }

        private static List<SubGradeRange> BuildSubGradeRanges()
        {
            var ranges = new List<SubGradeRange>();
            int cumLevel = 0;
            int bonusIdx = 0;

            foreach (var gc in _gradeConfigs)
            {
                var mainBonus = _mainGradeBonuses.FirstOrDefault(b => b.Grade == gc.Grade);
                bool hasMainBonus = _mainGradeBonuses.Any(b => b.Grade == gc.Grade);
                int subTierIdx = 0;

                for (int tier = 1; tier <= gc.Tiers; tier++)
                {
                    int cost = Mathf.FloorToInt(gc.BaseCost * Mathf.Pow(gc.CostGrowth, tier - 1));
                    bool hasBonus = false;
                    string bonusStat = "";
                    int bonusAmount = 0;

                    if (cumLevel > 0)
                    {
                        if (tier == 1 && hasMainBonus)
                        {
                            hasBonus = true;
                            bonusStat = mainBonus.Stat;
                            bonusAmount = mainBonus.Amount;
                        }
                        else
                        {
                            hasBonus = true;
                            bonusStat = bonusIdx % 2 == 0 ? "DEF" : "ATK";
                            bonusAmount = gc.TierBonusBase + subTierIdx * gc.TierBonusGrowth;
                            subTierIdx++;
                        }
                        bonusIdx++;
                    }

                    ranges.Add(new SubGradeRange
                    {
                        Grade = gc.Grade,
                        Tier = tier,
                        Levels = _levelsPerTier,
                        Cost = cost,
                        HasBonus = hasBonus,
                        BonusStat = bonusStat,
                        BonusAmount = bonusAmount,
                        StartLevel = cumLevel,
                        EndLevel = cumLevel + _levelsPerTier,
                    });
                    cumLevel += _levelsPerTier;
                }
            }
            return ranges;
        }

        private static List<GradeThreshold> BuildGradeThresholds()
        {
            var seen = new HashSet<TalentGrade>();
            var thresholds = new List<GradeThreshold>();
            foreach (var r in _subGradeRanges)
            {
                if (!seen.Contains(r.Grade))
                {
                    seen.Add(r.Grade);
                    thresholds.Add(new GradeThreshold { Grade = r.Grade, TotalLevel = r.StartLevel });
                }
            }
            var lastGrade = _gradeOrder[_gradeOrder.Length - 1];
            if (!seen.Contains(lastGrade))
            {
                thresholds.Add(new GradeThreshold { Grade = lastGrade, TotalLevel = _totalMaxLevel });
            }
            return thresholds;
        }

        private static int GetUpgradeCostAtLevel(int totalLevel)
        {
            foreach (var r in _subGradeRanges)
            {
                if (totalLevel < r.EndLevel) return r.Cost;
            }
            return _subGradeRanges[_subGradeRanges.Count - 1].Cost;
        }

        private static List<TalentMilestone> GenerateMilestones()
        {
            var result = new List<TalentMilestone>();
            var boundaries = new HashSet<int>(_subGradeRanges.Select(r => r.EndLevel));

            var levels = new List<int>();
            for (int lv = _milestoneInterval; lv <= _totalMaxLevel; lv += _milestoneInterval)
            {
                if (!boundaries.Contains(lv)) levels.Add(lv);
            }

            foreach (int level in levels)
            {
                if (result.Count % 2 == 0)
                {
                    result.Add(new TalentMilestone
                    {
                        Level = level,
                        RewardType = ResourceType.GOLD.ToString(),
                        RewardAmount = GetUpgradeCostAtLevel(level) * _goldCostMultiplier,
                    });
                }
                else
                {
                    result.Add(new TalentMilestone
                    {
                        Level = level,
                        RewardType = "GOLD_BOOST",
                        RewardAmount = _goldBoostPercent,
                    });
                }
            }
            return result;
        }

        private static List<TransitionInfo> BuildMainTransitions()
        {
            var transitions = new List<TransitionInfo>();
            foreach (var mb in _mainGradeBonuses)
            {
                var threshold = _gradeThresholds.FirstOrDefault(t => t.Grade == mb.Grade);
                if (threshold.Grade != mb.Grade) continue;
                transitions.Add(new TransitionInfo
                {
                    Level = threshold.TotalLevel,
                    Grade = mb.Grade,
                    Tier = 1,
                    IsMainGrade = true,
                    BonusStat = mb.Stat,
                    BonusAmount = mb.Amount,
                });
            }
            return transitions;
        }

        private static List<TransitionInfo> BuildAllTransitions()
        {
            var mainLevels = new HashSet<int>(_mainTransitions.Select(t => t.Level));
            var transitions = new List<TransitionInfo>();
            foreach (var r in _subGradeRanges)
            {
                if (r.StartLevel == 0 || !r.HasBonus) continue;
                transitions.Add(new TransitionInfo
                {
                    Level = r.StartLevel,
                    Grade = r.Grade,
                    Tier = r.Tier,
                    IsMainGrade = mainLevels.Contains(r.StartLevel),
                    BonusStat = r.BonusStat,
                    BonusAmount = r.BonusAmount,
                });
            }
            return transitions;
        }

        public static int GetStatPerLevel(StatType statType)
        {
            EnsureLoaded();
            return _statPerLevel.TryGetValue(statType, out var val) ? val : 0;
        }

        public static int GetUpgradeCost(int totalLevel)
        {
            EnsureLoaded();
            foreach (var r in _subGradeRanges)
            {
                if (totalLevel < r.EndLevel) return r.Cost;
            }
            return _subGradeRanges[_subGradeRanges.Count - 1].Cost;
        }

        public static SubGradeInfo GetSubGradeInfo(int totalLevel)
        {
            EnsureLoaded();
            foreach (var r in _subGradeRanges)
            {
                if (totalLevel < r.EndLevel)
                {
                    var gc = _gradeConfigs.First(g => g.Grade == r.Grade);
                    return new SubGradeInfo
                    {
                        Grade = r.Grade,
                        Tier = r.Tier,
                        TierCount = gc.Tiers,
                        LevelInTier = totalLevel - r.StartLevel,
                        TierLevels = r.Levels,
                        Cost = r.Cost,
                    };
                }
            }
            var lastGrade = _gradeOrder[_gradeOrder.Length - 1];
            return new SubGradeInfo
            {
                Grade = lastGrade,
                Tier = 1,
                TierCount = 1,
                LevelInTier = totalLevel - _totalMaxLevel,
                TierLevels = 0,
                Cost = _subGradeRanges[_subGradeRanges.Count - 1].Cost,
            };
        }

        public static int GetMaxLevel()
        {
            EnsureLoaded();
            return _totalMaxLevel;
        }

        public static TalentGrade GetGradeForTotalLevel(int totalLevel)
        {
            EnsureLoaded();
            var result = _gradeOrder[0];
            foreach (var threshold in _gradeThresholds)
            {
                if (totalLevel >= threshold.TotalLevel)
                    result = threshold.Grade;
            }
            return result;
        }

        public static TalentGrade[] GetGradeOrder()
        {
            EnsureLoaded();
            return _gradeOrder;
        }

        public static int GetGradeIndex(TalentGrade grade)
        {
            EnsureLoaded();
            return Array.IndexOf(_gradeOrder, grade);
        }

        public static int? GetNextGradeThreshold(TalentGrade currentGrade)
        {
            EnsureLoaded();
            int idx = Array.IndexOf(_gradeOrder, currentGrade);
            if (idx >= _gradeOrder.Length - 1) return null;
            var nextGrade = _gradeOrder[idx + 1];
            var threshold = _gradeThresholds.FirstOrDefault(t => t.Grade == nextGrade);
            if (threshold.Grade != nextGrade) return null;
            return threshold.TotalLevel;
        }

        public static int GetGradeStartLevel(TalentGrade grade)
        {
            EnsureLoaded();
            var threshold = _gradeThresholds.FirstOrDefault(t => t.Grade == grade);
            if (threshold.Grade != grade) return 0;
            return threshold.TotalLevel;
        }

        public static Stats GetStatBonus(int totalLevel)
        {
            EnsureLoaded();
            int atk = 0;
            int def = 0;

            foreach (var r in _subGradeRanges)
            {
                if (totalLevel < r.StartLevel) break;
                if (!r.HasBonus) continue;
                if (r.BonusStat == "ATK") atk += r.BonusAmount;
                else if (r.BonusStat == "DEF") def += r.BonusAmount;
            }

            var heroBonus = _mainGradeBonuses.FirstOrDefault(b => b.Grade == _gradeOrder[_gradeOrder.Length - 1]);
            bool hasHeroBonus = _mainGradeBonuses.Any(b => b.Grade == _gradeOrder[_gradeOrder.Length - 1]);
            if (hasHeroBonus && totalLevel >= _totalMaxLevel)
            {
                if (heroBonus.Stat == "ATK") atk += heroBonus.Amount;
                else if (heroBonus.Stat == "DEF") def += heroBonus.Amount;
            }

            return Stats.Create(atk: atk, def: def);
        }

        public static List<TransitionInfo> GetAllTransitions()
        {
            EnsureLoaded();
            return _allTransitions;
        }

        public static List<TransitionInfo> GetMainTransitions()
        {
            EnsureLoaded();
            return _mainTransitions;
        }

        public static List<TalentMilestone> GetMilestonesInRange(int fromLevel, int toLevel)
        {
            EnsureLoaded();
            return _milestones.Where(m => m.Level > fromLevel && m.Level < toLevel).ToList();
        }

        public static List<TalentMilestone> GetAllMilestones()
        {
            EnsureLoaded();
            return _milestones;
        }

        public static int GetLevelsPerStat()
        {
            EnsureLoaded();
            return _levelsPerStat;
        }

        public static int GetLevelsPerTier()
        {
            EnsureLoaded();
            return _levelsPerTier;
        }

        public static string GetGradeLabel(TalentGrade grade)
        {
            EnsureLoaded();
            return GradeLabels.TryGetValue(grade, out var label) ? label : grade.ToString();
        }

        public static string GetSubGradeLabel(int totalLevel)
        {
            var info = GetSubGradeInfo(totalLevel);
            string gradeLabel = GetGradeLabel(info.Grade);
            return $"{gradeLabel} {info.Tier}\ub2e8";
        }
    }
}
