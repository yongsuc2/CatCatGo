using CatCatGo.Server.Core.Models;

namespace CatCatGo.Server.Core.Interfaces;

public interface ISaveRepository
{
    Task<ServerSaveData?> GetByAccountIdAsync(Guid accountId);
    Task UpsertAsync(ServerSaveData saveData);
    Task DeleteAsync(Guid accountId);
}
