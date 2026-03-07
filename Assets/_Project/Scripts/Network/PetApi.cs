using System;

namespace CatCatGo.Network
{
    public static class PetApi
    {
        public static void Hatch(Action<ApiResponse<ServerResponse<PetHatchResponseData>>> callback)
        {
            ApiClient.Instance.Post("api/pet/hatch", new { }, callback);
        }

        public static void Feed(string petId, int amount, Action<ApiResponse<ServerResponse<object>>> callback)
        {
            ApiClient.Instance.Post("api/pet/feed", new { petId, amount }, callback);
        }

        public static void Deploy(string petId, Action<ApiResponse<ServerResponse<object>>> callback)
        {
            ApiClient.Instance.Post("api/pet/deploy", new { petId }, callback);
        }
    }
}
