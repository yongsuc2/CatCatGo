using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;

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

    public async Task<AttendanceClaimResult> ClaimAttendanceAsync(Guid accountId)
    {
        var attendance = await _dailyRepo.GetAttendanceAsync(accountId) ?? new DailyAttendance
        {
            AccountId = accountId,
            CurrentDay = 1,
            CycleStartDate = DateTime.UtcNow.Date,
            UpdatedAt = DateTime.UtcNow,
        };

        if (attendance.LastClaimDate.Date >= DateTime.UtcNow.Date)
            return new AttendanceClaimResult { Success = false, Error = "ALREADY_CLAIMED_TODAY" };

        var day = attendance.CurrentDay;
        await GrantAttendanceRewardAsync(accountId, day);

        attendance.LastClaimDate = DateTime.UtcNow;
        attendance.CurrentDay++;

        if (attendance.CurrentDay > AttendanceCycleDays)
        {
            attendance.CurrentDay = 1;
            attendance.CycleStartDate = DateTime.UtcNow.Date.AddDays(1);
        }

        attendance.UpdatedAt = DateTime.UtcNow;
        await _dailyRepo.UpsertAttendanceAsync(attendance);

        return new AttendanceClaimResult { Success = true, Day = day };
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

    public async Task<QuestClaimResult> ClaimQuestAsync(Guid accountId, string questId)
    {
        var today = DateTime.UtcNow.Date;
        var dailyQuests = await _dailyRepo.GetQuestsAsync(accountId, "DAILY", today);
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var weeklyQuests = await _dailyRepo.GetQuestsAsync(accountId, "WEEKLY", weekStart);

        var quest = dailyQuests.Concat(weeklyQuests).FirstOrDefault(q => q.QuestId == questId);
        if (quest == null)
            return new QuestClaimResult { Success = false, Error = "QUEST_NOT_FOUND" };
        if (!quest.IsCompleted)
            return new QuestClaimResult { Success = false, Error = "QUEST_NOT_COMPLETED" };
        if (quest.IsRewarded)
            return new QuestClaimResult { Success = false, Error = "ALREADY_REWARDED" };

        quest.IsRewarded = true;
        quest.UpdatedAt = DateTime.UtcNow;
        await _dailyRepo.UpsertQuestAsync(quest);

        await _resourceService.GrantAsync(accountId, "GEMS", 50, "QUEST_REWARD", questId);

        return new QuestClaimResult { Success = true };
    }

    private async Task GrantAttendanceRewardAsync(Guid accountId, int day)
    {
        var goldAmount = day * 1000.0;
        await _resourceService.GrantAsync(accountId, "GOLD", goldAmount, "ATTENDANCE_REWARD", day.ToString());

        if (day >= 5)
            await _resourceService.GrantAsync(accountId, "GEMS", 50, "ATTENDANCE_REWARD", day.ToString());
    }
}

public class AttendanceStatusResult
{
    public int CurrentDay { get; set; }
    public bool CanClaim { get; set; }
    public int CycleDays { get; set; }
}

public class AttendanceClaimResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public int Day { get; set; }
}

public class QuestStatusResult
{
    public List<QuestProgress> DailyQuests { get; set; } = new();
    public List<QuestProgress> WeeklyQuests { get; set; } = new();
}

public class QuestClaimResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
}
