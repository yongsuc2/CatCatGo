using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using CatCatGo.Domain.Enums;
using CatCatGo.Infrastructure;

namespace CatCatGo.Domain.Data
{
    public struct SubStat
    {
        public string Stat;
        public float Value;

        public SubStat(string stat, float value)
        {
            Stat = stat;
            Value = value;
        }
    }

    public static class EquipmentSubStatTable
    {
        private static Dictionary<EquipmentGrade, int> _substatCount;
        private static Dictionary<SlotType, List<(string stat, Dictionary<EquipmentGrade, (float min, float max)> values)>> _pools;

        private static void EnsureLoaded()
        {
            if (_substatCount != null) return;
            var data = JsonDataLoader.LoadJObject("equipment-substats.data.json");

            _substatCount = new Dictionary<EquipmentGrade, int>();
            foreach (var kv in (JObject)data["substatCountByGrade"])
                _substatCount[(EquipmentGrade)Enum.Parse(typeof(EquipmentGrade), kv.Key)] = kv.Value.Value<int>();

            _pools = new Dictionary<SlotType, List<(string, Dictionary<EquipmentGrade, (float, float)>)>>();
            foreach (var kv in (JObject)data["pools"])
            {
                var slot = (SlotType)Enum.Parse(typeof(SlotType), kv.Key);
                var list = new List<(string, Dictionary<EquipmentGrade, (float, float)>)>();
                foreach (var entry in (JArray)kv.Value)
                {
                    var stat = entry["stat"].ToString();
                    var values = new Dictionary<EquipmentGrade, (float, float)>();
                    foreach (var vkv in (JObject)entry["values"])
                    {
                        var grade = (EquipmentGrade)Enum.Parse(typeof(EquipmentGrade), vkv.Key);
                        var arr = (JArray)vkv.Value;
                        values[grade] = (arr[0].Value<float>(), arr[1].Value<float>());
                    }
                    list.Add((stat, values));
                }
                _pools[slot] = list;
            }
        }

        public static int GetSubStatCount(EquipmentGrade grade)
        {
            EnsureLoaded();
            return _substatCount.TryGetValue(grade, out var v) ? v : 0;
        }

        public static SubStat RollSubStat(SlotType slot, EquipmentGrade grade, SeededRandom rng)
        {
            EnsureLoaded();
            var pool = _pools[slot];
            var entry = pool[rng.NextInt(0, pool.Count - 1)];
            var range = entry.values[grade];
            float value = entry.stat == "CRIT"
                ? Mathf.Round(rng.NextFloat(range.min, range.max) * 1000f) / 1000f
                : rng.NextInt((int)range.min, (int)range.max);
            return new SubStat(entry.stat, value);
        }

        public static List<SubStat> RollSubStats(SlotType slot, EquipmentGrade grade, int count, SeededRandom rng)
        {
            EnsureLoaded();
            var result = new List<SubStat>();
            for (int i = 0; i < count; i++)
                result.Add(RollSubStat(slot, grade, rng));
            return result;
        }

        public static List<SubStat> RollNewSubStats(SlotType slot, EquipmentGrade grade, SeededRandom rng)
        {
            int count = GetSubStatCount(grade);
            return RollSubStats(slot, grade, count, rng);
        }
    }
}
