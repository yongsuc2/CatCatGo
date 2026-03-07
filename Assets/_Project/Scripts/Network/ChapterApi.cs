using System;

namespace CatCatGo.Network
{
    public static class ChapterApi
    {
        public static void Start(int chapterId, string chapterType, Action<ApiResponse<ServerResponse<ChapterStartResponseData>>> callback)
        {
            ApiClient.Instance.Post("api/chapter/start", new { chapterId, chapterType }, callback);
        }

        public static void AdvanceDay(string sessionId, Action<ApiResponse<ServerResponse<ChapterAdvanceDayResponseData>>> callback)
        {
            ApiClient.Instance.Post("api/chapter/advance-day", new { sessionId }, callback);
        }

        public static void ResolveEncounter(string sessionId, int choiceIndex, Action<ApiResponse<ServerResponse<ChapterResolveEncounterResponseData>>> callback)
        {
            ApiClient.Instance.Post("api/chapter/resolve-encounter", new { sessionId, choiceIndex }, callback);
        }

        public static void SelectSkill(string sessionId, string skillId, Action<ApiResponse<ServerResponse<ChapterSelectSkillResponseData>>> callback)
        {
            ApiClient.Instance.Post("api/chapter/select-skill", new { sessionId, skillId }, callback);
        }

        public static void Reroll(string sessionId, Action<ApiResponse<ServerResponse<ChapterRerollResponseData>>> callback)
        {
            ApiClient.Instance.Post("api/chapter/reroll", new { sessionId }, callback);
        }

        public static void BattleResult(string sessionId, int battleSeed, string result, int turnCount, int playerRemainingHp,
            Action<ApiResponse<ServerResponse<ChapterBattleResultResponseData>>> callback)
        {
            ApiClient.Instance.Post("api/chapter/battle-result",
                new { sessionId, battleSeed, result, turnCount, playerRemainingHp }, callback);
        }

        public static void Abandon(string sessionId, Action<ApiResponse<ServerResponse<object>>> callback)
        {
            ApiClient.Instance.Post("api/chapter/abandon", new { sessionId }, callback);
        }
    }
}
