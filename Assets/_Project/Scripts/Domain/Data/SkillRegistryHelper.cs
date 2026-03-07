using System;
using System.Collections.Generic;

namespace CatCatGo.Domain.Data
{
    public static class SkillRegistryHelper
    {
        public static readonly Dictionary<int, string> TierSuffix = new Dictionary<int, string>
        {
            { 1, "" }, { 2, " II" }, { 3, " III" }, { 4, " IV" },
        };

        public static string Pct(float v)
        {
            return $"{Math.Round(v * 100)}%";
        }

        public static float V(Dictionary<string, float> d, string key)
        {
            return d.TryGetValue(key, out var v) ? v : 0f;
        }

        public static string GetTierSuffix(int tier)
        {
            return TierSuffix.TryGetValue(tier, out var suffix) ? suffix : "";
        }
    }
}
