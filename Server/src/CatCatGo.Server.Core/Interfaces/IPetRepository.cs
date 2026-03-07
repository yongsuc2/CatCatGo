using CatCatGo.Server.Core.Models;

namespace CatCatGo.Server.Core.Interfaces;

public interface IPetRepository
{
    Task<PetEntry?> GetByIdAsync(Guid id);
    Task<List<PetEntry>> GetByAccountIdAsync(Guid accountId);
    Task<PetEntry?> GetEquippedAsync(Guid accountId);
    Task CreateAsync(PetEntry entry);
    Task UpdateAsync(PetEntry entry);
}
