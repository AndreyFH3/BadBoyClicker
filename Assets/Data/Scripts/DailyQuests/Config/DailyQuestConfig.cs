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
            [SerializeField] private QuestDifficulty _difficulty = QuestDifficulty.Simple;
            [SerializeField] private DailyQuestResetPolicy _resetPolicy = DailyQuestResetPolicy.Daily;
            [Min(1)]
            [SerializeField] private long _targetValue = 1;
            [Min(0f)]
            [Tooltip("For income-scaled objectives, target recent income multiplied by this many minutes, with Target Value as the floor.")]
            [SerializeField] private float _incomeMinutes;
            [SerializeField] private string _targetId;
            [Min(1)]
            [SerializeField] private int _points = 10;
            [SerializeField] private List<QuestReward> _rewards = new();

            public string Id => _id;
            public string TitleLocalizationKey => _titleLocalizationKey;
            public string Title => string.IsNullOrEmpty(_title) ? _id : _title;
            public string DescriptionLocalizationKey => _descriptionLocalizationKey;
            public string Description => _description;
            public Sprite Icon => _icon;
            public QuestObjectiveType ObjectiveType => _objectiveType;
            public QuestDifficulty Difficulty => _difficulty;
            public DailyQuestResetPolicy ResetPolicy => _resetPolicy;
            public long TargetValue => _targetValue;
            public float IncomeMinutes => Mathf.Max(0f, _incomeMinutes);
            public string TargetId => _targetId;
            public int Points => _points;
            public IReadOnlyList<QuestReward> Rewards => _rewards;
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
