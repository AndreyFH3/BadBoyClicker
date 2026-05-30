using System;
using System.Collections.Generic;
using Core;

namespace DailyQuests
{
    public class DailyQuestRuntimeSave : ISavable<DailyQuestSaveData>
    {
        private readonly Dictionary<string, DailyQuestSaveData.DailyQuestState> _quests = new();
        private readonly Dictionary<int, DailyQuestSaveData.DailyQuestMilestoneState> _milestones = new();

        public string DayKey { get; private set; }
        public int Points { get; private set; }
        public event Action Changed;

        public DailyQuestSaveData.DailyQuestState GetQuestState(string id)
        {
            return !string.IsNullOrEmpty(id) && _quests.TryGetValue(id, out var state)
                ? state
                : new DailyQuestSaveData.DailyQuestState { Id = id };
        }

        public bool IsMilestoneClaimed(int requiredPoints)
        {
            return _milestones.TryGetValue(requiredPoints, out var state) && state.IsClaimed;
        }

        public void ResetForDay(string dayKey)
        {
            DayKey = dayKey;
            Points = 0;
            _quests.Clear();
            _milestones.Clear();
            Changed?.Invoke();
        }

        public void SetQuestState(DailyQuestSaveData.DailyQuestState state)
        {
            if (string.IsNullOrEmpty(state.Id))
            {
                return;
            }

            _quests[state.Id] = state;
            Changed?.Invoke();
        }

        public void AddPoints(int points)
        {
            if (points <= 0)
            {
                return;
            }

            Points += points;
            Changed?.Invoke();
        }

        public void SetMilestoneClaimed(int requiredPoints)
        {
            _milestones[requiredPoints] = new DailyQuestSaveData.DailyQuestMilestoneState
            {
                RequiredPoints = requiredPoints,
                IsClaimed = true
            };

            Changed?.Invoke();
        }

        public void Set(DailyQuestSaveData data)
        {
            _quests.Clear();
            _milestones.Clear();

            DayKey = data?.DayKey;
            Points = data?.Points ?? 0;

            if (data?.Quests != null)
            {
                foreach (var quest in data.Quests)
                {
                    if (!string.IsNullOrEmpty(quest.Id))
                    {
                        _quests[quest.Id] = quest;
                    }
                }
            }

            if (data?.Milestones != null)
            {
                foreach (var milestone in data.Milestones)
                {
                    if (milestone.RequiredPoints > 0)
                    {
                        _milestones[milestone.RequiredPoints] = milestone;
                    }
                }
            }

            Changed?.Invoke();
        }

        public DailyQuestSaveData Get()
        {
            var quests = new DailyQuestSaveData.DailyQuestState[_quests.Count];
            int questIndex = 0;
            foreach (var state in _quests.Values)
            {
                quests[questIndex] = state;
                questIndex++;
            }

            var milestones = new DailyQuestSaveData.DailyQuestMilestoneState[_milestones.Count];
            int milestoneIndex = 0;
            foreach (var state in _milestones.Values)
            {
                milestones[milestoneIndex] = state;
                milestoneIndex++;
            }

            return new DailyQuestSaveData
            {
                DayKey = DayKey,
                Points = Points,
                Quests = quests,
                Milestones = milestones
            };
        }
    }
}
