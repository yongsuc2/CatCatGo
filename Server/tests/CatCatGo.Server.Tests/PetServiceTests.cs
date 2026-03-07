using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using CatCatGo.Server.Core.Services;
using NSubstitute;
using Xunit;

namespace CatCatGo.Server.Tests;

public class PetServiceTests
{
    private readonly IPetRepository _petRepo;
    private readonly IResourceRepository _resourceRepo;
    private readonly ResourceService _resourceService;
    private readonly PetService _sut;

    private readonly Guid _accountId = Guid.NewGuid();

    public PetServiceTests()
    {
        _petRepo = Substitute.For<IPetRepository>();
        _resourceRepo = Substitute.For<IResourceRepository>();
        _resourceService = new ResourceService(_resourceRepo);
        _sut = new PetService(_petRepo, _resourceService);
    }

    [Fact]
    public async Task FeedAsync_SufficientFood_IncreasesExperience()
    {
        var petId = Guid.NewGuid();
        var pet = CreatePet(petId, "COMMON", 1, 0);
        _petRepo.GetByIdAsync(petId).Returns(pet);
        SetupBalance("PET_FOOD", 50);

        var result = await _sut.FeedAsync(_accountId, petId, 50);

        Assert.True(result.Success);
        Assert.Equal(50, result.Pet!.Experience);
    }

    [Fact]
    public async Task FeedAsync_EnoughForLevelUp_IncreasesLevel()
    {
        var petId = Guid.NewGuid();
        var pet = CreatePet(petId, "COMMON", 1, 50);
        _petRepo.GetByIdAsync(petId).Returns(pet);
        SetupBalance("PET_FOOD", 150);

        var result = await _sut.FeedAsync(_accountId, petId, 150);

        Assert.True(result.Success);
        Assert.Equal(3, result.Pet!.Level);
        Assert.Equal(0, result.Pet.Experience);
    }

    [Fact]
    public async Task FeedAsync_InsufficientFood_Fails()
    {
        var petId = Guid.NewGuid();
        var pet = CreatePet(petId, "COMMON", 1, 0);
        _petRepo.GetByIdAsync(petId).Returns(pet);
        SetupBalance("PET_FOOD", 5);

        var result = await _sut.FeedAsync(_accountId, petId, 10);

        Assert.False(result.Success);
        Assert.Equal("INSUFFICIENT_PET_FOOD", result.Error);
    }

    [Fact]
    public async Task FeedAsync_WrongAccount_Fails()
    {
        var petId = Guid.NewGuid();
        var pet = CreatePet(petId, "COMMON", 1, 0);
        pet.AccountId = Guid.NewGuid();
        _petRepo.GetByIdAsync(petId).Returns(pet);

        var result = await _sut.FeedAsync(_accountId, petId, 10);

        Assert.False(result.Success);
        Assert.Equal("PET_NOT_FOUND", result.Error);
    }

    [Fact]
    public async Task UpgradeAsync_SufficientDuplicates_UpgradesAndConsumes()
    {
        var petId = Guid.NewGuid();
        var pet = CreatePet(petId, "COMMON", 1, 0);
        pet.PetId = "cat_01";
        var dup1 = CreatePet(Guid.NewGuid(), "COMMON", 1, 0);
        dup1.PetId = "cat_01";
        _petRepo.GetByIdAsync(petId).Returns(pet);
        _petRepo.GetByAccountIdAsync(_accountId).Returns(new List<PetEntry> { pet, dup1 });

        var result = await _sut.UpgradeAsync(_accountId, petId);

        Assert.True(result.Success);
        Assert.Equal("RARE", result.Pet!.Grade);
        await _petRepo.Received(1).DeleteAsync(dup1.Id);
    }

    [Fact]
    public async Task UpgradeAsync_InsufficientDuplicates_Fails()
    {
        var petId = Guid.NewGuid();
        var pet = CreatePet(petId, "COMMON", 1, 0);
        pet.PetId = "cat_01";
        _petRepo.GetByIdAsync(petId).Returns(pet);
        _petRepo.GetByAccountIdAsync(_accountId).Returns(new List<PetEntry> { pet });

        var result = await _sut.UpgradeAsync(_accountId, petId);

        Assert.False(result.Success);
        Assert.Equal("INSUFFICIENT_DUPLICATES", result.Error);
    }

    [Fact]
    public async Task UpgradeAsync_MaxGrade_Fails()
    {
        var petId = Guid.NewGuid();
        var pet = CreatePet(petId, "IMMORTAL", 1, 0);
        _petRepo.GetByIdAsync(petId).Returns(pet);

        var result = await _sut.UpgradeAsync(_accountId, petId);

        Assert.False(result.Success);
        Assert.Equal("MAX_GRADE", result.Error);
    }

    [Fact]
    public async Task EquipAsync_UnequipsPreviousAndEquipsNew()
    {
        var newPetId = Guid.NewGuid();
        var oldPetId = Guid.NewGuid();
        var newPet = CreatePet(newPetId, "COMMON", 1, 0);
        var oldPet = CreatePet(oldPetId, "COMMON", 1, 0);
        oldPet.IsEquipped = true;
        _petRepo.GetByIdAsync(newPetId).Returns(newPet);
        _petRepo.GetEquippedAsync(_accountId).Returns(oldPet);

        var result = await _sut.EquipAsync(_accountId, newPetId);

        Assert.True(result.Success);
        Assert.True(result.Pet!.IsEquipped);
        Assert.False(oldPet.IsEquipped);
        await _petRepo.Received(2).UpdateAsync(Arg.Any<PetEntry>());
    }

    private PetEntry CreatePet(Guid id, string grade, int level, int experience)
    {
        return new PetEntry
        {
            Id = id,
            AccountId = _accountId,
            PetId = $"pet_{grade.ToLowerInvariant()}",
            Grade = grade,
            Level = level,
            Experience = experience,
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
