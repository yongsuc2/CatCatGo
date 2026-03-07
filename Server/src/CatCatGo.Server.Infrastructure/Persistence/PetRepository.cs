using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace CatCatGo.Server.Infrastructure.Persistence;

public class PetRepository : IPetRepository
{
    private readonly AppDbContext _db;

    public PetRepository(AppDbContext db) { _db = db; }

    public async Task<PetEntry?> GetByIdAsync(Guid id)
    {
        return await _db.PetEntries.FindAsync(id);
    }

    public async Task<List<PetEntry>> GetByAccountIdAsync(Guid accountId)
    {
        return await _db.PetEntries.Where(p => p.AccountId == accountId).ToListAsync();
    }

    public async Task<PetEntry?> GetEquippedAsync(Guid accountId)
    {
        return await _db.PetEntries.FirstOrDefaultAsync(p => p.AccountId == accountId && p.IsEquipped);
    }

    public async Task CreateAsync(PetEntry entry)
    {
        _db.PetEntries.Add(entry);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(PetEntry entry)
    {
        _db.PetEntries.Update(entry);
        await _db.SaveChangesAsync();
    }
}
