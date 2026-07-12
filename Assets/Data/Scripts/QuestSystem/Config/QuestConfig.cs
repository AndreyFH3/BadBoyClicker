using System.Collections.Generic;
using UnityEngine;

namespace QuestSystem
{
    [CreateAssetMenu(fileName = "QuestConfig", menuName = "Configs/QuestConfig")]
    public class QuestConfig : ScriptableObject
    {
        [SerializeField] private List<QuestData> _quests = new();

        public IReadOnlyList<QuestData> Quests => _quests;

        [System.Serializable]
        public class QuestData
        {
            [SerializeField] private string _id;
            [SerializeField] private string _titleLocalizationKey;
            [SerializeField] private string _title;
            [SerializeField] private string _descriptionLocalizationKey;
            [TextArea]
            [SerializeField] private string _description;
            [SerializeField] private QuestObjectiveType _objectiveType;
            [Min(1)]
            [SerializeField] private long _targetValue = 1;
            [SerializeField] private string _targetId;
            [SerializeField] private long _experienceReward = 100;
            [SerializeField] private List<QuestReward> _rewards = new();

            public string Id => _id;
            public string TitleLocalizationKey => _titleLocalizationKey;
            public string Title => string.IsNullOrEmpty(_title) ? _id : _title;
            public string DescriptionLocalizationKey => _descriptionLocalizationKey;
            public string Description => _description;
            public QuestObjectiveType ObjectiveType => _objectiveType;
            public long TargetValue => _targetValue;
            public string TargetId => _targetId;
            public long ExperienceReward => _experienceReward;
            public IReadOnlyList<QuestReward> Rewards => _rewards;
        }
    }
}
