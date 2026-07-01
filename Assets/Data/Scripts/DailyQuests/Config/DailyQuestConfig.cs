using System.Collections.Generic;
using QuestSystem;
using UnityEngine;

namespace DailyQuests
{
    [CreateAssetMenu(fileName = "DailyQuestConfig", menuName = "Configs/DailyQuestConfig")]
    public class DailyQuestConfig : ScriptableObject
    {
        [SerializeField] private List<DailyQuestData> _quests = new();
        [SerializeField] private List<DailyQuestMilestoneData> _milestones = new();

        public IReadOnlyList<DailyQuestData> Quests => _quests;
        public IReadOnlyList<DailyQuestMilestoneData> Milestones => _milestones;

        [System.Serializable]
        public class DailyQuestData
        {
            [SerializeField] private string _id;
            [SerializeField] private string _titleLocalizationKey;
            [SerializeField] private string _title;
            [SerializeField] private string _descriptionLocalizationKey;
            [TextArea]
            [SerializeField] private string _description;
            [SerializeField] private Sprite _icon;
            [SerializeField] private QuestObjectiveType _objectiveType;
            [Min(1)]
            [SerializeField] private long _targetValue = 1;
            [SerializeField] private string _targetId;
            [Min(1)]
            [SerializeField] private int _points = 10;

            public string Id => _id;
            public string TitleLocalizationKey => _titleLocalizationKey;
            public string Title => string.IsNullOrEmpty(_title) ? _id : _title;
            public string DescriptionLocalizationKey => _descriptionLocalizationKey;
            public string Description => _description;
            public Sprite Icon => _icon;
            public QuestObjectiveType ObjectiveType => _objectiveType;
            public long TargetValue => _targetValue;
            public string TargetId => _targetId;
            public int Points => _points;
        }

        [System.Serializable]
        public class DailyQuestMilestoneData
        {
            [Min(1)]
            [SerializeField] private int _requiredPoints = 20;
            [SerializeField] private List<QuestReward> _rewards = new();

            public int RequiredPoints => _requiredPoints;
            public IReadOnlyList<QuestReward> Rewards => _rewards;
        }
    }
}
