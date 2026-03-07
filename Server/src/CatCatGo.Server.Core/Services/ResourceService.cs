using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;

namespace CatCatGo.Server.Core.Services;

public class ResourceService
{
    private readonly IResourceRepository _resourceRepo;

    public ResourceService(IResourceRepository resourceRepo)
    {
        _resourceRepo = resourceRepo;
    }

    public async Task<Dictionary<string, double>> GetAllBalancesAsync(Guid accountId)
    {
        var balances = await _resourceRepo.GetAllBalancesAsync(accountId);
        return balances.ToDictionary(b => b.Type, b => b.Amount);
    }

    public async Task<double> GetBalanceAsync(Guid accountId, string type)
    {
        var balance = await _resourceRepo.GetBalanceAsync(accountId, type);
        return balance?.Amount ?? 0;
    }

    public async Task<bool> SpendAsync(Guid accountId, string type, double amount, string source, string? refId = null)
    {
        if (amount <= 0) return false;

        var balance = await _resourceRepo.GetBalanceAsync(accountId, type);
        var current = balance?.Amount ?? 0;

        if (current < amount) return false;

        var newBalance = current - amount;
        await UpdateBalanceAndLogAsync(accountId, type, -amount, newBalance, source, refId);
        return true;
    }

    public async Task GrantAsync(Guid accountId, string type, double amount, string source, string? refId = null)
    {
        if (amount <= 0) return;

        var balance = await _resourceRepo.GetBalanceAsync(accountId, type);
        var current = balance?.Amount ?? 0;
        var newBalance = current + amount;

        await UpdateBalanceAndLogAsync(accountId, type, amount, newBalance, source, refId);
    }

    public async Task<bool> SpendMultipleAsync(Guid accountId, Dictionary<string, double> costs, string source, string? refId = null)
    {
        foreach (var (type, amount) in costs)
        {
            var balance = await _resourceRepo.GetBalanceAsync(accountId, type);
            var current = balance?.Amount ?? 0;
            if (current < amount) return false;
        }

        foreach (var (type, amount) in costs)
        {
            await SpendAsync(accountId, type, amount, source, refId);
        }
        return true;
    }

    public async Task GrantMultipleAsync(Guid accountId, Dictionary<string, double> rewards, string source, string? refId = null)
    {
        foreach (var (type, amount) in rewards)
        {
            await GrantAsync(accountId, type, amount, source, refId);
        }
    }

    private async Task UpdateBalanceAndLogAsync(Guid accountId, string type, double delta, double newBalance, string source, string? refId)
    {
        var balanceEntity = new ResourceBalance
        {
            AccountId = accountId,
            Type = type,
            Amount = newBalance,
            UpdatedAt = DateTime.UtcNow,
        };
        await _resourceRepo.UpsertBalanceAsync(balanceEntity);

        var ledger = new ResourceLedger
        {
            AccountId = accountId,
            Type = type,
            Delta = delta,
            Balance = newBalance,
            Source = source,
            RefId = refId,
            CreatedAt = DateTime.UtcNow,
        };
        await _resourceRepo.AddLedgerEntryAsync(ledger);
    }
}
