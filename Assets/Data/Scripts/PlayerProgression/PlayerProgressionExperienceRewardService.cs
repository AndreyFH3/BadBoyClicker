using System;
using QuestSystem;

namespace PlayerProgression
{
    // Grants experience equal to a percentage of what the *next level requires*
    // (not the player's current progress), used by both the reward-ad bonus and
    // the crystal shop offers. This way the reward is worth the same regardless
    // of when it's claimed - a player at 0 experience still gets a meaningful
    // amount, instead of 10% of nothing right after leveling up.
    public class PlayerProgressionExperienceRewardService : ICustomRewardHandler
    {
        public const string Percent10RewardId = "exp_percent_10";
        public const string Percent25RewardId = "exp_percent_25";
        public const string Percent50RewardId = "exp_percent_50";
        public const string Percent100RewardId = "exp_percent_100";

        private readonly IPlayerProgressionService _progression;

        public PlayerProgressionExperienceRewardService(IPlayerProgressionService progression)
        {
            _progression = progression;
        }

        public bool TryGiveCustomReward(string rewardId)
        {
            float percent = GetPercent(rewardId);
            if (percent <= 0f)
            {
                return false;
            }

            long amount = Math.Max(1, (long)Math.Ceiling(_progression.ExperienceToNextLevel * percent / 100.0));
            _progression.AddExperience(PlayerExperienceSource.PercentBonus, amount);
            return true;
        }

        public static bool IsExperienceRewardId(string rewardId)
        {
            return GetPercent(rewardId) > 0f;
        }

        private static float GetPercent(string rewardId)
        {
            switch (rewardId)
            {
                case Percent10RewardId:
                    return 10f;
                case Percent25RewardId:
                    return 25f;
                case Percent50RewardId:
                    return 50f;
                case Percent100RewardId:
                    return 100f;
                default:
                    return 0f;
            }
        }
    }
}
