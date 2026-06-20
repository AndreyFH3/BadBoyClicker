namespace Analytics
{
    public interface IGameAnalyticsController
    {
        void GameStarted();
        void ClickMilestoneReached(long clicks);
        void ShopItemBought(string itemId);
        void LevelUp(int level);
        void FeatureUnlocked(string featureId);
        void DailyLoginClaimed(int dayNumber, string rewardType, string rewardId, long amount);
        void DailyQuestPointsClaimed(string questId, int points);
        void DailyQuestMilestoneClaimed(int requiredPoints);
        void QuestCompleted(string questId);
        void CardReceived(string cardId, string collectionId, int stars, int amount);
        void CardCollectionCompleted(string collectionId, int collectedCards, int totalCards);
        void ChestOpened(string chestId, string rewardType, string rewardId);
        void AdBonusOfferShown(string offerId);
        void AdBonusRewardGranted(string offerId);
        void AdBonusRewardFailed(string offerId);
        void CustomizationItemBought(string itemType, string itemId, long price);
        void CustomizationItemSelected(string itemType, string itemId);
    }
}
