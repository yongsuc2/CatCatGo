using System;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Battle;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Domain.Chapter;
using CatCatGo.Domain.Data;

namespace CatCatGo.Domain.Content
{
    public class CatacombDungeon
    {
        public int HighestFloor;
        public int CurrentRunFloor;
        public int CurrentBattleIndex;
        public bool IsRunning;

        private static CatacombConfig Config => DungeonDataTable.Catacomb;

        public CatacombDungeon(int highestFloor = 1)
        {
            HighestFloor = highestFloor;
            CurrentRunFloor = highestFloor;
            CurrentBattleIndex = 0;
            IsRunning = false;
        }

        public int BattlesPerFloor => Config.BattlesPerFloor;

        public void StartRun()
        {
            CurrentRunFloor = HighestFloor;
            CurrentBattleIndex = 0;
            IsRunning = true;
        }

        public Battle.Battle GetNextBattle(BattleUnit playerUnit)
        {
            if (!IsRunning) return null;

            bool isBoss = CurrentBattleIndex >= BattlesPerFloor;
            string enemyId = isBoss ? EnemyTable.GetRandomBossId() : EnemyTable.GetRandomEnemyId();
            var template = EnemyTemplate.FromId(enemyId);
            if (template == null) return null;

            int scalingFloor = CurrentRunFloor + CurrentBattleIndex / 2;
            var enemy = template.CreateTowerInstance(scalingFloor);

            return new Battle.Battle(playerUnit, enemy, (int)(DateTime.UtcNow.Ticks & 0x7FFFFFFF));
        }

        public CatacombBattleResult OnBattleResult(BattleState state)
        {
            if (state == BattleState.DEFEAT)
            {
                IsRunning = false;
                return new CatacombBattleResult { ContinueRun = false, Reward = GetFloorReward() };
            }

            CurrentBattleIndex += 1;

            if (CurrentBattleIndex > BattlesPerFloor)
            {
                CurrentRunFloor += 1;
                if (CurrentRunFloor > HighestFloor)
                    HighestFloor = CurrentRunFloor;
                CurrentBattleIndex = 0;
            }

            return new CatacombBattleResult { ContinueRun = true, Reward = Reward.Empty() };
        }

        public Reward EndRun()
        {
            IsRunning = false;
            return GetFloorReward();
        }

        private Reward GetFloorReward()
        {
            int floorsCleared = CurrentRunFloor - HighestFloor + 1;
            return Reward.FromResources(
                new ResourceReward(ResourceType.GOLD, Math.Max(0, floorsCleared) * Config.GoldPerFloor),
                new ResourceReward(ResourceType.EQUIPMENT_STONE, Math.Max(Config.BaseEquipmentStone, floorsCleared)));
        }
    }

    public class CatacombBattleResult
    {
        public bool ContinueRun;
        public Reward Reward;
    }
}
