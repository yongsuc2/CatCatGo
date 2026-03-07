using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using CatCatGo.Shared.Models;

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

    public async Task<ApiResponse<TowerChallengeResponse>> TowerChallengeAsync(Guid accountId)
    {
        var spent = await _resourceService.SpendAsync(accountId, "CHALLENGE_TOKEN", 1, "TOWER_CHALLENGE");
        if (!spent)
            return ApiResponse<TowerChallengeResponse>.Fail("INSUFFICIENT_CHALLENGE_TOKEN");

        var progress = await _contentRepo.GetProgressAsync(accountId, "TOWER") ?? new ContentProgress
        {
            AccountId = accountId,
            ContentType = "TOWER",
            UpdatedAt = DateTime.UtcNow,
        };

        var battleResult = Random.Shared.NextDouble() > 0.3 ? "VICTORY" : "DEFEAT";

        if (battleResult == "VICTORY")
        {
            progress.HighestStage++;
            progress.UpdatedAt = DateTime.UtcNow;
            await _contentRepo.UpsertProgressAsync(progress);
        }

        var deltaBuilder = new StateDeltaBuilder();
        var reward = new RewardData();

        var tokenBalance = await _resourceService.GetBalanceAsync(accountId, "CHALLENGE_TOKEN");
        deltaBuilder.AddResource("CHALLENGE_TOKEN", (float)tokenBalance);

        if (battleResult == "VICTORY" && progress.HighestStage % 5 == 0)
        {
            await _resourceService.GrantAsync(accountId, "POWER_STONE", 1, "TOWER_REWARD", progress.HighestStage.ToString());
            var stoneBalance = await _resourceService.GetBalanceAsync(accountId, "POWER_STONE");
            deltaBuilder.AddResource("POWER_STONE", (float)stoneBalance);
            reward.Type = "POWER_STONE";
            reward.Amount = 1;
        }

        var floor = (progress.HighestStage - 1) / 10 + 1;
        var stage = ((progress.HighestStage - 1) % 10) + 1;
        deltaBuilder.SetTower(floor, stage);

        return ApiResponse<TowerChallengeResponse>.Ok(
            new TowerChallengeResponse { BattleResult = battleResult, Reward = reward },
            deltaBuilder.Build());
    }

    public async Task<ApiResponse<DungeonChallengeResponse>> DungeonChallengeAsync(Guid accountId, string dungeonType)
    {
        var sharedProgress = await GetOrCreateProgressAsync(accountId, "DUNGEON_SHARED");

        if (sharedProgress.LastResetDate.Date < DateTime.UtcNow.Date)
        {
            sharedProgress.DailyRunsUsed = 0;
            sharedProgress.LastResetDate = DateTime.UtcNow.Date;
        }

        if (sharedProgress.DailyRunsUsed >= 3)
            return ApiResponse<DungeonChallengeResponse>.Fail("DAILY_LIMIT_REACHED");

        sharedProgress.DailyRunsUsed++;
        sharedProgress.UpdatedAt = DateTime.UtcNow;
        await _contentRepo.UpsertProgressAsync(sharedProgress);

        var typeProgress = await GetOrCreateProgressAsync(accountId, $"DUNGEON_{dungeonType}");

        var battleResult = Random.Shared.NextDouble() > 0.2 ? "VICTORY" : "DEFEAT";
        var deltaBuilder = new StateDeltaBuilder();
        var reward = new RewardData();

        if (battleResult == "VICTORY")
        {
            typeProgress.HighestStage++;
            typeProgress.UpdatedAt = DateTime.UtcNow;
            await _contentRepo.UpsertProgressAsync(typeProgress);

            var rewardType = dungeonType switch
            {
                "BEEHIVE" => "STAMINA",
                "ANCIENT_TREE" => "PET_EGG",
                "TIGER_CLIFF" => "EQUIPMENT_STONE",
                _ => "GOLD",
            };
            await _resourceService.GrantAsync(accountId, rewardType, 10, "DUNGEON_REWARD", dungeonType);

            var rewardBalance = await _resourceService.GetBalanceAsync(accountId, rewardType);
            deltaBuilder.AddResource(rewardType, (float)rewardBalance);
            reward.Type = rewardType;
            reward.Amount = 10;
        }

        var clearedStages = new Dictionary<string, int> { { dungeonType, typeProgress.HighestStage } };
        deltaBuilder.SetDungeons(sharedProgress.DailyRunsUsed, clearedStages);

        return ApiResponse<DungeonChallengeResponse>.Ok(
            new DungeonChallengeResponse { BattleResult = battleResult, Reward = reward },
            deltaBuilder.Build());
    }

    public async Task<ApiResponse<DungeonSweepResponse>> DungeonSweepAsync(Guid accountId, string dungeonType)
    {
        var sharedProgress = await GetOrCreateProgressAsync(accountId, "DUNGEON_SHARED");

        if (sharedProgress.LastResetDate.Date < DateTime.UtcNow.Date)
        {
            sharedProgress.DailyRunsUsed = 0;
            sharedProgress.LastResetDate = DateTime.UtcNow.Date;
        }

        if (sharedProgress.DailyRunsUsed >= 3)
            return ApiResponse<DungeonSweepResponse>.Fail("DAILY_LIMIT_REACHED");

        var typeProgress = await GetOrCreateProgressAsync(accountId, $"DUNGEON_{dungeonType}");
        if (typeProgress.HighestStage <= 0)
            return ApiResponse<DungeonSweepResponse>.Fail("NO_CLEAR_RECORD");

        sharedProgress.DailyRunsUsed++;
        sharedProgress.UpdatedAt = DateTime.UtcNow;
        await _contentRepo.UpsertProgressAsync(sharedProgress);

        var rewardType = dungeonType switch
        {
            "BEEHIVE" => "STAMINA",
            "ANCIENT_TREE" => "PET_EGG",
            "TIGER_CLIFF" => "EQUIPMENT_STONE",
            _ => "GOLD",
        };
        await _resourceService.GrantAsync(accountId, rewardType, 10, "DUNGEON_SWEEP", dungeonType);

        var rewardBalance = await _resourceService.GetBalanceAsync(accountId, rewardType);
        var delta = new StateDeltaBuilder()
            .AddResource(rewardType, (float)rewardBalance)
            .SetDungeons(sharedProgress.DailyRunsUsed)
            .Build();

        return ApiResponse<DungeonSweepResponse>.Ok(
            new DungeonSweepResponse { Reward = new RewardData { Type = rewardType, Amount = 10 } },
            delta);
    }

    public async Task<ApiResponse<GoblinMineResponse>> GoblinMineAsync(Guid accountId)
    {
        var spent = await _resourceService.SpendAsync(accountId, "PICKAXE", 1, "GOBLIN_MINE");
        if (!spent)
            return ApiResponse<GoblinMineResponse>.Fail("INSUFFICIENT_PICKAXE");

        var oreGained = Random.Shared.Next(1, 5);
        var progress = await GetOrCreateProgressAsync(accountId, "GOBLIN");
        progress.HighestStage += oreGained;
        progress.UpdatedAt = DateTime.UtcNow;
        await _contentRepo.UpsertProgressAsync(progress);

        var pickaxeBalance = await _resourceService.GetBalanceAsync(accountId, "PICKAXE");
        var delta = new StateDeltaBuilder()
            .AddResource("PICKAXE", (float)pickaxeBalance)
            .SetGoblinOreCount(progress.HighestStage)
            .Build();

        return ApiResponse<GoblinMineResponse>.Ok(
            new GoblinMineResponse { OreGained = oreGained },
            delta);
    }

    public async Task<ApiResponse<GoblinCartResponse>> GoblinCartAsync(Guid accountId)
    {
        var progress = await GetOrCreateProgressAsync(accountId, "GOBLIN");
        if (progress.HighestStage < 30)
            return ApiResponse<GoblinCartResponse>.Fail("INSUFFICIENT_ORE");

        var goldReward = progress.HighestStage * 50.0;
        progress.HighestStage = 0;
        progress.UpdatedAt = DateTime.UtcNow;
        await _contentRepo.UpsertProgressAsync(progress);

        await _resourceService.GrantAsync(accountId, "GOLD", goldReward, "GOBLIN_CART");

        var goldBalance = await _resourceService.GetBalanceAsync(accountId, "GOLD");
        var delta = new StateDeltaBuilder()
            .AddResource("GOLD", (float)goldBalance)
            .SetGoblinOreCount(0)
            .Build();

        return ApiResponse<GoblinCartResponse>.Ok(
            new GoblinCartResponse { Reward = new RewardData { Type = "GOLD", Amount = goldReward } },
            delta);
    }

    public async Task<ApiResponse<object>> CatacombStartAsync(Guid accountId)
    {
        var progress = await GetOrCreateProgressAsync(accountId, "CATACOMB");
        if (progress.DailyRunsUsed > 0)
            return ApiResponse<object>.Fail("RUN_ALREADY_ACTIVE");

        progress.DailyRunsUsed = 1;
        progress.UpdatedAt = DateTime.UtcNow;
        await _contentRepo.UpsertProgressAsync(progress);

        var delta = new StateDeltaBuilder()
            .SetCatacomb(progress.HighestStage, 0, true)
            .Build();

        return ApiResponse<object>.Ok(delta: delta);
    }

    public async Task<ApiResponse<CatacombBattleResponse>> CatacombBattleAsync(Guid accountId)
    {
        var progress = await GetOrCreateProgressAsync(accountId, "CATACOMB");
        if (progress.DailyRunsUsed <= 0)
            return ApiResponse<CatacombBattleResponse>.Fail("NO_ACTIVE_RUN");

        var currentFloor = progress.HighestStage + 1;
        var battleResult = Random.Shared.NextDouble() > 0.3 ? "VICTORY" : "DEFEAT";
        var continueRun = battleResult == "VICTORY";
        var reward = new RewardData();

        if (battleResult == "VICTORY")
        {
            progress.HighestStage = currentFloor;
        }

        if (!continueRun)
        {
            var goldReward = currentFloor * 200.0;
            await _resourceService.GrantAsync(accountId, "GOLD", goldReward, "CATACOMB_REWARD", currentFloor.ToString());
            progress.DailyRunsUsed = 0;
            reward.Type = "GOLD";
            reward.Amount = goldReward;
        }

        progress.UpdatedAt = DateTime.UtcNow;
        await _contentRepo.UpsertProgressAsync(progress);

        var deltaBuilder = new StateDeltaBuilder()
            .SetCatacomb(progress.HighestStage, currentFloor, continueRun);

        if (!continueRun)
        {
            var goldBalance = await _resourceService.GetBalanceAsync(accountId, "GOLD");
            deltaBuilder.AddResource("GOLD", (float)goldBalance);
        }

        return ApiResponse<CatacombBattleResponse>.Ok(
            new CatacombBattleResponse { BattleResult = battleResult, ContinueRun = continueRun, Reward = reward },
            deltaBuilder.Build());
    }

    public async Task<ApiResponse<CatacombEndResponse>> CatacombEndAsync(Guid accountId)
    {
        var progress = await GetOrCreateProgressAsync(accountId, "CATACOMB");
        if (progress.DailyRunsUsed <= 0)
            return ApiResponse<CatacombEndResponse>.Fail("NO_ACTIVE_RUN");

        var goldReward = progress.HighestStage * 200.0;
        await _resourceService.GrantAsync(accountId, "GOLD", goldReward, "CATACOMB_END_REWARD", progress.HighestStage.ToString());

        progress.DailyRunsUsed = 0;
        progress.UpdatedAt = DateTime.UtcNow;
        await _contentRepo.UpsertProgressAsync(progress);

        var goldBalance = await _resourceService.GetBalanceAsync(accountId, "GOLD");
        var delta = new StateDeltaBuilder()
            .AddResource("GOLD", (float)goldBalance)
            .SetCatacomb(progress.HighestStage, null, false)
            .Build();

        return ApiResponse<CatacombEndResponse>.Ok(
            new CatacombEndResponse { Reward = new RewardData { Type = "GOLD", Amount = goldReward } },
            delta);
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

public class RewardData
{
    public string Type { get; set; } = string.Empty;
    public double Amount { get; set; }
}

public class TowerChallengeResponse
{
    public string BattleResult { get; set; } = string.Empty;
    public RewardData Reward { get; set; } = new();
}

public class DungeonChallengeResponse
{
    public string BattleResult { get; set; } = string.Empty;
    public RewardData Reward { get; set; } = new();
}

public class DungeonSweepResponse
{
    public RewardData Reward { get; set; } = new();
}

public class GoblinMineResponse
{
    public int OreGained { get; set; }
}

public class GoblinCartResponse
{
    public RewardData Reward { get; set; } = new();
}

public class CatacombBattleResponse
{
    public string BattleResult { get; set; } = string.Empty;
    public bool ContinueRun { get; set; }
    public RewardData Reward { get; set; } = new();
}

public class CatacombEndResponse
{
    public RewardData Reward { get; set; } = new();
}
