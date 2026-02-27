using System;
using System.Collections.Generic;
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
        public readonly int MaxFloor = 100;
        public readonly int StagesPerFloor = 10;

        public Tower(int currentFloor = 1, int currentStage = 1)
        {
            CurrentFloor = currentFloor;
            CurrentStage = currentStage;
        }

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
            var rewards = new List<ResourceReward>
            {
                new ResourceReward(ResourceType.GOLD, 50 * floor),
            };

            if (stage == 5 || stage == 10)
                rewards.Add(new ResourceReward(ResourceType.POWER_STONE, 1));

            if (stage == 10)
                rewards.Add(new ResourceReward(ResourceType.EQUIPMENT_STONE, 3));

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
