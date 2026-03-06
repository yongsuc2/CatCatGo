using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using CatCatGo.Server.Core.Services;
using CatCatGo.Shared.Requests;
using NSubstitute;
using Xunit;

namespace CatCatGo.Server.Tests;

public class ShopServiceTests
{
    private readonly IProductRepository _productRepo;
    private readonly IPurchaseRepository _purchaseRepo;
    private readonly IReceiptVerifier _googleVerifier;
    private readonly ShopService _sut;

    public ShopServiceTests()
    {
        _productRepo = Substitute.For<IProductRepository>();
        _purchaseRepo = Substitute.For<IPurchaseRepository>();
        _googleVerifier = Substitute.For<IReceiptVerifier>();
        _googleVerifier.Store.Returns("GOOGLE_PLAY");

        _sut = new ShopService(_productRepo, _purchaseRepo, new[] { _googleVerifier });
    }

    [Fact]
    public async Task GetCatalogAsync_ReturnsActiveProducts()
    {
        _productRepo.GetActiveProductsAsync().Returns(new List<Product>
        {
            new() { Id = "gems_100", Name = "100 Gems", Type = "GEMS", IsActive = true, GemsAmount = 100 },
            new() { Id = "gems_500", Name = "500 Gems", Type = "GEMS", IsActive = true, GemsAmount = 500 },
        });

        var result = await _sut.GetCatalogAsync();

        Assert.Equal(2, result.Products.Count);
        Assert.Equal("gems_100", result.Products[0].Id);
        Assert.Equal(100, result.Products[0].GemsAmount);
    }

    [Fact]
    public async Task GetCatalogAsync_FiltersEventProductsWithinPeriod()
    {
        var now = DateTime.UtcNow;
        _productRepo.GetActiveProductsAsync().Returns(new List<Product>
        {
            new() { Id = "event_active", Name = "Active Event", Type = "EVENT", IsActive = true,
                StartAt = now.AddDays(-1), EndAt = now.AddDays(1) },
            new() { Id = "regular", Name = "Regular", Type = "GEMS", IsActive = true },
        });

        var result = await _sut.GetCatalogAsync();

        Assert.Equal(2, result.Products.Count);
    }

    [Fact]
    public async Task GetCatalogAsync_ExcludesExpiredEventProducts()
    {
        var now = DateTime.UtcNow;
        _productRepo.GetActiveProductsAsync().Returns(new List<Product>
        {
            new() { Id = "event_expired", Name = "Expired Event", Type = "EVENT", IsActive = true,
                StartAt = now.AddDays(-10), EndAt = now.AddDays(-1) },
            new() { Id = "regular", Name = "Regular", Type = "GEMS", IsActive = true },
        });

        var result = await _sut.GetCatalogAsync();

        Assert.Single(result.Products);
        Assert.Equal("regular", result.Products[0].Id);
    }

    [Fact]
    public async Task ProcessPurchaseAsync_ValidReceipt_ReturnsSuccessAndCreatesRecord()
    {
        var accountId = Guid.NewGuid();
        _purchaseRepo.GetByReceiptIdAsync("receipt-001").Returns((Purchase?)null);
        _productRepo.GetByIdAsync("gems_100").Returns(new Product
        {
            Id = "gems_100", Name = "100 Gems", Type = "GEMS", IsActive = true,
            GemsAmount = 100, Rewards = "{\"gems\":100}"
        });
        _googleVerifier.VerifyAsync("valid-receipt-data").Returns(new ReceiptVerificationResult
        {
            IsValid = true, TransactionId = "txn-001"
        });

        var result = await _sut.ProcessPurchaseAsync(accountId, new PurchaseRequest
        {
            ProductId = "gems_100",
            Store = "GOOGLE_PLAY",
            ReceiptId = "receipt-001",
            ReceiptData = "valid-receipt-data"
        });

        Assert.True(result.Success);
        Assert.Equal("{\"gems\":100}", result.RewardsJson);
        await _purchaseRepo.Received(1).CreateAsync(Arg.Is<Purchase>(p =>
            p.AccountId == accountId && p.Status == "VERIFIED"));
    }

    [Fact]
    public async Task ProcessPurchaseAsync_DuplicateReceipt_ReturnsDuplicateError()
    {
        var accountId = Guid.NewGuid();
        _purchaseRepo.GetByReceiptIdAsync("receipt-dup").Returns(new Purchase
        {
            Id = Guid.NewGuid(), ReceiptId = "receipt-dup"
        });

        var result = await _sut.ProcessPurchaseAsync(accountId, new PurchaseRequest
        {
            ProductId = "gems_100",
            Store = "GOOGLE_PLAY",
            ReceiptId = "receipt-dup",
            ReceiptData = "data"
        });

        Assert.False(result.Success);
        Assert.Equal("DUPLICATE_RECEIPT", result.Error);
    }

    [Fact]
    public async Task ProcessPurchaseAsync_InvalidProduct_ReturnsInvalidProductError()
    {
        var accountId = Guid.NewGuid();
        _purchaseRepo.GetByReceiptIdAsync("receipt-002").Returns((Purchase?)null);
        _productRepo.GetByIdAsync("nonexistent").Returns((Product?)null);

        var result = await _sut.ProcessPurchaseAsync(accountId, new PurchaseRequest
        {
            ProductId = "nonexistent",
            Store = "GOOGLE_PLAY",
            ReceiptId = "receipt-002",
            ReceiptData = "data"
        });

        Assert.False(result.Success);
        Assert.Equal("INVALID_PRODUCT", result.Error);
    }

    [Fact]
    public async Task ProcessPurchaseAsync_VerificationFailed_ReturnsError()
    {
        var accountId = Guid.NewGuid();
        _purchaseRepo.GetByReceiptIdAsync("receipt-003").Returns((Purchase?)null);
        _productRepo.GetByIdAsync("gems_100").Returns(new Product
        {
            Id = "gems_100", Name = "100 Gems", Type = "GEMS", IsActive = true
        });
        _googleVerifier.VerifyAsync("bad-receipt").Returns(new ReceiptVerificationResult
        {
            IsValid = false, Error = "INVALID_SIGNATURE"
        });

        var result = await _sut.ProcessPurchaseAsync(accountId, new PurchaseRequest
        {
            ProductId = "gems_100",
            Store = "GOOGLE_PLAY",
            ReceiptId = "receipt-003",
            ReceiptData = "bad-receipt"
        });

        Assert.False(result.Success);
        Assert.Contains("VERIFICATION_FAILED", result.Error);
        await _purchaseRepo.DidNotReceive().CreateAsync(Arg.Any<Purchase>());
    }
}
