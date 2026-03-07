using System.Collections.Generic;

namespace CatCatGo.Domain.Data
{
    public static class ActiveSkillTierData
    {
        private static readonly SkillTierDataLoader _loader = new SkillTierDataLoader("active-skill-tier.data.json");

        public static Dictionary<string, Dictionary<int, Dictionary<string, float>>> GetAll() => _loader.GetAll();
        public static Dictionary<string, float> GetTierData(string id, int tier) => _loader.GetTierData(id, tier);
        public static Dictionary<int, Dictionary<string, float>> GetFamily(string id) => _loader.GetFamily(id);
    }
}
