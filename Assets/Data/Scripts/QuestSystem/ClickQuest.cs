namespace QuestSystem
{
    public class ClickQuest : Quest
    {
        public ClickQuest(QuestConfig.QuestData data, long currentValue, bool isCompleted, bool isRewardClaimed)
            : base(data, currentValue, isCompleted, isRewardClaimed)
        {
        }

        public void OnClick()
        {
            AddProgress(1);
        }
    }
}
