using System;
using System.Collections.Generic;
using QuestSystem;
using UnityEngine;

namespace CardCollections
{
    [CreateAssetMenu(fileName = "CardCollectionConfig", menuName = "Configs/CardCollectionConfig")]
    public class CardCollectionConfig : ScriptableObject
    {
        [SerializeField] private List<CardCollectionData> _collections = new();

        public IReadOnlyList<CardCollectionData> Collections => _collections;

        [Serializable]
        public class CardCollectionData
        {
            [SerializeField] private string _id;
            [SerializeField] private string _titleLocalizationKey;
            [SerializeField] private string _title;
            [SerializeField] private string _descriptionLocalizationKey;
            [TextArea]
            [SerializeField] private string _description;
            [SerializeField] private List<CardData> _cards = new();
            [SerializeField] private List<QuestReward> _rewards = new();

            public string Id => _id;
            public string TitleLocalizationKey => _titleLocalizationKey;
            public string Title => string.IsNullOrEmpty(_title) ? _id : _title;
            public string DescriptionLocalizationKey => _descriptionLocalizationKey;
            public string Description => _description;
            public IReadOnlyList<CardData> Cards => _cards;
            public IReadOnlyList<QuestReward> Rewards => _rewards;
        }

        [Serializable]
        public class CardData
        {
            [SerializeField] private string _id;
            [SerializeField] private string _titleLocalizationKey;
            [SerializeField] private string _title;
            [SerializeField] private string _descriptionLocalizationKey;
            [TextArea]
            [SerializeField] private string _description;
            [Min(1)]
            [SerializeField] private int _requiredAmount = 1;
            [Range(1, 5)]
            [SerializeField] private int _stars = 1;
            [SerializeField] private Sprite _icon;

            public string Id => _id;
            public string TitleLocalizationKey => _titleLocalizationKey;
            public string Title => string.IsNullOrEmpty(_title) ? _id : _title;
            public string DescriptionLocalizationKey => _descriptionLocalizationKey;
            public string Description => _description;
            public int RequiredAmount => Math.Max(1, _requiredAmount);
            public int Stars => Math.Max(1, _stars);
            public Sprite Icon => _icon;
        }
    }
}
