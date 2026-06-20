using System;

namespace Shop
{
    public class ShopPurchaseConfirmationNullView : IShopPurchaseConfirmationView
    {
        public event Action ConfirmRequested;
        public event Action CancelRequested
        {
            add { }
            remove { }
        }

        public void Show(ShopPurchaseConfirmationData data)
        {
            ConfirmRequested?.Invoke();
        }

        public void Hide() { }
    }
}
