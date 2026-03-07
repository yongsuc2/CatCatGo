using System;

namespace CatCatGo.Network
{
    public static class TalentApi
    {
        public static void Upgrade(string statType, Action<ApiResponse<ServerResponse<object>>> callback)
        {
            ApiClient.Instance.Post("api/talent/upgrade", new { statType }, callback);
        }

        public static void ClaimMilestone(int milestoneLevel, Action<ApiResponse<ServerResponse<TalentMilestoneResponseData>>> callback)
        {
            ApiClient.Instance.Post("api/talent/claim-milestone", new { milestoneLevel }, callback);
        }

        public static void ClaimAllMilestones(Action<ApiResponse<ServerResponse<ClaimAllMilestonesResponseData>>> callback)
        {
            ApiClient.Instance.Post("api/talent/claim-all-milestones", new { }, callback);
        }
    }
}
