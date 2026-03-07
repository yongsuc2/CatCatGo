using CatCatGo.Server.Core.Models;

namespace CatCatGo.Server.Core.Interfaces;

public interface IHeritageRepository
{
    Task<HeritageState?> GetByAccountIdAsync(Guid accountId);
    Task UpsertAsync(HeritageState state);
}
