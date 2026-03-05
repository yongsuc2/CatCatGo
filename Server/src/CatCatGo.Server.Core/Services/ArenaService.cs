using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using CatCatGo.Shared.Responses;

namespace CatCatGo.Server.Core.Services;

public class ArenaService
{
    private readonly IArenaRepository _arenaRepo;
    private readonly IAccountRepository _accountRepo;

    private static readonly string[] TierOrder = { "BRONZE", "SILVER", "GOLD", "PLATINUM", "DIAMOND", "MASTER" };
    private static readonly int[] TierThresholds = { 0, 1000, 2500, 5000, 8000, 12000 };

    public ArenaService(IArenaRepository arenaRepo, IAccountRepository accountRepo)
    {
        _arenaRepo = arenaRepo;
        _accountRepo = accountRepo;
    }

    public async Task<ArenaMatchResponse?> MatchAsync(Guid accountId)
    {
        var myRanking = await _arenaRepo.GetByAccountIdAsync(accountId);
        if (myRanking == null)
        {
            myRanking = new ArenaRanking
            {
                AccountId = accountId,
                Tier = "BRONZE",
                Points = 0,
                Season = 1,
                UpdatedAt = DateTime.UtcNow,
            };
            await _arenaRepo.UpsertAsync(myRanking);
        }

        var candidates = await _arenaRepo.GetByTierAsync(myRanking.Tier, 20);
        var opponents = candidates
            .Where(c => c.AccountId != accountId)
            .OrderBy(_ => Random.Shared.Next())
            .Take(3)
            .ToList();

        var opponentDtos = new List<ArenaOpponentDto>();
        foreach (var opp in opponents)
        {
            var account = await _accountRepo.GetByIdAsync(opp.AccountId);
            opponentDtos.Add(new ArenaOpponentDto
            {
                AccountId = opp.AccountId.ToString(),
                DisplayName = account?.DisplayName ?? "Unknown",
                Tier = opp.Tier,
                Points = opp.Points,
                PlayerDataJson = opp.PlayerData,
            });
        }

        return new ArenaMatchResponse
        {
            MatchId = Guid.NewGuid().ToString(),
            Opponents = opponentDtos,
        };
    }

    public async Task<ArenaRankingResponse> GetRankingsAsync(Guid accountId, int season)
    {
        var top = await _arenaRepo.GetTopRankingsAsync(season, 100);
        var myRanking = await _arenaRepo.GetByAccountIdAsync(accountId);

        var rankings = new List<ArenaRankEntry>();
        for (int i = 0; i < top.Count; i++)
        {
            var account = await _accountRepo.GetByIdAsync(top[i].AccountId);
            rankings.Add(new ArenaRankEntry
            {
                Rank = i + 1,
                DisplayName = account?.DisplayName ?? "Unknown",
                Tier = top[i].Tier,
                Points = top[i].Points,
            });
        }

        int myRank = myRanking != null
            ? top.FindIndex(r => r.AccountId == accountId) + 1
            : 0;

        return new ArenaRankingResponse
        {
            Rankings = rankings,
            MyRank = myRank > 0 ? myRank : top.Count + 1,
            MyTier = myRanking?.Tier ?? "BRONZE",
            MyPoints = myRanking?.Points ?? 0,
        };
    }

    public async Task UpdateResultAsync(Guid accountId, int rank)
    {
        var ranking = await _arenaRepo.GetByAccountIdAsync(accountId);
        if (ranking == null) return;

        int pointDelta = rank switch
        {
            1 => 30,
            2 => 15,
            3 => 0,
            _ => -10,
        };

        ranking.Points = Math.Max(0, ranking.Points + pointDelta);
        if (rank <= 2) ranking.Wins++;
        else ranking.Losses++;

        ranking.Tier = CalculateTier(ranking.Points);
        ranking.UpdatedAt = DateTime.UtcNow;

        await _arenaRepo.UpsertAsync(ranking);
    }

    private static string CalculateTier(int points)
    {
        for (int i = TierThresholds.Length - 1; i >= 0; i--)
        {
            if (points >= TierThresholds[i])
                return TierOrder[i];
        }
        return "BRONZE";
    }
}
