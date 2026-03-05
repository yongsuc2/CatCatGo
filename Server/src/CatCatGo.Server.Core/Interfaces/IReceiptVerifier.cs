namespace CatCatGo.Server.Core.Interfaces;

public class ReceiptVerificationResult
{
    public bool IsValid { get; set; }
    public string? TransactionId { get; set; }
    public string? ProductId { get; set; }
    public string? Error { get; set; }
}

public interface IReceiptVerifier
{
    string Store { get; }
    Task<ReceiptVerificationResult> VerifyAsync(string receiptData);
}
