using System;

namespace DailyQuests
{
    [Serializable]
    public class DailyQuestSaveData
    {
        public string DayKey;
        public int Points;
        public DailyQuestState[] Quests;
        public DailyQuestMilestoneState[] Milestones;

        [Serializable]
        public struct DailyQuestState
        {
            public string Id;
            public long CurrentValue;
            public bool IsCompleted;
            public bool IsPointsAdded;
        }

        [Serializable]
        public struct DailyQuestMilestoneState
        {
            public int RequiredPoints;
            public bool IsClaimed;
        }
    }
}
