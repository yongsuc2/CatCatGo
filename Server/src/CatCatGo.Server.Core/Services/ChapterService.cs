using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;

namespace CatCatGo.Server.Core.Services;

public class ChapterService
{
    private readonly IChapterRepository _chapterRepo;
    private readonly ResourceService _resourceService;

    private const double StaminaCost = 5;
    private const int TotalDays = 60;
    private const int MaxRerollsPerSession = 2;

    public ChapterService(IChapterRepository chapterRepo, ResourceService resourceService)
    {
        _chapterRepo = chapterRepo;
        _resourceService = resourceService;
    }

    public async Task<ChapterStartResult> StartAsync(Guid accountId)
    {
        var activeSession = await _chapterRepo.GetActiveSessionAsync(accountId);
        if (activeSession != null)
            return new ChapterStartResult { Success = false, Error = "SESSION_ALREADY_ACTIVE" };

        var spent = await _resourceService.SpendAsync(accountId, "STAMINA", StaminaCost, "CHAPTER_START");
        if (!spent)
            return new ChapterStartResult { Success = false, Error = "INSUFFICIENT_STAMINA" };

        var progress = await _chapterRepo.GetProgressAsync(accountId);
        var chapterId = (progress?.ClearedChapterMax ?? 0) + 1;
        var seed = Random.Shared.Next(0, 999999);

        var session = new ChapterSession
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            ChapterId = chapterId,
            CurrentDay = 1,
            Seed = seed,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        await _chapterRepo.CreateSessionAsync(session);

        return new ChapterStartResult { Success = true, SessionId = session.Id.ToString(), ChapterId = chapterId, Seed = seed };
    }

    public async Task<EncounterResult> GenerateEncounterAsync(Guid accountId)
    {
        var session = await _chapterRepo.GetActiveSessionAsync(accountId);
        if (session == null)
            return new EncounterResult { Success = false, Error = "NO_ACTIVE_SESSION" };

        var encounterType = DetermineEncounterType(session);
        session.PendingEncounter = $"{{\"type\":\"{encounterType}\",\"day\":{session.CurrentDay}}}";

        if (encounterType is "CHANCE" or "DEMON")
        {
            var skills = GenerateSkillChoices(session.Seed, session.CurrentDay);
            session.PendingSkillChoices = System.Text.Json.JsonSerializer.Serialize(skills);
        }

        session.UpdatedAt = DateTime.UtcNow;
        await _chapterRepo.UpdateSessionAsync(session);

        return new EncounterResult { Success = true, EncounterType = encounterType, Day = session.CurrentDay };
    }

    public async Task<EncounterResolveResult> ResolveEncounterAsync(Guid accountId, string choice)
    {
        var session = await _chapterRepo.GetActiveSessionAsync(accountId);
        if (session == null)
            return new EncounterResolveResult { Success = false, Error = "NO_ACTIVE_SESSION" };

        session.CurrentDay++;
        session.PendingEncounter = "{}";
        session.PendingSkillChoices = "[]";

        if (session.CurrentDay > TotalDays)
        {
            session.IsActive = false;
            var progress = await _chapterRepo.GetProgressAsync(accountId) ?? new ChapterProgress
            {
                AccountId = accountId,
            };
            progress.ClearedChapterMax = Math.Max(progress.ClearedChapterMax, session.ChapterId);
            progress.UpdatedAt = DateTime.UtcNow;
            await _chapterRepo.UpsertProgressAsync(progress);
        }

        session.UpdatedAt = DateTime.UtcNow;
        await _chapterRepo.UpdateSessionAsync(session);

        return new EncounterResolveResult { Success = true, NewDay = session.CurrentDay, IsChapterComplete = session.CurrentDay > TotalDays };
    }

    public async Task<SkillSelectResult> SelectSkillAsync(Guid accountId, int skillIndex)
    {
        var session = await _chapterRepo.GetActiveSessionAsync(accountId);
        if (session == null)
            return new SkillSelectResult { Success = false, Error = "NO_ACTIVE_SESSION" };

        var choices = System.Text.Json.JsonSerializer.Deserialize<List<string>>(session.PendingSkillChoices) ?? new();
        if (skillIndex < 0 || skillIndex >= choices.Count)
            return new SkillSelectResult { Success = false, Error = "INVALID_SKILL_INDEX" };

        var sessionSkills = System.Text.Json.JsonSerializer.Deserialize<List<string>>(session.SessionSkills) ?? new();
        sessionSkills.Add(choices[skillIndex]);
        session.SessionSkills = System.Text.Json.JsonSerializer.Serialize(sessionSkills);
        session.PendingSkillChoices = "[]";
        session.UpdatedAt = DateTime.UtcNow;
        await _chapterRepo.UpdateSessionAsync(session);

        return new SkillSelectResult { Success = true, SelectedSkill = choices[skillIndex] };
    }

    public async Task<SkillRerollResult> RerollSkillsAsync(Guid accountId)
    {
        var session = await _chapterRepo.GetActiveSessionAsync(accountId);
        if (session == null)
            return new SkillRerollResult { Success = false, Error = "NO_ACTIVE_SESSION" };

        if (session.RerollsUsed >= MaxRerollsPerSession)
            return new SkillRerollResult { Success = false, Error = "NO_REROLLS_REMAINING" };

        session.RerollsUsed++;
        var skills = GenerateSkillChoices(session.Seed + session.RerollsUsed, session.CurrentDay);
        session.PendingSkillChoices = System.Text.Json.JsonSerializer.Serialize(skills);
        session.UpdatedAt = DateTime.UtcNow;
        await _chapterRepo.UpdateSessionAsync(session);

        return new SkillRerollResult { Success = true, NewChoices = skills, RerollsRemaining = MaxRerollsPerSession - session.RerollsUsed };
    }

    public async Task<ChapterAbandonResult> AbandonAsync(Guid accountId)
    {
        var session = await _chapterRepo.GetActiveSessionAsync(accountId);
        if (session == null)
            return new ChapterAbandonResult { Success = false, Error = "NO_ACTIVE_SESSION" };

        session.IsActive = false;
        session.BestSurvivalDays = session.CurrentDay;
        session.UpdatedAt = DateTime.UtcNow;
        await _chapterRepo.UpdateSessionAsync(session);

        var progress = await _chapterRepo.GetProgressAsync(accountId) ?? new ChapterProgress { AccountId = accountId };
        var bestDays = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(progress.BestSurvivalDays) ?? new();
        var key = session.ChapterId.ToString();
        bestDays[key] = Math.Max(bestDays.GetValueOrDefault(key, 0), session.CurrentDay);
        progress.BestSurvivalDays = System.Text.Json.JsonSerializer.Serialize(bestDays);
        progress.UpdatedAt = DateTime.UtcNow;
        await _chapterRepo.UpsertProgressAsync(progress);

        return new ChapterAbandonResult { Success = true };
    }

    public async Task<ChapterStateResult> GetStateAsync(Guid accountId)
    {
        var session = await _chapterRepo.GetActiveSessionAsync(accountId);
        var progress = await _chapterRepo.GetProgressAsync(accountId);

        return new ChapterStateResult
        {
            HasActiveSession = session != null,
            Session = session,
            ClearedChapterMax = progress?.ClearedChapterMax ?? 0,
        };
    }

    public async Task<TreasureClaimResult> ClaimTreasureAsync(Guid accountId, int chapterId, string milestoneKey)
    {
        var progress = await _chapterRepo.GetProgressAsync(accountId);
        if (progress == null)
            return new TreasureClaimResult { Success = false, Error = "NO_PROGRESS" };

        var claimed = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, List<string>>>(progress.ClaimedTreasures) ?? new();
        var chapterKey = chapterId.ToString();
        if (!claimed.ContainsKey(chapterKey))
            claimed[chapterKey] = new();

        if (claimed[chapterKey].Contains(milestoneKey))
            return new TreasureClaimResult { Success = false, Error = "ALREADY_CLAIMED" };

        claimed[chapterKey].Add(milestoneKey);
        progress.ClaimedTreasures = System.Text.Json.JsonSerializer.Serialize(claimed);
        progress.UpdatedAt = DateTime.UtcNow;
        await _chapterRepo.UpsertProgressAsync(progress);

        var goldReward = chapterId * 100.0;
        await _resourceService.GrantAsync(accountId, "GOLD", goldReward, "CHAPTER_TREASURE", $"{chapterId}_{milestoneKey}");

        return new TreasureClaimResult { Success = true };
    }

    private static string DetermineEncounterType(ChapterSession session)
    {
        var forcedBattleDays = new[] { 20, 30, 40, 50, 60 };
        if (forcedBattleDays.Contains(session.CurrentDay))
            return "COMBAT";

        if (session.JungbakCounter >= 10)
        {
            session.JungbakCounter = 0;
            return "CHANCE";
        }

        if (session.DaebakCounter >= 30)
        {
            session.DaebakCounter = 0;
            return "DEMON";
        }

        var rng = new Random(session.Seed + session.CurrentDay);
        var roll = rng.Next(100);

        string encounterType = roll switch
        {
            < 40 => "COMBAT",
            < 47 => "DEMON",
            _ => "CHANCE",
        };

        if (encounterType == "COMBAT")
        {
            session.JungbakCounter++;
            session.DaebakCounter++;
        }
        else if (encounterType == "CHANCE")
        {
            session.JungbakCounter = 0;
        }
        else if (encounterType == "DEMON")
        {
            session.DaebakCounter = 0;
        }

        return encounterType;
    }

    private static List<string> GenerateSkillChoices(int seed, int day)
    {
        var rng = new Random(seed + day * 1000);
        return new List<string>
        {
            $"skill_{rng.Next(1, 50)}",
            $"skill_{rng.Next(1, 50)}",
            $"skill_{rng.Next(1, 50)}",
        };
    }
}

public class ChapterStartResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? SessionId { get; set; }
    public int ChapterId { get; set; }
    public int Seed { get; set; }
}

public class EncounterResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? EncounterType { get; set; }
    public int Day { get; set; }
}

public class EncounterResolveResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public int NewDay { get; set; }
    public bool IsChapterComplete { get; set; }
}

public class SkillSelectResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? SelectedSkill { get; set; }
}

public class SkillRerollResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public List<string> NewChoices { get; set; } = new();
    public int RerollsRemaining { get; set; }
}

public class ChapterAbandonResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
}

public class ChapterStateResult
{
    public bool HasActiveSession { get; set; }
    public ChapterSession? Session { get; set; }
    public int ClearedChapterMax { get; set; }
}

public class TreasureClaimResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
}
