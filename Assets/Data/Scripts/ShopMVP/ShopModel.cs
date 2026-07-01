using System;
using System.Collections.Generic;
using UnityEngine;
using Core;
using Utils;
using Zenject;
using PlayerProgression;
using GameLocalization;
using AdBonusOffers;
using Purchases;
using QuestSystem;
using Rewards;

namespace Shop
{
    public class ShopModel : IShopModel, IInitializable, IDisposable
    {
        [SerializeField] private bool _isOpenOnStart;

        private const float PriceGrowth = 1.07f;

        private GameConfig _config;
        private Wallet _wallet;
        private IShopRuntimeSave _save;
        private IPlayerProgressionService _playerProgression;
        private ILocalizationService _localization;
        private IAdBonusEffectService _bonusEffectService;
        private IPurchaseSystem _purchaseSystem;
        private IQuestRewardService _rewardService;
        private readonly Dictionary<string, ShopItem> _items = new();
        private readonly Dictionary<string, ShopItem> _paidItemsByPaymentId = new();

        public bool IsOpen { get; private set; }
        public long AutoIncomePerSecond => _save?.AutoIncomePerSecond ?? 0;

        public event Action StateChanged;
        public event Action<string> ItemBought;

        [Zenject.Inject]
        public void Construct(
            GameConfig config,
            Wallet wallet,
            IShopRuntimeSave save,
            IPlayerProgressionService playerProgression,
            ILocalizationService localization,
            IAdBonusEffectService bonusEffectService,
            IPurchaseSystem purchaseSystem,
            IQuestRewardService rewardService)
        {
            _config = config;
            _wallet = wallet;
            _save = save;
            _playerProgression = playerProgression;
            _localization = localization;
            _bonusEffectService = bonusEffectService;
            _purchaseSystem = purchaseSystem;
            _rewardService = rewardService;

            BuildItems();
            _save.Recalculate(_config);
            IsOpen = _isOpenOnStart;
        }

        public void Initialize()
        {
            _wallet.OnChanged += OnWalletChanged;
            _playerProgression.Changed += OnProgressionChanged;
            _bonusEffectService.Changed += OnBonusEffectsChanged;
            _purchaseSystem.PurchaseSucceeded += OnPurchaseSucceeded;
            _purchaseSystem.PurchaseFailed += OnPurchaseFailed;
        }

        public void Dispose()
        {
            _wallet.OnChanged -= OnWalletChanged;
            _playerProgression.Changed -= OnProgressionChanged;
            _bonusEffectService.Changed -= OnBonusEffectsChanged;
            _purchaseSystem.PurchaseSucceeded -= OnPurchaseSucceeded;
            _purchaseSystem.PurchaseFailed -= OnPurchaseFailed;
        }

        public List<ShopElementData> GetAllData()
        {
            List<ShopElementData> datas = new();
            foreach (var item in _items.Values)
            {
                datas.Add(CreateElementData(item));
            }
            
            return datas;
        }

        public ShopElementData GetShopPositionData(string id)
        {
            return _items.TryGetValue(id, out var item) ? CreateElementData(item) : null;
        }

        public ShopPurchaseConfirmationData GetPurchaseConfirmationData(string id)
        {
            if (!_items.TryGetValue(id, out var item) || item.Type != ShopItemType.PaidBuy || item.PaidData == null)
            {
                return null;
            }

            var data = item.PaidData;
            string name = _localization.Localize(data.NameLocalizationKey);
            string price = GetPaidPriceText(data);
            string reward = _localization.Localize(data.RewardTextLocalizationKey);

            return new ShopPurchaseConfirmationData
            {
                Id = data.Id,
                Description = _localization.Format(
                    "shop.purchase.confirmation",
                    name,
                    price,
                    reward)
            };
        }

        public void Buy(string id)
        {
            if (!_items.TryGetValue(id, out var item))
            {
                Debug.LogWarning($"Shop item with id '{id}' was not found.");
                return;
            }

            if (item.Type == ShopItemType.PaidBuy)
            {
                BuyPaid(item);
                return;
            }

            long price = CalculatePrice(item.ClickData.BasePrice, _save.GetLevel(item.Type, item.ClickData.Id));
            if (!_wallet.SpendSoft(price))
            {
                return;
            }

            _save.AddLevel(item.Type, item.ClickData.Id);
            _playerProgression.AddExperience(PlayerExperienceSource.ShopPurchase);
            _save.Recalculate(_config);
            ItemBought?.Invoke(item.ClickData.Id);
            StateChanged?.Invoke();
        }

        public void Open()
        {
            SetOpenState(true);
        }

        public void Close()
        {
            SetOpenState(false);
        }

        private void SetOpenState(bool isOpen)
        {
            if (IsOpen == isOpen)
            {
                return;
            }

            IsOpen = isOpen;
            StateChanged?.Invoke();
        }

        private void BuildItems()
        {
            _items.Clear();
            _paidItemsByPaymentId.Clear();
            if (_config == null)
            {
                return;
            }

            AddItems(_config.Clicks, ShopItemType.Click);
            AddItems(_config.AutoBuys, ShopItemType.AutoBuy);
            AddPaidItems(_config.PaidBuys);
        }

        private void AddItems(IReadOnlyList<GameConfig.ShopDataClick> items, ShopItemType type)
        {
            if (items == null)
            {
                return;
            }

            foreach (var data in items)
            {
                if (data == null || string.IsNullOrEmpty(data.Id))
                {
                    continue;
                }

                if (_items.ContainsKey(data.Id))
                {
                    Debug.LogWarning($"Duplicate shop item id '{data.Id}'. The first item will be used.");
                    continue;
                }

                _items.Add(data.Id, new ShopItem(type, data));
            }
        }

        private ShopElementData CreateElementData(ShopItem item)
        {
            if (item.Type == ShopItemType.PaidBuy)
            {
                return CreatePaidElementData(item);
            }

            int level = _save.GetLevel(item.Type, item.ClickData.Id);
            long price = CalculatePrice(item.ClickData.BasePrice, level);

            return new ShopElementData
            {
                Id = item.ClickData.Id,
                Type = item.Type,
                Icon = item.ClickData.Icon,
                Name = _localization.Localize(item.ClickData.NameLocalizationKey),
                Level = _localization.Format("shop.item.level", level),
                PriceIcon = GetPriceIcon(item),
                Price = price.ConvertFromLongToString(),
                Bonus = _localization.Format(
                    "shop.item.bonus",
                    GetEffectiveBonus(item).ConvertFromLongToString(),
                    _localization.Localize(GetBonusRateLocalizationKey(item.Type))),
                CanBuy = CanBuy(item, price)
            };
        }

        private ShopElementData CreatePaidElementData(ShopItem item)
        {
            var data = item.PaidData;

            return new ShopElementData
            {
                Id = data.Id,
                Type = item.Type,
                Icon = data.Icon,
                Name = _localization.Localize(data.NameLocalizationKey),
                Level = string.Empty,
                PriceIcon = GetPriceIcon(item),
                Price = GetPaidPriceText(data),
                Bonus = _localization.Localize(data.RewardTextLocalizationKey),
                CanBuy = CanBuy(item, 0),
                RewardGroup = DetermineRewardGroup(data.Rewards),
                IsRealMoney = data.PurchaseKind == GameConfig.PaidShopPurchaseKind.RealMoney,
                Rewards = BuildRewardDisplays(data.Rewards)
            };
        }

        private ShopRewardGroup DetermineRewardGroup(IReadOnlyList<QuestReward> rewards)
        {
            if (rewards == null)
            {
                return ShopRewardGroup.Soft;
            }

            // Group by the first currency reward the offer grants; non-currency
            // rewards (backgrounds, boosts, chests) don't define a slot on their own.
            foreach (var reward in rewards)
            {
                if (reward == null || reward.RewardType != QuestRewardType.Currency)
                {
                    continue;
                }

                return ToRewardGroup(reward.CurrencyType);
            }

            return ShopRewardGroup.Soft;
        }

        private static ShopRewardGroup ToRewardGroup(QuestRewardCurrencyType currencyType)
        {
            switch (currencyType)
            {
                case QuestRewardCurrencyType.Hard:
                    return ShopRewardGroup.Hard;
                case QuestRewardCurrencyType.Decor:
                    return ShopRewardGroup.Decor;
                default:
                    return ShopRewardGroup.Soft;
            }
        }

        private List<RewardDisplay> BuildRewardDisplays(IReadOnlyList<QuestReward> rewards)
        {
            if (rewards == null || rewards.Count == 0)
            {
                return null;
            }

            var displays = new List<RewardDisplay>(rewards.Count);
            foreach (var reward in rewards)
            {
                if (reward == null)
                {
                    continue;
                }

                displays.Add(new RewardDisplay
                {
                    Icon = reward.Icon,
                    Amount = reward.Amount > 0 ? reward.Amount.ConvertFromLongToString() : string.Empty
                });
            }

            return displays;
        }

        // Bonus shown on the card is the effective per-purchase gain, i.e. the
        // config BaseBonus scaled by the player-progression multiplier, so the
        // number matches what the upgrade actually adds to income (see
        // ShopRuntimeSave.Recalculate). The multiplication is done in double,
        // not float, because long values can exceed float's ~7 significant
        // digits and would otherwise lose precision at high levels.
        private long GetEffectiveBonus(ShopItem item)
        {
            long baseBonus = item.ClickData.BaseBonus;
            if (baseBonus <= 0)
            {
                return 0;
            }

            float multiplier = item.Type == ShopItemType.AutoBuy
                ? _playerProgression?.PassiveIncomeMultiplier ?? 1f
                : _playerProgression?.ClickIncomeMultiplier ?? 1f;

            return Math.Max(1, (long)Math.Round(baseBonus * (double)Math.Max(0f, multiplier)));
        }

        private static string GetBonusRateLocalizationKey(ShopItemType type)
        {
            return type == ShopItemType.AutoBuy
                ? "shop.item.per_second"
                : "shop.item.per_click";
        }

        private long CalculatePrice(long basePrice, int level)
        {
            float priceMultiplier = _bonusEffectService?.ShopPriceMultiplier ?? 1f;
            return Math.Max(1, (long)Mathf.Ceil(basePrice * Mathf.Pow(PriceGrowth, level) * Mathf.Max(0f, priceMultiplier)));
        }

        private Sprite GetPriceIcon(ShopItem item)
        {
            if (item.Type != ShopItemType.PaidBuy)
            {
                return _config.SoftPriceIcon;
            }

            if (item.PaidData == null || item.PaidData.PurchaseKind == GameConfig.PaidShopPurchaseKind.RealMoney)
            {
                return _config.PaidPriceIcon;
            }

            return item.PaidData.PriceCurrencyType == QuestRewardCurrencyType.Soft
                ? _config.SoftPriceIcon
                : _config.PaidPriceIcon;
        }

        private bool CanBuy(ShopItem item, long price)
        {
            if (item.Type == ShopItemType.PaidBuy)
                return CanBuyPaid(item.PaidData);

            return _wallet.CanSpendSoft(price);
        }

        private bool CanBuyPaid(GameConfig.PaidShopData data)
        {
            if (data == null || data.Rewards == null || data.Rewards.Count == 0)
            {
                return false;
            }

            if (data.PurchaseKind == GameConfig.PaidShopPurchaseKind.RealMoney)
            {
                return _purchaseSystem != null && _purchaseSystem.IsAvailable;
            }

            return CanSpendCurrency(data.PriceCurrencyType, data.PriceAmount);
        }

        private void AddPaidItems(IReadOnlyList<GameConfig.PaidShopData> items)
        {
            if (items == null)
            {
                return;
            }

            foreach (var data in items)
            {
                if (data == null || string.IsNullOrEmpty(data.Id))
                {
                    continue;
                }

                if (_items.ContainsKey(data.Id))
                {
                    Debug.LogWarning($"Duplicate shop item id '{data.Id}'. The first item will be used.");
                    continue;
                }

                var item = new ShopItem(data);
                _items.Add(data.Id, item);

                if (!string.IsNullOrEmpty(data.PaymentId))
                    _paidItemsByPaymentId[data.PaymentId] = item;
            }
        }

        private void BuyPaid(ShopItem item)
        {
            if (item.PaidData == null)
            {
                return;
            }

            if (item.PaidData.PurchaseKind == GameConfig.PaidShopPurchaseKind.RealMoney)
            {
                _purchaseSystem.Buy(item.PaidData.PaymentId);
                return;
            }

            if (!SpendCurrency(item.PaidData.PriceCurrencyType, item.PaidData.PriceAmount))
            {
                return;
            }

            GivePaidRewards(item);
        }

        private void OnPurchaseSucceeded(string paymentId)
        {
            if (!_paidItemsByPaymentId.TryGetValue(paymentId, out var item))
            {
                Debug.LogWarning($"Paid shop purchase with payment id '{paymentId}' was not found.");
                return;
            }

            GivePaidRewards(item);
        }

        private void OnPurchaseFailed(string paymentId)
        {
            Debug.LogWarning($"Paid shop purchase failed: {paymentId}");
            StateChanged?.Invoke();
        }

        private void GivePaidRewards(ShopItem item)
        {
            _rewardService.GiveRewards(item.PaidData.Rewards);
            _playerProgression.AddExperience(PlayerExperienceSource.ShopPurchase);
            ItemBought?.Invoke(item.PaidData.Id);
            StateChanged?.Invoke();
        }

        private string GetPaidPriceText(GameConfig.PaidShopData data)
        {
            if (data.PurchaseKind == GameConfig.PaidShopPurchaseKind.RealMoney)
            {
                return _purchaseSystem.GetPrice(data.PaymentId, data.PriceText);
            }

            return _localization.Format(data.PriceAmount.ConvertFromLongToString(), GetCurrencyName(data.PriceCurrencyType));
        }

        private string GetCurrencyName(QuestRewardCurrencyType currencyType)
        {
            switch (currencyType)
            {
                case QuestRewardCurrencyType.Soft:
                    return _localization.Localize("currency.soft");
                case QuestRewardCurrencyType.Decor:
                    return _localization.Localize("currency.decor");
                case QuestRewardCurrencyType.Hard:
                    return _localization.Localize("currency.hard");
                default:
                    return currencyType.ToString();
            }
        }

        private bool CanSpendCurrency(QuestRewardCurrencyType currencyType, long amount)
        {
            if (amount <= 0)
            {
                return true;
            }

            switch (currencyType)
            {
                case QuestRewardCurrencyType.Soft:
                    return _wallet.CanSpendSoft(amount);
                case QuestRewardCurrencyType.Decor:
                    return _wallet.CanSpendMiddle(amount);
                case QuestRewardCurrencyType.Hard:
                    return _wallet.CanSpendHard(amount);
                default:
                    return false;
            }
        }

        private bool SpendCurrency(QuestRewardCurrencyType currencyType, long amount)
        {
            if (amount <= 0)
            {
                return true;
            }

            switch (currencyType)
            {
                case QuestRewardCurrencyType.Soft:
                    return _wallet.SpendSoft(amount);
                case QuestRewardCurrencyType.Decor:
                    return _wallet.SpendMiddle(amount);
                case QuestRewardCurrencyType.Hard:
                    return _wallet.SpendHard(amount);
                default:
                    return false;
            }
        }

        private void OnWalletChanged()
        {
            StateChanged?.Invoke();
        }

        private void OnProgressionChanged()
        {
            _save.Recalculate(_config);
            StateChanged?.Invoke();
        }

        private void OnBonusEffectsChanged()
        {
            StateChanged?.Invoke();
        }

        private class ShopItem
        {
            public readonly ShopItemType Type;
            public readonly GameConfig.ShopDataClick ClickData;
            public readonly GameConfig.PaidShopData PaidData;

            public ShopItem(ShopItemType type, GameConfig.ShopDataClick data)
            {
                Type = type;
                ClickData = data;
            }

            public ShopItem(GameConfig.PaidShopData data)
            {
                Type = ShopItemType.PaidBuy;
                PaidData = data;
            }
        }
    }
}
