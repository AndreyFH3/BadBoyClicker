using System;
using System.Collections.Generic;
using QuestSystem;
using UnityEngine;

namespace Chests
{
    [CreateAssetMenu(fileName = "ChestConfig", menuName = "Configs/ChestConfig")]
    public class ChestConfig : ScriptableObject
    {
        [SerializeField] private List<ChestData> _chests = new();
        [Min(1)]
        [SerializeField] private long _softRewardMinAmount = 250;
        [Min(0f)]
        [SerializeField] private float _softRewardMinIncomeMinutes = 1f;
        [Min(0f)]
        [SerializeField] private float _softRewardMaxIncomeMinutes = 3f;
        [Min(0f)]
        [SerializeField] private float _softRewardAssumedClicksPerSecond = 2f;

        public IReadOnlyList<ChestData> Chests => _chests;
        public long SoftRewardMinAmount => Math.Max(1, _softRewardMinAmount);
        public float SoftRewardMinIncomeMinutes => Mathf.Max(0f, _softRewardMinIncomeMinutes);
        public float SoftRewardMaxIncomeMinutes => Mathf.Max(SoftRewardMinIncomeMinutes, _softRewardMaxIncomeMinutes);
        public float SoftRewardAssumedClicksPerSecond => Mathf.Max(0f, _softRewardAssumedClicksPerSecond);

        [Serializable]
        public class ChestData
        {
            [SerializeField] private string _id;
            [SerializeField] private string _titleLocalizationKey;
            [SerializeField] private string _title;
            [SerializeField] private Sprite _icon;
            [SerializeField] private List<ChestRewardEntry> _rewards = new();

            public string Id => _id;
            public string TitleLocalizationKey => _titleLocalizationKey;
            public string Title => string.IsNullOrEmpty(_title) ? _id : _title;
            public Sprite Icon => _icon;
            public IReadOnlyList<ChestRewardEntry> Rewards => _rewards;
        }

        [Serializable]
        public class ChestRewardEntry
        {
            [Min(1)]
            [SerializeField] private int _rarityWeight = 1;
            [SerializeField] private ChestRewardKind _rewardKind;
            [SerializeField] private QuestReward _reward;
            [SerializeField] private string _cardCollectionId;
            [SerializeField] private bool _allowDuplicateCards = true;
            [SerializeField] private long _minAmount;
            [SerializeField] private long _maxAmount;
            [SerializeField] private bool _scaleWithIncomeMultiplier;

            public int RarityWeight => Math.Max(1, _rarityWeight);
            public ChestRewardKind RewardKind => _rewardKind;
            public QuestReward Reward => _reward;
            public string CardCollectionId => _cardCollectionId;
            public bool AllowDuplicateCards => _allowDuplicateCards;
            public long MinAmount => _minAmount;
            public long MaxAmount => _maxAmount;
            public bool HasAmountRange => _maxAmount > _minAmount && _minAmount > 0;
            public bool ScaleWithIncomeMultiplier => _scaleWithIncomeMultiplier;
        }

        public enum ChestRewardKind
        {
            ConfiguredReward = 0,
            RandomCard = 1
        }
    }
}
