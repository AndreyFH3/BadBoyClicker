namespace QuestSystem
{
    public interface ICustomRewardHandler
    {
        // Returns true if this handler recognized and granted the reward id.
        bool TryGiveCustomReward(string rewardId);
    }
}
