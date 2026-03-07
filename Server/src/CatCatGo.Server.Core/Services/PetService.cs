using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using CatCatGo.Shared.Models;

namespace CatCatGo.Server.Core.Services;

public class PetService
{
    private readonly IPetRepository _petRepo;
    private readonly ResourceService _resourceService;

    private static readonly string[] GradeOrder = { "COMMON", "RARE", "EPIC", "LEGENDARY", "IMMORTAL" };

    public PetService(IPetRepository petRepo, ResourceService resourceService)
    {
        _petRepo = petRepo;
        _resourceService = resourceService;
    }

    public async Task<ApiResponse<PetHatchResponse>> HatchAsync(Guid accountId)
    {
        var spent = await _resourceService.SpendAsync(accountId, "PET_EGG", 1, "PET_HATCH");
        if (!spent)
            return ApiResponse<PetHatchResponse>.Fail("INSUFFICIENT_PET_EGG");

        var petTier = RollPetTier();
        var petTemplateId = $"pet_t{petTier}_{Random.Shared.Next(1, 20)}";

        var pet = new PetEntry
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            PetId = petTemplateId,
            Grade = "COMMON",
            Level = 1,
            Experience = 0,
            IsEquipped = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        await _petRepo.CreateAsync(pet);

        var allPets = await _petRepo.GetByAccountIdAsync(accountId);
        var isFirstPet = allPets.Count == 1;
        if (isFirstPet)
        {
            pet.IsEquipped = true;
            await _petRepo.UpdateAsync(pet);
        }

        var eggBalance = await _resourceService.GetBalanceAsync(accountId, "PET_EGG");
        var deltaBuilder = new StateDeltaBuilder()
            .AddResource("PET_EGG", (float)eggBalance)
            .AddPet(ToPetDeltaData(pet));

        if (isFirstPet)
            deltaBuilder.SetActivePet(pet.Id.ToString());

        var petData = ToPetDeltaData(pet);

        return ApiResponse<PetHatchResponse>.Ok(
            new PetHatchResponse { Pet = petData },
            deltaBuilder.Build());
    }

    public async Task<ApiResponse<object>> FeedAsync(Guid accountId, string petId, int amount)
    {
        if (!Guid.TryParse(petId, out var petGuid))
            return ApiResponse<object>.Fail("INVALID_PET_ID");

        var pet = await _petRepo.GetByIdAsync(petGuid);
        if (pet == null || pet.AccountId != accountId)
            return ApiResponse<object>.Fail("PET_NOT_FOUND");

        var spent = await _resourceService.SpendAsync(accountId, "PET_FOOD", amount, "PET_FEED", petId);
        if (!spent)
            return ApiResponse<object>.Fail("INSUFFICIENT_PET_FOOD");

        pet.Experience += amount;
        var levelUps = pet.Experience / 100;
        if (levelUps > 0)
        {
            pet.Level += levelUps;
            pet.Experience %= 100;
        }

        pet.UpdatedAt = DateTime.UtcNow;
        await _petRepo.UpdateAsync(pet);

        var foodBalance = await _resourceService.GetBalanceAsync(accountId, "PET_FOOD");
        var delta = new StateDeltaBuilder()
            .AddResource("PET_FOOD", (float)foodBalance)
            .UpdatePet(petId, pet.Level, pet.Experience)
            .Build();

        return ApiResponse<object>.Ok(delta: delta);
    }

    public async Task<ApiResponse<object>> DeployAsync(Guid accountId, string petId)
    {
        if (!Guid.TryParse(petId, out var petGuid))
            return ApiResponse<object>.Fail("INVALID_PET_ID");

        var pet = await _petRepo.GetByIdAsync(petGuid);
        if (pet == null || pet.AccountId != accountId)
            return ApiResponse<object>.Fail("PET_NOT_FOUND");

        var currentEquipped = await _petRepo.GetEquippedAsync(accountId);
        if (currentEquipped != null && currentEquipped.Id != petGuid)
        {
            currentEquipped.IsEquipped = false;
            currentEquipped.UpdatedAt = DateTime.UtcNow;
            await _petRepo.UpdateAsync(currentEquipped);
        }

        pet.IsEquipped = true;
        pet.UpdatedAt = DateTime.UtcNow;
        await _petRepo.UpdateAsync(pet);

        var delta = new StateDeltaBuilder()
            .SetActivePet(petId)
            .Build();

        return ApiResponse<object>.Ok(delta: delta);
    }

    private static PetDeltaData ToPetDeltaData(PetEntry pet) => new()
    {
        Id = pet.Id.ToString(),
        Name = pet.PetId,
        Tier = "",
        Grade = pet.Grade,
        Level = pet.Level,
        Exp = pet.Experience,
    };

    private static int RollPetTier()
    {
        var weights = new Dictionary<int, double> { { 1, 50 }, { 2, 30 }, { 3, 15 }, { 4, 4 }, { 5, 1 } };
        var totalWeight = weights.Values.Sum();
        var roll = Random.Shared.NextDouble() * totalWeight;
        var cumulative = 0.0;
        foreach (var (tier, weight) in weights)
        {
            cumulative += weight;
            if (roll <= cumulative) return tier;
        }
        return 1;
    }
}

public class PetHatchResponse
{
    public PetDeltaData? Pet { get; set; }
}
