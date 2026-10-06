using System.Collections.Generic;
using Chests;
using DailyLogin;
using Utils;
using GameLocalization;

namespace DailyLoginMVP
{
    public class DailyLoginModel
    {
        private readonly IDailyLoginService _dailyLoginService;
        private readonly ILocalizationService _localization;
        private readonly ChestConfig _chestConfig;

        public DailyLoginModel(
            IDailyLoginService dailyLoginService,
            ILocalizationService localization,
            ChestConfig chestConfig)
        {
            _dailyLoginService = dailyLoginService;
            _localization = localization;
            _chestConfig = chestConfig;
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
                case RewardType.Chest:
                    // Never show the raw id ("Chest_2") - fall back to the chest's own title.
                    ChestConfig.ChestData chest = _chestConfig != null
                        ? _chestConfig.GetChest(reward.RewardId)
                        : null;
                    return chest == null
                        ? reward.RewardId
                        : LocalizeOrFallback(chest.TitleLocalizationKey, chest.Title);
                case RewardType.Boost:
                case RewardType.Cosmetic:
                    return reward.RewardId;
                default:
                    return string.Empty;
            }
        }

        private string LocalizeOrFallback(string key, string fallback)
        {
            if (string.IsNullOrEmpty(key))
            {
                return fallback;
            }

            string localized = _localization.Localize(key);
            return string.IsNullOrEmpty(localized) || localized == key ? fallback : localized;
        }
    }
}
