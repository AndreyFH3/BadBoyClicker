using UnityEngine;
using System.Collections.Generic;
using PlayerProgression;
using QuestSystem;

namespace Core
{
    [CreateAssetMenu(fileName = "GameConfig", order = 0, menuName = "Config")]
    public class GameConfig : ScriptableObject
    {
        [SerializeField] private List<ShopDataClick> _clicks;
        [SerializeField] private List<ShopDataClick> _autoBuys;
        [SerializeField] private List<PaidShopData> _paidBuys;
        [Min(1f)]
        [SerializeField] private float _shopPriceGrowth = 1.16f;
        [Min(0f)]
        [Tooltip("Assumed active clicks per second when converting current income into time-based rewards.")]
        [SerializeField] private float _rewardIncomeClicksPerSecond = 2f;
        [SerializeField] private Sprite _softPriceIcon;
        [SerializeField] private Sprite _paidPriceIcon;
        [SerializeField] private OfflineIncomeData _offlineIncome = new();
        [SerializeField] private PlayerProgressionData _playerProgression = new();
        [SerializeField] private ApplicationData _data;

        public IReadOnlyList<ShopDataClick> Clicks => _clicks;
        public IReadOnlyList<ShopDataClick> AutoBuys => _autoBuys;
        public IReadOnlyList<PaidShopData> PaidBuys => _paidBuys;
        public float ShopPriceGrowth => Mathf.Max(1f, _shopPriceGrowth);
        public float RewardIncomeClicksPerSecond => Mathf.Max(0f, _rewardIncomeClicksPerSecond);
        public Sprite SoftPriceIcon => _softPriceIcon;
        public Sprite PaidPriceIcon => _paidPriceIcon;
        public OfflineIncomeData OfflineIncome => _offlineIncome;
        public PlayerProgressionData PlayerProgression => _playerProgression;

        [System.Serializable]
        public class PlayerProgressionData
        {
            [SerializeField] private long _middleRewardPerLevel = 5;
            [SerializeField] private Sprite _middleRewardIcon;
            [SerializeField] private float _purchaseExperiencePercent = 0.5f;
            [SerializeField] private long _baseExperienceToComplete = 1000;
            [SerializeField] private float _experienceGrowth = 1.4f;
            [Min(0f)]
            [SerializeField] private float _levelBonusScale = 1f;
            [SerializeField] private List<ExperienceRewardData> _experienceRewards = new();
            [SerializeField] private List<PlayerLevelData> _levels = new();

            public long MiddleRewardPerLevel => _middleRewardPerLevel;
            public Sprite MiddleRewardIcon => _middleRewardIcon;
            public float PurchaseExperiencePercent => _purchaseExperiencePercent;
            public long BaseExperienceToComplete => _baseExperienceToComplete;
            public float ExperienceGrowth => _experienceGrowth;
            public float LevelBonusScale => Mathf.Max(0f, _levelBonusScale);
            public IReadOnlyList<ExperienceRewardData> ExperienceRewards => _experienceRewards;
            public IReadOnlyList<PlayerLevelData> Levels => _levels;
        }

        [System.Serializable]
        public class ExperienceRewardData
        {
            [SerializeField] private PlayerExperienceSource _source;
            [SerializeField] private long _experience;

            public PlayerExperienceSource Source => _source;
            public long Experience => _experience;
        }

        [System.Serializable]
        public class PlayerLevelData
        {
            [SerializeField] private long _decorReward;
            [SerializeField] private List<PlayerProgressBonusData> _bonuses = new();
            [SerializeField] private List<QuestReward> _rewards = new();

            public long DecorReward => _decorReward;
            public IReadOnlyList<PlayerProgressBonusData> Bonuses => _bonuses;
            public IReadOnlyList<QuestReward> Rewards => _rewards;
        }

        [System.Serializable]
        public class PlayerProgressBonusData
        {
            [SerializeField] private PlayerProgressBonusType _type;
            [SerializeField] private float _percent;
            [SerializeField] private Sprite _icon;

            public PlayerProgressBonusType Type => _type;
            public float Percent => _percent;
            public Sprite Icon => _icon;
        }

        [System.Serializable]
        public class OfflineIncomeData
        {
            [SerializeField] private int _hardClaimCost = 10;
            [SerializeField] private int _adMultiplier = 2;
            [SerializeField] private int _paidMultiplier = 2;
            [SerializeField] private int _minSecondsToShow = 60;
            [Min(0)]
            [SerializeField] private int _maxSecondsToReward = 28800;
            [Range(0f, 100f)]
            [SerializeField] private float _baseIncomePercent = 50f;

            public int HardClaimCost => _hardClaimCost;
            public int AdMultiplier => _adMultiplier;
            public int PaidMultiplier => _paidMultiplier;
            public int MinSecondsToShow => _minSecondsToShow;
            public int MaxSecondsToReward => Mathf.Max(0, _maxSecondsToReward);
            public float BaseIncomePercent => _baseIncomePercent;
        }

        [System.Serializable]
        public class ApplicationData
        {
            [SerializeField] private ScenesId _scenesId;
                 
            [System.Serializable]
            public class ScenesId
            {
                [SerializeField] private string _mainSceneId;

            }

        }

        [System.Serializable]
        public class ShopDataClick
        {
            [SerializeField] private string _id;
            [SerializeField] private string _nameLocalizationKey;
            [SerializeField] private string _name;
            [SerializeField] private Sprite _icon;
            [SerializeField] private long _basePrice;
            [SerializeField] private long _baseBonus;

            public string Id => _id;
            public string NameLocalizationKey => _nameLocalizationKey;
            public string Name => string.IsNullOrEmpty(_name) ? _id : _name;
            public Sprite Icon => _icon;
            public long BasePrice => _basePrice;
            public long BaseBonus => _baseBonus;
        }

        [System.Serializable]
        public class PaidShopData
        {
            [SerializeField] private string _id;
            [SerializeField] private PaidShopPurchaseKind _purchaseKind;
            [SerializeField] private string _paymentId;
            [SerializeField] private QuestRewardCurrencyType _priceCurrencyType;
            [SerializeField] private long _priceAmount;
            [SerializeField] private string _nameLocalizationKey;
            [SerializeField] private string _name;
            [SerializeField] private Sprite _icon;
            [SerializeField] private string _priceText;
            [SerializeField] private string _rewardTextLocalizationKey;
            [SerializeField] private string _rewardText;
            [Tooltip("When > 0, the offer's soft-currency reward is not the fixed amount below: " +
                     "it's computed as the player's current income-per-second times this many minutes " +
                     "(i.e. \"claim N minutes of production instantly\"), recalculated at display/purchase time.")]
            [SerializeField] private int _timeBasedRewardMinutes;
            [SerializeField] private List<QuestReward> _rewards = new();

            public string Id => _id;
            public PaidShopPurchaseKind PurchaseKind => _purchaseKind;
            public string PaymentId => string.IsNullOrEmpty(_paymentId) ? _id : _paymentId;
            public QuestRewardCurrencyType PriceCurrencyType => _priceCurrencyType;
            public long PriceAmount => _priceAmount;
            public string NameLocalizationKey => _nameLocalizationKey;
            public string Name => string.IsNullOrEmpty(_name) ? _id : _name;
            public Sprite Icon => _icon;
            public string PriceText => _priceText;
            public string RewardTextLocalizationKey => _rewardTextLocalizationKey;
            public string RewardText => _rewardText;
            public int TimeBasedRewardMinutes => _timeBasedRewardMinutes;
            public IReadOnlyList<QuestReward> Rewards => _rewards;
        }

        public enum PaidShopPurchaseKind
        {
            RealMoney = 0,
            InGameCurrency = 1,
            RewardedAd = 2
        }
    }
}
