using CatCatGo.Server.Core.Models;

namespace CatCatGo.Server.Core.Interfaces;

public interface IPurchaseRepository
{
    Task<Purchase?> GetByReceiptIdAsync(string receiptId);
    Task<List<Purchase>> GetByAccountIdAsync(Guid accountId);
    Task<Purchase> CreateAsync(Purchase purchase);
    Task UpdateAsync(Purchase purchase);
}
