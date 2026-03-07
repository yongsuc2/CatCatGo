using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace CatCatGo.Server.Infrastructure.Persistence;

public class ResourceRepository : IResourceRepository
{
    private readonly AppDbContext _db;

    public ResourceRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ResourceBalance?> GetBalanceAsync(Guid accountId, string type)
    {
        return await _db.ResourceBalances
            .FirstOrDefaultAsync(b => b.AccountId == accountId && b.Type == type);
    }

    public async Task<List<ResourceBalance>> GetAllBalancesAsync(Guid accountId)
    {
        return await _db.ResourceBalances
            .Where(b => b.AccountId == accountId)
            .ToListAsync();
    }

    public async Task UpsertBalanceAsync(ResourceBalance balance)
    {
        var existing = await _db.ResourceBalances
            .FirstOrDefaultAsync(b => b.AccountId == balance.AccountId && b.Type == balance.Type);

        if (existing == null)
        {
            _db.ResourceBalances.Add(balance);
        }
        else
        {
            existing.Amount = balance.Amount;
            existing.UpdatedAt = balance.UpdatedAt;
        }
        await _db.SaveChangesAsync();
    }

    public async Task AddLedgerEntryAsync(ResourceLedger entry)
    {
        _db.ResourceLedgers.Add(entry);
        await _db.SaveChangesAsync();
    }
}
