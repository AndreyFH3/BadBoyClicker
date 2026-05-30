namespace QuestSystem
{
    public class EarnSoftQuest : Quest
    {
        public EarnSoftQuest(QuestConfig.QuestData data, long currentValue, bool isCompleted, bool isRewardClaimed)
            : base(data, currentValue, isCompleted, isRewardClaimed)
        {
        }

        public void OnSoftEarned(long amount)
        {
            AddProgress(amount);
        }
    }
}
