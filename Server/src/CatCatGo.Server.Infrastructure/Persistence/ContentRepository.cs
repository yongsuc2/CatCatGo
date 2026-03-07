using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace CatCatGo.Server.Infrastructure.Persistence;

public class ContentRepository : IContentRepository
{
    private readonly AppDbContext _db;

    public ContentRepository(AppDbContext db) { _db = db; }

    public async Task<ContentProgress?> GetProgressAsync(Guid accountId, string contentType)
    {
        return await _db.ContentProgresses
            .FirstOrDefaultAsync(p => p.AccountId == accountId && p.ContentType == contentType);
    }

    public async Task UpsertProgressAsync(ContentProgress progress)
    {
        var existing = await _db.ContentProgresses
            .FirstOrDefaultAsync(p => p.AccountId == progress.AccountId && p.ContentType == progress.ContentType);

        if (existing == null)
            _db.ContentProgresses.Add(progress);
        else
        {
            existing.HighestStage = progress.HighestStage;
            existing.DailyRunsUsed = progress.DailyRunsUsed;
            existing.LastResetDate = progress.LastResetDate;
            existing.UpdatedAt = progress.UpdatedAt;
        }
        await _db.SaveChangesAsync();
    }

    public async Task<List<ContentProgress>> GetAllProgressAsync(Guid accountId)
    {
        return await _db.ContentProgresses.Where(p => p.AccountId == accountId).ToListAsync();
    }
}
