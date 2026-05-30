using System;
using System.Collections.Generic;
using Core;

namespace QuestSystem
{
    public class QuestRuntimeSave : ISavable<QuestSaveData>
    {
        private readonly Dictionary<string, QuestSaveData.QuestState> _states = new();

        public event Action Changed;

        public QuestSaveData.QuestState GetState(string id)
        {
            return !string.IsNullOrEmpty(id) && _states.TryGetValue(id, out var state)
                ? state
                : new QuestSaveData.QuestState { Id = id };
        }

        public void SetQuestState(Quest quest)
        {
            if (quest == null || string.IsNullOrEmpty(quest.Id))
            {
                return;
            }

            _states[quest.Id] = new QuestSaveData.QuestState
            {
                Id = quest.Id,
                CurrentValue = quest.CurrentValue,
                IsCompleted = quest.IsCompleted,
                IsRewardClaimed = quest.IsRewardClaimed
            };

            Changed?.Invoke();
        }

        public void Set(QuestSaveData data)
        {
            _states.Clear();

            if (data?.Quests != null)
            {
                foreach (var state in data.Quests)
                {
                    if (!string.IsNullOrEmpty(state.Id))
                    {
                        _states[state.Id] = state;
                    }
                }
            }

            Changed?.Invoke();
        }

        public QuestSaveData Get()
        {
            var quests = new QuestSaveData.QuestState[_states.Count];
            int index = 0;

            foreach (var state in _states.Values)
            {
                quests[index] = state;
                index++;
            }

            return new QuestSaveData
            {
                Quests = quests
            };
        }
    }
}
