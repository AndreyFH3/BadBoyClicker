using System.Collections.Generic;

namespace QuestSystem
{
    public interface IQuestRewardService
    {
        void GiveRewards(IReadOnlyList<QuestReward> rewards);
        void GiveReward(QuestReward reward);
    }
}
