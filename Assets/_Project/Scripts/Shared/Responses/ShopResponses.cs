using System;
using System.Collections.Generic;

namespace CatCatGo.Shared.Responses
{
    [Serializable]
    public class ShopCatalogResponse
    {
        public List<ProductDto> Products;
        public SubscriptionDto ActiveSubscription;
    }

    [Serializable]
    public class ProductDto
    {
        public string Id;
        public string Name;
        public string Type;
        public string PriceTier;
        public int GemsAmount;
        public int BonusGems;
        public string RewardsJson;
        public bool IsActive;
        public long StartAt;
        public long EndAt;
    }

    [Serializable]
    public class SubscriptionDto
    {
        public string ProductId;
        public long ExpiresAt;
        public bool IsActive;
    }

    [Serializable]
    public class PurchaseResponse
    {
        public bool Success;
        public string Error;
        public string RewardsJson;
    }
}
