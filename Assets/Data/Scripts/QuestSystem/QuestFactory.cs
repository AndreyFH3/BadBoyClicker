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
                case QuestObjectiveType.EarnSoft:
                    return new EarnSoftQuest(data, state.CurrentValue, state.IsCompleted, state.IsRewardClaimed);
                case QuestObjectiveType.BuyShopItem:
                    return new BuyShopItemQuest(data, state.CurrentValue, state.IsCompleted, state.IsRewardClaimed);
                case QuestObjectiveType.CompletePlayerLevel:
                    return new CompletePlayerLevelQuest(data, state.CurrentValue, state.IsCompleted, state.IsRewardClaimed);
                default:
                    Debug.LogWarning($"Quest objective type '{data.ObjectiveType}' is not supported.");
                    return null;
            }
        }
    }
}
