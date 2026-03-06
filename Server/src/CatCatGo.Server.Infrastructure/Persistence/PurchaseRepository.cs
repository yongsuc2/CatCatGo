using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace CatCatGo.Server.Infrastructure.Persistence;

public class PurchaseRepository : IPurchaseRepository
{
    private readonly AppDbContext _db;

    public PurchaseRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Purchase?> GetByReceiptIdAsync(string receiptId)
    {
        return await _db.Purchases.FirstOrDefaultAsync(p => p.ReceiptId == receiptId);
    }

    public async Task<List<Purchase>> GetByAccountIdAsync(Guid accountId)
    {
        return await _db.Purchases
            .Where(p => p.AccountId == accountId)
            .OrderByDescending(p => p.PurchasedAt)
            .ToListAsync();
    }

    public async Task<Purchase> CreateAsync(Purchase purchase)
    {
        _db.Purchases.Add(purchase);
        await _db.SaveChangesAsync();
        return purchase;
    }

    public async Task UpdateAsync(Purchase purchase)
    {
        _db.Purchases.Update(purchase);
        await _db.SaveChangesAsync();
    }
}
