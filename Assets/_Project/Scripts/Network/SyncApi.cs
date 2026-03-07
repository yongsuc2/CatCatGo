using System;

namespace CatCatGo.Network
{
    public static class SyncApi
    {
        public static void GetFull(Action<ApiResponse<ServerResponse<SyncFullResponseData>>> callback)
        {
            ApiClient.Instance.Get("api/sync/full", callback);
        }

        public static void Push(string saveState, long clientTimestamp, Action<ApiResponse<ServerResponse<SyncPushResponseData>>> callback)
        {
            ApiClient.Instance.Post("api/sync/push", new { saveState, clientTimestamp }, callback);
        }
    }
}
