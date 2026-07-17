using System;
using UnityEngine;

namespace QuestSystem
{
    [Serializable]
    public class QuestReward
    {
        [SerializeField] private QuestRewardType _rewardType;
        [SerializeField] private QuestRewardCurrencyType _currencyType;
        [SerializeField] private long _amount;
        [SerializeField] private string _rewardId;
        [SerializeField] private Sprite _icon;
        [SerializeField] private string _displayTextLocalizationKey;
        [SerializeField] private string _displayText;
        [SerializeField] private bool _requiresActivation;
        [SerializeField] private string _activationTitleLocalizationKey;
        [SerializeField] private string _activationTitle;
        [SerializeField] private string _activationDescriptionLocalizationKey;
        [SerializeField] private string _activationDescription;

        public QuestRewardType RewardType => _rewardType;
        public QuestRewardCurrencyType CurrencyType => _currencyType;
        public long Amount => _amount;

        public void SetAmount(long amount) => _amount = amount;
        public string RewardId => _rewardId;
        public Sprite Icon => _icon;
        public string DisplayTextLocalizationKey => _displayTextLocalizationKey;
        public string DisplayText => _displayText;
        public bool RequiresActivation => _requiresActivation;
        public string ActivationTitleLocalizationKey => _activationTitleLocalizationKey;
        public string ActivationTitle => _activationTitle;
        public string ActivationDescriptionLocalizationKey => _activationDescriptionLocalizationKey;
        public string ActivationDescription => _activationDescription;
    }
}
