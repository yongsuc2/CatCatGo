using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;

namespace CatCatGo.Server.Infrastructure.Persistence;

public class TalentRepository : ITalentRepository
{
    private readonly AppDbContext _db;

    public TalentRepository(AppDbContext db) { _db = db; }

    public async Task<TalentState?> GetByAccountIdAsync(Guid accountId)
    {
        return await _db.TalentStates.FindAsync(accountId);
    }

    public async Task UpsertAsync(TalentState state)
    {
        var existing = await _db.TalentStates.FindAsync(state.AccountId);
        if (existing == null)
            _db.TalentStates.Add(state);
        else
        {
            existing.AtkLevel = state.AtkLevel;
            existing.HpLevel = state.HpLevel;
            existing.DefLevel = state.DefLevel;
            existing.TotalLevel = state.TotalLevel;
            existing.Grade = state.Grade;
            existing.SubGrade = state.SubGrade;
            existing.ClaimedMilestones = state.ClaimedMilestones;
            existing.UpdatedAt = state.UpdatedAt;
        }
        await _db.SaveChangesAsync();
    }
}
