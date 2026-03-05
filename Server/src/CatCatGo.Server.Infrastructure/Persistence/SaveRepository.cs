using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace CatCatGo.Server.Infrastructure.Persistence;

public class SaveRepository : ISaveRepository
{
    private readonly AppDbContext _db;

    public SaveRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ServerSaveData?> GetByAccountIdAsync(Guid accountId)
    {
        return await _db.SaveData.FindAsync(accountId);
    }

    public async Task UpsertAsync(ServerSaveData saveData)
    {
        var existing = await _db.SaveData.FindAsync(saveData.AccountId);
        if (existing == null)
        {
            _db.SaveData.Add(saveData);
        }
        else
        {
            existing.Data = saveData.Data;
            existing.Version = saveData.Version;
            existing.UpdatedAt = saveData.UpdatedAt;
            existing.Checksum = saveData.Checksum;
        }
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid accountId)
    {
        var save = await _db.SaveData.FindAsync(accountId);
        if (save != null)
        {
            _db.SaveData.Remove(save);
            await _db.SaveChangesAsync();
        }
    }
}
