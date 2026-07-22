using System;
using QuestSystem;

namespace RewardActivation
{
    public interface IRewardActivationService
    {
        void Enqueue(QuestReward reward, Action grantAction, Action postponeAction = null);
        void Enqueue(RewardActivationViewData data, Action closeAction = null);
    }
}
