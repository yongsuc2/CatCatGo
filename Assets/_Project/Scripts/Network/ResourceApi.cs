using System;
using CatCatGo.Shared.Responses;

namespace CatCatGo.Network
{
    public static class ResourceApi
    {
        public static void GetBalance(Action<ApiResponse<ResourceBalanceResponse>> callback)
        {
            ApiClient.Instance.Get<ResourceBalanceResponse>("api/resource/balance", callback);
        }
    }
}
