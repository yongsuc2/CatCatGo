using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using CatCatGo.Infrastructure;

namespace CatCatGo.Domain.Data
{
    public class DungeonDef
    {
        public string EnemyId;
        public List<DungeonBaseReward> BaseRewards;
        public float RewardScaling;
    }

    public class DungeonBaseReward
    {
        public string Type;
        public int Amount;
    }

    public class DungeonStageScaling
    {
        public float StatMultiplierBase;
        public float StatMultiplierPerStage;
    }

    public static class DungeonDataTable
    {
        private static int _dailyLimit;
        private static Dictionary<string, DungeonDef> _dungeons;
        private static DungeonStageScaling _stageScaling;

        private static void EnsureLoaded()
        {
            if (_dungeons != null) return;

            var data = JsonDataLoader.LoadJObject("dungeon.data.json");
            if (data == null) return;

            _dailyLimit = data["dailyLimit"].Value<int>();

            _dungeons = new Dictionary<string, DungeonDef>();
            foreach (var kv in (JObject)data["dungeons"])
            {
                var d = kv.Value;
                var rewards = new List<DungeonBaseReward>();
                foreach (var r in d["baseRewards"])
                    rewards.Add(new DungeonBaseReward { Type = r["type"].ToString(), Amount = r["amount"].Value<int>() });

                _dungeons[kv.Key] = new DungeonDef
                {
                    EnemyId = d["enemyId"].ToString(),
                    BaseRewards = rewards,
                    RewardScaling = d["rewardScaling"].Value<float>(),
                };
            }

            var ss = data["stageScaling"];
            _stageScaling = new DungeonStageScaling
            {
                StatMultiplierBase = ss["statMultiplierBase"].Value<float>(),
                StatMultiplierPerStage = ss["statMultiplierPerStage"].Value<float>(),
            };
        }

        public static int DailyLimit
        {
            get { EnsureLoaded(); return _dailyLimit; }
        }

        public static DungeonDef GetDungeon(string key)
        {
            EnsureLoaded();
            _dungeons.TryGetValue(key, out var d);
            return d;
        }

        public static DungeonStageScaling StageScaling
        {
            get { EnsureLoaded(); return _stageScaling; }
        }
    }
}
