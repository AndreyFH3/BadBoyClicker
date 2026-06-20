using System;
using UnityEngine;
using YG;
using Zenject;

namespace Purchases
{    
    public class PurchaseSystem : IPurchaseSystem, IInitializable, IDisposable
    {
        public event Action<string> PurchaseSucceeded;
        public event Action<string> PurchaseFailed;

        public bool IsAvailable
        {
            get
            {
#if Payments_yg
                return true;
#else
                return false;
#endif
            }
        }

        public void Initialize()
        {
#if Payments_yg
            YG2.onPurchaseSuccess += OnPurchaseSuccess;
            YG2.onPurchaseFailed += OnPurchaseFailed;
#endif
        }

        public void Dispose()
        {
#if Payments_yg
            YG2.onPurchaseSuccess -= OnPurchaseSuccess;
            YG2.onPurchaseFailed -= OnPurchaseFailed;
#endif
        }

        public string GetPrice(string paymentId, string fallbackPrice)
        {
#if Payments_yg
            var purchase = YG2.PurchaseByID(paymentId);
            if (purchase != null && !string.IsNullOrEmpty(purchase.price))
                return purchase.price;
#endif
            return fallbackPrice;
        }

        public void Buy(string paymentId)
        {
            if (string.IsNullOrEmpty(paymentId))
            {
                Debug.LogWarning("Payment id is empty.");
                PurchaseFailed?.Invoke(paymentId);
                return;
            }

#if Payments_yg
            YG2.BuyPayments(paymentId);
#else
            Debug.LogWarning($"PluginYG2 Payments module is not enabled. Purchase was not started: {paymentId}");
            PurchaseFailed?.Invoke(paymentId);
#endif
        }

        private void OnPurchaseSuccess(string paymentId)
        {
            PurchaseSucceeded?.Invoke(paymentId);
#if Payments_yg
            YG2.ConsumePurchaseByID(paymentId, false);
#endif
        }

        private void OnPurchaseFailed(string paymentId)
        {
            PurchaseFailed?.Invoke(paymentId);
        }
    }
}
