using System;

namespace CatCatGo.Shared.Requests
{
    [Serializable]
    public class PurchaseRequest
    {
        public string ProductId;
        public string Store;
        public string ReceiptId;
        public string ReceiptData;
    }
}
