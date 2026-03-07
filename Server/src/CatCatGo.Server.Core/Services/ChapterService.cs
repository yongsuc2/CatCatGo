using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using CatCatGo.Shared.Models;

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

    public async Task<ApiResponse<ChapterStartResponse>> StartAsync(Guid accountId, int chapterId, string chapterType)
    {
        var activeSession = await _chapterRepo.GetActiveSessionAsync(accountId);
        if (activeSession != null)
            return ApiResponse<ChapterStartResponse>.Fail("SESSION_ALREADY_ACTIVE");

        var progress = await _chapterRepo.GetProgressAsync(accountId);
        var expectedChapter = (progress?.ClearedChapterMax ?? 0) + 1;
        if (chapterId != expectedChapter)
            return ApiResponse<ChapterStartResponse>.Fail("INVALID_CHAPTER_ID");

        var spent = await _resourceService.SpendAsync(accountId, "STAMINA", StaminaCost, "CHAPTER_START");
        if (!spent)
            return ApiResponse<ChapterStartResponse>.Fail("INSUFFICIENT_STAMINA");

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

        var staminaBalance = await _resourceService.GetBalanceAsync(accountId, "STAMINA");
        var delta = new StateDeltaBuilder()
            .AddResource("STAMINA", (float)staminaBalance)
            .SetChapterSession(new ChapterSessionDelta
            {
                SessionId = session.Id.ToString(),
                CurrentDay = 1,
                SessionRerollsRemaining = MaxRerollsPerSession,
                SessionEnded = false,
            })
            .Build();

        return ApiResponse<ChapterStartResponse>.Ok(
            new ChapterStartResponse { SessionId = session.Id.ToString(), ChapterId = chapterId, Seed = seed },
            delta);
    }

    public async Task<ApiResponse<AdvanceDayResponse>> AdvanceDayAsync(Guid accountId, string sessionId)
    {
        var session = await _chapterRepo.GetActiveSessionAsync(accountId);
        if (session == null || session.Id.ToString() != sessionId)
            return ApiResponse<AdvanceDayResponse>.Fail("NO_ACTIVE_SESSION");

        var encounterType = DetermineEncounterType(session);
        session.PendingEncounter = $"{{\"type\":\"{encounterType}\",\"day\":{session.CurrentDay}}}";

        string? battleRequired = "NONE";
        int? battleSeed = null;
        string? enemyTemplateId = null;

        if (encounterType == "COMBAT")
        {
            battleRequired = DetermineBattleType(session.CurrentDay);
            battleSeed = session.Seed + session.CurrentDay * 100;
            enemyTemplateId = $"enemy_{battleRequired.ToLowerInvariant()}_{session.CurrentDay}";
        }

        if (encounterType is "CHANCE" or "DEMON")
        {
            var skills = GenerateSkillChoices(session.Seed, session.CurrentDay);
            session.PendingSkillChoices = System.Text.Json.JsonSerializer.Serialize(skills);
        }

        session.UpdatedAt = DateTime.UtcNow;
        await _chapterRepo.UpdateSessionAsync(session);

        var delta = new StateDeltaBuilder()
            .SetChapterSession(new ChapterSessionDelta
            {
                SessionId = sessionId,
                CurrentDay = session.CurrentDay,
                JungbakCount = session.JungbakCounter,
                DaebakCount = session.DaebakCounter,
            })
            .Build();

        return ApiResponse<AdvanceDayResponse>.Ok(
            new AdvanceDayResponse
            {
                BattleRequired = battleRequired,
                BattleSeed = battleSeed,
                EnemyTemplateId = enemyTemplateId,
            },
            delta);
    }

    public async Task<ApiResponse<object>> ResolveEncounterAsync(Guid accountId, string sessionId, int choiceIndex)
    {
        var session = await _chapterRepo.GetActiveSessionAsync(accountId);
        if (session == null || session.Id.ToString() != sessionId)
            return ApiResponse<object>.Fail("NO_ACTIVE_SESSION");

        session.CurrentDay++;
        session.PendingEncounter = "{}";
        session.PendingSkillChoices = "[]";

        var isComplete = session.CurrentDay > TotalDays;
        if (isComplete)
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

        var delta = new StateDeltaBuilder()
            .SetChapterSession(new ChapterSessionDelta
            {
                SessionId = sessionId,
                CurrentDay = session.CurrentDay,
                SessionEnded = isComplete,
            })
            .Build();

        return ApiResponse<object>.Ok(delta: delta);
    }

    public async Task<ApiResponse<object>> RerollAsync(Guid accountId, string sessionId)
    {
        var session = await _chapterRepo.GetActiveSessionAsync(accountId);
        if (session == null || session.Id.ToString() != sessionId)
            return ApiResponse<object>.Fail("NO_ACTIVE_SESSION");

        if (session.RerollsUsed >= MaxRerollsPerSession)
            return ApiResponse<object>.Fail("NO_REROLLS_REMAINING");

        session.RerollsUsed++;
        var skills = GenerateSkillChoices(session.Seed + session.RerollsUsed, session.CurrentDay);
        session.PendingSkillChoices = System.Text.Json.JsonSerializer.Serialize(skills);
        session.UpdatedAt = DateTime.UtcNow;
        await _chapterRepo.UpdateSessionAsync(session);

        var delta = new StateDeltaBuilder()
            .SetChapterSession(new ChapterSessionDelta
            {
                SessionId = sessionId,
                SessionRerollsRemaining = MaxRerollsPerSession - session.RerollsUsed,
            })
            .Build();

        return ApiResponse<object>.Ok(delta: delta);
    }

    public async Task<ApiResponse<object>> SelectSkillAsync(Guid accountId, string sessionId, string skillId)
    {
        var session = await _chapterRepo.GetActiveSessionAsync(accountId);
        if (session == null || session.Id.ToString() != sessionId)
            return ApiResponse<object>.Fail("NO_ACTIVE_SESSION");

        var choices = System.Text.Json.JsonSerializer.Deserialize<List<string>>(session.PendingSkillChoices) ?? new();
        if (!choices.Contains(skillId))
            return ApiResponse<object>.Fail("INVALID_SKILL_ID");

        var sessionSkills = System.Text.Json.JsonSerializer.Deserialize<List<string>>(session.SessionSkills) ?? new();
        sessionSkills.Add(skillId);
        session.SessionSkills = System.Text.Json.JsonSerializer.Serialize(sessionSkills);
        session.PendingSkillChoices = "[]";
        session.UpdatedAt = DateTime.UtcNow;
        await _chapterRepo.UpdateSessionAsync(session);

        var delta = new StateDeltaBuilder()
            .SetChapterSession(new ChapterSessionDelta
            {
                SessionId = sessionId,
                SessionSkillIds = sessionSkills,
            })
            .Build();

        return ApiResponse<object>.Ok(delta: delta);
    }

    public async Task<ApiResponse<BattleResultResponse>> BattleResultAsync(Guid accountId, string sessionId, int battleSeed, string result, int turnCount, int playerRemainingHp)
    {
        var session = await _chapterRepo.GetActiveSessionAsync(accountId);
        if (session == null || session.Id.ToString() != sessionId)
            return ApiResponse<BattleResultResponse>.Fail("NO_ACTIVE_SESSION");

        var deltaBuilder = new StateDeltaBuilder();
        var goldEarned = 0;

        var isBoss = session.CurrentDay >= TotalDays;

        if (result == "VICTORY")
        {
            goldEarned = isBoss ? session.ChapterId * 500 : session.CurrentDay * 10;
            await _resourceService.GrantAsync(accountId, "GOLD", goldEarned, "CHAPTER_BATTLE", sessionId);

            if (isBoss)
            {
                await _resourceService.GrantAsync(accountId, "GEMS", 100, "CHAPTER_CLEAR", sessionId);
                session.IsActive = false;

                var progress = await _chapterRepo.GetProgressAsync(accountId) ?? new ChapterProgress { AccountId = accountId };
                progress.ClearedChapterMax = Math.Max(progress.ClearedChapterMax, session.ChapterId);
                progress.UpdatedAt = DateTime.UtcNow;
                await _chapterRepo.UpsertProgressAsync(progress);

                deltaBuilder.SetClearedChapterMax(progress.ClearedChapterMax);

                var gemsBalance = await _resourceService.GetBalanceAsync(accountId, "GEMS");
                deltaBuilder.AddResource("GEMS", (float)gemsBalance);
            }

            session.CurrentDay++;
        }
        else
        {
            session.IsActive = false;
        }

        var progress2 = await _chapterRepo.GetProgressAsync(accountId) ?? new ChapterProgress { AccountId = accountId };
        var bestDays = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(progress2.BestSurvivalDays) ?? new();
        var key = session.ChapterId.ToString();
        bestDays[key] = Math.Max(bestDays.GetValueOrDefault(key, 0), session.CurrentDay);
        progress2.BestSurvivalDays = System.Text.Json.JsonSerializer.Serialize(bestDays);
        progress2.UpdatedAt = DateTime.UtcNow;
        await _chapterRepo.UpsertProgressAsync(progress2);

        session.BestSurvivalDays = session.CurrentDay;
        session.UpdatedAt = DateTime.UtcNow;
        await _chapterRepo.UpdateSessionAsync(session);

        var goldBalance = await _resourceService.GetBalanceAsync(accountId, "GOLD");
        deltaBuilder.AddResource("GOLD", (float)goldBalance);
        deltaBuilder.SetBestSurvivalDays(bestDays);
        deltaBuilder.SetChapterSession(new ChapterSessionDelta
        {
            SessionId = sessionId,
            CurrentDay = session.CurrentDay,
            SessionEnded = !session.IsActive,
        });

        return ApiResponse<BattleResultResponse>.Ok(
            new BattleResultResponse { Verified = true, GoldEarned = goldEarned },
            deltaBuilder.Build());
    }

    public async Task<ApiResponse<object>> AbandonAsync(Guid accountId, string sessionId)
    {
        var session = await _chapterRepo.GetActiveSessionAsync(accountId);
        if (session == null || (sessionId != null && session.Id.ToString() != sessionId))
            return ApiResponse<object>.Fail("NO_ACTIVE_SESSION");

        session.IsActive = false;
        session.BestSurvivalDays = session.CurrentDay;
        session.UpdatedAt = DateTime.UtcNow;
        await _chapterRepo.UpdateSessionAsync(session);

        var progress = await _chapterRepo.GetProgressAsync(accountId) ?? new ChapterProgress { AccountId = accountId };
        var bestDays = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(progress.BestSurvivalDays) ?? new();
        var chapterKey = session.ChapterId.ToString();
        bestDays[chapterKey] = Math.Max(bestDays.GetValueOrDefault(chapterKey, 0), session.CurrentDay);
        progress.BestSurvivalDays = System.Text.Json.JsonSerializer.Serialize(bestDays);
        progress.UpdatedAt = DateTime.UtcNow;
        await _chapterRepo.UpsertProgressAsync(progress);

        var delta = new StateDeltaBuilder()
            .SetBestSurvivalDays(bestDays)
            .SetChapterSession(new ChapterSessionDelta
            {
                SessionId = session.Id.ToString(),
                SessionEnded = true,
            })
            .Build();

        return ApiResponse<object>.Ok(delta: delta);
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

    private static string DetermineBattleType(int day) => day switch
    {
        60 => "BOSS",
        50 => "MIDBOSS",
        30 or 40 => "ELITE",
        _ => "COMBAT",
    };

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

public class ChapterStartResponse
{
    public string SessionId { get; set; } = string.Empty;
    public int ChapterId { get; set; }
    public int Seed { get; set; }
}

public class AdvanceDayResponse
{
    public string BattleRequired { get; set; } = "NONE";
    public int? BattleSeed { get; set; }
    public string? EnemyTemplateId { get; set; }
}

public class BattleResultResponse
{
    public bool Verified { get; set; }
    public int GoldEarned { get; set; }
}

public class ChapterStateResult
{
    public bool HasActiveSession { get; set; }
    public ChapterSession? Session { get; set; }
    public int ClearedChapterMax { get; set; }
}
