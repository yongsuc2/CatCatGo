using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using CatCatGo.Domain.Enums;
using CatCatGo.Infrastructure;

namespace CatCatGo.Domain.Data
{
    public class EncounterTypeInfo
    {
        public string Label;
        public string Description;
    }

    public class EncounterWeight
    {
        public EncounterType Type;
        public int Weight;
    }

    public class DemonConfig
    {
        public float HpCostPercent;
        public string RejectLabel;
        public string RejectDescription;

        public string SkillLabel(string name) => name;
        public string SkillDescription(string desc) => $"\uccb4\ub825 20% \uc18c\ubaa8 | {desc}";
    }

    public class ChanceConfig
    {
        public float SpringHealPercent;
        public string SpringLabel;
        public string SpringDescription;
        public int BlessingGoldBase;
        public int BlessingGoldPerChapter;
        public string BlessingLabel;
        public string BlessingDescription;
        public Dictionary<string, int> SubWeights;
    }

    public class JungbakRouletteConfig
    {
        public float HealPercent;
        public string HealLabel;
        public string HealDescription;
        public int GoldAmount;
        public string GoldLabel;
        public string GoldDescription;

        public string SkillLabel(string name) => name;
        public string SkillDescription(string desc) => desc;
    }

    public class DaebakRouletteConfig
    {
        public float NormalRate;
        public float AngelRate;
        public float DemonRate;
        public string NormalLabel;
        public string NormalDescription;
        public string AngelLabel;
        public string AngelDescription;
        public string DemonLabel;
        public string DemonDescription;
        public string SkipLabel;
        public string SkipDescription;
    }

    public class SkillSwapConfig
    {
        public string SkipLabel;
        public string SkipDescription;
    }

    public class CounterThresholdConfig
    {
        public int Jungbak;
        public int Daebak;
    }

    public class ForcedBattleDaysConfig
    {
        public int Elite;
        public int MidBoss;
    }

    public class OptionalEliteDay
    {
        public int Day;
        public float Chance;
    }

    public class ChapterClearRewardConfig
    {
        public int GoldBase;
        public int GoldPerChapter;
        public int GemsBase;
        public int GemsPerChapter;
    }

    public static class EncounterDataTable
    {
        private static Dictionary<EncounterType, EncounterTypeInfo> _typeInfo;
        private static Dictionary<ChapterType, List<EncounterWeight>> _weights;
        private static DemonConfig _demon;
        private static ChanceConfig _chance;
        private static JungbakRouletteConfig _jungbakRoulette;
        private static DaebakRouletteConfig _daebakRoulette;
        private static int _rerollsPerSession;
        private static SkillSwapConfig _skillSwap;
        private static CounterThresholdConfig _counterThreshold;
        private static ForcedBattleDaysConfig _forcedBattleDays;
        private static List<OptionalEliteDay> _optionalEliteDays;
        private static ChapterClearRewardConfig _chapterClearReward;

        private static void EnsureLoaded()
        {
            if (_typeInfo != null) return;

            var data = JsonDataLoader.LoadJObject("encounter.data.json");
            if (data == null) return;

            _typeInfo = new Dictionary<EncounterType, EncounterTypeInfo>();
            foreach (var kv in (JObject)data["typeInfo"])
            {
                var et = (EncounterType)Enum.Parse(typeof(EncounterType), kv.Key);
                _typeInfo[et] = new EncounterTypeInfo
                {
                    Label = kv.Value["label"].ToString(),
                    Description = kv.Value["description"].ToString(),
                };
            }

            _weights = new Dictionary<ChapterType, List<EncounterWeight>>();
            foreach (var kv in (JObject)data["weights"])
            {
                var ct = (ChapterType)Enum.Parse(typeof(ChapterType), kv.Key);
                var list = new List<EncounterWeight>();
                foreach (var w in (JArray)kv.Value)
                {
                    list.Add(new EncounterWeight
                    {
                        Type = (EncounterType)Enum.Parse(typeof(EncounterType), w["type"].ToString()),
                        Weight = w["weight"].Value<int>(),
                    });
                }
                _weights[ct] = list;
            }

            var dm = data["demon"];
            _demon = new DemonConfig
            {
                HpCostPercent = dm["hpCostPercent"].Value<float>(),
                RejectLabel = dm["rejectLabel"].ToString(),
                RejectDescription = dm["rejectDescription"].ToString(),
            };

            var ch = data["chance"];
            _chance = new ChanceConfig
            {
                SpringHealPercent = ch["springHealPercent"].Value<float>(),
                SpringLabel = ch["springLabel"].ToString(),
                SpringDescription = ch["springDescription"].ToString(),
                BlessingGoldBase = ch["blessingGoldBase"].Value<int>(),
                BlessingGoldPerChapter = ch["blessingGoldPerChapter"].Value<int>(),
                BlessingLabel = ch["blessingLabel"].ToString(),
                BlessingDescription = ch["blessingDescription"].ToString(),
                SubWeights = new Dictionary<string, int>(),
            };
            foreach (var kv in (JObject)ch["subWeights"])
                _chance.SubWeights[kv.Key] = kv.Value.Value<int>();

            var jr = data["jungbakRoulette"];
            _jungbakRoulette = new JungbakRouletteConfig
            {
                HealPercent = jr["healPercent"].Value<float>(),
                HealLabel = jr["healLabel"].ToString(),
                HealDescription = jr["healDescription"].ToString(),
                GoldAmount = jr["goldAmount"].Value<int>(),
                GoldLabel = jr["goldLabel"].ToString(),
                GoldDescription = jr["goldDescription"].ToString(),
            };

            var dr = data["daebakRoulette"];
            _daebakRoulette = new DaebakRouletteConfig
            {
                NormalRate = dr["normalRate"].Value<float>(),
                AngelRate = dr["angelRate"].Value<float>(),
                DemonRate = dr["demonRate"].Value<float>(),
                NormalLabel = dr["normalLabel"].ToString(),
                NormalDescription = dr["normalDescription"].ToString(),
                AngelLabel = dr["angelLabel"].ToString(),
                AngelDescription = dr["angelDescription"].ToString(),
                DemonLabel = dr["demonLabel"].ToString(),
                DemonDescription = dr["demonDescription"].ToString(),
                SkipLabel = dr["skipLabel"].ToString(),
                SkipDescription = dr["skipDescription"].ToString(),
            };

            _rerollsPerSession = data["rerollsPerSession"].Value<int>();

            var sw = data["skillSwap"];
            _skillSwap = new SkillSwapConfig
            {
                SkipLabel = sw["skipLabel"].ToString(),
                SkipDescription = sw["skipDescription"].ToString(),
            };

            var ct2 = data["counterThreshold"];
            _counterThreshold = new CounterThresholdConfig
            {
                Jungbak = ct2["jungbak"].Value<int>(),
                Daebak = ct2["daebak"].Value<int>(),
            };

            var fb = data["forcedBattleDays"];
            _forcedBattleDays = new ForcedBattleDaysConfig
            {
                Elite = fb["elite"].Value<int>(),
                MidBoss = fb["midBoss"].Value<int>(),
            };

            _optionalEliteDays = new List<OptionalEliteDay>();
            foreach (var d in data["optionalEliteDays"])
            {
                _optionalEliteDays.Add(new OptionalEliteDay
                {
                    Day = d["day"].Value<int>(),
                    Chance = d["chance"].Value<float>(),
                });
            }

            var ccr = data["chapterClearReward"];
            _chapterClearReward = new ChapterClearRewardConfig
            {
                GoldBase = ccr["goldBase"].Value<int>(),
                GoldPerChapter = ccr["goldPerChapter"].Value<int>(),
                GemsBase = ccr["gemsBase"].Value<int>(),
                GemsPerChapter = ccr["gemsPerChapter"].Value<int>(),
            };

        }

        public static EncounterTypeInfo GetTypeInfo(EncounterType type)
        {
            EnsureLoaded();
            return _typeInfo[type];
        }

        public static string GetLabel(EncounterType type)
        {
            EnsureLoaded();
            return _typeInfo[type].Label;
        }

        public static string GetDescription(EncounterType type)
        {
            EnsureLoaded();
            return _typeInfo[type].Description;
        }

        public static List<EncounterWeight> GetWeights(ChapterType chapterType)
        {
            EnsureLoaded();
            return _weights[chapterType];
        }

        public static DemonConfig Demon { get { EnsureLoaded(); return _demon; } }
        public static ChanceConfig Chance { get { EnsureLoaded(); return _chance; } }
        public static JungbakRouletteConfig JungbakRoulette { get { EnsureLoaded(); return _jungbakRoulette; } }
        public static DaebakRouletteConfig DaebakRoulette { get { EnsureLoaded(); return _daebakRoulette; } }
        public static int RerollsPerSession { get { EnsureLoaded(); return _rerollsPerSession; } }
        public static SkillSwapConfig SkillSwap { get { EnsureLoaded(); return _skillSwap; } }
        public static CounterThresholdConfig CounterThreshold { get { EnsureLoaded(); return _counterThreshold; } }
        public static ForcedBattleDaysConfig ForcedBattleDays { get { EnsureLoaded(); return _forcedBattleDays; } }
        public static List<OptionalEliteDay> OptionalEliteDays { get { EnsureLoaded(); return _optionalEliteDays; } }
        public static ChapterClearRewardConfig ChapterClearReward { get { EnsureLoaded(); return _chapterClearReward; } }

        public static int GetChapterClearGold(int chapterId)
        {
            EnsureLoaded();
            return _chapterClearReward.GoldBase + _chapterClearReward.GoldPerChapter * chapterId;
        }

        public static int GetChapterClearGems(int chapterId)
        {
            EnsureLoaded();
            return _chapterClearReward.GemsBase + _chapterClearReward.GemsPerChapter * chapterId;
        }

    }
}
