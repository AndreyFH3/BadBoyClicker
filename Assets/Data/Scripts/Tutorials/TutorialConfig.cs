using System;
using System.Collections.Generic;
using PlayerFeatures;
using UnityEngine;

namespace Tutorials
{
    [CreateAssetMenu(fileName = "TutorialConfig", menuName = "Configs/TutorialConfig")]
    public class TutorialConfig : ScriptableObject
    {
        [SerializeField] private List<TutorialData> _tutorials = new();

        public IReadOnlyList<TutorialData> Tutorials => _tutorials;

        public bool TryGet(PlayerFeatureType feature, out TutorialData tutorial)
        {
            if (_tutorials == null)
            {
                tutorial = null;
                return false;
            }

            for (int i = 0; i < _tutorials.Count; i++)
            {
                TutorialData candidate = _tutorials[i];
                if (candidate != null && candidate.Feature == feature)
                {
                    tutorial = candidate;
                    return true;
                }
            }

            tutorial = null;
            return false;
        }

        [Serializable]
        public class TutorialData
        {
            [SerializeField] private PlayerFeatureType _feature;
            [SerializeField] private Sprite _icon;
            [SerializeField] private string _headerLocalizationKey;
            [SerializeField] private string _descriptionLocalizationKey;

            public PlayerFeatureType Feature => _feature;
            public Sprite Icon => _icon;
            public string HeaderLocalizationKey => _headerLocalizationKey;
            public string DescriptionLocalizationKey => _descriptionLocalizationKey;
        }
    }
}
