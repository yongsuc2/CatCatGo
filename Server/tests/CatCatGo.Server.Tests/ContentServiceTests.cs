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
    public async Task TowerChallengeAsync_WithToken_ReturnsResult()
    {
        SetupBalance("CHALLENGE_TOKEN", 5);
        _contentRepo.GetProgressAsync(_accountId, "TOWER").Returns((ContentProgress?)null);

        var result = await _sut.TowerChallengeAsync(_accountId);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Delta);
        Assert.NotNull(result.Delta!.Tower);
    }

    [Fact]
    public async Task TowerChallengeAsync_NoToken_Fails()
    {
        SetupBalance("CHALLENGE_TOKEN", 0);

        var result = await _sut.TowerChallengeAsync(_accountId);

        Assert.False(result.Success);
        Assert.Equal("INSUFFICIENT_CHALLENGE_TOKEN", result.ErrorCode);
    }

    [Fact]
    public async Task DungeonChallengeAsync_FirstEntry_Succeeds()
    {
        _contentRepo.GetProgressAsync(_accountId, "DUNGEON_SHARED").Returns((ContentProgress?)null);
        _contentRepo.GetProgressAsync(_accountId, "DUNGEON_BEEHIVE").Returns((ContentProgress?)null);

        var result = await _sut.DungeonChallengeAsync(_accountId, "BEEHIVE");

        Assert.True(result.Success);
        Assert.NotNull(result.Delta);
        Assert.NotNull(result.Delta!.Dungeons);
    }

    [Fact]
    public async Task DungeonChallengeAsync_SharedLimitReached_Fails()
    {
        _contentRepo.GetProgressAsync(_accountId, "DUNGEON_SHARED").Returns(new ContentProgress
        {
            AccountId = _accountId, ContentType = "DUNGEON_SHARED",
            DailyRunsUsed = 3, LastResetDate = DateTime.UtcNow.Date
        });

        var result = await _sut.DungeonChallengeAsync(_accountId, "TIGER_CLIFF");

        Assert.False(result.Success);
        Assert.Equal("DAILY_LIMIT_REACHED", result.ErrorCode);
    }

    [Fact]
    public async Task DungeonSweepAsync_WithClearRecord_Succeeds()
    {
        _contentRepo.GetProgressAsync(_accountId, "DUNGEON_SHARED").Returns(new ContentProgress
        {
            AccountId = _accountId, ContentType = "DUNGEON_SHARED",
            DailyRunsUsed = 0, LastResetDate = DateTime.UtcNow.Date
        });
        _contentRepo.GetProgressAsync(_accountId, "DUNGEON_BEEHIVE").Returns(new ContentProgress
        {
            AccountId = _accountId, ContentType = "DUNGEON_BEEHIVE", HighestStage = 5
        });
        _resourceRepo.GetBalanceAsync(_accountId, "STAMINA").Returns((ResourceBalance?)null);

        var result = await _sut.DungeonSweepAsync(_accountId, "BEEHIVE");

        Assert.True(result.Success);
        Assert.NotNull(result.Delta);
    }

    [Fact]
    public async Task DungeonSweepAsync_NoClearRecord_Fails()
    {
        _contentRepo.GetProgressAsync(_accountId, "DUNGEON_SHARED").Returns(new ContentProgress
        {
            AccountId = _accountId, ContentType = "DUNGEON_SHARED",
            DailyRunsUsed = 0, LastResetDate = DateTime.UtcNow.Date
        });
        _contentRepo.GetProgressAsync(_accountId, "DUNGEON_BEEHIVE").Returns(new ContentProgress
        {
            AccountId = _accountId, ContentType = "DUNGEON_BEEHIVE", HighestStage = 0
        });

        var result = await _sut.DungeonSweepAsync(_accountId, "BEEHIVE");

        Assert.False(result.Success);
        Assert.Equal("NO_CLEAR_RECORD", result.ErrorCode);
    }

    [Fact]
    public async Task GoblinMineAsync_WithPickaxe_ReturnsOreGained()
    {
        SetupBalance("PICKAXE", 3);
        _contentRepo.GetProgressAsync(_accountId, "GOBLIN").Returns((ContentProgress?)null);

        var result = await _sut.GoblinMineAsync(_accountId);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data!.OreGained > 0);
        Assert.NotNull(result.Delta!.GoblinOreCount);
    }

    [Fact]
    public async Task GoblinMineAsync_NoPickaxe_Fails()
    {
        SetupBalance("PICKAXE", 0);

        var result = await _sut.GoblinMineAsync(_accountId);

        Assert.False(result.Success);
        Assert.Equal("INSUFFICIENT_PICKAXE", result.ErrorCode);
    }

    [Fact]
    public async Task GoblinCartAsync_SufficientOre_ReturnsReward()
    {
        _contentRepo.GetProgressAsync(_accountId, "GOBLIN").Returns(new ContentProgress
        {
            AccountId = _accountId, ContentType = "GOBLIN", HighestStage = 50
        });
        _resourceRepo.GetBalanceAsync(_accountId, "GOLD").Returns((ResourceBalance?)null);

        var result = await _sut.GoblinCartAsync(_accountId);

        Assert.True(result.Success);
        Assert.Equal(0, result.Delta!.GoblinOreCount);
    }

    [Fact]
    public async Task GoblinCartAsync_InsufficientOre_Fails()
    {
        _contentRepo.GetProgressAsync(_accountId, "GOBLIN").Returns(new ContentProgress
        {
            AccountId = _accountId, ContentType = "GOBLIN", HighestStage = 10
        });

        var result = await _sut.GoblinCartAsync(_accountId);

        Assert.False(result.Success);
        Assert.Equal("INSUFFICIENT_ORE", result.ErrorCode);
    }

    [Fact]
    public async Task CatacombStartAsync_NoActiveRun_Succeeds()
    {
        _contentRepo.GetProgressAsync(_accountId, "CATACOMB").Returns(new ContentProgress
        {
            AccountId = _accountId, ContentType = "CATACOMB", DailyRunsUsed = 0, HighestStage = 3
        });

        var result = await _sut.CatacombStartAsync(_accountId);

        Assert.True(result.Success);
        Assert.True(result.Delta!.Catacomb!.IsRunning!.Value);
    }

    [Fact]
    public async Task CatacombStartAsync_ActiveRun_Fails()
    {
        _contentRepo.GetProgressAsync(_accountId, "CATACOMB").Returns(new ContentProgress
        {
            AccountId = _accountId, ContentType = "CATACOMB", DailyRunsUsed = 1
        });

        var result = await _sut.CatacombStartAsync(_accountId);

        Assert.False(result.Success);
        Assert.Equal("RUN_ALREADY_ACTIVE", result.ErrorCode);
    }

    private void SetupBalance(string type, double amount)
    {
        _resourceRepo.GetBalanceAsync(_accountId, type).Returns(new ResourceBalance
        {
            AccountId = _accountId, Type = type, Amount = amount
        });
    }
}
