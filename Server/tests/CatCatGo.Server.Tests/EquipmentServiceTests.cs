using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using CatCatGo.Server.Core.Services;
using NSubstitute;
using Xunit;

namespace CatCatGo.Server.Tests;

public class EquipmentServiceTests
{
    private readonly IEquipmentRepository _equipmentRepo;
    private readonly IResourceRepository _resourceRepo;
    private readonly ResourceService _resourceService;
    private readonly EquipmentService _sut;

    private readonly Guid _accountId = Guid.NewGuid();

    public EquipmentServiceTests()
    {
        _equipmentRepo = Substitute.For<IEquipmentRepository>();
        _resourceRepo = Substitute.For<IResourceRepository>();
        _resourceService = new ResourceService(_resourceRepo);
        _sut = new EquipmentService(_equipmentRepo, _resourceService);
    }

    [Fact]
    public async Task UpgradeAsync_SufficientResources_ReturnsSuccessWithDelta()
    {
        var equipId = Guid.NewGuid();
        var equip = CreateEquipment(equipId, "COMMON", 0);
        _equipmentRepo.GetByIdAsync(equipId).Returns(equip);
        SetupBalance("EQUIPMENT_STONE", 10);

        var result = await _sut.UpgradeAsync(_accountId, equipId.ToString());

        Assert.True(result.Success);
        Assert.NotNull(result.Delta);
        Assert.NotNull(result.Delta!.UpgradedEquipments);
    }

    [Fact]
    public async Task UpgradeAsync_WrongAccount_Fails()
    {
        var equipId = Guid.NewGuid();
        var equip = CreateEquipment(equipId, "COMMON", 0);
        equip.AccountId = Guid.NewGuid();
        _equipmentRepo.GetByIdAsync(equipId).Returns(equip);

        var result = await _sut.UpgradeAsync(_accountId, equipId.ToString());

        Assert.False(result.Success);
        Assert.Equal("EQUIPMENT_NOT_FOUND", result.ErrorCode);
    }

    [Fact]
    public async Task ForgeAsync_ThreeSameGrade_ProducesNextGrade()
    {
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        foreach (var id in ids)
            _equipmentRepo.GetByIdAsync(id).Returns(CreateEquipment(id, "COMMON", 0));

        var result = await _sut.ForgeAsync(_accountId, ids.Select(id => id.ToString()).ToList());

        Assert.True(result.Success);
        Assert.NotNull(result.Delta);
        Assert.NotNull(result.Delta!.AddedEquipments);
        Assert.Equal("UNCOMMON", result.Delta.AddedEquipments![0].Grade);
    }

    [Fact]
    public async Task ForgeAsync_SGradeEquipment_Blocked()
    {
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var sEquip = CreateEquipment(ids[0], "EPIC", 0);
        sEquip.IsS = true;
        _equipmentRepo.GetByIdAsync(ids[0]).Returns(sEquip);
        _equipmentRepo.GetByIdAsync(ids[1]).Returns(CreateEquipment(ids[1], "EPIC", 0));
        _equipmentRepo.GetByIdAsync(ids[2]).Returns(CreateEquipment(ids[2], "EPIC", 0));

        var result = await _sut.ForgeAsync(_accountId, ids.Select(id => id.ToString()).ToList());

        Assert.False(result.Success);
        Assert.Equal("CANNOT_FORGE_S_GRADE", result.ErrorCode);
    }

    [Fact]
    public async Task ForgeAsync_MixedGrades_Fails()
    {
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        _equipmentRepo.GetByIdAsync(ids[0]).Returns(CreateEquipment(ids[0], "COMMON", 0));
        _equipmentRepo.GetByIdAsync(ids[1]).Returns(CreateEquipment(ids[1], "RARE", 0));
        _equipmentRepo.GetByIdAsync(ids[2]).Returns(CreateEquipment(ids[2], "COMMON", 0));

        var result = await _sut.ForgeAsync(_accountId, ids.Select(id => id.ToString()).ToList());

        Assert.False(result.Success);
        Assert.Equal("GRADE_MISMATCH", result.ErrorCode);
    }

    [Fact]
    public async Task ForgeAsync_EnhancedMaterials_RefundsStones()
    {
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        _equipmentRepo.GetByIdAsync(ids[0]).Returns(CreateEquipment(ids[0], "COMMON", 3));
        _equipmentRepo.GetByIdAsync(ids[1]).Returns(CreateEquipment(ids[1], "COMMON", 2));
        _equipmentRepo.GetByIdAsync(ids[2]).Returns(CreateEquipment(ids[2], "COMMON", 0));
        _resourceRepo.GetBalanceAsync(_accountId, "EQUIPMENT_STONE").Returns((ResourceBalance?)null);

        var result = await _sut.ForgeAsync(_accountId, ids.Select(id => id.ToString()).ToList());

        Assert.True(result.Success);
        await _resourceRepo.Received(1).UpsertBalanceAsync(Arg.Is<ResourceBalance>(b =>
            b.Type == "EQUIPMENT_STONE" && b.Amount == 5));
    }

    [Fact]
    public async Task ForgeAsync_LessThanThreeMaterials_Fails()
    {
        var ids = new List<string> { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };

        var result = await _sut.ForgeAsync(_accountId, ids);

        Assert.False(result.Success);
        Assert.Equal("INSUFFICIENT_MATERIALS", result.ErrorCode);
    }

    [Fact]
    public async Task EquipAsync_ValidEquipment_Equips()
    {
        var equipId = Guid.NewGuid();
        _equipmentRepo.GetByIdAsync(equipId).Returns(CreateEquipment(equipId, "COMMON", 0));
        _equipmentRepo.GetEquippedAsync(_accountId).Returns(new List<EquipmentEntry>());

        var result = await _sut.EquipAsync(_accountId, equipId.ToString());

        Assert.True(result.Success);
        Assert.NotNull(result.Delta);
        Assert.NotNull(result.Delta!.EquipmentSlotChanges);
    }

    [Fact]
    public async Task SellAsync_NormalEquipment_ReturnsGold()
    {
        var equipId = Guid.NewGuid();
        _equipmentRepo.GetByIdAsync(equipId).Returns(CreateEquipment(equipId, "RARE", 0));
        _resourceRepo.GetBalanceAsync(_accountId, "GOLD").Returns((ResourceBalance?)null);

        var result = await _sut.SellAsync(_accountId, equipId.ToString());

        Assert.True(result.Success);
        Assert.NotNull(result.Delta);
        Assert.NotNull(result.Delta!.RemovedEquipmentIds);
    }

    [Fact]
    public async Task SellAsync_SGradeEquipment_Blocked()
    {
        var equipId = Guid.NewGuid();
        var equip = CreateEquipment(equipId, "EPIC", 0);
        equip.IsS = true;
        _equipmentRepo.GetByIdAsync(equipId).Returns(equip);

        var result = await _sut.SellAsync(_accountId, equipId.ToString());

        Assert.False(result.Success);
        Assert.Equal("CANNOT_SELL_S_GRADE", result.ErrorCode);
    }

    [Fact]
    public async Task SellAsync_EquippedItem_Blocked()
    {
        var equipId = Guid.NewGuid();
        var equip = CreateEquipment(equipId, "COMMON", 0);
        equip.SlotIndex = 0;
        _equipmentRepo.GetByIdAsync(equipId).Returns(equip);

        var result = await _sut.SellAsync(_accountId, equipId.ToString());

        Assert.False(result.Success);
        Assert.Equal("CANNOT_SELL_EQUIPPED", result.ErrorCode);
    }

    private EquipmentEntry CreateEquipment(Guid id, string grade, int enhancementLevel)
    {
        return new EquipmentEntry
        {
            Id = id,
            AccountId = _accountId,
            TemplateId = $"equip_{grade.ToLowerInvariant()}_1",
            Grade = grade,
            EnhancementLevel = enhancementLevel,
            SubStats = "[]",
            SlotIndex = -1,
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
