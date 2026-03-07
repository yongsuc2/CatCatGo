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
    public async Task FeedAsync_SufficientFood_ReturnsSuccessWithDelta()
    {
        var petId = Guid.NewGuid();
        var pet = CreatePet(petId, "COMMON", 1, 0);
        _petRepo.GetByIdAsync(petId).Returns(pet);
        SetupBalance("PET_FOOD", 50);

        var result = await _sut.FeedAsync(_accountId, petId.ToString(), 50);

        Assert.True(result.Success);
        Assert.NotNull(result.Delta);
        Assert.NotNull(result.Delta!.UpdatedPets);
    }

    [Fact]
    public async Task FeedAsync_EnoughForLevelUp_IncreasesLevel()
    {
        var petId = Guid.NewGuid();
        var pet = CreatePet(petId, "COMMON", 1, 50);
        _petRepo.GetByIdAsync(petId).Returns(pet);
        SetupBalance("PET_FOOD", 150);

        var result = await _sut.FeedAsync(_accountId, petId.ToString(), 150);

        Assert.True(result.Success);
        Assert.NotNull(result.Delta?.UpdatedPets);
        Assert.Equal(3, result.Delta!.UpdatedPets![0].Level);
    }

    [Fact]
    public async Task FeedAsync_InsufficientFood_Fails()
    {
        var petId = Guid.NewGuid();
        var pet = CreatePet(petId, "COMMON", 1, 0);
        _petRepo.GetByIdAsync(petId).Returns(pet);
        SetupBalance("PET_FOOD", 5);

        var result = await _sut.FeedAsync(_accountId, petId.ToString(), 10);

        Assert.False(result.Success);
        Assert.Equal("INSUFFICIENT_PET_FOOD", result.ErrorCode);
    }

    [Fact]
    public async Task FeedAsync_WrongAccount_Fails()
    {
        var petId = Guid.NewGuid();
        var pet = CreatePet(petId, "COMMON", 1, 0);
        pet.AccountId = Guid.NewGuid();
        _petRepo.GetByIdAsync(petId).Returns(pet);

        var result = await _sut.FeedAsync(_accountId, petId.ToString(), 10);

        Assert.False(result.Success);
        Assert.Equal("PET_NOT_FOUND", result.ErrorCode);
    }

    [Fact]
    public async Task HatchAsync_SufficientEggs_ReturnsNewPet()
    {
        SetupBalance("PET_EGG", 5);
        _petRepo.GetByAccountIdAsync(_accountId).Returns(new List<PetEntry>());

        var result = await _sut.HatchAsync(_accountId);

        // First hatch with no existing pets will create and then count 0 existing
        // But CreateAsync adds to DB, mock GetByAccountIdAsync returns empty
        // So isFirstPet logic depends on mock setup
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Delta);
        Assert.NotNull(result.Delta!.AddedPets);
        await _petRepo.Received(1).CreateAsync(Arg.Any<PetEntry>());
    }

    [Fact]
    public async Task HatchAsync_InsufficientEggs_Fails()
    {
        SetupBalance("PET_EGG", 0);

        var result = await _sut.HatchAsync(_accountId);

        Assert.False(result.Success);
        Assert.Equal("INSUFFICIENT_PET_EGG", result.ErrorCode);
    }

    [Fact]
    public async Task DeployAsync_ValidPet_SetActivePetId()
    {
        var newPetId = Guid.NewGuid();
        var oldPetId = Guid.NewGuid();
        var newPet = CreatePet(newPetId, "COMMON", 1, 0);
        var oldPet = CreatePet(oldPetId, "COMMON", 1, 0);
        oldPet.IsEquipped = true;
        _petRepo.GetByIdAsync(newPetId).Returns(newPet);
        _petRepo.GetEquippedAsync(_accountId).Returns(oldPet);

        var result = await _sut.DeployAsync(_accountId, newPetId.ToString());

        Assert.True(result.Success);
        Assert.Equal(newPetId.ToString(), result.Delta!.ActivePetId);
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
