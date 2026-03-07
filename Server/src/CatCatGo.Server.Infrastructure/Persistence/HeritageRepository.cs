using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;

namespace CatCatGo.Server.Infrastructure.Persistence;

public class HeritageRepository : IHeritageRepository
{
    private readonly AppDbContext _db;

    public HeritageRepository(AppDbContext db) { _db = db; }

    public async Task<HeritageState?> GetByAccountIdAsync(Guid accountId)
    {
        return await _db.HeritageStates.FindAsync(accountId);
    }

    public async Task UpsertAsync(HeritageState state)
    {
        var existing = await _db.HeritageStates.FindAsync(state.AccountId);
        if (existing == null)
            _db.HeritageStates.Add(state);
        else
        {
            existing.SkullLevel = state.SkullLevel;
            existing.KnightLevel = state.KnightLevel;
            existing.RangerLevel = state.RangerLevel;
            existing.GhostLevel = state.GhostLevel;
            existing.UpdatedAt = state.UpdatedAt;
        }
        await _db.SaveChangesAsync();
    }
}
