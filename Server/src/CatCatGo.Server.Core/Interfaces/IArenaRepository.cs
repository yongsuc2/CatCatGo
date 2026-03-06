using CatCatGo.Server.Core.Models;

namespace CatCatGo.Server.Core.Interfaces;

public interface IArenaRepository
{
    Task<ArenaRanking?> GetByAccountIdAsync(Guid accountId);
    Task<List<ArenaRanking>> GetByTierAsync(string tier, int limit);
    Task<List<ArenaRanking>> GetTopRankingsAsync(int season, int limit);
    Task UpsertAsync(ArenaRanking ranking);
}
