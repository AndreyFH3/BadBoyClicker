using System;
using System.Collections.Generic;
using UnityEngine;
using Core;
using Utils;
using Zenject;
using PlayerFeatures;
using PlayerProgression;
using GameLocalization;
using AdBonusOffers;
using CardCollections;
using Purchases;
using QuestSystem;
using Rewards;
using Chests;

namespace Shop
{
    public class ShopModel : IShopModel, IInitializable, IDisposable
    {
        private GameConfig _config;
        private Wallet _wallet;
        private IShopRuntimeSave _save;
        private IPlayerProgressionService _playerProgression;
        private ILocalizationService _localization;
        private IBuffService _buffService;
        private ICardCollectionBonusService _collectionBonusService;
        private IPurchaseSystem _purchaseSystem;
        private IQuestRewardService _rewardService;
        private IPlayerFeatureUnlockService _featureUnlockService;
        private ICardCollectionService _cardCollectionService;
        private ChestConfig _chestConfig;
        private readonly Dictionary<string, ShopItem> _items = new();
        private readonly Dictionary<string, ShopItem> _paidItemsByPaymentId = new();

        // Buying an item cascades through Wallet/PlayerProgression/CollectionBonus
        // events that each also want to raise StateChanged (they're the only ones
        // who can for changes coming from outside a purchase, e.g. auto-income).
        // Without this guard, a single Buy() rebuilt the whole shop catalog 2-3x
        // in one frame. While > 0, those handlers skip the redundant raise and the
        // purchase call raises StateChanged exactly once when it's actually done.
        private int _suppressStateChangedDepth;

        public long AutoIncomePerSecond => _save?.AutoIncomePerSecond ?? 0;

        public event Action StateChanged;
        public event Action<string> ItemBought;
        public event Action<string> PaidPurchaseSucceeded;
        public event Action<string> PaidPurchaseFailed;

        [Zenject.Inject]
        public void Construct(
            GameConfig config,
            Wallet wallet,
            IShopRuntimeSave save,
            IPlayerProgressionService playerProgression,
            ILocalizationService localization,
            IBuffService buffService,
            ICardCollectionBonusService collectionBonusService,
            IPurchaseSystem purchaseSystem,
            IQuestRewardService rewardService,
            IPlayerFeatureUnlockService featureUnlockService,
            ICardCollectionService cardCollectionService,
            ChestConfig chestConfig)
        {
            _config = config;
            _wallet = wallet;
            _save = save;
            _playerProgression = playerProgression;
            _localization = localization;
            _buffService = buffService;
            _collectionBonusService = collectionBonusService;
            _purchaseSystem = purchaseSystem;
            _rewardService = rewardService;
            _featureUnlockService = featureUnlockService;
            _cardCollectionService = cardCollectionService;
            _chestConfig = chestConfig;

            BuildItems();
            _save.Recalculate(_config);
        }

        public void Initialize()
        {
            _wallet.OnChanged += OnWalletChanged;
            _playerProgression.Changed += OnProgressionChanged;
            _buffService.Changed += OnBonusEffectsChanged;
            _collectionBonusService.Changed += OnCollectionBonusChanged;
            _purchaseSystem.PurchaseSucceeded += OnPurchaseSucceeded;
            _purchaseSystem.PurchaseFailed += OnPurchaseFailed;
            _cardCollectionService.Changed += OnCardCollectionsChanged;
        }

        public void Dispose()
        {
            _wallet.OnChanged -= OnWalletChanged;
            _playerProgression.Changed -= OnProgressionChanged;
            _buffService.Changed -= OnBonusEffectsChanged;
            _collectionBonusService.Changed -= OnCollectionBonusChanged;
            _purchaseSystem.PurchaseSucceeded -= OnPurchaseSucceeded;
            _purchaseSystem.PurchaseFailed -= OnPurchaseFailed;
            _cardCollectionService.Changed -= OnCardCollectionsChanged;
        }

        // Cheap check for sign/badge UI: whether anything of the given type (or,
        // with no type, anything at all) is currently buyable. Unlike GetAllData()
        // it never localizes text or allocates per-item view data.
        public bool HasAnyBuyable(ShopItemType? type = null)
        {
            foreach (var item in _items.Values)
            {
                if (type.HasValue && item.Type != type.Value)
                {
                    continue;
                }

                if (!IsAvailable(item))
                {
                    continue;
                }

                long price = item.Type == ShopItemType.PaidBuy
                    ? 0
                    : CalculatePrice(item.ClickData.BasePrice, _save.GetLevel(item.Type, item.ClickData.Id));

                if (CanBuy(item, price))
                {
                    return true;
                }
            }

            return false;
        }

        public List<ShopElementData> GetAllData()
        {
            List<ShopElementData> datas = new();
            foreach (var item in _items.Values)
            {
                if (!IsAvailable(item))
                {
                    continue;
                }

                datas.Add(CreateElementData(item));
            }

            return datas;
        }

        public ShopElementData GetShopPositionData(string id)
        {
            return _items.TryGetValue(id, out var item) && IsAvailable(item) ? CreateElementData(item) : null;
        }

        public ShopElementData GetCardChestPurchaseData()
        {
            foreach (var item in _items.Values)
            {
                if (item.Type == ShopItemType.PaidBuy && IsCardChestOffer(item.PaidData) && IsAvailable(item))
                {
                    return CreateElementData(item);
                }
            }

            return null;
        }

        public string GetCardChestId()
        {
            if (_chestConfig?.Chests == null)
            {
                return null;
            }

            foreach (var chest in _chestConfig.Chests)
            {
                if (IsCardOnlyChest(chest))
                {
                    return chest.Id;
                }
            }

            return null;
        }

        public ShopPurchaseConfirmationData GetPurchaseConfirmationData(string id)
        {
            if (!_items.TryGetValue(id, out var item) || item.Type != ShopItemType.PaidBuy || item.PaidData == null || !IsAvailable(item))
            {
                return null;
            }

            var data = item.PaidData;
            string name = _localization.Localize(data.NameLocalizationKey);
            string price = GetPaidPriceText(data);
            string reward = ResolvePaidRewardText(data);

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

            if (!IsAvailable(item))
            {
                Debug.LogWarning($"Shop item '{id}' is locked and cannot be purchased.");
                return;
            }

            if (item.Type == ShopItemType.PaidBuy)
            {
                BuyPaid(item);
                return;
            }

            long price = CalculatePrice(item.ClickData.BasePrice, _save.GetLevel(item.Type, item.ClickData.Id));

            bool spent;
            _suppressStateChangedDepth++;
            try
            {
                spent = _wallet.SpendSoft(price);
                if (spent)
                {
                    _save.AddLevel(item.Type, item.ClickData.Id);
                    _playerProgression.AddExperience(PlayerExperienceSource.ShopPurchase, price);
                    _save.Recalculate(_config);
                }
            }
            finally
            {
                _suppressStateChangedDepth--;
            }

            if (!spent)
            {
                return;
            }

            ItemBought?.Invoke(item.ClickData.Id);
            StateChanged?.Invoke();
        }

        // Some paid offers grant content owned by another gated system (e.g. a chest
        // reward requires the Chests feature to actually open, a background reward
        // requires Customization to be given). Selling those offers before that
        // system unlocks would let the player pay for a reward they can never
        // receive, so the offer itself stays hidden and unbuyable until then.
        private bool IsAvailable(ShopItem item)
        {
            if (item.Type != ShopItemType.PaidBuy)
            {
                return item.ClickData == null ||
                       _playerProgression == null ||
                       _playerProgression.CurrentLevel >= item.ClickData.RequiredPlayerLevel;
            }

            return RewardFeatureGate.AreAvailable(item.PaidData?.Rewards, _featureUnlockService) &&
                   !(_cardCollectionService?.AreAllCollectionsCompleted == true && IsCardChestOffer(item.PaidData));
        }

        private bool IsCardChestOffer(GameConfig.PaidShopData data)
        {
            if (data?.Rewards == null)
            {
                return false;
            }

            foreach (var reward in data.Rewards)
            {
                if (reward?.RewardType != QuestRewardType.Chest)
                {
                    continue;
                }

                var chest = FindChest(reward.RewardId);
                if (chest?.Rewards == null || chest.Rewards.Count == 0)
                {
                    continue;
                }

                if (IsCardOnlyChest(chest))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsCardOnlyChest(ChestConfig.ChestData chest)
        {
            if (chest?.Rewards == null || chest.Rewards.Count == 0)
            {
                return false;
            }

            foreach (var entry in chest.Rewards)
            {
                if (entry == null || entry.RewardKind != ChestConfig.ChestRewardKind.RandomCard)
                {
                    return false;
                }
            }

            return true;
        }

        private ChestConfig.ChestData FindChest(string chestId)
        {
            if (string.IsNullOrEmpty(chestId) || _chestConfig?.Chests == null)
            {
                return null;
            }

            foreach (var chest in _chestConfig.Chests)
            {
                if (chest != null && chest.Id == chestId)
                {
                    return chest;
                }
            }

            return null;
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
                Bonus = ResolvePaidRewardText(data),
                CanBuy = CanBuy(item, 0),
                RewardGroup = DetermineRewardGroup(data.Rewards),
                IsRealMoney = data.PurchaseKind == GameConfig.PaidShopPurchaseKind.RealMoney,
                Rewards = BuildRewardDisplays(data)
            };
        }

        private ShopRewardGroup DetermineRewardGroup(IReadOnlyList<QuestReward> rewards)
        {
            if (rewards == null)
            {
                return ShopRewardGroup.Soft;
            }

            // Chests and experience-percent offers get their own dedicated slots
            // regardless of any other rewards the offer might also grant, so they
            // never fall back into whichever currency slot happens to be resolved last.
            foreach (var reward in rewards)
            {
                if (reward != null && reward.RewardType == QuestRewardType.Chest)
                {
                    return ShopRewardGroup.Chest;
                }

                if (reward != null && reward.RewardType == QuestRewardType.Custom &&
                    PlayerProgressionExperienceRewardService.IsExperienceRewardId(reward.RewardId))
                {
                    return ShopRewardGroup.Experience;
                }
            }

            // Otherwise group by the first currency reward the offer grants;
            // non-currency rewards (backgrounds, boosts) don't define a slot on their own.
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

        private List<RewardDisplay> BuildRewardDisplays(GameConfig.PaidShopData data)
        {
            var rewards = data?.Rewards;
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

                bool isTimeBased = IsTimeBasedSoftReward(data, reward);
                long amount = isTimeBased ? ComputeTimeBasedSoftAmount(data) : reward.Amount;

                displays.Add(new RewardDisplay
                {
                    Icon = reward.Icon,
                    // Time-based rewards show their computed amount even when it's
                    // zero (no income yet), so the player sees why the offer is locked.
                    Amount = isTimeBased || amount > 0 ? amount.ConvertFromLongToString() : string.Empty
                });
            }

            return displays;
        }

        // A paid offer opts into the "N minutes of production" reward by setting
        // TimeBasedRewardMinutes > 0 on a soft-currency reward entry; the configured
        // Amount on that entry is then ignored in favor of the live computation.
        private bool IsTimeBasedSoftReward(GameConfig.PaidShopData data, QuestReward reward)
        {
            return data != null && data.TimeBasedRewardMinutes > 0 &&
                   reward != null &&
                   reward.RewardType == QuestRewardType.Currency &&
                   reward.CurrencyType == QuestRewardCurrencyType.Soft;
        }

        private long ComputeTimeBasedSoftAmount(GameConfig.PaidShopData data)
        {
            double incomePerSecond = Math.Max(0, AutoIncomePerSecond) +
                                     Math.Max(0, _save?.ClickValue ?? 0) *
                                     (_config?.RewardIncomeClicksPerSecond ?? 0f);
            double amount = incomePerSecond * Math.Max(0, data.TimeBasedRewardMinutes) * 60d;
            return amount >= long.MaxValue ? long.MaxValue : Math.Max(0, (long)Math.Ceiling(amount));
        }

        private string ResolvePaidRewardText(GameConfig.PaidShopData data)
        {
            var timeBasedReward = FindTimeBasedSoftReward(data);
            if (timeBasedReward == null)
            {
                return _localization.Localize(data.RewardTextLocalizationKey);
            }

            long amount = ComputeTimeBasedSoftAmount(data);
            if (amount <= 0)
            {
                return _localization.Localize("shop.paid.time_reward.locked");
            }

            return _localization.Format(
                "shop.item.bonus",
                amount.ConvertFromLongToString(),
                _localization.Localize("currency.soft"));
        }

        private QuestReward FindTimeBasedSoftReward(GameConfig.PaidShopData data)
        {
            if (data?.Rewards == null)
            {
                return null;
            }

            foreach (var reward in data.Rewards)
            {
                if (IsTimeBasedSoftReward(data, reward))
                {
                    return reward;
                }
            }

            return null;
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
                ? (_playerProgression?.PassiveIncomeMultiplier ?? 1f) * (_collectionBonusService?.PassiveIncomeMultiplier ?? 1f)
                : (_playerProgression?.ClickIncomeMultiplier ?? 1f) * (_collectionBonusService?.ClickIncomeMultiplier ?? 1f);

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
            float priceMultiplier = (_buffService?.ShopPriceMultiplier ?? 1f) * (_collectionBonusService?.ShopPriceMultiplier ?? 1f);
            double price = Math.Max(0L, basePrice) *
                           Math.Pow(_config?.ShopPriceGrowth ?? 1.16f, Math.Max(0, level)) *
                           Math.Max(0f, priceMultiplier);

            if (price >= long.MaxValue)
            {
                return long.MaxValue;
            }

            return Math.Max(1, (long)Math.Ceiling(price));
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

            var timeBasedReward = FindTimeBasedSoftReward(data);
            if (timeBasedReward != null && ComputeTimeBasedSoftAmount(data) <= 0)
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

            bool spent;
            _suppressStateChangedDepth++;
            try
            {
                spent = SpendCurrency(item.PaidData.PriceCurrencyType, item.PaidData.PriceAmount);
                if (spent)
                {
                    ApplyPaidRewards(item);
                }
            }
            finally
            {
                _suppressStateChangedDepth--;
            }

            if (!spent)
            {
                PaidPurchaseFailed?.Invoke(item.PaidData.Id);
                return;
            }

            ItemBought?.Invoke(item.PaidData.Id);
            PaidPurchaseSucceeded?.Invoke(item.PaidData.Id);
            StateChanged?.Invoke();
        }

        private void OnPurchaseSucceeded(string paymentId)
        {
            if (!_paidItemsByPaymentId.TryGetValue(paymentId, out var item))
            {
                Debug.LogWarning($"Paid shop purchase with payment id '{paymentId}' was not found.");
                PaidPurchaseFailed?.Invoke(paymentId);
                return;
            }

            _suppressStateChangedDepth++;
            try
            {
                ApplyPaidRewards(item);
            }
            finally
            {
                _suppressStateChangedDepth--;
            }

            ItemBought?.Invoke(item.PaidData.Id);
            PaidPurchaseSucceeded?.Invoke(item.PaidData.Id);
            StateChanged?.Invoke();
        }

        private void OnPurchaseFailed(string paymentId)
        {
            Debug.LogWarning($"Paid shop purchase failed: {paymentId}");
            string itemId = _paidItemsByPaymentId.TryGetValue(paymentId, out var item)
                ? item.PaidData.Id
                : paymentId;
            PaidPurchaseFailed?.Invoke(itemId);
            StateChanged?.Invoke();
        }

        private void ApplyPaidRewards(ShopItem item)
        {
            ApplyTimeBasedRewardAmount(item.PaidData);
            _rewardService.GiveRewardsImmediately(item.PaidData.Rewards);
            _playerProgression.AddExperience(PlayerExperienceSource.ShopPurchase);
        }

        private void ApplyTimeBasedRewardAmount(GameConfig.PaidShopData data)
        {
            var reward = FindTimeBasedSoftReward(data);
            reward?.SetAmount(ComputeTimeBasedSoftAmount(data));
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
            RaiseStateChanged();
        }

        private void OnProgressionChanged()
        {
            _save.Recalculate(_config);
            RaiseStateChanged();
        }

        private void OnBonusEffectsChanged()
        {
            RaiseStateChanged();
        }

        private void OnCollectionBonusChanged()
        {
            _save.Recalculate(_config);
            RaiseStateChanged();
        }

        private void OnCardCollectionsChanged()
        {
            RaiseStateChanged();
        }

        // These handlers also fire while a purchase is in progress, since buying
        // spends the wallet / adds experience itself. Buy()/BuyPaid() already
        // raise StateChanged once when the purchase completes, so this skips the
        // redundant raise from inside that same call.
        private void RaiseStateChanged()
        {
            if (_suppressStateChangedDepth <= 0)
            {
                StateChanged?.Invoke();
            }
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
