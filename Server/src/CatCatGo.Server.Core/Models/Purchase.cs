namespace CatCatGo.Server.Core.Models;

public class Purchase
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string Store { get; set; } = string.Empty;
    public string ReceiptId { get; set; } = string.Empty;
    public string ReceiptData { get; set; } = string.Empty;
    public string Status { get; set; } = "PENDING";
    public decimal? AmountPaid { get; set; }
    public string? Currency { get; set; }
    public DateTime PurchasedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }

    public Account? Account { get; set; }
    public Product? Product { get; set; }
}
