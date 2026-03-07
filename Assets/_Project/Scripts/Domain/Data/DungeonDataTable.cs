using System.Collections.Generic;
using System.Linq;
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

    public class TowerConfig
    {
        public int MaxFloor;
        public int StagesPerFloor;
        public int[] RewardStages;
        public Dictionary<string, int> GoldPerFloor;
        public Dictionary<string, List<DungeonBaseReward>> RewardPerStage;
    }

    public class CatacombConfig
    {
        public int BattlesPerFloor;
        public int GoldPerFloor;
        public int BaseEquipmentStone;
    }

    public class GoblinMinerCartRewardConfig
    {
        public int GoldMin;
        public int GoldMax;
        public int StoneMin;
        public int StoneMax;
    }

    public class GoblinMinerConfig
    {
        public int OrePerMine;
        public int CartThreshold;
        public GoblinMinerCartRewardConfig CartReward;
    }

    public static class DungeonDataTable
    {
        private static int _dailyLimit;
        private static Dictionary<string, DungeonDef> _dungeons;
        private static DungeonStageScaling _stageScaling;
        private static TowerConfig _tower;
        private static CatacombConfig _catacomb;
        private static GoblinMinerConfig _goblinMiner;

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

            var tw = data["tower"];
            _tower = new TowerConfig
            {
                MaxFloor = tw["maxFloor"].Value<int>(),
                StagesPerFloor = tw["stagesPerFloor"].Value<int>(),
                RewardStages = tw["rewardStages"].Select(s => s.Value<int>()).ToArray(),
                GoldPerFloor = new Dictionary<string, int>(),
                RewardPerStage = new Dictionary<string, List<DungeonBaseReward>>(),
            };
            foreach (var kv in (JObject)tw["goldPerFloor"])
                _tower.GoldPerFloor[kv.Key] = kv.Value.Value<int>();
            foreach (var kv in (JObject)tw["rewardPerStage"])
            {
                var list = new List<DungeonBaseReward>();
                foreach (var r in kv.Value)
                    list.Add(new DungeonBaseReward { Type = r["type"].ToString(), Amount = r["amount"].Value<int>() });
                _tower.RewardPerStage[kv.Key] = list;
            }

            var cb = data["catacomb"];
            _catacomb = new CatacombConfig
            {
                BattlesPerFloor = cb["battlesPerFloor"].Value<int>(),
                GoldPerFloor = cb["goldPerFloor"].Value<int>(),
                BaseEquipmentStone = cb["baseEquipmentStone"].Value<int>(),
            };

            var gm = data["goblinMiner"];
            if (gm != null)
            {
                var cr = gm["cartReward"];
                _goblinMiner = new GoblinMinerConfig
                {
                    OrePerMine = gm["orePerMine"].Value<int>(),
                    CartThreshold = gm["cartThreshold"].Value<int>(),
                    CartReward = new GoblinMinerCartRewardConfig
                    {
                        GoldMin = cr["goldMin"].Value<int>(),
                        GoldMax = cr["goldMax"].Value<int>(),
                        StoneMin = cr["stoneMin"].Value<int>(),
                        StoneMax = cr["stoneMax"].Value<int>(),
                    },
                };
            }
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

        public static TowerConfig Tower
        {
            get { EnsureLoaded(); return _tower; }
        }

        public static CatacombConfig Catacomb
        {
            get { EnsureLoaded(); return _catacomb; }
        }

        public static GoblinMinerConfig GoblinMiner
        {
            get { EnsureLoaded(); return _goblinMiner; }
        }
    }
}
