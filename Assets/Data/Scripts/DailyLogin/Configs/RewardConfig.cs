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
        [SerializeField] private long _amountMax;
        [SerializeField] private string _rewardId;
        [SerializeField] private Sprite _icon;
        [SerializeField] private string _displayTextLocalizationKey;
        [SerializeField] private string _displayText;

        public RewardType RewardType => _rewardType;
        public CurrencyType CurrencyType => _currencyType;
        public long Amount => _amount;
        public long AmountMax => _amountMax;
        public bool HasAmountRange => _amountMax > _amount;
        public string RewardId => _rewardId;
        public Sprite Icon => _icon;
        public string DisplayTextLocalizationKey => _displayTextLocalizationKey;
        public string DisplayText => _displayText;

        public long RollAmount()
        {
            if (!HasAmountRange)
            {
                return _amount;
            }

            ulong range = (ulong)(_amountMax - _amount) + 1;
            ulong offset = (ulong)(UnityEngine.Random.value * range);
            return _amount + (long)Math.Min(offset, range - 1);
        }
    }
}
