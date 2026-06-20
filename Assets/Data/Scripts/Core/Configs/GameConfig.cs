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
        [SerializeField] private Sprite _softPriceIcon;
        [SerializeField] private Sprite _paidPriceIcon;
        [SerializeField] private OfflineIncomeData _offlineIncome = new();
        [SerializeField] private PlayerProgressionData _playerProgression = new();
        [SerializeField] private ApplicationData _data;

        public IReadOnlyList<ShopDataClick> Clicks => _clicks;
        public IReadOnlyList<ShopDataClick> AutoBuys => _autoBuys;
        public IReadOnlyList<PaidShopData> PaidBuys => _paidBuys;
        public Sprite SoftPriceIcon => _softPriceIcon;
        public Sprite PaidPriceIcon => _paidPriceIcon;
        public OfflineIncomeData OfflineIncome => _offlineIncome;
        public PlayerProgressionData PlayerProgression => _playerProgression;

        [System.Serializable]
        public class PlayerProgressionData
        {
            [SerializeField] private long _middleRewardPerLevel = 5;
            [SerializeField] private List<ExperienceRewardData> _experienceRewards = new();
            [SerializeField] private List<PlayerLevelData> _levels = new();

            public long MiddleRewardPerLevel => _middleRewardPerLevel;
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
            [SerializeField] private long _experienceToComplete = 100;
            [SerializeField] private Sprite _rewardIcon;
            [SerializeField] private List<PlayerProgressBonusData> _bonuses = new();

            public long ExperienceToComplete => _experienceToComplete;
            public Sprite RewardIcon => _rewardIcon;
            public IReadOnlyList<PlayerProgressBonusData> Bonuses => _bonuses;
        }

        [System.Serializable]
        public class PlayerProgressBonusData
        {
            [SerializeField] private PlayerProgressBonusType _type;
            [SerializeField] private float _percent;

            public PlayerProgressBonusType Type => _type;
            public float Percent => _percent;
        }

        [System.Serializable]
        public class OfflineIncomeData
        {
            [SerializeField] private int _hardClaimCost = 10;
            [SerializeField] private int _adMultiplier = 2;
            [SerializeField] private int _paidMultiplier = 2;
            [SerializeField] private int _minSecondsToShow = 60;

            public int HardClaimCost => _hardClaimCost;
            public int AdMultiplier => _adMultiplier;
            public int PaidMultiplier => _paidMultiplier;
            public int MinSecondsToShow => _minSecondsToShow;
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
            public IReadOnlyList<QuestReward> Rewards => _rewards;
        }

        public enum PaidShopPurchaseKind
        {
            RealMoney = 0,
            InGameCurrency = 1
        }
    }
}
