using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using CatCatGo.Shared.Models;

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

    public async Task<ApiResponse<object>> UpgradeAsync(Guid accountId, string route)
    {
        if (!RouteToBook.TryGetValue(route, out var bookType))
            return ApiResponse<object>.Fail("INVALID_ROUTE");

        var talent = await _talentRepo.GetByAccountIdAsync(accountId);
        if (talent == null || talent.Grade != "HERO")
            return ApiResponse<object>.Fail("HERITAGE_LOCKED");

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
            return ApiResponse<object>.Fail($"INSUFFICIENT_{bookType}");

        var goldSpent = await _resourceService.SpendAsync(accountId, "GOLD", goldCost, "HERITAGE_UPGRADE", route);
        if (!goldSpent)
        {
            await _resourceService.GrantAsync(accountId, bookType, bookCost, "HERITAGE_UPGRADE_REFUND", route);
            return ApiResponse<object>.Fail("INSUFFICIENT_GOLD");
        }

        switch (route)
        {
            case "SKULL": state.SkullLevel++; break;
            case "KNIGHT": state.KnightLevel++; break;
            case "RANGER": state.RangerLevel++; break;
            case "GHOST": state.GhostLevel++; break;
        }

        var newLevel = route switch
        {
            "SKULL" => state.SkullLevel,
            "KNIGHT" => state.KnightLevel,
            "RANGER" => state.RangerLevel,
            "GHOST" => state.GhostLevel,
            _ => 0,
        };

        state.UpdatedAt = DateTime.UtcNow;
        await _heritageRepo.UpsertAsync(state);

        var bookBalance = await _resourceService.GetBalanceAsync(accountId, bookType);
        var goldBalance = await _resourceService.GetBalanceAsync(accountId, "GOLD");

        var delta = new StateDeltaBuilder()
            .AddResource(bookType, (float)bookBalance)
            .AddResource("GOLD", (float)goldBalance)
            .SetHeritage(route, newLevel)
            .Build();

        return ApiResponse<object>.Ok(delta: delta);
    }
}

public class HeritageStatusResult
{
    public HeritageState? State { get; set; }
    public bool IsUnlocked { get; set; }
}
