using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;

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

    public async Task<TalentUpgradeResult> UpgradeAsync(Guid accountId, string statType)
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
        {
            return new TalentUpgradeResult { Success = false, Error = "MAX_SUB_LEVEL_REACHED" };
        }

        var tier = state.TotalLevel / LevelsPerSubGrade;
        var cost = Math.Floor(BaseCost * Math.Pow(CostGrowth, tier));

        var spent = await _resourceService.SpendAsync(accountId, "GOLD", cost, "TALENT_UPGRADE");
        if (!spent)
            return new TalentUpgradeResult { Success = false, Error = "INSUFFICIENT_GOLD" };

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

        return new TalentUpgradeResult { Success = true, State = state };
    }

    public async Task<TalentClaimResult> ClaimMilestoneAsync(Guid accountId, int milestoneLevel)
    {
        var state = await GetStatusAsync(accountId);

        if (state.TotalLevel < milestoneLevel)
            return new TalentClaimResult { Success = false, Error = "LEVEL_NOT_REACHED" };

        if (state.ClaimedMilestones.Contains($"\"{milestoneLevel}\""))
            return new TalentClaimResult { Success = false, Error = "ALREADY_CLAIMED" };

        var claimed = System.Text.Json.JsonSerializer.Deserialize<List<int>>(state.ClaimedMilestones) ?? new();
        claimed.Add(milestoneLevel);
        state.ClaimedMilestones = System.Text.Json.JsonSerializer.Serialize(claimed);

        var milestoneIndex = milestoneLevel / 10;
        if (milestoneIndex % 2 == 0)
        {
            var tier = milestoneLevel / LevelsPerSubGrade;
            var goldReward = Math.Floor(BaseCost * Math.Pow(CostGrowth, tier)) * 5;
            await _resourceService.GrantAsync(accountId, "GOLD", goldReward, "TALENT_MILESTONE", milestoneLevel.ToString());
        }

        state.UpdatedAt = DateTime.UtcNow;
        await _talentRepo.UpsertAsync(state);

        return new TalentClaimResult { Success = true };
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

public class TalentUpgradeResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public TalentState? State { get; set; }
}

public class TalentClaimResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
}
