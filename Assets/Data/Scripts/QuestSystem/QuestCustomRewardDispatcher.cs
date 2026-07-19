using System.Collections.Generic;
using UnityEngine;

namespace QuestSystem
{
    // Fans a Custom quest reward id out to every bound ICustomRewardHandler
    // so unrelated reward domains (offline income, card collections, ...)
    // can each own their own reward ids without competing for the single
    // IQuestCustomRewardService slot that QuestRewardService/ChestRewardService inject.
    public class QuestCustomRewardDispatcher : IQuestCustomRewardService
    {
        private readonly List<ICustomRewardHandler> _handlers;

        public QuestCustomRewardDispatcher(List<ICustomRewardHandler> handlers)
        {
            _handlers = handlers;
        }

        public void GiveCustomReward(string rewardId)
        {
            foreach (var handler in _handlers)
            {
                if (handler != null && handler.TryGiveCustomReward(rewardId))
                {
                    return;
                }
            }

            Debug.LogWarning($"Unsupported custom reward id: {rewardId}");
        }
    }
}
