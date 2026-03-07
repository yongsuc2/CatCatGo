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

        var result = await _sut.StartAsync(_accountId, 1, "SIXTY_DAY");

        Assert.True(result.Success);
        Assert.Equal(1, result.Data!.ChapterId);
        Assert.True(result.Data.Seed >= 0);
        await _chapterRepo.Received(1).CreateSessionAsync(Arg.Any<ChapterSession>());
    }

    [Fact]
    public async Task StartAsync_ActiveSessionExists_Fails()
    {
        _chapterRepo.GetActiveSessionAsync(_accountId).Returns(new ChapterSession
        {
            AccountId = _accountId, IsActive = true
        });

        var result = await _sut.StartAsync(_accountId, 1, "SIXTY_DAY");

        Assert.False(result.Success);
        Assert.Equal("SESSION_ALREADY_ACTIVE", result.ErrorCode);
    }

    [Fact]
    public async Task StartAsync_InsufficientStamina_Fails()
    {
        _chapterRepo.GetActiveSessionAsync(_accountId).Returns((ChapterSession?)null);
        _chapterRepo.GetProgressAsync(_accountId).Returns((ChapterProgress?)null);
        SetupBalance("STAMINA", 2);

        var result = await _sut.StartAsync(_accountId, 1, "SIXTY_DAY");

        Assert.False(result.Success);
        Assert.Equal("INSUFFICIENT_STAMINA", result.ErrorCode);
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

        var result = await _sut.StartAsync(_accountId, 6, "SIXTY_DAY");

        Assert.True(result.Success);
        Assert.Equal(6, result.Data!.ChapterId);
    }

    [Fact]
    public async Task AdvanceDayAsync_NoSession_Fails()
    {
        _chapterRepo.GetActiveSessionAsync(_accountId).Returns((ChapterSession?)null);

        var result = await _sut.AdvanceDayAsync(_accountId, "invalid-session");

        Assert.False(result.Success);
        Assert.Equal("NO_ACTIVE_SESSION", result.ErrorCode);
    }

    [Fact]
    public async Task ResolveEncounterAsync_AdvancesDay()
    {
        var session = CreateSession(5);
        _chapterRepo.GetActiveSessionAsync(_accountId).Returns(session);

        var result = await _sut.ResolveEncounterAsync(_accountId, session.Id.ToString(), 0);

        Assert.True(result.Success);
        Assert.NotNull(result.Delta);
        Assert.Equal(6, result.Delta!.ChapterSession!.CurrentDay);
    }

    [Fact]
    public async Task ResolveEncounterAsync_Day60_CompletesChapter()
    {
        var session = CreateSession(60);
        _chapterRepo.GetActiveSessionAsync(_accountId).Returns(session);
        _chapterRepo.GetProgressAsync(_accountId).Returns((ChapterProgress?)null);

        var result = await _sut.ResolveEncounterAsync(_accountId, session.Id.ToString(), 0);

        Assert.True(result.Delta!.ChapterSession!.SessionEnded!.Value);
        await _chapterRepo.Received(1).UpsertProgressAsync(Arg.Any<ChapterProgress>());
    }

    [Fact]
    public async Task SelectSkillAsync_ValidSkill_AddsToSession()
    {
        var session = CreateSession(5);
        session.PendingSkillChoices = "[\"skill_1\",\"skill_2\",\"skill_3\"]";
        session.SessionSkills = "[]";
        _chapterRepo.GetActiveSessionAsync(_accountId).Returns(session);

        var result = await _sut.SelectSkillAsync(_accountId, session.Id.ToString(), "skill_2");

        Assert.True(result.Success);
        Assert.Contains("skill_2", result.Delta!.ChapterSession!.SessionSkillIds!);
    }

    [Fact]
    public async Task SelectSkillAsync_InvalidSkillId_Fails()
    {
        var session = CreateSession(5);
        session.PendingSkillChoices = "[\"skill_1\",\"skill_2\",\"skill_3\"]";
        _chapterRepo.GetActiveSessionAsync(_accountId).Returns(session);

        var result = await _sut.SelectSkillAsync(_accountId, session.Id.ToString(), "skill_999");

        Assert.False(result.Success);
        Assert.Equal("INVALID_SKILL_ID", result.ErrorCode);
    }

    [Fact]
    public async Task RerollAsync_WithinLimit_Succeeds()
    {
        var session = CreateSession(5);
        session.RerollsUsed = 0;
        _chapterRepo.GetActiveSessionAsync(_accountId).Returns(session);

        var result = await _sut.RerollAsync(_accountId, session.Id.ToString());

        Assert.True(result.Success);
        Assert.Equal(1, result.Delta!.ChapterSession!.SessionRerollsRemaining);
    }

    [Fact]
    public async Task RerollAsync_ExceedsLimit_Fails()
    {
        var session = CreateSession(5);
        session.RerollsUsed = 2;
        _chapterRepo.GetActiveSessionAsync(_accountId).Returns(session);

        var result = await _sut.RerollAsync(_accountId, session.Id.ToString());

        Assert.False(result.Success);
        Assert.Equal("NO_REROLLS_REMAINING", result.ErrorCode);
    }

    [Fact]
    public async Task AbandonAsync_ActiveSession_DeactivatesAndRecordsBest()
    {
        var session = CreateSession(25);
        _chapterRepo.GetActiveSessionAsync(_accountId).Returns(session);
        _chapterRepo.GetProgressAsync(_accountId).Returns((ChapterProgress?)null);

        var result = await _sut.AbandonAsync(_accountId, session.Id.ToString());

        Assert.True(result.Success);
        Assert.False(session.IsActive);
        Assert.Equal(25, session.BestSurvivalDays);
        Assert.True(result.Delta!.ChapterSession!.SessionEnded!.Value);
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
