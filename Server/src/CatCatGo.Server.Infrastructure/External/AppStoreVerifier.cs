using CatCatGo.Server.Core.Interfaces;

namespace CatCatGo.Server.Infrastructure.External;

public class AppStoreVerifier : IReceiptVerifier
{
    public string Store => "APP_STORE";

    public async Task<ReceiptVerificationResult> VerifyAsync(string receiptData)
    {
        // TODO: App Store Server API V2로 영수증 검증
        // https://developer.apple.com/documentation/appstoreserverapi
        await Task.CompletedTask;

        return new ReceiptVerificationResult
        {
            IsValid = false,
            Error = "NOT_IMPLEMENTED",
        };
    }
}
