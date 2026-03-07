using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace CatCatGo.Server.Infrastructure.Persistence;

public class ChapterRepository : IChapterRepository
{
    private readonly AppDbContext _db;

    public ChapterRepository(AppDbContext db) { _db = db; }

    public async Task<ChapterSession?> GetActiveSessionAsync(Guid accountId)
    {
        return await _db.ChapterSessions
            .FirstOrDefaultAsync(s => s.AccountId == accountId && s.IsActive);
    }

    public async Task<ChapterSession?> GetByIdAsync(Guid id)
    {
        return await _db.ChapterSessions.FindAsync(id);
    }

    public async Task CreateSessionAsync(ChapterSession session)
    {
        _db.ChapterSessions.Add(session);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateSessionAsync(ChapterSession session)
    {
        _db.ChapterSessions.Update(session);
        await _db.SaveChangesAsync();
    }

    public async Task<ChapterProgress?> GetProgressAsync(Guid accountId)
    {
        return await _db.ChapterProgresses.FindAsync(accountId);
    }

    public async Task UpsertProgressAsync(ChapterProgress progress)
    {
        var existing = await _db.ChapterProgresses.FindAsync(progress.AccountId);
        if (existing == null)
            _db.ChapterProgresses.Add(progress);
        else
        {
            existing.ClearedChapterMax = progress.ClearedChapterMax;
            existing.BestSurvivalDays = progress.BestSurvivalDays;
            existing.ClaimedTreasures = progress.ClaimedTreasures;
            existing.UpdatedAt = progress.UpdatedAt;
        }
        await _db.SaveChangesAsync();
    }
}
