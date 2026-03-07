using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace CatCatGo.Server.Infrastructure.Persistence;

public class EquipmentRepository : IEquipmentRepository
{
    private readonly AppDbContext _db;

    public EquipmentRepository(AppDbContext db) { _db = db; }

    public async Task<EquipmentEntry?> GetByIdAsync(Guid id)
    {
        return await _db.EquipmentEntries.FindAsync(id);
    }

    public async Task<List<EquipmentEntry>> GetByAccountIdAsync(Guid accountId)
    {
        return await _db.EquipmentEntries.Where(e => e.AccountId == accountId).ToListAsync();
    }

    public async Task<List<EquipmentEntry>> GetEquippedAsync(Guid accountId)
    {
        return await _db.EquipmentEntries.Where(e => e.AccountId == accountId && e.SlotIndex >= 0).ToListAsync();
    }

    public async Task CreateAsync(EquipmentEntry entry)
    {
        _db.EquipmentEntries.Add(entry);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(EquipmentEntry entry)
    {
        _db.EquipmentEntries.Update(entry);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entry = await _db.EquipmentEntries.FindAsync(id);
        if (entry != null)
        {
            _db.EquipmentEntries.Remove(entry);
            await _db.SaveChangesAsync();
        }
    }
}
