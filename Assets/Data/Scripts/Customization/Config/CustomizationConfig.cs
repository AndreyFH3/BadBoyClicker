using System;
using System.Collections.Generic;
using UnityEngine;

namespace Customization
{
    [CreateAssetMenu(fileName = "CustomizationConfig", menuName = "Configs/CustomizationConfig")]
    public class CustomizationConfig : ScriptableObject
    {
        [SerializeField] private List<CustomizationItemData> _backgrounds = new();
        [SerializeField] private List<CustomizationItemData> _cats = new();
        [SerializeField] private Sprite _priceIcon;

        public IReadOnlyList<CustomizationItemData> Backgrounds => _backgrounds;
        public IReadOnlyList<CustomizationItemData> Cats => _cats;
        public Sprite PriceIcon => _priceIcon;

        public IReadOnlyList<CustomizationItemData> GetItems(CustomizationItemType type)
        {
            return type == CustomizationItemType.Background ? _backgrounds : _cats;
        }

        public CustomizationItemData GetItem(CustomizationItemType type, string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            IReadOnlyList<CustomizationItemData> items = GetItems(type);
            for (int i = 0; i < items.Count; i++)
            {
                CustomizationItemData item = items[i];
                if (item != null && item.Id == id)
                {
                    return item;
                }
            }

            return null;
        }

        public CustomizationItemData GetDefaultItem(CustomizationItemType type)
        {
            IReadOnlyList<CustomizationItemData> items = GetItems(type);
            return items.Count > 0 ? items[0] : null;
        }

        [Serializable]
        public class CustomizationItemData
        {
            [SerializeField] private string _id;
            [SerializeField] private string _titleLocalizationKey;
            [SerializeField] private string _title;
            [SerializeField] private string _descriptionLocalizationKey;
            [TextArea]
            [SerializeField] private string _description;
            [SerializeField] private Sprite _sprite;
            [Min(0)]
            [SerializeField] private long _price;
            [Tooltip("If true, this item cannot be bought directly in the shop and can only be unlocked via Give (e.g. a collection reward).")]
            [SerializeField] private bool _rewardOnly;

            public string Id => _id;
            public string TitleLocalizationKey => _titleLocalizationKey;
            public string Title => string.IsNullOrEmpty(_title) ? _id : _title;
            public string DescriptionLocalizationKey => _descriptionLocalizationKey;
            public string Description => _description;
            public Sprite Sprite => _sprite;
            public long Price => Math.Max(0, _price);
            public bool RewardOnly => _rewardOnly;
        }
    }
}
