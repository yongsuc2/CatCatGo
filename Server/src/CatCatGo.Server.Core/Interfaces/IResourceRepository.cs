using CatCatGo.Server.Core.Models;

namespace CatCatGo.Server.Core.Interfaces;

public interface IResourceRepository
{
    Task<ResourceBalance?> GetBalanceAsync(Guid accountId, string type);
    Task<List<ResourceBalance>> GetAllBalancesAsync(Guid accountId);
    Task UpsertBalanceAsync(ResourceBalance balance);
    Task AddLedgerEntryAsync(ResourceLedger entry);
}
