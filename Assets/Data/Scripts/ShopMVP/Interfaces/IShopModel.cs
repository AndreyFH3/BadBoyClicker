using System;
using System.Collections.Generic;
namespace Shop
{
    public interface IShopModel
    {
        long AutoIncomePerSecond { get; }
        event Action StateChanged;
        event Action<string> ItemBought;
        List<ShopElementData> GetAllData();
        ShopElementData GetShopPositionData(string id);
        ShopPurchaseConfirmationData GetPurchaseConfirmationData(string id);
        void Buy(string id);
    }
}
