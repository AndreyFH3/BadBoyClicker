using System;
using UnityEngine;

namespace DailyLogin
{
    [Serializable]
    public class RewardConfig
    {
        [SerializeField] private RewardType _rewardType;
        [SerializeField] private CurrencyType _currencyType;
        [SerializeField] private long _amount;
        [SerializeField] private string _rewardId;
        [SerializeField] private Sprite _icon;
        [SerializeField] private string _displayTextLocalizationKey;
        [SerializeField] private string _displayText;

        public RewardType RewardType => _rewardType;
        public CurrencyType CurrencyType => _currencyType;
        public long Amount => _amount;
        public string RewardId => _rewardId;
        public Sprite Icon => _icon;
        public string DisplayTextLocalizationKey => _displayTextLocalizationKey;
        public string DisplayText => _displayText;
    }
}
