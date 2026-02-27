using System;
using System.Collections.Generic;
using System.Linq;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Domain.Battle;
using CatCatGo.Domain.Chapter;
using CatCatGo.Domain.Data;

namespace CatCatGo.Domain.Content
{
    public class DailyDungeon
    {
        public DungeonType Type;
        public int ClearedStage;
        private DungeonDef _config;

        public DailyDungeon(DungeonType type)
        {
            Type = type;
            ClearedStage = 0;
            _config = DungeonDataTable.GetDungeon(type.ToString());
        }

        public int GetNextStage()
        {
            return ClearedStage + 1;
        }

        public Result<DungeonBattleResult> CreateBattle(BattleUnit playerUnit)
        {
            var template = EnemyTemplate.FromId(_config.EnemyId);
            if (template == null)
                return Result.Fail<DungeonBattleResult>("Enemy not found");

            int stage = GetNextStage();
            var scaling = DungeonDataTable.StageScaling;
            float multiplier = scaling.StatMultiplierBase + scaling.StatMultiplierPerStage * (stage - 1);

            var enemyUnit = template.CreateInstance(1, multiplier);
            var battle = new Battle.Battle(playerUnit, enemyUnit, (int)(DateTime.UtcNow.Ticks & 0x7FFFFFFF));

            return Result.Ok(new DungeonBattleResult { Battle = battle });
        }

        public Reward OnBattleVictory()
        {
            ClearedStage += 1;
            return GetRewardForCurrentStage();
        }

        public List<ResourceReward> GetRewardForStage(int stage)
        {
            float scaling = 1 + _config.RewardScaling * (stage - 1);
            return _config.BaseRewards.Select(r => new ResourceReward(
                (ResourceType)Enum.Parse(typeof(ResourceType), r.Type),
                (int)Math.Floor(r.Amount * scaling)
            )).ToList();
        }

        private Reward GetRewardForCurrentStage()
        {
            var resources = GetRewardForStage(ClearedStage);
            return Reward.FromResources(resources.ToArray());
        }

        public Reward GetSweepReward()
        {
            if (ClearedStage <= 0) return Reward.Empty();

            var totals = new Dictionary<ResourceType, int>();
            for (int s = 1; s <= ClearedStage; s++)
            {
                foreach (var r in GetRewardForStage(s))
                {
                    totals.TryGetValue(r.Type, out int current);
                    totals[r.Type] = current + r.Amount;
                }
            }

            return Reward.FromResources(
                totals.Select(kv => new ResourceReward(kv.Key, kv.Value)).ToArray());
        }

        public List<ResourceReward> GetRewardPreview()
        {
            return GetRewardForStage(GetNextStage());
        }
    }

    public class DungeonBattleResult
    {
        public Battle.Battle Battle;
    }

    public class DailyDungeonManager
    {
        public Dictionary<DungeonType, DailyDungeon> Dungeons;
        public int TodayCount;
        public readonly int DailyLimit;

        public DailyDungeonManager()
        {
            TodayCount = 0;
            DailyLimit = DungeonDataTable.DailyLimit;
            Dungeons = new Dictionary<DungeonType, DailyDungeon>
            {
                { DungeonType.DRAGON_NEST, new DailyDungeon(DungeonType.DRAGON_NEST) },
                { DungeonType.CELESTIAL_TREE, new DailyDungeon(DungeonType.CELESTIAL_TREE) },
                { DungeonType.SKY_ISLAND, new DailyDungeon(DungeonType.SKY_ISLAND) },
            };
        }

        public DailyDungeon GetDungeon(DungeonType type)
        {
            return Dungeons[type];
        }

        public bool IsAvailable()
        {
            return TodayCount < DailyLimit;
        }

        public int GetRemainingCount()
        {
            return Math.Max(0, DailyLimit - TodayCount);
        }

        public void ConsumeEntry()
        {
            TodayCount += 1;
        }

        public List<DailyDungeon> GetAvailableDungeons()
        {
            return Dungeons.Values.ToList();
        }

        public void DailyResetAll()
        {
            TodayCount = 0;
        }

        public int GetTotalRemainingCount()
        {
            return GetRemainingCount();
        }
    }
}
