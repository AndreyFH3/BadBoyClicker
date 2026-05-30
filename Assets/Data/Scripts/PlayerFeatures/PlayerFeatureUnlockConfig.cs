using System.Collections.Generic;
using UnityEngine;

namespace PlayerFeatures
{
    [CreateAssetMenu(fileName = "PlayerFeatureUnlockConfig", menuName = "Config/Player Feature Unlocks")]
    public class PlayerFeatureUnlockConfig : ScriptableObject
    {
        [SerializeField] private List<PlayerFeatureUnlockData> _features = new()
        {
            new PlayerFeatureUnlockData(PlayerFeatureType.OfflineIncome, 2),
            new PlayerFeatureUnlockData(PlayerFeatureType.DailyLoginReward, 3),
            new PlayerFeatureUnlockData(PlayerFeatureType.DailyQuest, 4),
            new PlayerFeatureUnlockData(PlayerFeatureType.CardCollection, 5)
        };

        public IReadOnlyList<PlayerFeatureUnlockData> Features => _features;
    }
}
