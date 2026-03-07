using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using CatCatGo.Server.Core.Services;
using NSubstitute;
using Xunit;

namespace CatCatGo.Server.Tests;

public class HeritageServiceTests
{
    private readonly IHeritageRepository _heritageRepo;
    private readonly ITalentRepository _talentRepo;
    private readonly IResourceRepository _resourceRepo;
    private readonly ResourceService _resourceService;
    private readonly HeritageService _sut;

    private readonly Guid _accountId = Guid.NewGuid();

    public HeritageServiceTests()
    {
        _heritageRepo = Substitute.For<IHeritageRepository>();
        _talentRepo = Substitute.For<ITalentRepository>();
        _resourceRepo = Substitute.For<IResourceRepository>();
        _resourceService = new ResourceService(_resourceRepo);
        _sut = new HeritageService(_heritageRepo, _talentRepo, _resourceService);
    }

    [Fact]
    public async Task GetStatusAsync_HeroGrade_ReturnsUnlocked()
    {
        _heritageRepo.GetByAccountIdAsync(_accountId).Returns((HeritageState?)null);
        _talentRepo.GetByAccountIdAsync(_accountId).Returns(new TalentState
        {
            AccountId = _accountId, Grade = "HERO"
        });

        var result = await _sut.GetStatusAsync(_accountId);

        Assert.True(result.IsUnlocked);
    }

    [Fact]
    public async Task GetStatusAsync_NonHeroGrade_ReturnsLocked()
    {
        _heritageRepo.GetByAccountIdAsync(_accountId).Returns((HeritageState?)null);
        _talentRepo.GetByAccountIdAsync(_accountId).Returns(new TalentState
        {
            AccountId = _accountId, Grade = "WARRIOR"
        });

        var result = await _sut.GetStatusAsync(_accountId);

        Assert.False(result.IsUnlocked);
    }

    [Fact]
    public async Task UpgradeAsync_HeroWithResources_IncreasesLevel()
    {
        _talentRepo.GetByAccountIdAsync(_accountId).Returns(new TalentState
        {
            AccountId = _accountId, Grade = "HERO"
        });
        _heritageRepo.GetByAccountIdAsync(_accountId).Returns(new HeritageState
        {
            AccountId = _accountId, SkullLevel = 0
        });
        SetupBalance("SKULL_BOOK", 10);
        SetupBalance("GOLD", 10000);

        var result = await _sut.UpgradeAsync(_accountId, "SKULL");

        Assert.True(result.Success);
        Assert.Equal(1, result.State!.SkullLevel);
    }

    [Fact]
    public async Task UpgradeAsync_NotHero_Fails()
    {
        _talentRepo.GetByAccountIdAsync(_accountId).Returns(new TalentState
        {
            AccountId = _accountId, Grade = "WARRIOR"
        });

        var result = await _sut.UpgradeAsync(_accountId, "SKULL");

        Assert.False(result.Success);
        Assert.Equal("HERITAGE_LOCKED", result.Error);
    }

    [Fact]
    public async Task UpgradeAsync_InvalidRoute_Fails()
    {
        var result = await _sut.UpgradeAsync(_accountId, "INVALID");

        Assert.False(result.Success);
        Assert.Equal("INVALID_ROUTE", result.Error);
    }

    [Fact]
    public async Task UpgradeAsync_InsufficientBook_Fails()
    {
        _talentRepo.GetByAccountIdAsync(_accountId).Returns(new TalentState
        {
            AccountId = _accountId, Grade = "HERO"
        });
        _heritageRepo.GetByAccountIdAsync(_accountId).Returns((HeritageState?)null);
        SetupBalance("KNIGHT_BOOK", 0);

        var result = await _sut.UpgradeAsync(_accountId, "KNIGHT");

        Assert.False(result.Success);
        Assert.Contains("INSUFFICIENT", result.Error);
    }

    [Fact]
    public async Task UpgradeAsync_InsufficientGold_RefundsBook()
    {
        _talentRepo.GetByAccountIdAsync(_accountId).Returns(new TalentState
        {
            AccountId = _accountId, Grade = "HERO"
        });
        _heritageRepo.GetByAccountIdAsync(_accountId).Returns(new HeritageState
        {
            AccountId = _accountId, RangerLevel = 0
        });
        SetupBalance("RANGER_BOOK", 10);
        SetupBalance("GOLD", 0);

        var result = await _sut.UpgradeAsync(_accountId, "RANGER");

        Assert.False(result.Success);
        Assert.Equal("INSUFFICIENT_GOLD", result.Error);
        await _resourceRepo.Received().UpsertBalanceAsync(Arg.Is<ResourceBalance>(b =>
            b.Type == "RANGER_BOOK"));
    }

    [Theory]
    [InlineData("SKULL")]
    [InlineData("KNIGHT")]
    [InlineData("RANGER")]
    [InlineData("GHOST")]
    public async Task UpgradeAsync_AllRoutes_Valid(string route)
    {
        _talentRepo.GetByAccountIdAsync(_accountId).Returns(new TalentState
        {
            AccountId = _accountId, Grade = "HERO"
        });
        _heritageRepo.GetByAccountIdAsync(_accountId).Returns(new HeritageState
        {
            AccountId = _accountId
        });
        SetupBalance($"{route}_BOOK", 10);
        SetupBalance("GOLD", 100000);

        var result = await _sut.UpgradeAsync(_accountId, route);

        Assert.True(result.Success);
    }

    private void SetupBalance(string type, double amount)
    {
        _resourceRepo.GetBalanceAsync(_accountId, type).Returns(new ResourceBalance
        {
            AccountId = _accountId, Type = type, Amount = amount
        });
    }
}
