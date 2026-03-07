using System;

namespace CatCatGo.Network
{
    public static class TreasureApi
    {
        public static void Claim(string milestoneId, Action<ApiResponse<ServerResponse<object>>> callback)
        {
            ApiClient.Instance.Post("api/treasure/claim", new { milestoneId }, callback);
        }
    }
}
