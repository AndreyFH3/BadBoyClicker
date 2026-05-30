using System;
using UnityEngine;

namespace PlayerFeatures
{
    [Serializable]
    public class PlayerFeatureUnlockData
    {
        [SerializeField] private PlayerFeatureType _feature;
        [SerializeField] private int _requiredLevel = 1;

        public PlayerFeatureType Feature => _feature;
        public int RequiredLevel => _requiredLevel;

        public PlayerFeatureUnlockData(PlayerFeatureType feature, int requiredLevel)
        {
            _feature = feature;
            _requiredLevel = requiredLevel;
        }
    }
}
