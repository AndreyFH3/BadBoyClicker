using System;
using System.Collections.Generic;
using UnityEngine;
using Core;
using Utils;
using Zenject;
using PlayerProgression;
using GameLocalization;
using AdBonusOffers;

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
        private readonly Dictionary<string, ShopItem> _items = new();

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
            IAdBonusEffectService bonusEffectService)
        {
            _config = config;
            _wallet = wallet;
            _save = save;
            _playerProgression = playerProgression;
            _localization = localization;
            _bonusEffectService = bonusEffectService;

            BuildItems();
            _save.Recalculate(_config);
            IsOpen = _isOpenOnStart;
        }

        public void Initialize()
        {
            _wallet.OnSoftChanged += OnWalletChanged;
            _playerProgression.Changed += OnProgressionChanged;
            _bonusEffectService.Changed += OnBonusEffectsChanged;
        }

        public void Dispose()
        {
            _wallet.OnSoftChanged -= OnWalletChanged;
            _playerProgression.Changed -= OnProgressionChanged;
            _bonusEffectService.Changed -= OnBonusEffectsChanged;
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

        public void Buy(string id)
        {
            if (!_items.TryGetValue(id, out var item))
            {
                Debug.LogWarning($"Shop item with id '{id}' was not found.");
                return;
            }

            if (item.Type == ShopItemType.PaidBuy)
            {
                Debug.LogWarning("Paid shop purchases are not implemented yet.");
                return;
            }

            long price = CalculatePrice(item.Data.BasePrice, _save.GetLevel(item.Type, item.Data.Id));
            if (!_wallet.SpendSoft(price))
            {
                return;
            }

            _save.AddLevel(item.Type, item.Data.Id);
            _playerProgression.AddExperience(PlayerExperienceSource.ShopPurchase);
            _save.Recalculate(_config);
            ItemBought?.Invoke(item.Data.Id);
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
            if (_config == null)
            {
                return;
            }

            AddItems(_config.Clicks, ShopItemType.Click);
            AddItems(_config.AutoBuys, ShopItemType.AutoBuy);
            AddItems(_config.PaidBuys, ShopItemType.PaidBuy);
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
            int level = _save.GetLevel(item.Type, item.Data.Id);
            long price = CalculatePrice(item.Data.BasePrice, level);

            return new ShopElementData
            {
                Id = item.Data.Id,
                Type = item.Type,
                Icon = item.Data.Icon,
                Name = _localization.Localize(item.Data.NameLocalizationKey, item.Data.Name),
                Level = _localization.Format("shop.item.level", "Level {0}", level),
                PriceIcon = GetPriceIcon(item.Type),
                Price = price.ConvertFromLongToString(),
                Bonus = _localization.Format("shop.item.bonus", "+{0}", item.Data.BaseBonus.ConvertFromLongToString()),
                CanBuy = CanBuy(item, price)
            };
        }

        private long CalculatePrice(long basePrice, int level)
        {
            float priceMultiplier = _bonusEffectService?.ShopPriceMultiplier ?? 1f;
            return Math.Max(1, (long)Mathf.Ceil(basePrice * Mathf.Pow(PriceGrowth, level) * Mathf.Max(0f, priceMultiplier)));
        }

        private Sprite GetPriceIcon(ShopItemType type)
        {
            return type == ShopItemType.PaidBuy ? _config.PaidPriceIcon : _config.SoftPriceIcon;
        }

        private bool CanBuy(ShopItem item, long price)
        {
            if (item.Type == ShopItemType.PaidBuy)
                return false;

            return _wallet.CanSpendSoft(price);
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
            public readonly GameConfig.ShopDataClick Data;

            public ShopItem(ShopItemType type, GameConfig.ShopDataClick data)
            {
                Type = type;
                Data = data;
            }
        }
    }
}
