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
            int currentCycle = _dailyLoginService.GetCurrentCycle();
            List<DailyLoginRewardViewData> rewards = new();

            for (int i = 0; i < dayConfigs.Count; i++)
            {
                RewardConfig reward = dayConfigs[i]?.GetReward(currentCycle);
                if (reward == null)
                {
                    continue;
                }

                rewards.Add(new DailyLoginRewardViewData(
                    reward.Icon,
                    CreateRewardText(reward),
                    _localization.Format("day_text", i + 1),
                    reward.RewardType == RewardType.Currency,
                    i == currentDayIndex,
                    i < currentDayIndex,
                    i > currentDayIndex,
                    dayConfigs[i].IsMilestone));
            }

            return new DailyLoginViewData(rewards);
        }

        public bool Claim()
        {
            return _dailyLoginService.Claim();
        }

        private string CreateRewardText(RewardConfig reward)
        {
            if (!string.IsNullOrEmpty(reward.DisplayTextLocalizationKey))
            {
                return _localization.Localize(reward.DisplayTextLocalizationKey);
            }

            if (!string.IsNullOrEmpty(reward.DisplayText))
            {
                return reward.DisplayText;
            }

            switch (reward.RewardType)
            {
                case RewardType.Currency:
                    return reward.HasAmountRange
                        ? $"{reward.Amount.ConvertFromLongToString()}-{reward.AmountMax.ConvertFromLongToString()}"
                        : reward.Amount.ConvertFromLongToString();
                case RewardType.Boost:
                case RewardType.Chest:
                case RewardType.Cosmetic:
                    return reward.RewardId;
                default:
                    return string.Empty;
            }
        }
    }
}
