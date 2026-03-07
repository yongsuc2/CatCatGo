using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using CatCatGo.Server.Core.Services;
using NSubstitute;
using Xunit;

namespace CatCatGo.Server.Tests;

public class ContentServiceTests
{
    private readonly IContentRepository _contentRepo;
    private readonly IResourceRepository _resourceRepo;
    private readonly ResourceService _resourceService;
    private readonly ContentService _sut;

    private readonly Guid _accountId = Guid.NewGuid();

    public ContentServiceTests()
    {
        _contentRepo = Substitute.For<IContentRepository>();
        _resourceRepo = Substitute.For<IResourceRepository>();
        _resourceService = new ResourceService(_resourceRepo);
        _sut = new ContentService(_contentRepo, _resourceService);
    }

    [Fact]
    public async Task TowerChallengeAsync_WithToken_IncreasesStage()
    {
        SetupBalance("CHALLENGE_TOKEN", 5);
        _contentRepo.GetProgressAsync(_accountId, "TOWER").Returns((ContentProgress?)null);

        var result = await _sut.TowerChallengeAsync(_accountId);

        Assert.True(result.Success);
        Assert.Equal(1, result.Stage);
    }

    [Fact]
    public async Task TowerChallengeAsync_NoToken_Fails()
    {
        SetupBalance("CHALLENGE_TOKEN", 0);

        var result = await _sut.TowerChallengeAsync(_accountId);

        Assert.False(result.Success);
        Assert.Equal("INSUFFICIENT_CHALLENGE_TOKEN", result.Error);
    }

    [Fact]
    public async Task TowerChallengeAsync_EveryFifthFloor_GrantsPowerStone()
    {
        SetupBalance("CHALLENGE_TOKEN", 5);
        _contentRepo.GetProgressAsync(_accountId, "TOWER").Returns(new ContentProgress
        {
            AccountId = _accountId, ContentType = "TOWER", HighestStage = 4
        });
        _resourceRepo.GetBalanceAsync(_accountId, "POWER_STONE").Returns((ResourceBalance?)null);

        var result = await _sut.TowerChallengeAsync(_accountId);

        Assert.Equal(5, result.Stage);
        await _resourceRepo.Received(1).UpsertBalanceAsync(Arg.Is<ResourceBalance>(b =>
            b.Type == "POWER_STONE"));
    }

    [Fact]
    public async Task DungeonEnterAsync_FirstEntry_Succeeds()
    {
        _contentRepo.GetProgressAsync(_accountId, "DUNGEON_SHARED").Returns((ContentProgress?)null);
        _contentRepo.GetProgressAsync(_accountId, "DUNGEON_BEEHIVE").Returns((ContentProgress?)null);

        var result = await _sut.DungeonEnterAsync(_accountId, "BEEHIVE");

        Assert.True(result.Success);
        Assert.Equal(2, result.RunsRemaining);
    }

    [Fact]
    public async Task DungeonEnterAsync_SharedLimitReached_Fails()
    {
        _contentRepo.GetProgressAsync(_accountId, "DUNGEON_SHARED").Returns(new ContentProgress
        {
            AccountId = _accountId, ContentType = "DUNGEON_SHARED",
            DailyRunsUsed = 3, LastResetDate = DateTime.UtcNow.Date
        });

        var result = await _sut.DungeonEnterAsync(_accountId, "TIGER_CLIFF");

        Assert.False(result.Success);
        Assert.Equal("DAILY_LIMIT_REACHED", result.Error);
    }

    [Fact]
    public async Task DungeonEnterAsync_NewDay_ResetsCounter()
    {
        _contentRepo.GetProgressAsync(_accountId, "DUNGEON_SHARED").Returns(new ContentProgress
        {
            AccountId = _accountId, ContentType = "DUNGEON_SHARED",
            DailyRunsUsed = 3, LastResetDate = DateTime.UtcNow.Date.AddDays(-1)
        });
        _contentRepo.GetProgressAsync(_accountId, "DUNGEON_BEEHIVE").Returns((ContentProgress?)null);

        var result = await _sut.DungeonEnterAsync(_accountId, "BEEHIVE");

        Assert.True(result.Success);
        Assert.Equal(2, result.RunsRemaining);
    }

    [Fact]
    public async Task DungeonResultAsync_Victory_IncreasesStageAndRewards()
    {
        _contentRepo.GetProgressAsync(_accountId, "DUNGEON_BEEHIVE").Returns(new ContentProgress
        {
            AccountId = _accountId, ContentType = "DUNGEON_BEEHIVE", HighestStage = 5
        });
        _resourceRepo.GetBalanceAsync(_accountId, "STAMINA").Returns((ResourceBalance?)null);

        var result = await _sut.DungeonResultAsync(_accountId, "BEEHIVE", true);

        Assert.True(result.Success);
        Assert.Equal(6, result.Stage);
    }

    [Fact]
    public async Task DungeonResultAsync_Defeat_NoStageIncrease()
    {
        var result = await _sut.DungeonResultAsync(_accountId, "BEEHIVE", false);

        Assert.True(result.Success);
        Assert.Equal(0, result.Stage);
    }

    [Fact]
    public async Task TravelStartAsync_SufficientStamina_Succeeds()
    {
        SetupBalance("STAMINA", 50);

        var result = await _sut.TravelStartAsync(_accountId, 10);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task TravelStartAsync_InsufficientStamina_Fails()
    {
        SetupBalance("STAMINA", 5);

        var result = await _sut.TravelStartAsync(_accountId, 10);

        Assert.False(result.Success);
        Assert.Equal("INSUFFICIENT_STAMINA", result.Error);
    }

    [Fact]
    public async Task TravelCompleteAsync_CalculatesGoldReward()
    {
        _resourceRepo.GetBalanceAsync(_accountId, "GOLD").Returns((ResourceBalance?)null);

        var result = await _sut.TravelCompleteAsync(_accountId, 5, 2.0);

        Assert.True(result.Success);
        var expected = 100.0 * (1 + 5 * 0.5) * 2.0;
        Assert.Equal(expected, result.GoldEarned);
    }

    [Fact]
    public async Task GoblinMineAsync_WithPickaxe_GrantsGold()
    {
        SetupBalance("PICKAXE", 3);
        _resourceRepo.GetBalanceAsync(_accountId, "GOLD").Returns((ResourceBalance?)null);

        var result = await _sut.GoblinMineAsync(_accountId);

        Assert.True(result.Success);
        Assert.True(result.GoldEarned > 0);
    }

    [Fact]
    public async Task GoblinMineAsync_NoPickaxe_Fails()
    {
        SetupBalance("PICKAXE", 0);

        var result = await _sut.GoblinMineAsync(_accountId);

        Assert.False(result.Success);
        Assert.Equal("INSUFFICIENT_PICKAXE", result.Error);
    }

    [Fact]
    public async Task CatacombRunAsync_IncreasesStageAndRewards()
    {
        _contentRepo.GetProgressAsync(_accountId, "CATACOMB").Returns(new ContentProgress
        {
            AccountId = _accountId, ContentType = "CATACOMB", HighestStage = 2
        });
        _resourceRepo.GetBalanceAsync(_accountId, "GOLD").Returns((ResourceBalance?)null);

        var result = await _sut.CatacombRunAsync(_accountId);

        Assert.True(result.Success);
        Assert.Equal(3, result.Stage);
        Assert.Equal(600.0, result.GoldEarned);
    }

    private void SetupBalance(string type, double amount)
    {
        _resourceRepo.GetBalanceAsync(_accountId, type).Returns(new ResourceBalance
        {
            AccountId = _accountId, Type = type, Amount = amount
        });
    }
}
