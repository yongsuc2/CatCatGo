using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using CatCatGo.Shared.Requests;
using CatCatGo.Shared.Responses;

namespace CatCatGo.Server.Core.Services;

public class ShopService
{
    private readonly IProductRepository _productRepo;
    private readonly IPurchaseRepository _purchaseRepo;
    private readonly Dictionary<string, IReceiptVerifier> _verifiers;

    public ShopService(
        IProductRepository productRepo,
        IPurchaseRepository purchaseRepo,
        IEnumerable<IReceiptVerifier> verifiers)
    {
        _productRepo = productRepo;
        _purchaseRepo = purchaseRepo;
        _verifiers = verifiers.ToDictionary(v => v.Store, v => v);
    }

    public async Task<ShopCatalogResponse> GetCatalogAsync()
    {
        var products = await _productRepo.GetActiveProductsAsync();
        var now = DateTime.UtcNow;

        var productDtos = products
            .Where(p => p.StartAt == null || p.StartAt <= now)
            .Where(p => p.EndAt == null || p.EndAt >= now)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Type = p.Type,
                PriceTier = p.PriceTier ?? "",
                GemsAmount = p.GemsAmount,
                BonusGems = p.BonusGems,
                RewardsJson = p.Rewards,
                IsActive = p.IsActive,
                StartAt = p.StartAt.HasValue ? new DateTimeOffset(p.StartAt.Value, TimeSpan.Zero).ToUnixTimeMilliseconds() : null,
                EndAt = p.EndAt.HasValue ? new DateTimeOffset(p.EndAt.Value, TimeSpan.Zero).ToUnixTimeMilliseconds() : null,
            })
            .ToList();

        return new ShopCatalogResponse
        {
            Products = productDtos,
            ActiveSubscription = null,
        };
    }

    public async Task<PurchaseResponse> ProcessPurchaseAsync(Guid accountId, PurchaseRequest request)
    {
        var existingPurchase = await _purchaseRepo.GetByReceiptIdAsync(request.ReceiptId);
        if (existingPurchase != null)
            return new PurchaseResponse { Success = false, Error = "DUPLICATE_RECEIPT" };

        var product = await _productRepo.GetByIdAsync(request.ProductId);
        if (product == null || !product.IsActive)
            return new PurchaseResponse { Success = false, Error = "INVALID_PRODUCT" };

        if (!_verifiers.TryGetValue(request.Store, out var verifier))
            return new PurchaseResponse { Success = false, Error = "UNSUPPORTED_STORE" };

        var verification = await verifier.VerifyAsync(request.ReceiptData);
        if (!verification.IsValid)
            return new PurchaseResponse { Success = false, Error = $"VERIFICATION_FAILED: {verification.Error}" };

        var purchase = new Purchase
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            ProductId = request.ProductId,
            Store = request.Store,
            ReceiptId = request.ReceiptId,
            ReceiptData = request.ReceiptData,
            Status = "VERIFIED",
            PurchasedAt = DateTime.UtcNow,
            VerifiedAt = DateTime.UtcNow,
        };
        await _purchaseRepo.CreateAsync(purchase);

        return new PurchaseResponse
        {
            Success = true,
            RewardsJson = product.Rewards,
        };
    }

    public async Task<List<Purchase>> GetPurchaseHistoryAsync(Guid accountId)
    {
        return await _purchaseRepo.GetByAccountIdAsync(accountId);
    }
}
