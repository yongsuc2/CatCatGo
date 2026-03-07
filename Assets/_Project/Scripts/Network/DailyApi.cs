using System;

namespace CatCatGo.Network
{
    public static class DailyApi
    {
        public static void ClaimAttendance(Action<ApiResponse<ServerResponse<AttendanceClaimResponseData>>> callback)
        {
            ApiClient.Instance.Post("api/daily/attendance/claim", new { }, callback);
        }

        public static void ClaimQuest(string eventId, string missionId, Action<ApiResponse<ServerResponse<object>>> callback)
        {
            ApiClient.Instance.Post("api/daily/quest/claim", new { eventId, missionId }, callback);
        }

        public static void ClaimAllQuests(string eventId, Action<ApiResponse<ServerResponse<QuestClaimAllResponseData>>> callback)
        {
            ApiClient.Instance.Post("api/daily/quest/claim-all", new { eventId }, callback);
        }
    }
}
