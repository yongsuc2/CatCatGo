using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace CatCatGo.Server.Infrastructure.Persistence;

public class ArenaRepository : IArenaRepository
{
    private readonly AppDbContext _db;

    public ArenaRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ArenaRanking?> GetByAccountIdAsync(Guid accountId)
    {
        return await _db.ArenaRankings.FindAsync(accountId);
    }

    public async Task<List<ArenaRanking>> GetByTierAsync(string tier, int limit)
    {
        return await _db.ArenaRankings
            .Where(r => r.Tier == tier)
            .OrderByDescending(r => r.Points)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<ArenaRanking>> GetTopRankingsAsync(int season, int limit)
    {
        return await _db.ArenaRankings
            .Where(r => r.Season == season)
            .OrderByDescending(r => r.Points)
            .Take(limit)
            .ToListAsync();
    }

    public async Task UpsertAsync(ArenaRanking ranking)
    {
        var existing = await _db.ArenaRankings.FindAsync(ranking.AccountId);
        if (existing == null)
        {
            _db.ArenaRankings.Add(ranking);
        }
        else
        {
            existing.Tier = ranking.Tier;
            existing.Points = ranking.Points;
            existing.Wins = ranking.Wins;
            existing.Losses = ranking.Losses;
            existing.PlayerData = ranking.PlayerData;
            existing.UpdatedAt = ranking.UpdatedAt;
        }
        await _db.SaveChangesAsync();
    }
}
