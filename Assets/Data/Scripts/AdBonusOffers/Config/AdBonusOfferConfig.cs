using System;
using System.Collections.Generic;
using QuestSystem;
using UnityEngine;

namespace AdBonusOffers
{
    [CreateAssetMenu(fileName = "AdBonusOfferConfig", menuName = "Configs/AdBonusOfferConfig")]
    public class AdBonusOfferConfig : ScriptableObject
    {
        [SerializeField] private float _initialDelaySeconds = 15f;
        [SerializeField] private float _offerIntervalSeconds = 90f;
        [SerializeField] private float _visibleDurationSeconds = 20f;
        [SerializeField] private List<AdBonusOfferData> _offers = new();
        [SerializeField] private List<AdBonusEffectData> _boosts = new();

        public float InitialDelaySeconds => Mathf.Max(0f, _initialDelaySeconds);
        public float OfferIntervalSeconds => Mathf.Max(1f, _offerIntervalSeconds);
        public float VisibleDurationSeconds => Mathf.Max(1f, _visibleDurationSeconds);
        public IReadOnlyList<AdBonusOfferData> Offers => _offers;
        public IReadOnlyList<AdBonusEffectData> Boosts => _boosts;

        [Serializable]
        public class AdBonusOfferData
        {
            [SerializeField] private string _id;
            [SerializeField] private string _placementId;
            [SerializeField] private string _confirmationTitleLocalizationKey;
            [SerializeField] private string _confirmationTitle;
            [SerializeField] private string _confirmationDescriptionLocalizationKey;
            [TextArea]
            [SerializeField] private string _confirmationDescription;
            [SerializeField] private string _resultDescriptionLocalizationKey;
            [TextArea]
            [SerializeField] private string _resultDescription;
            [SerializeField] private Sprite _icon;
            [Min(1)]
            [SerializeField] private int _weight = 1;
            [Min(0)]
            [SerializeField] private float _cooldownSeconds = 300f;
            [Min(0)]
            [SerializeField] private long _hardPrice;
            [SerializeField] private List<QuestReward> _rewards = new();
            [SerializeField] private List<AdBonusEffectData> _effects = new();

            public string Id => _id;
            public string PlacementId => string.IsNullOrEmpty(_placementId) ? _id : _placementId;
            public string ConfirmationTitleLocalizationKey => _confirmationTitleLocalizationKey;
            public string ConfirmationTitle => string.IsNullOrEmpty(_confirmationTitle) ? _id : _confirmationTitle;
            public string ConfirmationDescriptionLocalizationKey => _confirmationDescriptionLocalizationKey;
            public string ConfirmationDescription => _confirmationDescription;
            public string ResultDescriptionLocalizationKey => _resultDescriptionLocalizationKey;
            public string ResultDescription => string.IsNullOrEmpty(_resultDescription) ? _confirmationDescription : _resultDescription;
            public Sprite Icon => _icon;
            public int Weight => Mathf.Max(1, _weight);
            public float CooldownSeconds => Mathf.Max(0f, _cooldownSeconds);
            public long HardPrice => Math.Max(0, _hardPrice);
            public IReadOnlyList<QuestReward> Rewards => _rewards;
            public IReadOnlyList<AdBonusEffectData> Effects => _effects;
        }

        [Serializable]
        public class AdBonusEffectData
        {
            [SerializeField] private string _id;
            [SerializeField] private AdBonusEffectType _effectType;
            [Min(0f)]
            [SerializeField] private float _durationSeconds = 60f;
            [Min(0f)]
            [SerializeField] private float _multiplier = 2f;
            [Range(0f, 100f)]
            [SerializeField] private float _discountPercent = 25f;
            [SerializeField] private QuestReward _reward;

            public string Id => _id;
            public AdBonusEffectType EffectType => _effectType;
            public float DurationSeconds => Mathf.Max(0f, _durationSeconds);
            public float Multiplier => Mathf.Max(0f, _multiplier);
            public float DiscountPercent => Mathf.Clamp(_discountPercent, 0f, 100f);
            public QuestReward Reward => _reward;
        }
    }
}
