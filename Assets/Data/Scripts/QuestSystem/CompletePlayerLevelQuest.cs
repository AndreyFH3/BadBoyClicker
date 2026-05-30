namespace QuestSystem
{
    public class CompletePlayerLevelQuest : Quest
    {
        public CompletePlayerLevelQuest(QuestConfig.QuestData data, long currentValue, bool isCompleted, bool isRewardClaimed)
            : base(data, currentValue, isCompleted, isRewardClaimed)
        {
        }

        public void OnPlayerLevelCompleted()
        {
            AddProgress(1);
        }
    }
}
