using System;
using System.Collections.Generic;
using GameLocalization;
using Utils;
using Zenject;

namespace Customization
{
    public class CustomizationModel : ICustomizationModel, IInitializable, IDisposable
    {
        private ICustomizationService _service;
        private ILocalizationService _localization;
        private CustomizationConfig _config;

        public bool IsOpen { get; private set; }
        public event Action StateChanged;

        [Inject]
        public void Construct(ICustomizationService service, ILocalizationService localization, CustomizationConfig config)
        {
            _service = service;
            _localization = localization;
            _config = config;
        }

        public void Initialize()
        {
            _service.Changed += OnServiceChanged;
        }

        public void Dispose()
        {
            _service.Changed -= OnServiceChanged;
        }

        public List<CustomizationElementViewData> GetAllData()
        {
            var result = new List<CustomizationElementViewData>();
            AddItems(result, CustomizationItemType.Background);
            AddItems(result, CustomizationItemType.Cat);
            return result;
        }

        public void Open()
        {
            SetOpenState(true);
        }

        public void Close()
        {
            SetOpenState(false);
        }

        public void BuyOrSelect(CustomizationItemType type, string id)
        {
            if (IsSelected(type, id))
            {
                return;
            }

            if (_service.IsPurchased(type, id))
            {
                _service.Select(type, id);
                return;
            }

            _service.Buy(type, id);
        }

        private void AddItems(List<CustomizationElementViewData> result, CustomizationItemType type)
        {
            IReadOnlyList<CustomizationConfig.CustomizationItemData> items = _service.GetItems(type);
            for (int i = 0; i < items.Count; i++)
            {
                CustomizationConfig.CustomizationItemData item = items[i];
                if (item == null || string.IsNullOrEmpty(item.Id))
                {
                    continue;
                }

                bool isPurchased = _service.IsPurchased(type, item.Id);
                result.Add(new CustomizationElementViewData
                {
                    Id = item.Id,
                    Type = type,
                    Sprite = item.Sprite,
                    Title = _localization.Localize(item.TitleLocalizationKey),
                    Description = _localization.Localize(item.DescriptionLocalizationKey),
                    PriceIcon = _config != null ? _config.PriceIcon : null,
                    Price = item.Price.ConvertFromLongToString(),
                    IsPurchased = isPurchased,
                    IsSelected = IsSelected(type, item.Id),
                    CanBuy = !isPurchased && _service.CanBuy(type, item.Id)
                });
            }
        }

        private bool IsSelected(CustomizationItemType type, string id)
        {
            return type == CustomizationItemType.Background
                ? _service.ActiveBackgroundId == id
                : _service.ActiveCatId == id;
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

        private void OnServiceChanged()
        {
            StateChanged?.Invoke();
        }
    }
}
