using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using CatCatGo.Infrastructure;

namespace CatCatGo.Domain.Data
{
    public static class ActiveSkillTierData
    {
        private static Dictionary<string, Dictionary<int, Dictionary<string, float>>> _nested;

        private static void EnsureLoaded()
        {
            if (_nested != null) return;

            _nested = new Dictionary<string, Dictionary<int, Dictionary<string, float>>>();
            var rows = JsonDataLoader.LoadJArray("active-skill-tier.data.json");
            if (rows == null) return;

            foreach (var row in rows)
            {
                var obj = (JObject)row;
                var skill = obj["skill"].ToString();
                var tier = obj["tier"].Value<int>();

                if (!_nested.ContainsKey(skill))
                    _nested[skill] = new Dictionary<int, Dictionary<string, float>>();

                var param = new Dictionary<string, float>();
                foreach (var kv in obj)
                {
                    if (kv.Key == "skill" || kv.Key == "tier") continue;
                    param[kv.Key] = kv.Value.Value<float>();
                }
                _nested[skill][tier] = param;
            }
        }

        public static Dictionary<string, Dictionary<int, Dictionary<string, float>>> GetAll()
        {
            EnsureLoaded();
            return _nested;
        }

        public static Dictionary<string, float> GetTierData(string id, int tier)
        {
            EnsureLoaded();
            if (_nested.TryGetValue(id, out var family) && family.TryGetValue(tier, out var data))
                return data;
            return null;
        }

        public static Dictionary<int, Dictionary<string, float>> GetFamily(string id)
        {
            EnsureLoaded();
            _nested.TryGetValue(id, out var family);
            return family;
        }
    }
}
