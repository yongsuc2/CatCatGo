using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using CatCatGo.Server.Core.Services;
using NSubstitute;
using Xunit;

namespace CatCatGo.Server.Tests;

public class ChapterServiceTests
{
    private readonly IChapterRepository _chapterRepo;
    private readonly IResourceRepository _resourceRepo;
    private readonly ResourceService _resourceService;
    private readonly ChapterService _sut;

    private readonly Guid _accountId = Guid.NewGuid();

    public ChapterServiceTests()
    {
        _chapterRepo = Substitute.For<IChapterRepository>();
        _resourceRepo = Substitute.For<IResourceRepository>();
        _resourceService = new ResourceService(_resourceRepo);
        _sut = new ChapterService(_chapterRepo, _resourceService);
    }

    [Fact]
    public async Task StartAsync_NoActiveSession_CreatesNewSession()
    {
        _chapterRepo.GetActiveSessionAsync(_accountId).Returns((ChapterSession?)null);
        _chapterRepo.GetProgressAsync(_accountId).Returns((ChapterProgress?)null);
        SetupBalance("STAMINA", 100);

        var result = await _sut.StartAsync(_accountId);

        Assert.True(result.Success);
        Assert.Equal(1, result.ChapterId);
        Assert.True(result.Seed >= 0);
        await _chapterRepo.Received(1).CreateSessionAsync(Arg.Any<ChapterSession>());
    }

    [Fact]
    public async Task StartAsync_ActiveSessionExists_Fails()
    {
        _chapterRepo.GetActiveSessionAsync(_accountId).Returns(new ChapterSession
        {
            AccountId = _accountId, IsActive = true
        });

        var result = await _sut.StartAsync(_accountId);

        Assert.False(result.Success);
        Assert.Equal("SESSION_ALREADY_ACTIVE", result.Error);
    }

    [Fact]
    public async Task StartAsync_InsufficientStamina_Fails()
    {
        _chapterRepo.GetActiveSessionAsync(_accountId).Returns((ChapterSession?)null);
        SetupBalance("STAMINA", 2);

        var result = await _sut.StartAsync(_accountId);

        Assert.False(result.Success);
        Assert.Equal("INSUFFICIENT_STAMINA", result.Error);
    }

    [Fact]
    public async Task StartAsync_WithProgress_UsesNextChapter()
    {
        _chapterRepo.GetActiveSessionAsync(_accountId).Returns((ChapterSession?)null);
        _chapterRepo.GetProgressAsync(_accountId).Returns(new ChapterProgress
        {
            AccountId = _accountId, ClearedChapterMax = 5
        });
        SetupBalance("STAMINA", 100);

        var result = await _sut.StartAsync(_accountId);

        Assert.True(result.Success);
        Assert.Equal(6, result.ChapterId);
    }

    [Fact]
    public async Task GenerateEncounterAsync_NoSession_Fails()
    {
        _chapterRepo.GetActiveSessionAsync(_accountId).Returns((ChapterSession?)null);

        var result = await _sut.GenerateEncounterAsync(_accountId);

        Assert.False(result.Success);
        Assert.Equal("NO_ACTIVE_SESSION", result.Error);
    }

    [Fact]
    public async Task GenerateEncounterAsync_ForcedBattleDay_ReturnsCombat()
    {
        var session = CreateSession(20);
        _chapterRepo.GetActiveSessionAsync(_accountId).Returns(session);

        var result = await _sut.GenerateEncounterAsync(_accountId);

        Assert.True(result.Success);
        Assert.Equal("COMBAT", result.EncounterType);
        Assert.Equal(20, result.Day);
    }

    [Theory]
    [InlineData(30)]
    [InlineData(40)]
    [InlineData(50)]
    [InlineData(60)]
    public async Task GenerateEncounterAsync_AllForcedBattleDays_ReturnCombat(int day)
    {
        var session = CreateSession(day);
        _chapterRepo.GetActiveSessionAsync(_accountId).Returns(session);

        var result = await _sut.GenerateEncounterAsync(_accountId);

        Assert.Equal("COMBAT", result.EncounterType);
    }

    [Fact]
    public async Task ResolveEncounterAsync_AdvancesDay()
    {
        var session = CreateSession(5);
        _chapterRepo.GetActiveSessionAsync(_accountId).Returns(session);

        var result = await _sut.ResolveEncounterAsync(_accountId, "FIGHT");

        Assert.True(result.Success);
        Assert.Equal(6, result.NewDay);
        Assert.False(result.IsChapterComplete);
    }

    [Fact]
    public async Task ResolveEncounterAsync_Day60_CompletesChapter()
    {
        var session = CreateSession(60);
        _chapterRepo.GetActiveSessionAsync(_accountId).Returns(session);
        _chapterRepo.GetProgressAsync(_accountId).Returns((ChapterProgress?)null);

        var result = await _sut.ResolveEncounterAsync(_accountId, "FIGHT");

        Assert.True(result.IsChapterComplete);
        await _chapterRepo.Received(1).UpsertProgressAsync(Arg.Any<ChapterProgress>());
    }

    [Fact]
    public async Task SelectSkillAsync_ValidIndex_AddsToSession()
    {
        var session = CreateSession(5);
        session.PendingSkillChoices = "[\"skill_1\",\"skill_2\",\"skill_3\"]";
        session.SessionSkills = "[]";
        _chapterRepo.GetActiveSessionAsync(_accountId).Returns(session);

        var result = await _sut.SelectSkillAsync(_accountId, 1);

        Assert.True(result.Success);
        Assert.Equal("skill_2", result.SelectedSkill);
    }

    [Fact]
    public async Task SelectSkillAsync_InvalidIndex_Fails()
    {
        var session = CreateSession(5);
        session.PendingSkillChoices = "[\"skill_1\",\"skill_2\",\"skill_3\"]";
        _chapterRepo.GetActiveSessionAsync(_accountId).Returns(session);

        var result = await _sut.SelectSkillAsync(_accountId, 5);

        Assert.False(result.Success);
        Assert.Equal("INVALID_SKILL_INDEX", result.Error);
    }

    [Fact]
    public async Task RerollSkillsAsync_WithinLimit_Succeeds()
    {
        var session = CreateSession(5);
        session.RerollsUsed = 0;
        _chapterRepo.GetActiveSessionAsync(_accountId).Returns(session);

        var result = await _sut.RerollSkillsAsync(_accountId);

        Assert.True(result.Success);
        Assert.Equal(3, result.NewChoices.Count);
        Assert.Equal(1, result.RerollsRemaining);
    }

    [Fact]
    public async Task RerollSkillsAsync_ExceedsLimit_Fails()
    {
        var session = CreateSession(5);
        session.RerollsUsed = 2;
        _chapterRepo.GetActiveSessionAsync(_accountId).Returns(session);

        var result = await _sut.RerollSkillsAsync(_accountId);

        Assert.False(result.Success);
        Assert.Equal("NO_REROLLS_REMAINING", result.Error);
    }

    [Fact]
    public async Task AbandonAsync_ActiveSession_DeactivatesAndRecordsBest()
    {
        var session = CreateSession(25);
        _chapterRepo.GetActiveSessionAsync(_accountId).Returns(session);
        _chapterRepo.GetProgressAsync(_accountId).Returns((ChapterProgress?)null);

        var result = await _sut.AbandonAsync(_accountId);

        Assert.True(result.Success);
        Assert.False(session.IsActive);
        Assert.Equal(25, session.BestSurvivalDays);
    }

    [Fact]
    public async Task ClaimTreasureAsync_ValidMilestone_GrantsReward()
    {
        _chapterRepo.GetProgressAsync(_accountId).Returns(new ChapterProgress
        {
            AccountId = _accountId, ClearedChapterMax = 3, ClaimedTreasures = "{}"
        });
        _resourceRepo.GetBalanceAsync(_accountId, "GOLD").Returns((ResourceBalance?)null);

        var result = await _sut.ClaimTreasureAsync(_accountId, 1, "day_30");

        Assert.True(result.Success);
    }

    [Fact]
    public async Task ClaimTreasureAsync_AlreadyClaimed_Fails()
    {
        _chapterRepo.GetProgressAsync(_accountId).Returns(new ChapterProgress
        {
            AccountId = _accountId, ClaimedTreasures = "{\"1\":[\"day_30\"]}"
        });

        var result = await _sut.ClaimTreasureAsync(_accountId, 1, "day_30");

        Assert.False(result.Success);
        Assert.Equal("ALREADY_CLAIMED", result.Error);
    }

    private ChapterSession CreateSession(int currentDay)
    {
        return new ChapterSession
        {
            Id = Guid.NewGuid(),
            AccountId = _accountId,
            ChapterId = 1,
            CurrentDay = currentDay,
            Seed = 12345,
            SessionSkills = "[]",
            PendingEncounter = "{}",
            PendingSkillChoices = "[]",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    private void SetupBalance(string type, double amount)
    {
        _resourceRepo.GetBalanceAsync(_accountId, type).Returns(new ResourceBalance
        {
            AccountId = _accountId, Type = type, Amount = amount
        });
    }
}
