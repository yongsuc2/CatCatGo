using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;

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

    public async Task<PetFeedResult> FeedAsync(Guid accountId, Guid petId, int amount)
    {
        var pet = await _petRepo.GetByIdAsync(petId);
        if (pet == null || pet.AccountId != accountId)
            return new PetFeedResult { Success = false, Error = "PET_NOT_FOUND" };

        var spent = await _resourceService.SpendAsync(accountId, "PET_FOOD", amount, "PET_FEED", petId.ToString());
        if (!spent)
            return new PetFeedResult { Success = false, Error = "INSUFFICIENT_PET_FOOD" };

        pet.Experience += amount;
        var levelUps = pet.Experience / 100;
        if (levelUps > 0)
        {
            pet.Level += levelUps;
            pet.Experience %= 100;
        }

        pet.UpdatedAt = DateTime.UtcNow;
        await _petRepo.UpdateAsync(pet);

        return new PetFeedResult { Success = true, Pet = pet };
    }

    public async Task<PetUpgradeResult> UpgradeAsync(Guid accountId, Guid petId)
    {
        var pet = await _petRepo.GetByIdAsync(petId);
        if (pet == null || pet.AccountId != accountId)
            return new PetUpgradeResult { Success = false, Error = "PET_NOT_FOUND" };

        var currentIndex = Array.IndexOf(GradeOrder, pet.Grade);
        if (currentIndex < 0 || currentIndex >= GradeOrder.Length - 1)
            return new PetUpgradeResult { Success = false, Error = "MAX_GRADE" };

        var duplicatesNeeded = currentIndex + 2;
        var allPets = await _petRepo.GetByAccountIdAsync(accountId);
        var duplicates = allPets.Where(p => p.PetId == pet.PetId && p.Id != pet.Id && p.Grade == pet.Grade).ToList();
        if (duplicates.Count < duplicatesNeeded - 1)
            return new PetUpgradeResult { Success = false, Error = "INSUFFICIENT_DUPLICATES" };

        var consumed = duplicates.Take(duplicatesNeeded - 1).ToList();
        foreach (var dup in consumed)
            await _petRepo.DeleteAsync(dup.Id);

        pet.Grade = GradeOrder[currentIndex + 1];
        pet.UpdatedAt = DateTime.UtcNow;
        await _petRepo.UpdateAsync(pet);

        return new PetUpgradeResult { Success = true, Pet = pet };
    }

    public async Task<PetEquipResult> EquipAsync(Guid accountId, Guid petId)
    {
        var pet = await _petRepo.GetByIdAsync(petId);
        if (pet == null || pet.AccountId != accountId)
            return new PetEquipResult { Success = false, Error = "PET_NOT_FOUND" };

        var currentEquipped = await _petRepo.GetEquippedAsync(accountId);
        if (currentEquipped != null && currentEquipped.Id != petId)
        {
            currentEquipped.IsEquipped = false;
            currentEquipped.UpdatedAt = DateTime.UtcNow;
            await _petRepo.UpdateAsync(currentEquipped);
        }

        pet.IsEquipped = true;
        pet.UpdatedAt = DateTime.UtcNow;
        await _petRepo.UpdateAsync(pet);

        return new PetEquipResult { Success = true, Pet = pet };
    }
}

public class PetFeedResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public PetEntry? Pet { get; set; }
}

public class PetUpgradeResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public PetEntry? Pet { get; set; }
}

public class PetEquipResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public PetEntry? Pet { get; set; }
}
