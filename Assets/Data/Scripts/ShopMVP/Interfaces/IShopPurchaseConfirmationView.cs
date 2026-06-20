using System;

namespace Shop
{
    public interface IShopPurchaseConfirmationView
    {
        event Action ConfirmRequested;
        event Action CancelRequested;

        void Show(ShopPurchaseConfirmationData data);
        void Hide();
    }
}
