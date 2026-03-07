using CatCatGo.Server.Core.Models;

namespace CatCatGo.Server.Core.Interfaces;

public interface ITalentRepository
{
    Task<TalentState?> GetByAccountIdAsync(Guid accountId);
    Task UpsertAsync(TalentState state);
}
