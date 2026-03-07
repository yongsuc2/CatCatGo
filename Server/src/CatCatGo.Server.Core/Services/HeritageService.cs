using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;

namespace CatCatGo.Server.Core.Services;

public class HeritageService
{
    private readonly IHeritageRepository _heritageRepo;
    private readonly ITalentRepository _talentRepo;
    private readonly ResourceService _resourceService;

    private static readonly Dictionary<string, string> RouteToBook = new()
    {
        { "SKULL", "SKULL_BOOK" },
        { "KNIGHT", "KNIGHT_BOOK" },
        { "RANGER", "RANGER_BOOK" },
        { "GHOST", "GHOST_BOOK" },
    };

    public HeritageService(IHeritageRepository heritageRepo, ITalentRepository talentRepo, ResourceService resourceService)
    {
        _heritageRepo = heritageRepo;
        _talentRepo = talentRepo;
        _resourceService = resourceService;
    }

    public async Task<HeritageStatusResult> GetStatusAsync(Guid accountId)
    {
        var state = await _heritageRepo.GetByAccountIdAsync(accountId);
        var talent = await _talentRepo.GetByAccountIdAsync(accountId);
        var isUnlocked = talent != null && talent.Grade == "HERO";

        if (state == null)
        {
            state = new HeritageState
            {
                AccountId = accountId,
                UpdatedAt = DateTime.UtcNow,
            };
        }

        return new HeritageStatusResult { State = state, IsUnlocked = isUnlocked };
    }

    public async Task<HeritageUpgradeResult> UpgradeAsync(Guid accountId, string route)
    {
        if (!RouteToBook.TryGetValue(route, out var bookType))
            return new HeritageUpgradeResult { Success = false, Error = "INVALID_ROUTE" };

        var talent = await _talentRepo.GetByAccountIdAsync(accountId);
        if (talent == null || talent.Grade != "HERO")
            return new HeritageUpgradeResult { Success = false, Error = "HERITAGE_LOCKED" };

        var state = await _heritageRepo.GetByAccountIdAsync(accountId) ?? new HeritageState
        {
            AccountId = accountId,
            UpdatedAt = DateTime.UtcNow,
        };

        var currentLevel = route switch
        {
            "SKULL" => state.SkullLevel,
            "KNIGHT" => state.KnightLevel,
            "RANGER" => state.RangerLevel,
            "GHOST" => state.GhostLevel,
            _ => 0,
        };

        var bookCost = 1.0 + currentLevel * 0.5;
        var goldCost = 500.0 * Math.Pow(1.15, currentLevel);

        var bookSpent = await _resourceService.SpendAsync(accountId, bookType, bookCost, "HERITAGE_UPGRADE", route);
        if (!bookSpent)
            return new HeritageUpgradeResult { Success = false, Error = $"INSUFFICIENT_{bookType}" };

        var goldSpent = await _resourceService.SpendAsync(accountId, "GOLD", goldCost, "HERITAGE_UPGRADE", route);
        if (!goldSpent)
        {
            await _resourceService.GrantAsync(accountId, bookType, bookCost, "HERITAGE_UPGRADE_REFUND", route);
            return new HeritageUpgradeResult { Success = false, Error = "INSUFFICIENT_GOLD" };
        }

        switch (route)
        {
            case "SKULL": state.SkullLevel++; break;
            case "KNIGHT": state.KnightLevel++; break;
            case "RANGER": state.RangerLevel++; break;
            case "GHOST": state.GhostLevel++; break;
        }

        state.UpdatedAt = DateTime.UtcNow;
        await _heritageRepo.UpsertAsync(state);

        return new HeritageUpgradeResult { Success = true, State = state };
    }
}

public class HeritageStatusResult
{
    public HeritageState? State { get; set; }
    public bool IsUnlocked { get; set; }
}

public class HeritageUpgradeResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public HeritageState? State { get; set; }
}
