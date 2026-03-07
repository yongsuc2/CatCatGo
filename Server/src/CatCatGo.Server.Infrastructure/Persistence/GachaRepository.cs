using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace CatCatGo.Server.Infrastructure.Persistence;

public class GachaRepository : IGachaRepository
{
    private readonly AppDbContext _db;

    public GachaRepository(AppDbContext db) { _db = db; }

    public async Task<GachaPity?> GetPityAsync(Guid accountId, string boxType)
    {
        return await _db.GachaPities
            .FirstOrDefaultAsync(p => p.AccountId == accountId && p.BoxType == boxType);
    }

    public async Task UpsertPityAsync(GachaPity pity)
    {
        var existing = await _db.GachaPities
            .FirstOrDefaultAsync(p => p.AccountId == pity.AccountId && p.BoxType == pity.BoxType);

        if (existing == null)
            _db.GachaPities.Add(pity);
        else
        {
            existing.PityCount = pity.PityCount;
            existing.UpdatedAt = pity.UpdatedAt;
        }
        await _db.SaveChangesAsync();
    }
}
