namespace QuestSystem
{
    public class BuyShopItemQuest : Quest
    {
        public BuyShopItemQuest(QuestConfig.QuestData data, long currentValue, bool isCompleted, bool isRewardClaimed)
            : base(data, currentValue, isCompleted, isRewardClaimed)
        {
        }

        public void OnShopItemBought(string itemId)
        {
            if (!string.IsNullOrEmpty(Data.TargetId) && Data.TargetId != itemId)
            {
                return;
            }

            AddProgress(1);
        }
    }
}
