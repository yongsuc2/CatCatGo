using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using CatCatGo.Server.Core.Services;
using NSubstitute;
using Xunit;

namespace CatCatGo.Server.Tests;

public class GachaServiceTests
{
    private readonly IGachaRepository _gachaRepo;
    private readonly IEquipmentRepository _equipmentRepo;
    private readonly IResourceRepository _resourceRepo;
    private readonly ResourceService _resourceService;
    private readonly GachaService _sut;

    private readonly Guid _accountId = Guid.NewGuid();

    public GachaServiceTests()
    {
        _gachaRepo = Substitute.For<IGachaRepository>();
        _equipmentRepo = Substitute.For<IEquipmentRepository>();
        _resourceRepo = Substitute.For<IResourceRepository>();
        _resourceService = new ResourceService(_resourceRepo);
        _sut = new GachaService(_gachaRepo, _equipmentRepo, _resourceService);
    }

    [Fact]
    public async Task PullAsync_SufficientGems_ReturnsEquipment()
    {
        SetupBalance("GEMS", 1000);
        _gachaRepo.GetPityAsync(_accountId, "EQUIPMENT").Returns((GachaPity?)null);

        var result = await _sut.PullAsync(_accountId);

        Assert.True(result.Success);
        Assert.Single(result.Data!.Results);
        Assert.NotNull(result.Data.Results[0].Grade);
        await _equipmentRepo.Received(1).CreateAsync(Arg.Any<EquipmentEntry>());
    }

    [Fact]
    public async Task PullAsync_InsufficientGems_Fails()
    {
        SetupBalance("GEMS", 100);

        var result = await _sut.PullAsync(_accountId);

        Assert.False(result.Success);
        Assert.Equal("INSUFFICIENT_GEMS", result.ErrorCode);
    }

    [Fact]
    public async Task Pull10Async_SufficientGems_Returns10Items()
    {
        SetupBalance("GEMS", 5000);
        _gachaRepo.GetPityAsync(_accountId, "EQUIPMENT").Returns((GachaPity?)null);

        var result = await _sut.Pull10Async(_accountId);

        Assert.True(result.Success);
        Assert.Equal(10, result.Data!.Results.Count);
    }

    [Fact]
    public async Task Pull10Async_InsufficientGems_Fails()
    {
        SetupBalance("GEMS", 2000);

        var result = await _sut.Pull10Async(_accountId);

        Assert.False(result.Success);
        Assert.Equal("INSUFFICIENT_GEMS", result.ErrorCode);
    }

    [Fact]
    public async Task PullAsync_PityThresholdReached_GuaranteesMythic()
    {
        SetupBalance("GEMS", 10000);
        var pity = new GachaPity
        {
            AccountId = _accountId, BoxType = "EQUIPMENT", PityCount = 179
        };
        _gachaRepo.GetPityAsync(_accountId, "EQUIPMENT").Returns(pity);

        var result = await _sut.PullAsync(_accountId);

        Assert.True(result.Success);
        Assert.Equal("MYTHIC", result.Data!.Results[0].Grade);
    }

    [Fact]
    public async Task PullAsync_MythicDrop_ResetsPityCounter()
    {
        SetupBalance("GEMS", 10000);
        var pity = new GachaPity
        {
            AccountId = _accountId, BoxType = "EQUIPMENT", PityCount = 179
        };
        _gachaRepo.GetPityAsync(_accountId, "EQUIPMENT").Returns(pity);

        await _sut.PullAsync(_accountId);

        await _gachaRepo.Received().UpsertPityAsync(Arg.Is<GachaPity>(p => p.PityCount == 0));
    }

    [Fact]
    public async Task GetPityAsync_ReturnsCurrentCountAndThreshold()
    {
        var pity = new GachaPity
        {
            AccountId = _accountId, BoxType = "EQUIPMENT", PityCount = 50
        };
        _gachaRepo.GetPityAsync(_accountId, "EQUIPMENT").Returns(pity);

        var result = await _sut.GetPityAsync(_accountId);

        Assert.Equal(50, result.PityCount);
        Assert.Equal(180, result.Threshold);
    }

    private void SetupBalance(string type, double amount)
    {
        _resourceRepo.GetBalanceAsync(_accountId, type).Returns(new ResourceBalance
        {
            AccountId = _accountId, Type = type, Amount = amount
        });
    }
}
