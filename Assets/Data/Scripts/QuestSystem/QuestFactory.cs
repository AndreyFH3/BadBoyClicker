using UnityEngine;

namespace QuestSystem
{
    public class QuestFactory
    {
        public Quest Create(QuestConfig.QuestData data, QuestSaveData.QuestState state)
        {
            switch (data.ObjectiveType)
            {
                case QuestObjectiveType.Click:
                    return new ClickQuest(data, state.CurrentValue, state.IsCompleted, state.IsRewardClaimed);
                case QuestObjectiveType.TotalEarned:
                    return new TotalEarnedQuest(data, state.CurrentValue, state.IsCompleted, state.IsRewardClaimed);
                case QuestObjectiveType.ShopBuy:
                    return new ShopBuyQuest(data, state.CurrentValue, state.IsCompleted, state.IsRewardClaimed);
                case QuestObjectiveType.PlayerLevel:
                    return new PlayerLevelQuest(data, state.CurrentValue, state.IsCompleted, state.IsRewardClaimed);
                default:
                    Debug.LogWarning($"Quest objective type '{data.ObjectiveType}' is not supported.");
                    return null;
            }
        }
    }
}
