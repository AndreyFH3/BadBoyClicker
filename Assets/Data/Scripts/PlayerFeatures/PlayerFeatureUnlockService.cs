using System.Collections.Generic;
using DailyLogin;
using PlayerProgression;
using QuestSystem;
using Zenject;

namespace PlayerFeatures
{
    public class PlayerFeatureUnlockService : IPlayerFeatureUnlockService, IInitializable, System.IDisposable
    {
        private PlayerFeatureUnlockConfig _config;
        private IPlayerProgressionService _progression;
        private readonly Dictionary<PlayerFeatureType, bool> _knownUnlockStates = new();

        public event System.Action<PlayerFeatureType> FeatureUnlocked;

        [Inject]
        public void Construct(PlayerFeatureUnlockConfig config, IPlayerProgressionService progression)
        {
            _config = config;
            _progression = progression;
        }

        public void Initialize()
        {
            CacheStates();
            _progression.Changed += CheckUnlocks;
        }

        public void Dispose()
        {
            _progression.Changed -= CheckUnlocks;
        }

        public bool IsUnlocked(PlayerFeatureType feature)
        {
            return _progression.CurrentLevel >= GetRequiredLevel(feature);
        }

        public int GetRequiredLevel(PlayerFeatureType feature)
        {
            var features = _config?.Features;
            if (features == null)
            {
                return 1;
            }

            foreach (var data in features)
            {
                if (data != null && data.Feature == feature)
                {
                    return System.Math.Max(1, data.RequiredLevel);
                }
            }

            return int.MaxValue;
        }

        private void CacheStates()
        {
            _knownUnlockStates.Clear();
            var features = _config?.Features;
            if (features == null)
            {
                return;
            }

            foreach (var data in features)
            {
                if (data != null)
                {
                    _knownUnlockStates[data.Feature] = IsUnlocked(data.Feature);
                }
            }
        }

        private void CheckUnlocks()
        {
            var features = _config?.Features;
            if (features == null)
            {
                return;
            }

            foreach (var data in features)
            {
                if (data == null)
                {
                    continue;
                }

                bool wasUnlocked = _knownUnlockStates.TryGetValue(data.Feature, out bool value) && value;
                bool isUnlocked = IsUnlocked(data.Feature);
                _knownUnlockStates[data.Feature] = isUnlocked;

                if (!wasUnlocked && isUnlocked)
                {
                    FeatureUnlocked?.Invoke(data.Feature);
                }
            }
        }
    }

    public static class RewardFeatureGate
    {
        public static bool AreAvailable(
            IReadOnlyList<QuestReward> rewards,
            IPlayerFeatureUnlockService featureUnlockService)
        {
            if (rewards == null)
            {
                return true;
            }

            foreach (QuestReward reward in rewards)
            {
                if (!IsAvailable(reward, featureUnlockService))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsAvailable(
            QuestReward reward,
            IPlayerFeatureUnlockService featureUnlockService)
        {
            if (reward == null)
            {
                return false;
            }

            if (featureUnlockService == null)
            {
                return true;
            }

            switch (reward.RewardType)
            {
                case QuestRewardType.PlayerBackground:
                    return featureUnlockService.IsUnlocked(PlayerFeatureType.Customization);
                case QuestRewardType.Boost:
                    return featureUnlockService.IsUnlocked(PlayerFeatureType.RewardAdBoosts);
                case QuestRewardType.Chest:
                    if (!featureUnlockService.IsUnlocked(PlayerFeatureType.Chests))
                    {
                        return false;
                    }

                    if (IsCardChest(reward.RewardId))
                    {
                        return featureUnlockService.IsUnlocked(PlayerFeatureType.CardCollection);
                    }

                    if (reward.RewardId == "Chest_5")
                    {
                        return featureUnlockService.IsUnlocked(PlayerFeatureType.RewardAdBoosts);
                    }

                    return true;
                case QuestRewardType.Custom:
                    return !IsCustomizationReward(reward.RewardId) ||
                           featureUnlockService.IsUnlocked(PlayerFeatureType.Customization);
                default:
                    return true;
            }
        }

        public static bool IsAvailable(
            RewardConfig reward,
            IPlayerFeatureUnlockService featureUnlockService)
        {
            if (reward == null)
            {
                return false;
            }

            if (featureUnlockService == null)
            {
                return true;
            }

            switch (reward.RewardType)
            {
                case RewardType.Boost:
                    return featureUnlockService.IsUnlocked(PlayerFeatureType.RewardAdBoosts);
                case RewardType.Chest:
                    if (!featureUnlockService.IsUnlocked(PlayerFeatureType.Chests))
                    {
                        return false;
                    }

                    if (IsCardChest(reward.RewardId))
                    {
                        return featureUnlockService.IsUnlocked(PlayerFeatureType.CardCollection);
                    }

                    return reward.RewardId != "Chest_5" ||
                           featureUnlockService.IsUnlocked(PlayerFeatureType.RewardAdBoosts);
                case RewardType.Cosmetic:
                    return featureUnlockService.IsUnlocked(PlayerFeatureType.Customization);
                default:
                    return true;
            }
        }

        public static bool IsCardChest(string chestId)
        {
            return chestId == "Chest_3" || chestId == "Chest_4";
        }

        private static bool IsCustomizationReward(string rewardId)
        {
            return !string.IsNullOrEmpty(rewardId) &&
                   rewardId.StartsWith("card_collection_unlock_skin_", System.StringComparison.Ordinal);
        }
    }
}
