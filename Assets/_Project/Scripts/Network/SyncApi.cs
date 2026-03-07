using System;
using CatCatGo.Shared.Requests;
using CatCatGo.Shared.Responses;

namespace CatCatGo.Network
{
    public static class SyncApi
    {
        public static void Load(Action<ApiResponse<SaveSyncResponse>> callback)
        {
            ApiClient.Instance.Get("api/save", callback);
        }

        public static void Sync(string data, long clientTimestamp, Action<ApiResponse<SaveSyncResponse>> callback)
        {
            var request = new SaveSyncRequest
            {
                Data = data,
                ClientTimestamp = clientTimestamp,
            };
            ApiClient.Instance.Post("api/save/sync", request, callback);
        }
    }
}
