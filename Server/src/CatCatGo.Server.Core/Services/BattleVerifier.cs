using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using CatCatGo.Shared.Requests;
using CatCatGo.Shared.Responses;

namespace CatCatGo.Server.Core.Services;

public class BattleVerifier
{
    private readonly ICheatFlagRepository _cheatFlagRepo;
    private readonly Dictionary<string, BattleSession> _activeSessions = new();

    public BattleVerifier(ICheatFlagRepository cheatFlagRepo)
    {
        _cheatFlagRepo = cheatFlagRepo;
    }

    public BattleStartResponse StartBattle(Guid accountId, BattleStartRequest request)
    {
        var battleId = Guid.NewGuid().ToString();
        var seed = Random.Shared.Next(0, 999999);

        var session = new BattleSession
        {
            BattleId = battleId,
            AccountId = accountId,
            Seed = seed,
            ChapterId = request.ChapterId,
            Day = request.Day,
            EncounterType = request.EncounterType,
            CreatedAt = DateTime.UtcNow,
        };
        _activeSessions[battleId] = session;

        return new BattleStartResponse
        {
            BattleId = battleId,
            Seed = seed,
        };
    }

    public async Task<BattleReportResponse> VerifyReportAsync(Guid accountId, BattleReportRequest request)
    {
        if (!_activeSessions.TryGetValue(request.BattleId, out var session))
            return new BattleReportResponse { Verified = false, Error = "INVALID_BATTLE_ID" };

        if (session.AccountId != accountId)
            return new BattleReportResponse { Verified = false, Error = "ACCOUNT_MISMATCH" };

        if (session.IsCompleted)
            return new BattleReportResponse { Verified = false, Error = "ALREADY_COMPLETED" };

        if (session.Seed != request.Seed)
        {
            await FlagCheatAsync(accountId, "SEED_MISMATCH", $"Expected {session.Seed}, got {request.Seed}");
            return new BattleReportResponse { Verified = false, Error = "SEED_MISMATCH" };
        }

        var elapsed = DateTime.UtcNow - session.CreatedAt;
        if (elapsed.TotalSeconds < request.TurnCount * 0.5)
        {
            await FlagCheatAsync(accountId, "SPEED_HACK",
                $"Turns: {request.TurnCount}, elapsed: {elapsed.TotalSeconds:F1}s");
        }

        // TODO: Domain 어셈블리 참조 후 전투 재현 검증 구현
        // var rng = new SeededRandom(session.Seed);
        // var battle = ReplayBattle(rng, session, request);
        // if (battle.State != request.Result) → BATTLE_MISMATCH flag

        session.IsCompleted = true;

        return new BattleReportResponse
        {
            Verified = true,
            RewardsJson = null,
        };
    }

    private async Task FlagCheatAsync(Guid accountId, string type, string details)
    {
        await _cheatFlagRepo.CreateAsync(new CheatFlag
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            Type = type,
            Severity = type == "SPEED_HACK" ? 1 : 3,
            Details = details,
            CreatedAt = DateTime.UtcNow,
        });
    }
}
