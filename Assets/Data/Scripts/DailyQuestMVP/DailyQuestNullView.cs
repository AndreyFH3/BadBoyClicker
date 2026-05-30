using System;
using DailyQuests;

namespace DailyQuestMVP
{
    public class DailyQuestNullView : IDailyQuestView
    {
        public event Action<string> ClaimQuestPointsRequested
        {
            add { }
            remove { }
        }

        public event Action<int> ClaimMilestoneRequested
        {
            add { }
            remove { }
        }

        public void SetData(DailyQuestBoardViewData data)
        {
        }
    }
}
