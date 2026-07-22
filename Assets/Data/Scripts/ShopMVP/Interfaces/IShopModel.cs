using System;
using System.Collections.Generic;
namespace Shop
{
    public interface IShopModel
    {
        long AutoIncomePerSecond { get; }
        event Action StateChanged;
        event Action<string> ItemBought;
        event Action<string> PaidPurchaseSucceeded;
        event Action<string> PaidPurchaseFailed;
        bool HasAnyBuyable(ShopItemType? type = null);
        List<ShopElementData> GetAllData();
        ShopElementData GetShopPositionData(string id);
        ShopElementData GetCardChestPurchaseData();
        string GetCardChestId();
        ShopPurchaseConfirmationData GetPurchaseConfirmationData(string id);
        void Buy(string id);
    }
}
