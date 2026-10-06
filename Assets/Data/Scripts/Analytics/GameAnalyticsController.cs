using System;
using System.Collections.Generic;
using AdBonusOffers;
using CardCollections;
using Chests;
using Customization;
using DailyLogin;
using DailyQuests;
using Installer.Init;
using PlayerProgression;
using QuestSystem;
using Shop;
using YG;
using Zenject;

namespace Analytics
{
    public class GameAnalyticsController : IGameAnalyticsController, IInitializable, IDisposable
    {
        private static readonly long[] ClickMilestones = { 10, 50, 100, 250, 500, 1000, 2500, 5000, 10000 };

        private readonly GameStartRouter _gameStartRouter;
        private readonly IShopModel _shopModel;
        private readonly IPlayerProgressionService _playerProgression;
        private readonly IDailyLoginService _dailyLoginService;
        private readonly IDailyQuestService _dailyQuestService;
        private readonly IQuestService _questService;
        private readonly ICardCollectionService _cardCollectionService;
        private readonly IChestService _chestService;
        private readonly AdBonusOfferService _adBonusOfferService;
        private readonly ICustomizationService _customizationService;
        private readonly IAnalyticsRuntimeSave _runtimeSave;

        public GameAnalyticsController(
            GameStartRouter gameStartRouter,
            IShopModel shopModel,
            IPlayerProgressionService playerProgression,
            IDailyLoginService dailyLoginService,
            IDailyQuestService dailyQuestService,
            IQuestService questService,
            ICardCollectionService cardCollectionService,
            IChestService chestService,
            AdBonusOfferService adBonusOfferService,
            ICustomizationService customizationService,
            IAnalyticsRuntimeSave runtimeSave)
        {
            _gameStartRouter = gameStartRouter;
            _shopModel = shopModel;
            _playerProgression = playerProgression;
            _dailyLoginService = dailyLoginService;
            _dailyQuestService = dailyQuestService;
            _questService = questService;
            _cardCollectionService = cardCollectionService;
            _chestService = chestService;
            _adBonusOfferService = adBonusOfferService;
            _customizationService = customizationService;
            _runtimeSave = runtimeSave;
        }

        public void Initialize()
        {
            GameStarted();

            _gameStartRouter.OnClickValueEvent += OnClickValue;
            _shopModel.ItemBought += OnShopItemBought;
            _playerProgression.LevelCompleted += OnLevelCompleted;
            _dailyLoginService.RewardClaimed += OnDailyLoginRewardClaimed;
            _dailyQuestService.QuestPointsClaimed += DailyQuestPointsClaimed;
            _dailyQuestService.MilestoneClaimed += DailyQuestMilestoneClaimed;
            _questService.QuestRewardClaimed += OnQuestRewardClaimed;
            _cardCollectionService.CardChanged += OnCardChanged;
            _cardCollectionService.CollectionRewardClaimed += OnCollectionRewardClaimed;
            _chestService.ChestOpened += OnChestOpened;
            _adBonusOfferService.OfferShown += OnAdBonusOfferShown;
            _adBonusOfferService.RewardGranted += OnAdBonusRewardGranted;
            _adBonusOfferService.RewardFailed += OnAdBonusRewardFailed;
            _customizationService.ItemBought += OnCustomizationItemBought;
            _customizationService.ActiveItemChanged += OnCustomizationItemSelected;
        }

        public void Dispose()
        {
            _gameStartRouter.OnClickValueEvent -= OnClickValue;
            _shopModel.ItemBought -= OnShopItemBought;
            _playerProgression.LevelCompleted -= OnLevelCompleted;
            _dailyLoginService.RewardClaimed -= OnDailyLoginRewardClaimed;
            _dailyQuestService.QuestPointsClaimed -= DailyQuestPointsClaimed;
            _dailyQuestService.MilestoneClaimed -= DailyQuestMilestoneClaimed;
            _questService.QuestRewardClaimed -= OnQuestRewardClaimed;
            _cardCollectionService.CardChanged -= OnCardChanged;
            _cardCollectionService.CollectionRewardClaimed -= OnCollectionRewardClaimed;
            _chestService.ChestOpened -= OnChestOpened;
            _adBonusOfferService.OfferShown -= OnAdBonusOfferShown;
            _adBonusOfferService.RewardGranted -= OnAdBonusRewardGranted;
            _adBonusOfferService.RewardFailed -= OnAdBonusRewardFailed;
            _customizationService.ItemBought -= OnCustomizationItemBought;
            _customizationService.ActiveItemChanged -= OnCustomizationItemSelected;
        }

        public void GameStarted()
        {
            Send("game_started");
        }

        public void ClickMilestoneReached(long clicks)
        {
            Send("click_milestone_reached", ("clicks", clicks));
        }

        public void ShopItemBought(string itemId, string shop)
        {
            Send("shop_item_bought", ("item_id", itemId), ("shop", shop));
        }

        public void LevelUp(int level)
        {
            Send("level_up", ("level", level));
        }

        public void DailyLoginClaimed(int dayNumber, string rewardType, string rewardId, long amount)
        {
            Send(
                "daily_login_claimed",
                ("day_number", dayNumber),
                ("reward_type", rewardType),
                ("reward_id", rewardId),
                ("amount", amount));
        }

        public void DailyQuestPointsClaimed(string questId, int points)
        {
            Send("daily_quest_points_claimed", ("quest_id", questId), ("points", points));
        }

        public void DailyQuestMilestoneClaimed(int requiredPoints)
        {
            Send("daily_quest_milestone_claimed", ("required_points", requiredPoints));
        }

        public void QuestCompleted(string questId)
        {
            Send("quest_completed", ("quest_id", questId));
        }

        public void CardReceived(string cardId, string collectionId, int stars, int amount)
        {
            Send(
                "card_received",
                ("card_id", cardId),
                ("collection_id", collectionId),
                ("stars", stars),
                ("amount", amount));
        }

        public void CardCollectionCompleted(string collectionId, int collectedCards, int totalCards)
        {
            Send(
                "card_collection_completed",
                ("collection_id", collectionId),
                ("collected_cards", collectedCards),
                ("total_cards", totalCards));
        }

        public void ChestOpened(string chestId, string rewardType, string rewardId)
        {
            Send("chest_opened", ("chest_id", chestId), ("reward_type", rewardType), ("reward_id", rewardId));
        }

        public void AdBonusOfferShown(string offerId)
        {
            Send("ad_bonus_offer_shown", ("offer_id", offerId));
        }

        public void AdBonusRewardGranted(string offerId)
        {
            Send("ad_bonus_reward_granted", ("offer_id", offerId));
        }

        public void AdBonusRewardFailed(string offerId, string reason)
        {
            Send("ad_bonus_reward_failed", ("offer_id", offerId), ("reason", reason));
        }

        public void CustomizationItemBought(string itemType, string itemId, long price, string currency)
        {
            Send("customization_item_bought", ("item_type", itemType), ("item_id", itemId), ("price", price), ("currency", currency), ("shop", "customization"));
        }

        public void CustomizationItemSelected(string itemType, string itemId)
        {
            Send("customization_item_selected", ("item_type", itemType), ("item_id", itemId));
        }

        private void OnClickValue(long value)
        {
            long totalClicks = _runtimeSave.TotalClicks + 1;
            int nextMilestoneIndex = _runtimeSave.NextClickMilestoneIndex;

            if (nextMilestoneIndex >= ClickMilestones.Length || totalClicks < ClickMilestones[nextMilestoneIndex])
            {
                _runtimeSave.SetClickProgress(totalClicks, nextMilestoneIndex);
                return;
            }

            long reachedMilestone = ClickMilestones[nextMilestoneIndex];
            _runtimeSave.SetClickProgress(totalClicks, nextMilestoneIndex + 1);
            ClickMilestoneReached(reachedMilestone);
        }

        private void OnLevelCompleted(int completedLevel)
        {
            LevelUp(completedLevel + 1);
        }

        private void OnDailyLoginRewardClaimed(int dayIndex, RewardConfig reward)
        {
            DailyLoginClaimed(
                dayIndex + 1,
                reward != null ? reward.RewardType.ToString() : string.Empty,
                reward != null ? reward.RewardId : string.Empty,
                reward != null ? reward.Amount : 0);
        }

        private void OnQuestRewardClaimed(Quest quest)
        {
            QuestCompleted(quest.Id);
        }

        private void OnCardChanged(CardViewData card)
        {
            if (card.CurrentAmount <= 0)
            {
                return;
            }

            CardReceived(card.Id, card.CollectionId, card.Stars, card.CurrentAmount);
        }

        private void OnCollectionRewardClaimed(CardCollectionViewData collection)
        {
            CardCollectionCompleted(collection.Id, collection.CollectedCards, collection.TotalCards);
        }

        private void OnChestOpened(ChestOpenResult result)
        {
            string rewardType = result.Card != null ? "Card" : result.Reward?.RewardType.ToString() ?? string.Empty;
            string rewardId = result.Card != null ? result.Card.Id : result.Reward?.RewardId ?? string.Empty;
            ChestOpened(result.Chest?.Id ?? string.Empty, rewardType, rewardId);
        }

        private void OnAdBonusOfferShown(AdBonusOfferViewData offer)
        {
            AdBonusOfferShown(offer.Id);
        }

        private void OnAdBonusRewardGranted(AdBonusOfferViewData offer)
        {
            AdBonusRewardGranted(offer.Id);
        }

        private void OnAdBonusRewardFailed(AdBonusOfferViewData offer, Core.Ads.RewardedAdFailureReason reason)
        {
            AdBonusRewardFailed(offer.Id, ToAnalyticsValue(reason));
        }

        private void OnCustomizationItemBought(CustomizationItemType type, string id, long price)
        {
            CustomizationItemBought(type.ToString(), id, price, "decor");
        }

        private void OnShopItemBought(string itemId)
        {
            ShopElementData item = _shopModel.GetShopPositionData(itemId);
            string shop = item?.Type switch
            {
                ShopItemType.Click => "clicks",
                ShopItemType.AutoBuy => "autoclick",
                ShopItemType.PaidBuy => "chest",
                _ => "unknown"
            };
            ShopItemBought(itemId, shop);
        }

        private static string ToAnalyticsValue(Core.Ads.RewardedAdFailureReason reason)
        {
            return reason switch
            {
                Core.Ads.RewardedAdFailureReason.NoFill => "no_fill",
                Core.Ads.RewardedAdFailureReason.Closed => "closed",
                _ => "error"
            };
        }

        private void OnCustomizationItemSelected(CustomizationItemType type, string id)
        {
            CustomizationItemSelected(type.ToString(), id);
        }

        private static void Send(string eventName)
        {
            YG2.MetricaSend(eventName);
        }

        private static void Send(string eventName, params (string Key, object Value)[] parameters)
        {
            var eventData = new Dictionary<string, object>();
            foreach (var parameter in parameters)
            {
                eventData[parameter.Key] = parameter.Value;
            }

            YG2.MetricaSend(eventName, eventData);
        }
    }
}
