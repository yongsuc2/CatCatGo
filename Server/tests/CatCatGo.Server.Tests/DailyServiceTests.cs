using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using CatCatGo.Server.Core.Services;
using NSubstitute;
using Xunit;

namespace CatCatGo.Server.Tests;

public class DailyServiceTests
{
    private readonly IDailyRepository _dailyRepo;
    private readonly IResourceRepository _resourceRepo;
    private readonly ResourceService _resourceService;
    private readonly DailyService _sut;

    private readonly Guid _accountId = Guid.NewGuid();

    public DailyServiceTests()
    {
        _dailyRepo = Substitute.For<IDailyRepository>();
        _resourceRepo = Substitute.For<IResourceRepository>();
        _resourceService = new ResourceService(_resourceRepo);
        _sut = new DailyService(_dailyRepo, _resourceService);
    }

    [Fact]
    public async Task GetAttendanceAsync_NewAccount_CreatesDefault()
    {
        _dailyRepo.GetAttendanceAsync(_accountId).Returns((DailyAttendance?)null);

        var result = await _sut.GetAttendanceAsync(_accountId);

        Assert.Equal(1, result.CurrentDay);
        Assert.True(result.CanClaim);
        Assert.Equal(7, result.CycleDays);
        await _dailyRepo.Received(1).UpsertAttendanceAsync(Arg.Any<DailyAttendance>());
    }

    [Fact]
    public async Task GetAttendanceAsync_AlreadyClaimedToday_CanClaimFalse()
    {
        _dailyRepo.GetAttendanceAsync(_accountId).Returns(new DailyAttendance
        {
            AccountId = _accountId,
            CurrentDay = 3,
            LastClaimDate = DateTime.UtcNow,
            CycleStartDate = DateTime.UtcNow.Date.AddDays(-2),
        });

        var result = await _sut.GetAttendanceAsync(_accountId);

        Assert.False(result.CanClaim);
        Assert.Equal(3, result.CurrentDay);
    }

    [Fact]
    public async Task ClaimAttendanceAsync_FirstClaim_GrantsReward()
    {
        _dailyRepo.GetAttendanceAsync(_accountId).Returns(new DailyAttendance
        {
            AccountId = _accountId,
            CurrentDay = 1,
            LastClaimDate = DateTime.UtcNow.AddDays(-1),
            CycleStartDate = DateTime.UtcNow.Date,
        });
        _resourceRepo.GetBalanceAsync(_accountId, "GOLD").Returns((ResourceBalance?)null);

        var result = await _sut.ClaimAttendanceAsync(_accountId);

        Assert.True(result.Success);
        Assert.Equal(1, result.Day);
        await _resourceRepo.Received(1).UpsertBalanceAsync(Arg.Is<ResourceBalance>(b =>
            b.Type == "GOLD" && b.Amount == 1000));
    }

    [Fact]
    public async Task ClaimAttendanceAsync_Day5_GrantsGoldAndGems()
    {
        _dailyRepo.GetAttendanceAsync(_accountId).Returns(new DailyAttendance
        {
            AccountId = _accountId,
            CurrentDay = 5,
            LastClaimDate = DateTime.UtcNow.AddDays(-1),
            CycleStartDate = DateTime.UtcNow.Date.AddDays(-4),
        });
        _resourceRepo.GetBalanceAsync(_accountId, "GOLD").Returns((ResourceBalance?)null);
        _resourceRepo.GetBalanceAsync(_accountId, "GEMS").Returns((ResourceBalance?)null);

        var result = await _sut.ClaimAttendanceAsync(_accountId);

        Assert.True(result.Success);
        await _resourceRepo.Received(1).UpsertBalanceAsync(Arg.Is<ResourceBalance>(b =>
            b.Type == "GOLD" && b.Amount == 5000));
        await _resourceRepo.Received(1).UpsertBalanceAsync(Arg.Is<ResourceBalance>(b =>
            b.Type == "GEMS" && b.Amount == 50));
    }

    [Fact]
    public async Task ClaimAttendanceAsync_AlreadyClaimedToday_Fails()
    {
        _dailyRepo.GetAttendanceAsync(_accountId).Returns(new DailyAttendance
        {
            AccountId = _accountId,
            CurrentDay = 3,
            LastClaimDate = DateTime.UtcNow,
        });

        var result = await _sut.ClaimAttendanceAsync(_accountId);

        Assert.False(result.Success);
        Assert.Equal("ALREADY_CLAIMED_TODAY", result.Error);
    }

    [Fact]
    public async Task ClaimAttendanceAsync_Day7_ResetsCycle()
    {
        _dailyRepo.GetAttendanceAsync(_accountId).Returns(new DailyAttendance
        {
            AccountId = _accountId,
            CurrentDay = 7,
            LastClaimDate = DateTime.UtcNow.AddDays(-1),
            CycleStartDate = DateTime.UtcNow.Date.AddDays(-6),
        });
        _resourceRepo.GetBalanceAsync(_accountId, "GOLD").Returns((ResourceBalance?)null);
        _resourceRepo.GetBalanceAsync(_accountId, "GEMS").Returns((ResourceBalance?)null);

        var result = await _sut.ClaimAttendanceAsync(_accountId);

        Assert.True(result.Success);
        await _dailyRepo.Received(1).UpsertAttendanceAsync(Arg.Is<DailyAttendance>(a =>
            a.CurrentDay == 1));
    }

    [Fact]
    public async Task ClaimQuestAsync_CompletedQuest_GrantsGems()
    {
        var questId = "daily_kill_10";
        var quest = new QuestProgress
        {
            AccountId = _accountId, QuestId = questId, QuestType = "DAILY",
            IsCompleted = true, IsRewarded = false, ResetDate = DateTime.UtcNow.Date,
        };
        _dailyRepo.GetQuestsAsync(_accountId, "DAILY", Arg.Any<DateTime>()).Returns(new List<QuestProgress> { quest });
        _dailyRepo.GetQuestsAsync(_accountId, "WEEKLY", Arg.Any<DateTime>()).Returns(new List<QuestProgress>());
        _resourceRepo.GetBalanceAsync(_accountId, "GEMS").Returns((ResourceBalance?)null);

        var result = await _sut.ClaimQuestAsync(_accountId, questId);

        Assert.True(result.Success);
        await _resourceRepo.Received(1).UpsertBalanceAsync(Arg.Is<ResourceBalance>(b =>
            b.Type == "GEMS" && b.Amount == 50));
    }

    [Fact]
    public async Task ClaimQuestAsync_NotCompleted_Fails()
    {
        var questId = "daily_kill_10";
        var quest = new QuestProgress
        {
            AccountId = _accountId, QuestId = questId, QuestType = "DAILY",
            IsCompleted = false, IsRewarded = false, ResetDate = DateTime.UtcNow.Date,
        };
        _dailyRepo.GetQuestsAsync(_accountId, "DAILY", Arg.Any<DateTime>()).Returns(new List<QuestProgress> { quest });
        _dailyRepo.GetQuestsAsync(_accountId, "WEEKLY", Arg.Any<DateTime>()).Returns(new List<QuestProgress>());

        var result = await _sut.ClaimQuestAsync(_accountId, questId);

        Assert.False(result.Success);
        Assert.Equal("QUEST_NOT_COMPLETED", result.Error);
    }

    [Fact]
    public async Task ClaimQuestAsync_AlreadyRewarded_Fails()
    {
        var questId = "daily_kill_10";
        var quest = new QuestProgress
        {
            AccountId = _accountId, QuestId = questId, QuestType = "DAILY",
            IsCompleted = true, IsRewarded = true, ResetDate = DateTime.UtcNow.Date,
        };
        _dailyRepo.GetQuestsAsync(_accountId, "DAILY", Arg.Any<DateTime>()).Returns(new List<QuestProgress> { quest });
        _dailyRepo.GetQuestsAsync(_accountId, "WEEKLY", Arg.Any<DateTime>()).Returns(new List<QuestProgress>());

        var result = await _sut.ClaimQuestAsync(_accountId, questId);

        Assert.False(result.Success);
        Assert.Equal("ALREADY_REWARDED", result.Error);
    }

    [Fact]
    public async Task ClaimQuestAsync_QuestNotFound_Fails()
    {
        _dailyRepo.GetQuestsAsync(_accountId, "DAILY", Arg.Any<DateTime>()).Returns(new List<QuestProgress>());
        _dailyRepo.GetQuestsAsync(_accountId, "WEEKLY", Arg.Any<DateTime>()).Returns(new List<QuestProgress>());

        var result = await _sut.ClaimQuestAsync(_accountId, "nonexistent");

        Assert.False(result.Success);
        Assert.Equal("QUEST_NOT_FOUND", result.Error);
    }
}
