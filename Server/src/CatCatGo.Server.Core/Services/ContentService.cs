using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;

namespace CatCatGo.Server.Core.Services;

public class ContentService
{
    private readonly IContentRepository _contentRepo;
    private readonly ResourceService _resourceService;

    public ContentService(IContentRepository contentRepo, ResourceService resourceService)
    {
        _contentRepo = contentRepo;
        _resourceService = resourceService;
    }

    public async Task<ContentResult> TowerChallengeAsync(Guid accountId)
    {
        var spent = await _resourceService.SpendAsync(accountId, "CHALLENGE_TOKEN", 1, "TOWER_CHALLENGE");
        if (!spent)
            return new ContentResult { Success = false, Error = "INSUFFICIENT_CHALLENGE_TOKEN" };

        var progress = await _contentRepo.GetProgressAsync(accountId, "TOWER") ?? new ContentProgress
        {
            AccountId = accountId,
            ContentType = "TOWER",
            UpdatedAt = DateTime.UtcNow,
        };

        progress.HighestStage++;
        progress.UpdatedAt = DateTime.UtcNow;
        await _contentRepo.UpsertProgressAsync(progress);

        if (progress.HighestStage % 5 == 0)
        {
            await _resourceService.GrantAsync(accountId, "POWER_STONE", 1, "TOWER_REWARD", progress.HighestStage.ToString());
        }

        return new ContentResult { Success = true, Stage = progress.HighestStage };
    }

    public async Task<ContentResult> DungeonEnterAsync(Guid accountId, string dungeonType)
    {
        var sharedProgress = await GetOrCreateProgressAsync(accountId, "DUNGEON_SHARED");

        if (sharedProgress.LastResetDate.Date < DateTime.UtcNow.Date)
        {
            sharedProgress.DailyRunsUsed = 0;
            sharedProgress.LastResetDate = DateTime.UtcNow.Date;
        }

        if (sharedProgress.DailyRunsUsed >= 3)
            return new ContentResult { Success = false, Error = "DAILY_LIMIT_REACHED" };

        sharedProgress.DailyRunsUsed++;
        sharedProgress.UpdatedAt = DateTime.UtcNow;
        await _contentRepo.UpsertProgressAsync(sharedProgress);

        var typeProgress = await GetOrCreateProgressAsync(accountId, $"DUNGEON_{dungeonType}");

        return new ContentResult { Success = true, RunsRemaining = 3 - sharedProgress.DailyRunsUsed };
    }

    public async Task<ContentResult> DungeonResultAsync(Guid accountId, string dungeonType, bool victory)
    {
        if (!victory)
            return new ContentResult { Success = true };

        var progress = await GetOrCreateProgressAsync(accountId, $"DUNGEON_{dungeonType}");
        progress.HighestStage++;
        progress.UpdatedAt = DateTime.UtcNow;
        await _contentRepo.UpsertProgressAsync(progress);

        var rewardType = dungeonType switch
        {
            "BEEHIVE" => "STAMINA",
            "ANCIENT_TREE" => "PET_EGG",
            "TIGER_CLIFF" => "EQUIPMENT_STONE",
            _ => "GOLD",
        };
        await _resourceService.GrantAsync(accountId, rewardType, 10, "DUNGEON_REWARD", dungeonType);

        return new ContentResult { Success = true, Stage = progress.HighestStage };
    }

    public async Task<ContentResult> TravelStartAsync(Guid accountId, double staminaCost)
    {
        var spent = await _resourceService.SpendAsync(accountId, "STAMINA", staminaCost, "TRAVEL_START");
        if (!spent)
            return new ContentResult { Success = false, Error = "INSUFFICIENT_STAMINA" };

        return new ContentResult { Success = true };
    }

    public async Task<ContentResult> TravelCompleteAsync(Guid accountId, int clearedChapterMax, double speedMultiplier)
    {
        var baseGold = 100.0 * (1 + clearedChapterMax * 0.5);
        var totalGold = baseGold * speedMultiplier;
        await _resourceService.GrantAsync(accountId, "GOLD", totalGold, "TRAVEL_REWARD");

        return new ContentResult { Success = true, GoldEarned = totalGold };
    }

    public async Task<ContentResult> GoblinMineAsync(Guid accountId)
    {
        var spent = await _resourceService.SpendAsync(accountId, "PICKAXE", 1, "GOBLIN_MINE");
        if (!spent)
            return new ContentResult { Success = false, Error = "INSUFFICIENT_PICKAXE" };

        var oreCount = Random.Shared.Next(1, 5);
        await _resourceService.GrantAsync(accountId, "GOLD", oreCount * 50.0, "GOBLIN_MINE_REWARD");

        return new ContentResult { Success = true, GoldEarned = oreCount * 50.0 };
    }

    public async Task<ContentResult> CatacombRunAsync(Guid accountId)
    {
        var progress = await GetOrCreateProgressAsync(accountId, "CATACOMB");
        progress.HighestStage++;
        progress.UpdatedAt = DateTime.UtcNow;
        await _contentRepo.UpsertProgressAsync(progress);

        var goldReward = progress.HighestStage * 200.0;
        await _resourceService.GrantAsync(accountId, "GOLD", goldReward, "CATACOMB_REWARD", progress.HighestStage.ToString());

        return new ContentResult { Success = true, Stage = progress.HighestStage, GoldEarned = goldReward };
    }

    private async Task<ContentProgress> GetOrCreateProgressAsync(Guid accountId, string contentType)
    {
        var progress = await _contentRepo.GetProgressAsync(accountId, contentType);
        if (progress == null)
        {
            progress = new ContentProgress
            {
                AccountId = accountId,
                ContentType = contentType,
                LastResetDate = DateTime.UtcNow.Date,
                UpdatedAt = DateTime.UtcNow,
            };
            await _contentRepo.UpsertProgressAsync(progress);
        }
        return progress;
    }
}

public class ContentResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public int Stage { get; set; }
    public int RunsRemaining { get; set; }
    public double GoldEarned { get; set; }
}
