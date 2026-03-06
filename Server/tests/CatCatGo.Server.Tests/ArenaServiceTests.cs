using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using CatCatGo.Server.Core.Services;
using NSubstitute;
using Xunit;

namespace CatCatGo.Server.Tests;

public class ArenaServiceTests
{
    private readonly IArenaRepository _arenaRepo;
    private readonly IAccountRepository _accountRepo;
    private readonly ArenaService _sut;

    public ArenaServiceTests()
    {
        _arenaRepo = Substitute.For<IArenaRepository>();
        _accountRepo = Substitute.For<IAccountRepository>();
        _sut = new ArenaService(_arenaRepo, _accountRepo);
    }

    [Fact]
    public async Task MatchAsync_NewUser_CreatesBronzeRankingAndReturnsOpponents()
    {
        var accountId = Guid.NewGuid();
        _arenaRepo.GetByAccountIdAsync(accountId).Returns((ArenaRanking?)null);
        _arenaRepo.GetByTierAsync("BRONZE", 20).Returns(new List<ArenaRanking>
        {
            CreateRanking(Guid.NewGuid(), "BRONZE", 100),
            CreateRanking(Guid.NewGuid(), "BRONZE", 200),
        });
        _accountRepo.GetByIdAsync(Arg.Any<Guid>()).Returns(new Account { DisplayName = "Opponent" });

        var result = await _sut.MatchAsync(accountId);

        Assert.NotNull(result);
        Assert.NotEmpty(result!.MatchId);
        Assert.True(result.Opponents.Count <= 3);
        await _arenaRepo.Received(1).UpsertAsync(Arg.Is<ArenaRanking>(r =>
            r.AccountId == accountId && r.Tier == "BRONZE" && r.Points == 0));
    }

    [Fact]
    public async Task MatchAsync_ExistingUser_UsesCurrentTier()
    {
        var accountId = Guid.NewGuid();
        _arenaRepo.GetByAccountIdAsync(accountId).Returns(new ArenaRanking
        {
            AccountId = accountId, Tier = "GOLD", Points = 3000
        });
        _arenaRepo.GetByTierAsync("GOLD", 20).Returns(new List<ArenaRanking>
        {
            CreateRanking(Guid.NewGuid(), "GOLD", 2800),
        });
        _accountRepo.GetByIdAsync(Arg.Any<Guid>()).Returns(new Account { DisplayName = "GoldPlayer" });

        var result = await _sut.MatchAsync(accountId);

        Assert.NotNull(result);
        await _arenaRepo.Received(1).GetByTierAsync("GOLD", 20);
    }

    [Fact]
    public async Task MatchAsync_ExcludesSelf()
    {
        var accountId = Guid.NewGuid();
        _arenaRepo.GetByAccountIdAsync(accountId).Returns(new ArenaRanking
        {
            AccountId = accountId, Tier = "BRONZE", Points = 0
        });
        _arenaRepo.GetByTierAsync("BRONZE", 20).Returns(new List<ArenaRanking>
        {
            new() { AccountId = accountId, Tier = "BRONZE", Points = 0 },
            CreateRanking(Guid.NewGuid(), "BRONZE", 100),
        });
        _accountRepo.GetByIdAsync(Arg.Any<Guid>()).Returns(new Account { DisplayName = "Player" });

        var result = await _sut.MatchAsync(accountId);

        Assert.NotNull(result);
        Assert.DoesNotContain(result!.Opponents, o => o.AccountId == accountId.ToString());
    }

    [Fact]
    public async Task UpdateResultAsync_FirstPlace_Adds30PointsAndWin()
    {
        var accountId = Guid.NewGuid();
        var ranking = new ArenaRanking
        {
            AccountId = accountId, Tier = "BRONZE", Points = 100, Wins = 5, Losses = 2
        };
        _arenaRepo.GetByAccountIdAsync(accountId).Returns(ranking);

        await _sut.UpdateResultAsync(accountId, 1);

        await _arenaRepo.Received(1).UpsertAsync(Arg.Is<ArenaRanking>(r =>
            r.Points == 130 && r.Wins == 6 && r.Losses == 2));
    }

    [Fact]
    public async Task UpdateResultAsync_SecondPlace_Adds15PointsAndWin()
    {
        var accountId = Guid.NewGuid();
        var ranking = new ArenaRanking
        {
            AccountId = accountId, Tier = "BRONZE", Points = 100, Wins = 5, Losses = 2
        };
        _arenaRepo.GetByAccountIdAsync(accountId).Returns(ranking);

        await _sut.UpdateResultAsync(accountId, 2);

        await _arenaRepo.Received(1).UpsertAsync(Arg.Is<ArenaRanking>(r =>
            r.Points == 115 && r.Wins == 6 && r.Losses == 2));
    }

    [Fact]
    public async Task UpdateResultAsync_ThirdPlace_ZeroPointsAndLoss()
    {
        var accountId = Guid.NewGuid();
        var ranking = new ArenaRanking
        {
            AccountId = accountId, Tier = "BRONZE", Points = 100, Wins = 5, Losses = 2
        };
        _arenaRepo.GetByAccountIdAsync(accountId).Returns(ranking);

        await _sut.UpdateResultAsync(accountId, 3);

        await _arenaRepo.Received(1).UpsertAsync(Arg.Is<ArenaRanking>(r =>
            r.Points == 100 && r.Wins == 5 && r.Losses == 3));
    }

    [Fact]
    public async Task UpdateResultAsync_FourthPlace_MinusPointsCappedAtZero()
    {
        var accountId = Guid.NewGuid();
        var ranking = new ArenaRanking
        {
            AccountId = accountId, Tier = "BRONZE", Points = 5, Wins = 0, Losses = 0
        };
        _arenaRepo.GetByAccountIdAsync(accountId).Returns(ranking);

        await _sut.UpdateResultAsync(accountId, 4);

        await _arenaRepo.Received(1).UpsertAsync(Arg.Is<ArenaRanking>(r =>
            r.Points == 0 && r.Losses == 1));
    }

    private static ArenaRanking CreateRanking(Guid accountId, string tier, int points)
    {
        return new ArenaRanking
        {
            AccountId = accountId,
            Tier = tier,
            Points = points,
            Season = 1,
            PlayerData = "{}",
            UpdatedAt = DateTime.UtcNow,
        };
    }
}
