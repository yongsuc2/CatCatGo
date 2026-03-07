using System;
using CatCatGo.Shared.Requests;
using CatCatGo.Shared.Responses;

namespace CatCatGo.Network
{
    public static class ArenaApi
    {
        public static void RequestMatch(Action<ApiResponse<ArenaMatchResponse>> callback)
        {
            ApiClient.Instance.Post("api/arena/match", null, callback);
        }

        public static void SubmitResult(string matchId, int rank, Action<ApiResponse<object>> callback)
        {
            var request = new ArenaResultRequest
            {
                MatchId = matchId,
                Rank = rank
            };
            ApiClient.Instance.PostNoResponse("api/arena/result", request, callback);
        }

        public static void GetRanking(int season, Action<ApiResponse<ArenaRankingResponse>> callback)
        {
            string endpoint = $"api/arena/ranking?season={season}";
            ApiClient.Instance.Get(endpoint, callback);
        }
    }
}
