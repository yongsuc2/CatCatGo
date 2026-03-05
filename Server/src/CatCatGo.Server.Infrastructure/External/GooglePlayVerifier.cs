using CatCatGo.Server.Core.Interfaces;

namespace CatCatGo.Server.Infrastructure.External;

public class GooglePlayVerifier : IReceiptVerifier
{
    public string Store => "GOOGLE_PLAY";

    public async Task<ReceiptVerificationResult> VerifyAsync(string receiptData)
    {
        // TODO: Google Play Developer API로 영수증 검증
        // https://developers.google.com/android-publisher/api-ref/rest/v3/purchases.products/get
        await Task.CompletedTask;

        return new ReceiptVerificationResult
        {
            IsValid = false,
            Error = "NOT_IMPLEMENTED",
        };
    }
}
