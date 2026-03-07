using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using CatCatGo.Shared.Models;

namespace CatCatGo.Server.Core.Services;

public class DailyService
{
    private readonly IDailyRepository _dailyRepo;
    private readonly ResourceService _resourceService;

    private const int AttendanceCycleDays = 7;

    public DailyService(IDailyRepository dailyRepo, ResourceService resourceService)
    {
        _dailyRepo = dailyRepo;
        _resourceService = resourceService;
    }

    public async Task<AttendanceStatusResult> GetAttendanceAsync(Guid accountId)
    {
        var attendance = await _dailyRepo.GetAttendanceAsync(accountId);
        if (attendance == null)
        {
            attendance = new DailyAttendance
            {
                AccountId = accountId,
                CurrentDay = 1,
                CycleStartDate = DateTime.UtcNow.Date,
                UpdatedAt = DateTime.UtcNow,
            };
            await _dailyRepo.UpsertAttendanceAsync(attendance);
        }

        var canClaim = attendance.LastClaimDate.Date < DateTime.UtcNow.Date;
        return new AttendanceStatusResult
        {
            CurrentDay = attendance.CurrentDay,
            CanClaim = canClaim,
            CycleDays = AttendanceCycleDays,
        };
    }

    public async Task<ApiResponse<AttendanceClaimResponse>> ClaimAttendanceAsync(Guid accountId)
    {
        var attendance = await _dailyRepo.GetAttendanceAsync(accountId) ?? new DailyAttendance
        {
            AccountId = accountId,
            CurrentDay = 1,
            CycleStartDate = DateTime.UtcNow.Date,
            UpdatedAt = DateTime.UtcNow,
        };

        if (attendance.LastClaimDate.Date >= DateTime.UtcNow.Date)
            return ApiResponse<AttendanceClaimResponse>.Fail("ALREADY_CLAIMED_TODAY");

        var day = attendance.CurrentDay;
        var rewardType = await GrantAttendanceRewardAsync(accountId, day);

        attendance.LastClaimDate = DateTime.UtcNow;
        attendance.CurrentDay++;

        if (attendance.CurrentDay > AttendanceCycleDays)
        {
            attendance.CurrentDay = 1;
            attendance.CycleStartDate = DateTime.UtcNow.Date.AddDays(1);
        }

        attendance.UpdatedAt = DateTime.UtcNow;
        await _dailyRepo.UpsertAttendanceAsync(attendance);

        var checkedDays = new bool[AttendanceCycleDays];
        for (int i = 0; i < Math.Min(day, AttendanceCycleDays); i++)
            checkedDays[i] = true;

        var deltaBuilder = new StateDeltaBuilder()
            .SetAttendance(checkedDays, DateTime.UtcNow.ToString("yyyy-MM-dd"));

        var goldBalance = await _resourceService.GetBalanceAsync(accountId, "GOLD");
        deltaBuilder.AddResource("GOLD", (float)goldBalance);

        if (day >= 5)
        {
            var gemsBalance = await _resourceService.GetBalanceAsync(accountId, "GEMS");
            deltaBuilder.AddResource("GEMS", (float)gemsBalance);
        }

        return ApiResponse<AttendanceClaimResponse>.Ok(
            new AttendanceClaimResponse { Day = day, RewardType = rewardType },
            deltaBuilder.Build());
    }

    public async Task<QuestStatusResult> GetQuestsAsync(Guid accountId)
    {
        var today = DateTime.UtcNow.Date;
        var dailyQuests = await _dailyRepo.GetQuestsAsync(accountId, "DAILY", today);
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var weeklyQuests = await _dailyRepo.GetQuestsAsync(accountId, "WEEKLY", weekStart);

        return new QuestStatusResult
        {
            DailyQuests = dailyQuests,
            WeeklyQuests = weeklyQuests,
        };
    }

    public async Task<ApiResponse<object>> ClaimQuestAsync(Guid accountId, string eventId, string missionId)
    {
        var today = DateTime.UtcNow.Date;
        var dailyQuests = await _dailyRepo.GetQuestsAsync(accountId, "DAILY", today);
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var weeklyQuests = await _dailyRepo.GetQuestsAsync(accountId, "WEEKLY", weekStart);

        var quest = dailyQuests.Concat(weeklyQuests).FirstOrDefault(q => q.QuestId == missionId);
        if (quest == null)
            return ApiResponse<object>.Fail("QUEST_NOT_FOUND");
        if (!quest.IsCompleted)
            return ApiResponse<object>.Fail("QUEST_NOT_COMPLETED");
        if (quest.IsRewarded)
            return ApiResponse<object>.Fail("ALREADY_REWARDED");

        quest.IsRewarded = true;
        quest.UpdatedAt = DateTime.UtcNow;
        await _dailyRepo.UpsertQuestAsync(quest);

        await _resourceService.GrantAsync(accountId, "GEMS", 50, "QUEST_REWARD", missionId);

        var gemsBalance = await _resourceService.GetBalanceAsync(accountId, "GEMS");
        var delta = new StateDeltaBuilder()
            .AddResource("GEMS", (float)gemsBalance)
            .AddMissionUpdate(eventId, missionId, claimed: true)
            .Build();

        return ApiResponse<object>.Ok(delta: delta);
    }

    public async Task<ApiResponse<QuestClaimAllResponse>> ClaimAllQuestsAsync(Guid accountId, string eventId)
    {
        var today = DateTime.UtcNow.Date;
        var dailyQuests = await _dailyRepo.GetQuestsAsync(accountId, "DAILY", today);
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var weeklyQuests = await _dailyRepo.GetQuestsAsync(accountId, "WEEKLY", weekStart);

        var allQuests = dailyQuests.Concat(weeklyQuests).ToList();
        var claimable = allQuests.Where(q => q.IsCompleted && !q.IsRewarded).ToList();

        var deltaBuilder = new StateDeltaBuilder();
        var claimedCount = 0;

        foreach (var quest in claimable)
        {
            quest.IsRewarded = true;
            quest.UpdatedAt = DateTime.UtcNow;
            await _dailyRepo.UpsertQuestAsync(quest);
            await _resourceService.GrantAsync(accountId, "GEMS", 50, "QUEST_REWARD", quest.QuestId);
            deltaBuilder.AddMissionUpdate(eventId, quest.QuestId, claimed: true);
            claimedCount++;
        }

        if (claimedCount > 0)
        {
            var gemsBalance = await _resourceService.GetBalanceAsync(accountId, "GEMS");
            deltaBuilder.AddResource("GEMS", (float)gemsBalance);
        }

        return ApiResponse<QuestClaimAllResponse>.Ok(
            new QuestClaimAllResponse { ClaimedCount = claimedCount },
            deltaBuilder.Build());
    }

    private async Task<string> GrantAttendanceRewardAsync(Guid accountId, int day)
    {
        var goldAmount = day * 1000.0;
        await _resourceService.GrantAsync(accountId, "GOLD", goldAmount, "ATTENDANCE_REWARD", day.ToString());

        if (day >= 5)
        {
            await _resourceService.GrantAsync(accountId, "GEMS", 50, "ATTENDANCE_REWARD", day.ToString());
            return "GEMS";
        }
        return "GOLD";
    }
}

public class AttendanceStatusResult
{
    public int CurrentDay { get; set; }
    public bool CanClaim { get; set; }
    public int CycleDays { get; set; }
}

public class AttendanceClaimResponse
{
    public int Day { get; set; }
    public string RewardType { get; set; } = string.Empty;
}

public class QuestStatusResult
{
    public List<QuestProgress> DailyQuests { get; set; } = new();
    public List<QuestProgress> WeeklyQuests { get; set; } = new();
}

public class QuestClaimAllResponse
{
    public int ClaimedCount { get; set; }
}
