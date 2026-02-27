using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Infrastructure;

namespace CatCatGo.Domain.Data
{
    public static class HeritageTable
    {
        private static JObject _data;
        private static Dictionary<HeritageRoute, ResourceType> _bookTypeMap;
        private static Dictionary<HeritageRoute, Stats> _passivePerLevel;
        private static float _baseUpgradeCost;
        private static float _costGrowth;
        private static float _baseSkillMultiplier;
        private static float _synergyMultiplierPerLevel;

        private static void EnsureLoaded()
        {
            if (_data != null) return;
            _data = JsonDataLoader.LoadJObject("heritage.data.json");

            _baseUpgradeCost = _data["baseUpgradeCost"].Value<float>();
            _costGrowth = _data["costGrowth"].Value<float>();
            _baseSkillMultiplier = _data["baseSkillMultiplier"].Value<float>();
            _synergyMultiplierPerLevel = _data["synergyMultiplierPerLevel"].Value<float>();

            _bookTypeMap = new Dictionary<HeritageRoute, ResourceType>();
            foreach (var kv in (JObject)_data["bookTypeMap"])
            {
                var route = (HeritageRoute)Enum.Parse(typeof(HeritageRoute), kv.Key);
                var book = (ResourceType)Enum.Parse(typeof(ResourceType), kv.Value.ToString());
                _bookTypeMap[route] = book;
            }

            _passivePerLevel = new Dictionary<HeritageRoute, Stats>();
            foreach (var kv in (JObject)_data["passivePerLevel"])
            {
                var route = (HeritageRoute)Enum.Parse(typeof(HeritageRoute), kv.Key);
                var raw = kv.Value;
                _passivePerLevel[route] = Stats.Create(
                    atk: raw["atk"]?.Value<int>() ?? 0,
                    def: raw["def"]?.Value<int>() ?? 0,
                    maxHp: raw["maxHp"]?.Value<int>() ?? 0,
                    crit: raw["crit"]?.Value<float>() ?? 0f
                );
            }
        }

        public static ResourceType GetBookType(HeritageRoute route)
        {
            EnsureLoaded();
            return _bookTypeMap[route];
        }

        public static int GetUpgradeCost(int level)
        {
            EnsureLoaded();
            return (int)(_baseUpgradeCost + level * _costGrowth);
        }

        public static Stats GetPassivePerLevel(HeritageRoute route)
        {
            EnsureLoaded();
            return _passivePerLevel[route];
        }

        public static float GetSkillMultiplier(HeritageRoute route, int level, bool isSynergy)
        {
            EnsureLoaded();
            if (!isSynergy) return _baseSkillMultiplier;
            return _baseSkillMultiplier + level * _synergyMultiplierPerLevel;
        }
    }
}
