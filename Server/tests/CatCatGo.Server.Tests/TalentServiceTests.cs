using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using CatCatGo.Server.Core.Services;
using NSubstitute;
using Xunit;

namespace CatCatGo.Server.Tests;

public class TalentServiceTests
{
    private readonly ITalentRepository _talentRepo;
    private readonly IResourceRepository _resourceRepo;
    private readonly ResourceService _resourceService;
    private readonly TalentService _sut;

    public TalentServiceTests()
    {
        _talentRepo = Substitute.For<ITalentRepository>();
        _resourceRepo = Substitute.For<IResourceRepository>();
        _resourceService = new ResourceService(_resourceRepo);
        _sut = new TalentService(_talentRepo, _resourceService);
    }

    [Fact]
    public async Task GetStatusAsync_NewAccount_CreatesDefaultState()
    {
        var accountId = Guid.NewGuid();
        _talentRepo.GetByAccountIdAsync(accountId).Returns((TalentState?)null);

        var result = await _sut.GetStatusAsync(accountId);

        Assert.Equal(accountId, result.AccountId);
        Assert.Equal(0, result.AtkLevel);
        Assert.Equal(0, result.HpLevel);
        Assert.Equal(0, result.DefLevel);
        await _talentRepo.Received(1).UpsertAsync(Arg.Any<TalentState>());
    }

    [Fact]
    public async Task UpgradeAsync_SufficientGold_IncreasesLevel()
    {
        var accountId = Guid.NewGuid();
        var state = new TalentState { AccountId = accountId, AtkLevel = 0, HpLevel = 0, DefLevel = 0, TotalLevel = 0, ClaimedMilestones = "[]" };
        _talentRepo.GetByAccountIdAsync(accountId).Returns(state);
        _resourceRepo.GetBalanceAsync(accountId, "GOLD").Returns(new ResourceBalance
        {
            AccountId = accountId, Type = "GOLD", Amount = 10000
        });

        var result = await _sut.UpgradeAsync(accountId, "ATK");

        Assert.True(result.Success);
        Assert.Equal(1, result.State!.AtkLevel);
        Assert.Equal(1, result.State.TotalLevel);
    }

    [Fact]
    public async Task UpgradeAsync_InsufficientGold_ReturnsFalse()
    {
        var accountId = Guid.NewGuid();
        var state = new TalentState { AccountId = accountId, TotalLevel = 0, ClaimedMilestones = "[]" };
        _talentRepo.GetByAccountIdAsync(accountId).Returns(state);
        _resourceRepo.GetBalanceAsync(accountId, "GOLD").Returns(new ResourceBalance
        {
            AccountId = accountId, Type = "GOLD", Amount = 1
        });

        var result = await _sut.UpgradeAsync(accountId, "ATK");

        Assert.False(result.Success);
        Assert.Equal("INSUFFICIENT_GOLD", result.Error);
    }

    [Fact]
    public async Task UpgradeAsync_InvalidStatType_ThrowsArgumentException()
    {
        var accountId = Guid.NewGuid();
        var state = new TalentState { AccountId = accountId, TotalLevel = 0, ClaimedMilestones = "[]" };
        _talentRepo.GetByAccountIdAsync(accountId).Returns(state);

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.UpgradeAsync(accountId, "INVALID"));
    }

    [Fact]
    public async Task ClaimMilestoneAsync_ValidMilestone_Success()
    {
        var accountId = Guid.NewGuid();
        var state = new TalentState { AccountId = accountId, TotalLevel = 20, ClaimedMilestones = "[]" };
        _talentRepo.GetByAccountIdAsync(accountId).Returns(state);
        _resourceRepo.GetBalanceAsync(accountId, "GOLD").Returns((ResourceBalance?)null);

        var result = await _sut.ClaimMilestoneAsync(accountId, 10);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task ClaimMilestoneAsync_LevelNotReached_Fails()
    {
        var accountId = Guid.NewGuid();
        var state = new TalentState { AccountId = accountId, TotalLevel = 5, ClaimedMilestones = "[]" };
        _talentRepo.GetByAccountIdAsync(accountId).Returns(state);

        var result = await _sut.ClaimMilestoneAsync(accountId, 10);

        Assert.False(result.Success);
        Assert.Equal("LEVEL_NOT_REACHED", result.Error);
    }

    [Fact]
    public async Task ClaimMilestoneAsync_AlreadyClaimed_Fails()
    {
        var accountId = Guid.NewGuid();
        var state = new TalentState { AccountId = accountId, TotalLevel = 20, ClaimedMilestones = "[10]" };
        _talentRepo.GetByAccountIdAsync(accountId).Returns(state);

        var result = await _sut.ClaimMilestoneAsync(accountId, 10);

        Assert.False(result.Success);
        Assert.Equal("ALREADY_CLAIMED", result.Error);
    }
}
