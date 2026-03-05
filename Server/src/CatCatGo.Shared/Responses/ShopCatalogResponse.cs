namespace CatCatGo.Shared.Responses;

public class ShopCatalogResponse
{
    public required List<ProductDto> Products { get; set; }
    public SubscriptionDto? ActiveSubscription { get; set; }
}

public class ProductDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; }
    public required string PriceTier { get; set; }
    public int GemsAmount { get; set; }
    public int BonusGems { get; set; }
    public string? RewardsJson { get; set; }
    public bool IsActive { get; set; }
    public long? StartAt { get; set; }
    public long? EndAt { get; set; }
}

public class SubscriptionDto
{
    public required string ProductId { get; set; }
    public long ExpiresAt { get; set; }
    public bool IsActive { get; set; }
}

public class PurchaseResponse
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? RewardsJson { get; set; }
}
