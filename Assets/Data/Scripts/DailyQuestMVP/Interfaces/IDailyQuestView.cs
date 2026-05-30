using System;
using DailyQuests;

namespace DailyQuestMVP
{
    public interface IDailyQuestView
    {
        event Action<string> ClaimQuestPointsRequested;
        event Action<int> ClaimMilestoneRequested;
        void SetData(DailyQuestBoardViewData data);
    }
}
