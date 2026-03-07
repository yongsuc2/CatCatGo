using System;
using CatCatGo.Shared.Requests;
using CatCatGo.Shared.Responses;

namespace CatCatGo.Network
{
    public static class ShopApi
    {
        public static void GetCatalog(Action<ApiResponse<ShopCatalogResponse>> callback)
        {
            ApiClient.Instance.Get("api/shop/catalog", callback);
        }

        public static void Purchase(string productId, string store, string receiptId, string receiptData,
            Action<ApiResponse<PurchaseResponse>> callback)
        {
            var request = new PurchaseRequest
            {
                ProductId = productId,
                Store = store,
                ReceiptId = receiptId,
                ReceiptData = receiptData
            };
            ApiClient.Instance.Post("api/shop/purchase", request, callback);
        }
    }
}
