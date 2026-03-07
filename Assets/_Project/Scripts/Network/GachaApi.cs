using System;

namespace CatCatGo.Network
{
    public static class GachaApi
    {
        public static void Pull(Action<ApiResponse<ServerResponse<GachaPullResponseData>>> callback)
        {
            ApiClient.Instance.Post("api/gacha/pull", new { chestType = "EQUIPMENT", count = 1 }, callback);
        }

        public static void Pull10(Action<ApiResponse<ServerResponse<GachaPullResponseData>>> callback)
        {
            ApiClient.Instance.Post("api/gacha/pull10", new { chestType = "EQUIPMENT" }, callback);
        }
    }
}
