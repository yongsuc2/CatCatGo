using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace CatCatGo.Server.Infrastructure.Persistence;

public class CheatFlagRepository : ICheatFlagRepository
{
    private readonly AppDbContext _db;

    public CheatFlagRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<CheatFlag> CreateAsync(CheatFlag flag)
    {
        _db.CheatFlags.Add(flag);
        await _db.SaveChangesAsync();
        return flag;
    }

    public async Task<List<CheatFlag>> GetByAccountIdAsync(Guid accountId)
    {
        return await _db.CheatFlags
            .Where(f => f.AccountId == accountId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetCountByAccountIdAsync(Guid accountId)
    {
        return await _db.CheatFlags.CountAsync(f => f.AccountId == accountId);
    }
}
