using System;
using QuestSystem;

namespace RewardActivation
{
    public interface IRewardActivationService
    {
        void Enqueue(QuestReward reward, Action grantAction);
    }
}
