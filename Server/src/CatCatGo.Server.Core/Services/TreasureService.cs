using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using CatCatGo.Shared.Models;

namespace CatCatGo.Server.Core.Services;

public class TreasureService
{
    private readonly IChapterRepository _chapterRepo;
    private readonly ResourceService _resourceService;

    public TreasureService(IChapterRepository chapterRepo, ResourceService resourceService)
    {
        _chapterRepo = chapterRepo;
        _resourceService = resourceService;
    }

    public async Task<ApiResponse<object>> ClaimAsync(Guid accountId, string milestoneId)
    {
        var progress = await _chapterRepo.GetProgressAsync(accountId);
        if (progress == null)
            return ApiResponse<object>.Fail("NO_PROGRESS");

        var claimed = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, List<string>>>(progress.ClaimedTreasures) ?? new();

        foreach (var chapterClaims in claimed.Values)
        {
            if (chapterClaims.Contains(milestoneId))
                return ApiResponse<object>.Fail("ALREADY_CLAIMED");
        }

        var parts = milestoneId.Split('_');
        var chapterKey = parts.Length > 1 ? parts[0] : "0";
        if (!claimed.ContainsKey(chapterKey))
            claimed[chapterKey] = new();

        claimed[chapterKey].Add(milestoneId);
        progress.ClaimedTreasures = System.Text.Json.JsonSerializer.Serialize(claimed);
        progress.UpdatedAt = DateTime.UtcNow;
        await _chapterRepo.UpsertProgressAsync(progress);

        var goldReward = 500.0;
        await _resourceService.GrantAsync(accountId, "GOLD", goldReward, "CHAPTER_TREASURE", milestoneId);

        var goldBalance = await _resourceService.GetBalanceAsync(accountId, "GOLD");
        var delta = new StateDeltaBuilder()
            .AddResource("GOLD", (float)goldBalance)
            .AddClaimedMilestone(milestoneId)
            .Build();

        return ApiResponse<object>.Ok(delta: delta);
    }
}
