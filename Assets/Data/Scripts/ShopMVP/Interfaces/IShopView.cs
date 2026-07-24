using System;
using System.Collections.Generic;
namespace Shop
{
    public interface IShopView
    {
        event Action<string> OnBuy;

        void SetData(List<ShopElementData> data);
        void SetEarnPerSecond(long value);
        void UpdateCard(ShopElementData data);
        void ScrollToBottom();
        void ScrollToTop();
    }
}
