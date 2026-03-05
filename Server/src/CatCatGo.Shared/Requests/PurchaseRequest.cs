namespace CatCatGo.Shared.Requests;

public class PurchaseRequest
{
    public required string ProductId { get; set; }
    public required string Store { get; set; }
    public required string ReceiptId { get; set; }
    public required string ReceiptData { get; set; }
}
