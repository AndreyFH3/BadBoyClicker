using System.Collections.Generic;
using DailyLogin;
using Utils;
using GameLocalization;

namespace DailyLoginMVP
{
    public class DailyLoginModel
    {
        private readonly IDailyLoginService _dailyLoginService;
        private readonly ILocalizationService _localization;

        public DailyLoginModel(IDailyLoginService dailyLoginService, ILocalizationService localization)
        {
            _dailyLoginService = dailyLoginService;
            _localization = localization;
        }

        public bool HasReward
        {
            get
            {
                DailyLoginDayConfig dayConfig = _dailyLoginService.GetCurrentRewardDay();
                return _dailyLoginService.CanClaim() && dayConfig?.Reward != null;
            }
        }

        public DailyLoginViewData CreateViewData()
        {
            IReadOnlyList<DailyLoginDayConfig> dayConfigs = _dailyLoginService.GetRewardDays();
            int currentDayIndex = _dailyLoginService.GetCurrentDayIndex();
            List<DailyLoginRewardViewData> rewards = new();

            for (int i = 0; i < dayConfigs.Count; i++)
            {
                RewardConfig reward = dayConfigs[i]?.Reward;
                if (reward == null)
                {
                    continue;
                }

                rewards.Add(new DailyLoginRewardViewData(
                    reward.Icon,
                    CreateRewardText(reward),
                    i == currentDayIndex));
            }

            return new DailyLoginViewData(rewards);
        }

        public bool Claim()
        {
            return _dailyLoginService.Claim();
        }

        private string CreateRewardText(RewardConfig reward)
        {
            if (!string.IsNullOrEmpty(reward.DisplayTextLocalizationKey) || !string.IsNullOrEmpty(reward.DisplayText))
            {
                return _localization.Localize(reward.DisplayTextLocalizationKey);
            }

            switch (reward.RewardType)
            {
                case RewardType.Currency:
                    return reward.Amount.ConvertFromLongToString();
                case RewardType.Boost:
                case RewardType.Chest:
                    return reward.RewardId;
                default:
                    return string.Empty;
            }
        }
    }
}
