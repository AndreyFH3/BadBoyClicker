using System;
using Core;

namespace DailyQuests
{
    public interface IDailyQuestService : ISavable<DailyQuestSaveData>
    {
        int Points { get; }
        event Action Changed;
        DailyQuestBoardViewData GetViewData();
        bool ClaimQuestPoints(string questId);
        bool ClaimMilestone(int requiredPoints);
    }
}
