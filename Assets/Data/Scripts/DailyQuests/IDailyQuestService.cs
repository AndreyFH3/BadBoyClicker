using System;
using Core;

namespace DailyQuests
{
    public interface IDailyQuestService : ISavable<DailyQuestSaveData>
    {
        int Points { get; }
        event Action Changed;
        event Action<string, int> QuestPointsClaimed;
        event Action<int> MilestoneClaimed;

        DailyQuestBoardViewData GetViewData();
        bool ClaimQuestPoints(string questId);
        bool ClaimMilestone(int requiredPoints);
    }
}
