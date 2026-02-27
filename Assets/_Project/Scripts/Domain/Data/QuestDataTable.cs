using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using CatCatGo.Domain.Enums;
using CatCatGo.Infrastructure;

namespace CatCatGo.Domain.Data
{
    public class QuestDef
    {
        public string Id;
        public string Description;
        public int Target;
        public ResourceType RewardType;
        public int RewardAmount;
    }

    public static class QuestDataTable
    {
        private static List<QuestDef> _daily;
        private static List<QuestDef> _weekly;

        private static void EnsureLoaded()
        {
            if (_daily != null) return;

            var data = JsonDataLoader.LoadJObject("quest.data.json");
            if (data == null) return;

            _daily = ParseQuests(data["daily"]);
            _weekly = ParseQuests(data["weekly"]);
        }

        private static List<QuestDef> ParseQuests(JToken arr)
        {
            return arr.Select(q => new QuestDef
            {
                Id = q["id"].ToString(),
                Description = q["description"].ToString(),
                Target = q["target"].Value<int>(),
                RewardType = (ResourceType)System.Enum.Parse(typeof(ResourceType), q["rewardType"].ToString()),
                RewardAmount = q["rewardAmount"].Value<int>(),
            }).ToList();
        }

        public static List<QuestDef> Daily { get { EnsureLoaded(); return _daily; } }
        public static List<QuestDef> Weekly { get { EnsureLoaded(); return _weekly; } }
    }
}
