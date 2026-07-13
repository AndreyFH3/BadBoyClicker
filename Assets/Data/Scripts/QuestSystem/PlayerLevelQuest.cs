namespace QuestSystem
{
    public class PlayerLevelQuest : Quest
    {
        public PlayerLevelQuest(QuestConfig.QuestData data, long currentValue, bool isCompleted, bool isRewardClaimed)
            : base(data, currentValue, isCompleted, isRewardClaimed)
        {
        }

        public void OnPlayerLevelCompleted()
        {
            AddProgress(1);
        }
    }
}
