using System;
using System.Collections.Generic;
using System.Linq;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Battle;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Domain.Chapter;
using CatCatGo.Domain.Data;

namespace CatCatGo.Domain.Content
{
    public class Tower
    {
        public int CurrentFloor;
        public int CurrentStage;

        private static TowerConfig Config => DungeonDataTable.Tower;

        public Tower(int currentFloor = 1, int currentStage = 1)
        {
            CurrentFloor = currentFloor;
            CurrentStage = currentStage;
        }

        public int MaxFloor => Config.MaxFloor;
        public int StagesPerFloor => Config.StagesPerFloor;

        public Result<TowerChallengeResult> Challenge(BattleUnit playerUnit, int challengeTokens)
        {
            if (challengeTokens < 1)
                return Result.Fail<TowerChallengeResult>("No challenge tokens");

            string enemyId = CurrentStage == StagesPerFloor
                ? EnemyTable.GetRandomBossId()
                : EnemyTable.GetRandomEnemyId();

            var template = EnemyTemplate.FromId(enemyId);
            if (template == null)
                return Result.Fail<TowerChallengeResult>("Enemy not found");

            var enemy = template.CreateTowerInstance(CurrentFloor);
            var battle = new Battle.Battle(playerUnit, enemy, (int)(DateTime.UtcNow.Ticks & 0x7FFFFFFF));

            return Result.Ok(new TowerChallengeResult { Battle = battle });
        }

        public TowerBattleResult OnBattleResult(BattleState state)
        {
            if (state == BattleState.DEFEAT)
                return new TowerBattleResult { Advanced = false, Reward = Reward.Empty(), TokenConsumed = false };

            var reward = GetReward(CurrentFloor, CurrentStage);

            if (CurrentStage >= StagesPerFloor)
            {
                CurrentStage = 1;
                CurrentFloor = Math.Min(CurrentFloor + 1, MaxFloor);
            }
            else
            {
                CurrentStage += 1;
            }

            return new TowerBattleResult { Advanced = true, Reward = reward, TokenConsumed = true };
        }

        public Reward GetReward(int floor, int stage)
        {
            if (!Config.RewardStages.Contains(stage))
                return Reward.Empty();

            string stageKey = $"stage{stage}";
            var rewards = new List<ResourceReward>();

            if (Config.GoldPerFloor.TryGetValue(stageKey, out int goldPerFloor))
                rewards.Add(new ResourceReward(ResourceType.GOLD, goldPerFloor * floor));

            if (Config.RewardPerStage.TryGetValue(stageKey, out var stageRewards))
            {
                foreach (var r in stageRewards)
                {
                    if (System.Enum.TryParse<ResourceType>(r.Type, out var resType))
                        rewards.Add(new ResourceReward(resType, r.Amount));
                }
            }

            return Reward.FromResources(rewards.ToArray());
        }

        public float GetProgress()
        {
            int totalStages = MaxFloor * StagesPerFloor;
            int currentTotal = (CurrentFloor - 1) * StagesPerFloor + CurrentStage;
            return (float)currentTotal / totalStages;
        }
    }

    public class TowerChallengeResult
    {
        public Battle.Battle Battle;
    }

    public class TowerBattleResult
    {
        public bool Advanced;
        public Reward Reward;
        public bool TokenConsumed;
    }
}
