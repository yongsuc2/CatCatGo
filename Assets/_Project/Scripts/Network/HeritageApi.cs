using System;

namespace CatCatGo.Network
{
    public static class HeritageApi
    {
        public static void Upgrade(string route, Action<ApiResponse<ServerResponse<object>>> callback)
        {
            ApiClient.Instance.Post("api/heritage/upgrade", new { route }, callback);
        }
    }
}
