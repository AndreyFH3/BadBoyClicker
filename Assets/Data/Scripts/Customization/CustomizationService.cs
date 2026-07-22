using System;
using System.Collections.Generic;
using Core;
using PlayerFeatures;
using UnityEngine;
using Zenject;

namespace Customization
{
    public class CustomizationService : ICustomizationService, IInitializable, IDisposable
    {
        private CustomizationConfig _config;
        private CustomizationRuntimeSave _save;
        private Wallet _wallet;
        private IPlayerFeatureUnlockService _featureUnlockService;

        // Buy() cascades into a wallet spend and an item select, each of which also
        // wants to raise Changed on its own (they're the only ones who can for
        // changes coming from outside a purchase). This coalesces those into the
        // single explicit raise at the end of Buy(), instead of firing 3x for one purchase.
        private int _suppressChangedDepth;

        public event Action Changed;
        public event Action<CustomizationItemType, string> ActiveItemChanged;
        public event Action<CustomizationItemType, string, long> ItemBought;

        public bool IsUnlocked => _featureUnlockService == null ||
                                   _featureUnlockService.IsUnlocked(PlayerFeatureType.Customization);

        public string ActiveBackgroundId => _save.ActiveBackgroundId;
        public string ActiveCatId => _save.ActiveCatId;
        public Sprite ActiveBackgroundSprite => GetActiveSprite(CustomizationItemType.Background);
        public Sprite ActiveCatSprite => GetActiveSprite(CustomizationItemType.Cat);

        [Inject]
        public void Construct(
            CustomizationConfig config,
            CustomizationRuntimeSave save,
            Wallet wallet,
            IPlayerFeatureUnlockService featureUnlockService)
        {
            _config = config;
            _save = save;
            _wallet = wallet;
            _featureUnlockService = featureUnlockService;
        }

        public void Initialize()
        {
            _wallet.OnMiddleChanged += OnWalletChanged;
            EnsureDefaults();
        }

        public void Dispose()
        {
            _wallet.OnMiddleChanged -= OnWalletChanged;
        }

        public IReadOnlyList<CustomizationConfig.CustomizationItemData> GetItems(CustomizationItemType type)
        {
            return _config != null ? _config.GetItems(type) : Array.Empty<CustomizationConfig.CustomizationItemData>();
        }

        public CustomizationConfig.CustomizationItemData GetItem(CustomizationItemType type, string id)
        {
            return _config != null ? _config.GetItem(type, id) : null;
        }

        public bool IsPurchased(CustomizationItemType type, string id)
        {
            return _save.IsPurchased(type, id);
        }

        public bool HasUnseen(CustomizationItemType type)
        {
            return _save.HasUnseen(type);
        }

        public void MarkSeen(CustomizationItemType type)
        {
            if (!_save.HasUnseen(type))
            {
                return;
            }

            _save.MarkSeen(type);
            Changed?.Invoke();
        }

        public bool CanBuy(CustomizationItemType type, string id)
        {
            if (!IsUnlocked)
            {
                return false;
            }

            CustomizationConfig.CustomizationItemData item = GetItem(type, id);
            return item != null && !item.RewardOnly && !IsPurchased(type, id) && _wallet.CanSpendMiddle(item.Price);
        }

        public bool Buy(CustomizationItemType type, string id)
        {
            if (!IsUnlocked)
            {
                return false;
            }

            CustomizationConfig.CustomizationItemData item = GetItem(type, id);
            if (item == null || item.RewardOnly || IsPurchased(type, id))
            {
                return false;
            }

            bool spent;
            _suppressChangedDepth++;
            try
            {
                spent = _wallet.SpendMiddle(item.Price);
                if (spent)
                {
                    _save.AddPurchased(type, id);
                    ItemBought?.Invoke(type, id, item.Price);
                    Select(type, id);
                }
            }
            finally
            {
                _suppressChangedDepth--;
            }

            if (!spent)
            {
                return false;
            }

            Changed?.Invoke();
            return true;
        }

        public bool Select(CustomizationItemType type, string id)
        {
            if (!IsUnlocked || GetItem(type, id) == null || !IsPurchased(type, id))
            {
                return false;
            }

            string previousId = GetActiveId(type);
            _save.SetActive(type, id);

            if (previousId != id)
            {
                ActiveItemChanged?.Invoke(type, id);
                RaiseChanged();
            }

            return true;
        }

        public void Give(CustomizationItemType type, string id)
        {
            if (GetItem(type, id) == null)
            {
                Debug.LogWarning($"Customization item with id '{id}' was not found in {type}.");
                return;
            }

            bool isNew = !IsPurchased(type, id);
            _save.AddPurchased(type, id);
            if (isNew)
            {
                _save.MarkUnseen(type, id);
            }
            Changed?.Invoke();
        }

        public void GiveBackground(string backgroundId)
        {
            Give(CustomizationItemType.Background, backgroundId);
        }

        public void Set(CustomizationSaveData data)
        {
            _save.Set(data);
            EnsureDefaults();
            Changed?.Invoke();
            ActiveItemChanged?.Invoke(CustomizationItemType.Background, ActiveBackgroundId);
            ActiveItemChanged?.Invoke(CustomizationItemType.Cat, ActiveCatId);
        }

        public CustomizationSaveData Get()
        {
            return _save.Get();
        }

        private void EnsureDefaults()
        {
            EnsureDefault(CustomizationItemType.Background);
            EnsureDefault(CustomizationItemType.Cat);
        }

        private void EnsureDefault(CustomizationItemType type)
        {
            CustomizationConfig.CustomizationItemData defaultItem = _config != null ? _config.GetDefaultItem(type) : null;
            if (defaultItem == null || string.IsNullOrEmpty(defaultItem.Id))
            {
                return;
            }

            _save.AddPurchased(type, defaultItem.Id);

            if (GetItem(type, GetActiveId(type)) == null || !IsPurchased(type, GetActiveId(type)))
            {
                _save.SetActive(type, defaultItem.Id);
                ActiveItemChanged?.Invoke(type, defaultItem.Id);
            }
        }

        private Sprite GetActiveSprite(CustomizationItemType type)
        {
            CustomizationConfig.CustomizationItemData item = GetItem(type, GetActiveId(type));
            return item != null ? item.Sprite : null;
        }

        private string GetActiveId(CustomizationItemType type)
        {
            return type == CustomizationItemType.Background ? _save.ActiveBackgroundId : _save.ActiveCatId;
        }

        private void OnWalletChanged()
        {
            RaiseChanged();
        }

        private void RaiseChanged()
        {
            if (_suppressChangedDepth <= 0)
            {
                Changed?.Invoke();
            }
        }
    }
}
