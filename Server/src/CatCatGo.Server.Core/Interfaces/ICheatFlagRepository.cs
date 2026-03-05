using CatCatGo.Server.Core.Models;

namespace CatCatGo.Server.Core.Interfaces;

public interface ICheatFlagRepository
{
    Task<CheatFlag> CreateAsync(CheatFlag flag);
    Task<List<CheatFlag>> GetByAccountIdAsync(Guid accountId);
    Task<int> GetCountByAccountIdAsync(Guid accountId);
}
