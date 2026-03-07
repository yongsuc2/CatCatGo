using System;

namespace CatCatGo.Network
{
    public static class GachaApi
    {
        public static void Pull(string chestType, Action<ApiResponse<ServerResponse<GachaPullResponseData>>> callback)
        {
            ApiClient.Instance.Post("api/gacha/pull", new { chestType, count = 1 }, callback);
        }

        public static void Pull10(string chestType, Action<ApiResponse<ServerResponse<GachaPullResponseData>>> callback)
        {
            ApiClient.Instance.Post("api/gacha/pull10", new { chestType }, callback);
        }
    }
}
