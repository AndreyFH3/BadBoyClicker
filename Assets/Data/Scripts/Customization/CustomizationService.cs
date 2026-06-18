using System;
using System.Collections.Generic;
using Core;
using UnityEngine;
using Zenject;

namespace Customization
{
    public class CustomizationService : ICustomizationService, IInitializable, IDisposable
    {
        private CustomizationConfig _config;
        private CustomizationRuntimeSave _save;
        private Wallet _wallet;

        public event Action Changed;
        public event Action<CustomizationItemType, string> ActiveItemChanged;

        public string ActiveBackgroundId => _save.ActiveBackgroundId;
        public string ActiveCatId => _save.ActiveCatId;
        public Sprite ActiveBackgroundSprite => GetActiveSprite(CustomizationItemType.Background);
        public Sprite ActiveCatSprite => GetActiveSprite(CustomizationItemType.Cat);

        [Inject]
        public void Construct(CustomizationConfig config, CustomizationRuntimeSave save, Wallet wallet)
        {
            _config = config;
            _save = save;
            _wallet = wallet;
        }

        public void Initialize()
        {
            _wallet.OnSoftChanged += OnWalletChanged;
            EnsureDefaults();
        }

        public void Dispose()
        {
            _wallet.OnSoftChanged -= OnWalletChanged;
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

        public bool CanBuy(CustomizationItemType type, string id)
        {
            CustomizationConfig.CustomizationItemData item = GetItem(type, id);
            return item != null && !IsPurchased(type, id) && _wallet.CanSpendSoft(item.Price);
        }

        public bool Buy(CustomizationItemType type, string id)
        {
            CustomizationConfig.CustomizationItemData item = GetItem(type, id);
            if (item == null || IsPurchased(type, id))
            {
                return false;
            }

            if (!_wallet.SpendSoft(item.Price))
            {
                return false;
            }

            _save.AddPurchased(type, id);
            Select(type, id);
            Changed?.Invoke();
            return true;
        }

        public bool Select(CustomizationItemType type, string id)
        {
            if (GetItem(type, id) == null || !IsPurchased(type, id))
            {
                return false;
            }

            string previousId = GetActiveId(type);
            _save.SetActive(type, id);

            if (previousId != id)
            {
                ActiveItemChanged?.Invoke(type, id);
                Changed?.Invoke();
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

            _save.AddPurchased(type, id);
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
            Changed?.Invoke();
        }
    }
}
