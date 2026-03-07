using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using CatCatGo.Shared.Models;

namespace CatCatGo.Server.Core.Services;

public class TalentService
{
    private readonly ITalentRepository _talentRepo;
    private readonly ResourceService _resourceService;

    private static readonly string[] Grades = { "TRAINEE", "ADVENTURER", "ELITE", "MASTER", "WARRIOR", "HERO" };
    private const double BaseCost = 100;
    private const double CostGrowth = 1.10;
    private const int MaxSubLevel = 10;
    private const int LevelsPerSubGrade = 30;

    public TalentService(ITalentRepository talentRepo, ResourceService resourceService)
    {
        _talentRepo = talentRepo;
        _resourceService = resourceService;
    }

    public async Task<TalentState> GetStatusAsync(Guid accountId)
    {
        var state = await _talentRepo.GetByAccountIdAsync(accountId);
        if (state == null)
        {
            state = new TalentState
            {
                AccountId = accountId,
                UpdatedAt = DateTime.UtcNow,
            };
            await _talentRepo.UpsertAsync(state);
        }
        return state;
    }

    public async Task<ApiResponse<object>> UpgradeAsync(Guid accountId, string statType)
    {
        var state = await GetStatusAsync(accountId);

        var currentLevel = statType switch
        {
            "ATK" => state.AtkLevel,
            "HP" => state.HpLevel,
            "DEF" => state.DefLevel,
            _ => throw new ArgumentException($"Invalid stat type: {statType}"),
        };

        var subLevelInGrade = currentLevel % MaxSubLevel;
        if (subLevelInGrade >= MaxSubLevel - 1 && currentLevel > 0)
            return ApiResponse<object>.Fail("MAX_SUB_LEVEL_REACHED");

        var tier = state.TotalLevel / LevelsPerSubGrade;
        var cost = Math.Floor(BaseCost * Math.Pow(CostGrowth, tier));

        var spent = await _resourceService.SpendAsync(accountId, "GOLD", cost, "TALENT_UPGRADE");
        if (!spent)
            return ApiResponse<object>.Fail("INSUFFICIENT_GOLD");

        switch (statType)
        {
            case "ATK": state.AtkLevel++; break;
            case "HP": state.HpLevel++; break;
            case "DEF": state.DefLevel++; break;
        }
        state.TotalLevel = state.AtkLevel + state.HpLevel + state.DefLevel;

        CheckPromotion(state);
        state.UpdatedAt = DateTime.UtcNow;
        await _talentRepo.UpsertAsync(state);

        var goldBalance = await _resourceService.GetBalanceAsync(accountId, "GOLD");
        var delta = new StateDeltaBuilder()
            .AddResource("GOLD", (float)goldBalance)
            .SetTalent(state.AtkLevel, state.HpLevel, state.DefLevel)
            .Build();

        return ApiResponse<object>.Ok(delta: delta);
    }

    public async Task<ApiResponse<TalentMilestoneResponse>> ClaimMilestoneAsync(Guid accountId, int milestoneLevel)
    {
        var state = await GetStatusAsync(accountId);

        if (state.TotalLevel < milestoneLevel)
            return ApiResponse<TalentMilestoneResponse>.Fail("LEVEL_NOT_REACHED");

        var claimed = System.Text.Json.JsonSerializer.Deserialize<List<int>>(state.ClaimedMilestones) ?? new();
        if (claimed.Contains(milestoneLevel))
            return ApiResponse<TalentMilestoneResponse>.Fail("ALREADY_CLAIMED");

        claimed.Add(milestoneLevel);
        state.ClaimedMilestones = System.Text.Json.JsonSerializer.Serialize(claimed);

        string? rewardType = null;
        double rewardAmount = 0;

        var milestoneIndex = milestoneLevel / 10;
        if (milestoneIndex % 2 == 0)
        {
            var tierVal = milestoneLevel / LevelsPerSubGrade;
            var goldReward = Math.Floor(BaseCost * Math.Pow(CostGrowth, tierVal)) * 5;
            await _resourceService.GrantAsync(accountId, "GOLD", goldReward, "TALENT_MILESTONE", milestoneLevel.ToString());
            rewardType = "GOLD";
            rewardAmount = goldReward;
        }

        state.UpdatedAt = DateTime.UtcNow;
        await _talentRepo.UpsertAsync(state);

        var deltaBuilder = new StateDeltaBuilder()
            .AddClaimedMilestone(milestoneLevel.ToString());

        if (rewardType != null)
        {
            var balance = await _resourceService.GetBalanceAsync(accountId, rewardType);
            deltaBuilder.AddResource(rewardType, (float)balance);
        }

        return ApiResponse<TalentMilestoneResponse>.Ok(
            new TalentMilestoneResponse { RewardType = rewardType, RewardAmount = rewardAmount },
            deltaBuilder.Build());
    }

    public async Task<ApiResponse<TalentClaimAllResponse>> ClaimAllMilestonesAsync(Guid accountId)
    {
        var state = await GetStatusAsync(accountId);
        var claimed = System.Text.Json.JsonSerializer.Deserialize<List<int>>(state.ClaimedMilestones) ?? new();
        var claimedCount = 0;
        var deltaBuilder = new StateDeltaBuilder();

        for (int milestone = 10; milestone <= state.TotalLevel; milestone += 10)
        {
            if (claimed.Contains(milestone)) continue;

            claimed.Add(milestone);
            deltaBuilder.AddClaimedMilestone(milestone.ToString());
            claimedCount++;

            var milestoneIndex = milestone / 10;
            if (milestoneIndex % 2 == 0)
            {
                var tierVal = milestone / LevelsPerSubGrade;
                var goldReward = Math.Floor(BaseCost * Math.Pow(CostGrowth, tierVal)) * 5;
                await _resourceService.GrantAsync(accountId, "GOLD", goldReward, "TALENT_MILESTONE", milestone.ToString());
            }
        }

        if (claimedCount > 0)
        {
            state.ClaimedMilestones = System.Text.Json.JsonSerializer.Serialize(claimed);
            state.UpdatedAt = DateTime.UtcNow;
            await _talentRepo.UpsertAsync(state);

            var goldBalance = await _resourceService.GetBalanceAsync(accountId, "GOLD");
            deltaBuilder.AddResource("GOLD", (float)goldBalance);
        }

        return ApiResponse<TalentClaimAllResponse>.Ok(
            new TalentClaimAllResponse { ClaimedCount = claimedCount },
            deltaBuilder.Build());
    }

    private static void CheckPromotion(TalentState state)
    {
        var atkInSub = state.AtkLevel % MaxSubLevel;
        var hpInSub = state.HpLevel % MaxSubLevel;
        var defInSub = state.DefLevel % MaxSubLevel;

        if (atkInSub == 0 && hpInSub == 0 && defInSub == 0 && state.TotalLevel > 0 && state.TotalLevel % LevelsPerSubGrade == 0)
        {
            var subGradeTotal = state.TotalLevel / LevelsPerSubGrade;
            var gradeIndex = 0;
            var subGradeSums = new[] { 3, 7, 12, 18, 25, 33 };
            for (int i = 0; i < subGradeSums.Length; i++)
            {
                if (subGradeTotal <= subGradeSums[i])
                {
                    gradeIndex = i;
                    break;
                }
            }
            state.Grade = gradeIndex < Grades.Length ? Grades[gradeIndex] : Grades[^1];
            state.SubGrade = subGradeTotal - (gradeIndex > 0 ? subGradeSums[gradeIndex - 1] : 0);
        }
    }
}

public class TalentMilestoneResponse
{
    public string? RewardType { get; set; }
    public double RewardAmount { get; set; }
}

public class TalentClaimAllResponse
{
    public int ClaimedCount { get; set; }
}
