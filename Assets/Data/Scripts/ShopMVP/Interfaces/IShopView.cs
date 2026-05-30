using System;
using System.Collections.Generic;
namespace Shop
{
    public interface IShopView
    {
        event Action OpenRequested;
        event Action CloseRequested;
        event Action<string> OnBuy;

        bool IsActive { get; }

        void SetOpenState(bool isOpen);
        void SetData(List<ShopElementData> data);
        void SetEarnPerSecond(long value);
        void UpdateCard(ShopElementData data);
    }
}
