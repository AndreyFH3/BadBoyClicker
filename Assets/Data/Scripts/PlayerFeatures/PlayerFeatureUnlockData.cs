using System;
using UnityEngine;

namespace PlayerFeatures
{
    [Serializable]
    public class PlayerFeatureUnlockData
    {
        [SerializeField] private PlayerFeatureType _feature;
        [SerializeField] private int _requiredLevel = 1;
        [SerializeField] private bool _isShowable;
        [SerializeField] private Sprite _icon;
        [SerializeField] private string _descriptionLocalizationKey;

        public PlayerFeatureType Feature => _feature;
        public int RequiredLevel => _requiredLevel;
        public bool IsShowable => _isShowable;
        public Sprite Icon => _icon;
        public string DescriptionLocalizationKey => _descriptionLocalizationKey;

        public PlayerFeatureUnlockData(
            PlayerFeatureType feature,
            int requiredLevel,
            bool isShowable = false,
            Sprite icon = null,
            string descriptionLocalizationKey = null)
        {
            _feature = feature;
            _requiredLevel = requiredLevel;
            _isShowable = isShowable;
            _icon = icon;
            _descriptionLocalizationKey = descriptionLocalizationKey;
        }
    }
}
