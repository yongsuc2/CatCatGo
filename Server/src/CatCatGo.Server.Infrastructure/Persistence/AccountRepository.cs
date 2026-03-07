using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace CatCatGo.Server.Infrastructure.Persistence;

public class AccountRepository : IAccountRepository
{
    private readonly AppDbContext _db;

    public AccountRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Account?> GetByIdAsync(Guid id)
    {
        return await _db.Accounts.FindAsync(id);
    }

    public async Task<Account?> GetByDeviceIdAsync(string deviceId)
    {
        return await _db.Accounts.FirstOrDefaultAsync(a => a.DeviceId == deviceId);
    }

    public async Task<Account?> GetBySocialIdAsync(string socialType, string socialId)
    {
        return await _db.Accounts.FirstOrDefaultAsync(a =>
            a.SocialType == socialType && a.SocialId == socialId);
    }

    public async Task<Account?> GetByRefreshTokenAsync(string refreshToken)
    {
        return await _db.Accounts.FirstOrDefaultAsync(a => a.RefreshToken == refreshToken);
    }

    public async Task<Account> CreateAsync(Account account)
    {
        _db.Accounts.Add(account);
        await _db.SaveChangesAsync();
        return account;
    }

    public async Task UpdateAsync(Account account)
    {
        _db.Accounts.Update(account);
        await _db.SaveChangesAsync();
    }
}
