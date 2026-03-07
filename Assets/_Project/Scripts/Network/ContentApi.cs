using System;

namespace CatCatGo.Network
{
    public static class ContentApi
    {
        public static void TowerChallenge(Action<ApiResponse<ServerResponse<TowerChallengeResponseData>>> callback)
        {
            ApiClient.Instance.Post("api/content/tower/challenge", new { }, callback);
        }

        public static void DungeonChallenge(string dungeonType, Action<ApiResponse<ServerResponse<DungeonChallengeResponseData>>> callback)
        {
            ApiClient.Instance.Post("api/content/dungeon/challenge", new { dungeonType }, callback);
        }

        public static void DungeonSweep(string dungeonType, Action<ApiResponse<ServerResponse<DungeonSweepResponseData>>> callback)
        {
            ApiClient.Instance.Post("api/content/dungeon/sweep", new { dungeonType }, callback);
        }

        public static void GoblinMine(Action<ApiResponse<ServerResponse<GoblinMineResponseData>>> callback)
        {
            ApiClient.Instance.Post("api/content/goblin/mine", new { }, callback);
        }

        public static void GoblinCart(Action<ApiResponse<ServerResponse<GoblinCartResponseData>>> callback)
        {
            ApiClient.Instance.Post("api/content/goblin/cart", new { }, callback);
        }

        public static void CatacombStart(Action<ApiResponse<ServerResponse<object>>> callback)
        {
            ApiClient.Instance.Post("api/content/catacomb/start", new { }, callback);
        }

        public static void CatacombBattle(Action<ApiResponse<ServerResponse<CatacombBattleResponseData>>> callback)
        {
            ApiClient.Instance.Post("api/content/catacomb/battle", new { }, callback);
        }

        public static void CatacombEnd(Action<ApiResponse<ServerResponse<CatacombEndResponseData>>> callback)
        {
            ApiClient.Instance.Post("api/content/catacomb/end", new { }, callback);
        }
    }
}
