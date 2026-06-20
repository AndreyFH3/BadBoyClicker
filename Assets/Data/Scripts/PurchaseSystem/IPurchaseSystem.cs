using System;

namespace Purchases
{    
    public interface IPurchaseSystem
    {
        event Action<string> PurchaseSucceeded;
        event Action<string> PurchaseFailed;

        bool IsAvailable { get; }
        string GetPrice(string paymentId, string fallbackPrice);
        void Buy(string paymentId);
    }
}
