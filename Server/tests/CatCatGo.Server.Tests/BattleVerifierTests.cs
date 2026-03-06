using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using CatCatGo.Server.Core.Services;
using CatCatGo.Shared.Requests;
using NSubstitute;
using Xunit;

namespace CatCatGo.Server.Tests;

public class BattleVerifierTests
{
    private readonly ICheatFlagRepository _cheatFlagRepo;
    private readonly BattleVerifier _sut;

    public BattleVerifierTests()
    {
        _cheatFlagRepo = Substitute.For<ICheatFlagRepository>();
        _sut = new BattleVerifier(_cheatFlagRepo);
    }

    [Fact]
    public void StartBattle_ReturnsValidBattleIdAndSeed()
    {
        var accountId = Guid.NewGuid();
        var result = _sut.StartBattle(accountId, new BattleStartRequest
        {
            ChapterId = 1,
            Day = 5,
            EncounterType = "COMBAT"
        });

        Assert.NotEmpty(result.BattleId);
        Assert.InRange(result.Seed, 0, 999999);
    }

    [Fact]
    public async Task VerifyReportAsync_ValidReport_ReturnsVerified()
    {
        var accountId = Guid.NewGuid();
        var startResult = _sut.StartBattle(accountId, new BattleStartRequest
        {
            ChapterId = 1, Day = 1, EncounterType = "COMBAT"
        });

        await Task.Delay(600);

        var report = await _sut.VerifyReportAsync(accountId, new BattleReportRequest
        {
            BattleId = startResult.BattleId,
            Seed = startResult.Seed,
            Result = "WIN",
            TurnCount = 1,
            PlayerSkillIds = new List<string> { "slash" },
            EnemyTemplateId = "ant_worker",
            GoldReward = 100
        });

        Assert.True(report.Verified);
        Assert.Null(report.Error);
    }

    [Fact]
    public async Task VerifyReportAsync_InvalidBattleId_ReturnsError()
    {
        var accountId = Guid.NewGuid();

        var report = await _sut.VerifyReportAsync(accountId, new BattleReportRequest
        {
            BattleId = "nonexistent-id",
            Seed = 12345,
            Result = "WIN",
            TurnCount = 5,
            PlayerSkillIds = new List<string>(),
            EnemyTemplateId = "ant_worker",
            GoldReward = 0
        });

        Assert.False(report.Verified);
        Assert.Equal("INVALID_BATTLE_ID", report.Error);
    }

    [Fact]
    public async Task VerifyReportAsync_AccountMismatch_ReturnsError()
    {
        var owner = Guid.NewGuid();
        var attacker = Guid.NewGuid();

        var startResult = _sut.StartBattle(owner, new BattleStartRequest
        {
            ChapterId = 1, Day = 1, EncounterType = "COMBAT"
        });

        var report = await _sut.VerifyReportAsync(attacker, new BattleReportRequest
        {
            BattleId = startResult.BattleId,
            Seed = startResult.Seed,
            Result = "WIN",
            TurnCount = 5,
            PlayerSkillIds = new List<string>(),
            EnemyTemplateId = "ant_worker",
            GoldReward = 0
        });

        Assert.False(report.Verified);
        Assert.Equal("ACCOUNT_MISMATCH", report.Error);
    }

    [Fact]
    public async Task VerifyReportAsync_AlreadyCompleted_ReturnsError()
    {
        var accountId = Guid.NewGuid();
        var startResult = _sut.StartBattle(accountId, new BattleStartRequest
        {
            ChapterId = 1, Day = 1, EncounterType = "COMBAT"
        });

        await Task.Delay(600);

        var request = new BattleReportRequest
        {
            BattleId = startResult.BattleId,
            Seed = startResult.Seed,
            Result = "WIN",
            TurnCount = 1,
            PlayerSkillIds = new List<string>(),
            EnemyTemplateId = "ant_worker",
            GoldReward = 0
        };

        await _sut.VerifyReportAsync(accountId, request);
        var secondReport = await _sut.VerifyReportAsync(accountId, request);

        Assert.False(secondReport.Verified);
        Assert.Equal("ALREADY_COMPLETED", secondReport.Error);
    }

    [Fact]
    public async Task VerifyReportAsync_SeedMismatch_FlagsCheatAndReturnsError()
    {
        var accountId = Guid.NewGuid();
        var startResult = _sut.StartBattle(accountId, new BattleStartRequest
        {
            ChapterId = 1, Day = 1, EncounterType = "COMBAT"
        });

        var report = await _sut.VerifyReportAsync(accountId, new BattleReportRequest
        {
            BattleId = startResult.BattleId,
            Seed = startResult.Seed + 999,
            Result = "WIN",
            TurnCount = 5,
            PlayerSkillIds = new List<string>(),
            EnemyTemplateId = "ant_worker",
            GoldReward = 0
        });

        Assert.False(report.Verified);
        Assert.Equal("SEED_MISMATCH", report.Error);
        await _cheatFlagRepo.Received(1).CreateAsync(Arg.Is<CheatFlag>(f =>
            f.Type == "SEED_MISMATCH" && f.Severity == 3 && f.AccountId == accountId));
    }

    [Fact]
    public async Task VerifyReportAsync_SpeedHack_FlagsCheatButVerifies()
    {
        var accountId = Guid.NewGuid();
        var startResult = _sut.StartBattle(accountId, new BattleStartRequest
        {
            ChapterId = 1, Day = 1, EncounterType = "COMBAT"
        });

        var report = await _sut.VerifyReportAsync(accountId, new BattleReportRequest
        {
            BattleId = startResult.BattleId,
            Seed = startResult.Seed,
            Result = "WIN",
            TurnCount = 100,
            PlayerSkillIds = new List<string>(),
            EnemyTemplateId = "ant_worker",
            GoldReward = 0
        });

        Assert.True(report.Verified);
        await _cheatFlagRepo.Received(1).CreateAsync(Arg.Is<CheatFlag>(f =>
            f.Type == "SPEED_HACK" && f.Severity == 1));
    }
}
