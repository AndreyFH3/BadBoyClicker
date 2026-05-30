using System;

namespace QuestSystem
{
    [Serializable]
    public class QuestSaveData
    {
        public QuestState[] Quests;

        [Serializable]
        public struct QuestState
        {
            public string Id;
            public long CurrentValue;
            public bool IsCompleted;
            public bool IsRewardClaimed;
        }
    }
}
