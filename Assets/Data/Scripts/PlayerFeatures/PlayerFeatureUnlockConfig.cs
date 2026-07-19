using System.Collections.Generic;
using UnityEngine;

namespace PlayerFeatures
{
    [CreateAssetMenu(fileName = "PlayerFeatureUnlockConfig", menuName = "Config/Player Feature Unlocks")]
    public class PlayerFeatureUnlockConfig : ScriptableObject
    {
        [SerializeField] private List<PlayerFeatureUnlockData> _features = new()
        {
            new PlayerFeatureUnlockData(PlayerFeatureType.Shop, 1),
            new PlayerFeatureUnlockData(PlayerFeatureType.OfflineIncome, 1),
            new PlayerFeatureUnlockData(PlayerFeatureType.DailyLoginReward, 3),
            new PlayerFeatureUnlockData(PlayerFeatureType.RewardAdBoosts, 5),
            new PlayerFeatureUnlockData(PlayerFeatureType.DailyQuest, 6),
            new PlayerFeatureUnlockData(PlayerFeatureType.AdvancedQuests, 7),
            new PlayerFeatureUnlockData(PlayerFeatureType.Chests, 8),
            new PlayerFeatureUnlockData(PlayerFeatureType.CardCollection, 8),
            new PlayerFeatureUnlockData(PlayerFeatureType.Customization, 16)
        };

        public IReadOnlyList<PlayerFeatureUnlockData> Features => _features;
    }
}
