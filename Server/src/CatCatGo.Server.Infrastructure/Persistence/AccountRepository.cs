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

    public async Task DeleteAsync(Guid id)
    {
        var account = await _db.Accounts.FindAsync(id);
        if (account != null)
        {
            await DeleteAllDataAsync(id);
            _db.Accounts.Remove(account);
            await _db.SaveChangesAsync();
        }
    }

    public async Task DeleteAllDataAsync(Guid accountId)
    {
        _db.SaveData.RemoveRange(_db.SaveData.Where(x => x.AccountId == accountId));
        _db.Purchases.RemoveRange(_db.Purchases.Where(x => x.AccountId == accountId));
        _db.ArenaRankings.RemoveRange(_db.ArenaRankings.Where(x => x.AccountId == accountId));
        _db.CheatFlags.RemoveRange(_db.CheatFlags.Where(x => x.AccountId == accountId));
        _db.ResourceBalances.RemoveRange(_db.ResourceBalances.Where(x => x.AccountId == accountId));
        _db.ResourceLedgers.RemoveRange(_db.ResourceLedgers.Where(x => x.AccountId == accountId));
        _db.TalentStates.RemoveRange(_db.TalentStates.Where(x => x.AccountId == accountId));
        _db.EquipmentEntries.RemoveRange(_db.EquipmentEntries.Where(x => x.AccountId == accountId));
        _db.ChapterSessions.RemoveRange(_db.ChapterSessions.Where(x => x.AccountId == accountId));
        _db.ChapterProgresses.RemoveRange(_db.ChapterProgresses.Where(x => x.AccountId == accountId));
        _db.GachaPities.RemoveRange(_db.GachaPities.Where(x => x.AccountId == accountId));
        _db.PetEntries.RemoveRange(_db.PetEntries.Where(x => x.AccountId == accountId));
        _db.HeritageStates.RemoveRange(_db.HeritageStates.Where(x => x.AccountId == accountId));
        _db.DailyAttendances.RemoveRange(_db.DailyAttendances.Where(x => x.AccountId == accountId));
        _db.QuestProgresses.RemoveRange(_db.QuestProgresses.Where(x => x.AccountId == accountId));
        _db.ContentProgresses.RemoveRange(_db.ContentProgresses.Where(x => x.AccountId == accountId));
        await _db.SaveChangesAsync();
    }
}
