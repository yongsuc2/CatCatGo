using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using CatCatGo.Server.Core.Services;
using NSubstitute;
using Xunit;

namespace CatCatGo.Server.Tests;

public class ResourceServiceTests
{
    private readonly IResourceRepository _resourceRepo;
    private readonly ResourceService _sut;

    public ResourceServiceTests()
    {
        _resourceRepo = Substitute.For<IResourceRepository>();
        _sut = new ResourceService(_resourceRepo);
    }

    [Fact]
    public async Task GetBalanceAsync_ExistingBalance_ReturnsAmount()
    {
        var accountId = Guid.NewGuid();
        _resourceRepo.GetBalanceAsync(accountId, "GOLD").Returns(new ResourceBalance
        {
            AccountId = accountId, Type = "GOLD", Amount = 5000
        });

        var result = await _sut.GetBalanceAsync(accountId, "GOLD");

        Assert.Equal(5000, result);
    }

    [Fact]
    public async Task GetBalanceAsync_NoBalance_ReturnsZero()
    {
        var accountId = Guid.NewGuid();
        _resourceRepo.GetBalanceAsync(accountId, "GOLD").Returns((ResourceBalance?)null);

        var result = await _sut.GetBalanceAsync(accountId, "GOLD");

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task GetAllBalancesAsync_ReturnsAllTypes()
    {
        var accountId = Guid.NewGuid();
        _resourceRepo.GetAllBalancesAsync(accountId).Returns(new List<ResourceBalance>
        {
            new() { AccountId = accountId, Type = "GOLD", Amount = 1000 },
            new() { AccountId = accountId, Type = "GEMS", Amount = 200 },
        });

        var result = await _sut.GetAllBalancesAsync(accountId);

        Assert.Equal(2, result.Count);
        Assert.Equal(1000, result["GOLD"]);
        Assert.Equal(200, result["GEMS"]);
    }

    [Fact]
    public async Task SpendAsync_SufficientBalance_ReturnsTrue()
    {
        var accountId = Guid.NewGuid();
        _resourceRepo.GetBalanceAsync(accountId, "GOLD").Returns(new ResourceBalance
        {
            AccountId = accountId, Type = "GOLD", Amount = 500
        });

        var result = await _sut.SpendAsync(accountId, "GOLD", 300, "TEST");

        Assert.True(result);
        await _resourceRepo.Received(1).UpsertBalanceAsync(Arg.Is<ResourceBalance>(b =>
            b.Type == "GOLD" && b.Amount == 200));
        await _resourceRepo.Received(1).AddLedgerEntryAsync(Arg.Is<ResourceLedger>(l =>
            l.Delta == -300 && l.Balance == 200 && l.Source == "TEST"));
    }

    [Fact]
    public async Task SpendAsync_InsufficientBalance_ReturnsFalse()
    {
        var accountId = Guid.NewGuid();
        _resourceRepo.GetBalanceAsync(accountId, "GOLD").Returns(new ResourceBalance
        {
            AccountId = accountId, Type = "GOLD", Amount = 100
        });

        var result = await _sut.SpendAsync(accountId, "GOLD", 500, "TEST");

        Assert.False(result);
        await _resourceRepo.DidNotReceive().UpsertBalanceAsync(Arg.Any<ResourceBalance>());
    }

    [Fact]
    public async Task SpendAsync_ZeroAmount_ReturnsFalse()
    {
        var accountId = Guid.NewGuid();

        var result = await _sut.SpendAsync(accountId, "GOLD", 0, "TEST");

        Assert.False(result);
    }

    [Fact]
    public async Task SpendAsync_NegativeAmount_ReturnsFalse()
    {
        var accountId = Guid.NewGuid();

        var result = await _sut.SpendAsync(accountId, "GOLD", -100, "TEST");

        Assert.False(result);
    }

    [Fact]
    public async Task SpendAsync_ExactBalance_ReturnsTrue()
    {
        var accountId = Guid.NewGuid();
        _resourceRepo.GetBalanceAsync(accountId, "GEMS").Returns(new ResourceBalance
        {
            AccountId = accountId, Type = "GEMS", Amount = 300
        });

        var result = await _sut.SpendAsync(accountId, "GEMS", 300, "GACHA");

        Assert.True(result);
        await _resourceRepo.Received(1).UpsertBalanceAsync(Arg.Is<ResourceBalance>(b =>
            b.Amount == 0));
    }

    [Fact]
    public async Task GrantAsync_AddsToExistingBalance()
    {
        var accountId = Guid.NewGuid();
        _resourceRepo.GetBalanceAsync(accountId, "GOLD").Returns(new ResourceBalance
        {
            AccountId = accountId, Type = "GOLD", Amount = 1000
        });

        await _sut.GrantAsync(accountId, "GOLD", 500, "BATTLE_REWARD");

        await _resourceRepo.Received(1).UpsertBalanceAsync(Arg.Is<ResourceBalance>(b =>
            b.Amount == 1500));
        await _resourceRepo.Received(1).AddLedgerEntryAsync(Arg.Is<ResourceLedger>(l =>
            l.Delta == 500 && l.Balance == 1500 && l.Source == "BATTLE_REWARD"));
    }

    [Fact]
    public async Task GrantAsync_NewResourceType_CreatesFromZero()
    {
        var accountId = Guid.NewGuid();
        _resourceRepo.GetBalanceAsync(accountId, "PET_FOOD").Returns((ResourceBalance?)null);

        await _sut.GrantAsync(accountId, "PET_FOOD", 10, "GACHA_BONUS");

        await _resourceRepo.Received(1).UpsertBalanceAsync(Arg.Is<ResourceBalance>(b =>
            b.Amount == 10 && b.Type == "PET_FOOD"));
    }

    [Fact]
    public async Task GrantAsync_ZeroAmount_DoesNothing()
    {
        var accountId = Guid.NewGuid();

        await _sut.GrantAsync(accountId, "GOLD", 0, "TEST");

        await _resourceRepo.DidNotReceive().UpsertBalanceAsync(Arg.Any<ResourceBalance>());
    }

    [Fact]
    public async Task SpendMultipleAsync_AllSufficient_ReturnsTrue()
    {
        var accountId = Guid.NewGuid();
        _resourceRepo.GetBalanceAsync(accountId, "GOLD").Returns(new ResourceBalance
        {
            AccountId = accountId, Type = "GOLD", Amount = 1000
        });
        _resourceRepo.GetBalanceAsync(accountId, "GEMS").Returns(new ResourceBalance
        {
            AccountId = accountId, Type = "GEMS", Amount = 500
        });

        var costs = new Dictionary<string, double> { { "GOLD", 500 }, { "GEMS", 200 } };
        var result = await _sut.SpendMultipleAsync(accountId, costs, "FORGE");

        Assert.True(result);
    }

    [Fact]
    public async Task SpendMultipleAsync_OneInsufficient_ReturnsFalse()
    {
        var accountId = Guid.NewGuid();
        _resourceRepo.GetBalanceAsync(accountId, "GOLD").Returns(new ResourceBalance
        {
            AccountId = accountId, Type = "GOLD", Amount = 1000
        });
        _resourceRepo.GetBalanceAsync(accountId, "GEMS").Returns(new ResourceBalance
        {
            AccountId = accountId, Type = "GEMS", Amount = 10
        });

        var costs = new Dictionary<string, double> { { "GOLD", 500 }, { "GEMS", 200 } };
        var result = await _sut.SpendMultipleAsync(accountId, costs, "FORGE");

        Assert.False(result);
    }
}
