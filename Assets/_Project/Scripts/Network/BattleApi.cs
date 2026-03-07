using System;
using System.Collections.Generic;
using CatCatGo.Shared.Requests;
using CatCatGo.Shared.Responses;

namespace CatCatGo.Network
{
    public static class BattleApi
    {
        public static void StartBattle(int chapterId, int day, string encounterType,
            Action<ApiResponse<BattleStartResponse>> callback)
        {
            var request = new BattleStartRequest
            {
                ChapterId = chapterId,
                Day = day,
                EncounterType = encounterType
            };
            ApiClient.Instance.Post("api/battle/start", request, callback);
        }

        public static void ReportResult(string battleId, int seed, string result, int turnCount,
            List<string> playerSkillIds, string enemyTemplateId, int goldReward,
            Action<ApiResponse<BattleReportResponse>> callback)
        {
            var request = new BattleReportRequest
            {
                BattleId = battleId,
                Seed = seed,
                Result = result,
                TurnCount = turnCount,
                PlayerSkillIds = playerSkillIds,
                EnemyTemplateId = enemyTemplateId,
                GoldReward = goldReward
            };
            ApiClient.Instance.Post("api/battle/report", request, callback);
        }
    }
}
