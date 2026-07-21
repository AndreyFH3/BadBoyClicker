using System;
using System.Collections.Generic;
using Customization;
using PlayerFeatures;
using QuestSystem;
using Rewards;
using Utils;
using Zenject;
using GameLocalization;

namespace CardCollections
{
    public class CardCollectionService : ICardCollectionService, IInitializable, IDisposable
    {
        private CardCollectionConfig _config;
        private CardCollectionRuntimeSave _save;
        private IPlayerFeatureUnlockService _featureUnlockService;
        private IQuestRewardService _rewardService;
        private ILocalizationService _localization;
        private ICustomizationService _customizationService;
        private readonly Dictionary<string, CardCollectionConfig.CardData> _cardsById = new();
        private readonly Dictionary<string, CardCollectionConfig.CardCollectionData> _collectionsById = new();
        private readonly Dictionary<string, CardCollectionConfig.CardCollectionData> _collectionsByCardId = new();

        public bool IsUnlocked => _featureUnlockService == null ||
                                  _featureUnlockService.IsUnlocked(PlayerFeatureType.CardCollection);

        public IReadOnlyList<CardCollectionConfig.CardCollectionData> Collections =>
            _config?.Collections ?? Array.Empty<CardCollectionConfig.CardCollectionData>();

        public event Action Changed;
        public event Action<CardCollectionViewData> CollectionChanged;
        public event Action<CardViewData> CardChanged;
        public event Action<CardCollectionViewData> CollectionRewardClaimed;

        [Inject]
        public void Construct(
            CardCollectionConfig config,
            CardCollectionRuntimeSave save,
            IPlayerFeatureUnlockService featureUnlockService,
            IQuestRewardService rewardService,
            ILocalizationService localization,
            ICustomizationService customizationService)
        {
            _config = config;
            _save = save;
            _featureUnlockService = featureUnlockService;
            _rewardService = rewardService;
            _localization = localization;
            _customizationService = customizationService;
        }

        public void Initialize()
        {
            RebuildIndex();
            _featureUnlockService.FeatureUnlocked += OnFeatureUnlocked;
        }

        public void Dispose()
        {
            _featureUnlockService.FeatureUnlocked -= OnFeatureUnlocked;
        }

        public bool HasCard(string cardId)
        {
            return GetCardAmount(cardId) > 0;
        }

        public int GetCardAmount(string cardId)
        {
            return _save.GetCardAmount(cardId);
        }

        public CardCollectionConfig.CardData GetCard(string cardId)
        {
            return !string.IsNullOrEmpty(cardId) && _cardsById.TryGetValue(cardId, out var card)
                ? card
                : null;
        }

        public CardCollectionConfig.CardCollectionData GetCollection(string collectionId)
        {
            return !string.IsNullOrEmpty(collectionId) && _collectionsById.TryGetValue(collectionId, out var collection)
                ? collection
                : null;
        }

        public CardCollectionConfig.CardCollectionData GetCollectionByCard(string cardId)
        {
            return !string.IsNullOrEmpty(cardId) && _collectionsByCardId.TryGetValue(cardId, out var collection)
                ? collection
                : null;
        }

        public IReadOnlyList<CardCollectionViewData> GetAllViewData()
        {
            var result = new List<CardCollectionViewData>();

            foreach (var collection in Collections)
            {
                if (collection != null && !string.IsNullOrEmpty(collection.Id))
                {
                    result.Add(CreateCollectionViewData(collection));
                }
            }

            return result;
        }

        public CardCollectionViewData GetViewData(string collectionId)
        {
            var collection = GetCollection(collectionId);
            return collection != null ? CreateCollectionViewData(collection) : null;
        }

        public bool TryAddCard(string cardId, int amount = 1)
        {
            if (!CanMutateCard(cardId) || amount <= 0)
            {
                return false;
            }

            int currentAmount = _save.GetCardAmount(cardId);
            return TrySetCardAmount(cardId, currentAmount + amount);
        }

        public bool TrySetCardAmount(string cardId, int amount)
        {
            if (!CanMutateCard(cardId))
            {
                return false;
            }

            _save.SetCardAmount(cardId, Math.Max(0, amount));
            NotifyCardAndCollection(cardId);
            Changed?.Invoke();
            return true;
        }

        public bool TryResetCard(string cardId)
        {
            return TrySetCardAmount(cardId, 0);
        }

        public bool TryResetCollectionProgress(string collectionId)
        {
            if (!IsUnlocked)
            {
                return false;
            }

            var collection = GetCollection(collectionId);
            if (collection == null)
            {
                return false;
            }

            _save.ResetCollection(GetCardIds(collection), collection.Id);
            CollectionChanged?.Invoke(CreateCollectionViewData(collection));
            Changed?.Invoke();
            return true;
        }

        public bool TryClaimReward(string collectionId)
        {
            if (!IsUnlocked)
            {
                return false;
            }

            var collection = GetCollection(collectionId);
            if (collection == null || _save.IsRewardClaimed(collection.Id) || !IsCollectionCompleted(collection))
            {
                return false;
            }

            _rewardService.GiveRewards(collection.Rewards);
            _save.MarkRewardClaimed(collection.Id);

            var viewData = CreateCollectionViewData(collection);
            CollectionRewardClaimed?.Invoke(viewData);
            CollectionChanged?.Invoke(viewData);
            return true;
        }

        public bool IsRewardClaimed(string collectionId)
        {
            var collection = GetCollection(collectionId);
            return collection != null && _save.IsRewardClaimed(collection.Id);
        }

        public void ResetAllProgress()
        {
            if (!IsUnlocked)
            {
                return;
            }

            _save.ResetAll();

            foreach (var collection in Collections)
            {
                if (collection != null && !string.IsNullOrEmpty(collection.Id))
                {
                    CollectionChanged?.Invoke(CreateCollectionViewData(collection));
                }
            }

            Changed?.Invoke();
        }

        public void Set(CardCollectionSaveData data)
        {
            _save.Set(data);
            RebuildIndex();
            Changed?.Invoke();
        }

        public CardCollectionSaveData Get()
        {
            return _save.Get();
        }

        private bool CanMutateCard(string cardId)
        {
            // Card rewards may arrive from an old save or an external grant before
            // the collection UI unlocks. Persist them now and reveal them later,
            // instead of silently discarding a valid reward.
            return !string.IsNullOrEmpty(cardId) && _cardsById.ContainsKey(cardId);
        }

        private void RebuildIndex()
        {
            _cardsById.Clear();
            _collectionsById.Clear();
            _collectionsByCardId.Clear();

            foreach (var collection in Collections)
            {
                if (collection == null || string.IsNullOrEmpty(collection.Id) ||
                    _collectionsById.ContainsKey(collection.Id))
                {
                    continue;
                }

                _collectionsById.Add(collection.Id, collection);

                if (collection.Cards == null)
                {
                    continue;
                }

                foreach (var card in collection.Cards)
                {
                    if (card == null || string.IsNullOrEmpty(card.Id) || _cardsById.ContainsKey(card.Id))
                    {
                        continue;
                    }

                    _cardsById.Add(card.Id, card);
                    _collectionsByCardId.Add(card.Id, collection);
                }
            }
        }

        private bool IsCollectionCompleted(CardCollectionConfig.CardCollectionData collection)
        {
            if (collection?.Cards == null || collection.Cards.Count == 0)
            {
                return false;
            }

            foreach (var card in collection.Cards)
            {
                if (card == null || _save.GetCardAmount(card.Id) < card.RequiredAmount)
                {
                    return false;
                }
            }

            return true;
        }

        private CardCollectionViewData CreateCollectionViewData(CardCollectionConfig.CardCollectionData collection)
        {
            var cards = new List<CardViewData>();
            int collectedCards = 0;
            int collectedStars = 0;
            int totalStars = 0;

            if (collection.Cards != null)
            {
                foreach (var card in collection.Cards)
                {
                    if (card == null)
                    {
                        continue;
                    }

                    var cardViewData = CreateCardViewData(collection.Id, card);
                    cards.Add(cardViewData);
                    totalStars += cardViewData.Stars;

                    if (cardViewData.IsCollected)
                    {
                        collectedCards++;
                        collectedStars += cardViewData.Stars;
                    }
                }
            }

            bool isCompleted = IsCollectionCompleted(collection);
            bool isRewardClaimed = _save.IsRewardClaimed(collection.Id);

            return new CardCollectionViewData
            {
                Id = collection.Id,
                Title = _localization.Localize(collection.TitleLocalizationKey),
                Description = _localization.Localize(collection.DescriptionLocalizationKey),
                Cards = cards,
                CollectedCards = collectedCards,
                TotalCards = cards.Count,
                CollectedStars = collectedStars,
                TotalStars = totalStars,
                IsCompleted = isCompleted,
                IsRewardClaimed = isRewardClaimed,
                RewardAvailable = isCompleted && !isRewardClaimed,
                Rewards = BuildRewardDisplays(collection.Rewards)
            };
        }

        private List<RewardDisplay> BuildRewardDisplays(IReadOnlyList<QuestReward> rewards)
        {
            if (rewards == null || rewards.Count == 0)
            {
                return null;
            }

            var displays = new List<RewardDisplay>(rewards.Count);
            foreach (var reward in rewards)
            {
                if (reward == null)
                {
                    continue;
                }

                displays.Add(new RewardDisplay
                {
                    Icon = reward.Icon,
                    Amount = FormatRewardAmount(reward)
                });
            }

            return displays;
        }

        // card_collection_* Custom rewards carry no meaningful currency amount:
        // percent bonuses (e.g. card_collection_bonus_shop_discount_10) should read
        // as "+10%"/"-10%", and skin unlocks should read as the unlocked cat's own
        // name rather than a raw 0 amount.
        private string FormatRewardAmount(QuestReward reward)
        {
            if (reward.RewardType == QuestRewardType.Custom && !string.IsNullOrEmpty(reward.RewardId))
            {
                if (CardCollectionRewardIds.SkinRewardIdToCatId.TryGetValue(reward.RewardId, out string catId))
                {
                    var item = _customizationService?.GetItem(CustomizationItemType.Cat, catId);
                    if (item != null)
                    {
                        return _localization.Localize(item.TitleLocalizationKey);
                    }
                }
                else if (CardCollectionRewardIds.IsPercentBonus(reward.RewardId))
                {
                    // Shop discount reads as a price reduction ("-10%"); the income/click
                    // bonuses read as a gain ("+10%").
                    string sign = reward.RewardId.StartsWith(CardCollectionRewardIds.ShopDiscountBonusPrefix, StringComparison.Ordinal)
                        ? "-"
                        : "+";
                    return $"{sign}{reward.Amount.ConvertFromLongToString()}%";
                }
            }

            return reward.Amount > 0 ? reward.Amount.ConvertFromLongToString() : string.Empty;
        }

        private CardViewData CreateCardViewData(string collectionId, CardCollectionConfig.CardData card)
        {
            int amount = _save.GetCardAmount(card.Id);

            return new CardViewData
            {
                Id = card.Id,
                CollectionId = collectionId,
                Title = _localization.Localize(card.TitleLocalizationKey),
                Description = _localization.Localize(card.DescriptionLocalizationKey),
                Stars = card.Stars,
                CurrentAmount = amount,
                RequiredAmount = card.RequiredAmount,
                IsCollected = amount >= card.RequiredAmount,
                Icon = card.Icon
            };
        }

        private IEnumerable<string> GetCardIds(CardCollectionConfig.CardCollectionData collection)
        {
            if (collection?.Cards == null)
            {
                yield break;
            }

            foreach (var card in collection.Cards)
            {
                if (card != null && !string.IsNullOrEmpty(card.Id))
                {
                    yield return card.Id;
                }
            }
        }

        private void NotifyCardAndCollection(string cardId)
        {
            var card = GetCard(cardId);
            var collection = GetCollectionByCard(cardId);

            if (card != null && collection != null)
            {
                CardChanged?.Invoke(CreateCardViewData(collection.Id, card));
                CollectionChanged?.Invoke(CreateCollectionViewData(collection));
            }
        }

        private void OnFeatureUnlocked(PlayerFeatureType feature)
        {
            if (feature != PlayerFeatureType.CardCollection)
            {
                return;
            }

            foreach (var collection in Collections)
            {
                if (collection != null && !string.IsNullOrEmpty(collection.Id))
                {
                    CollectionChanged?.Invoke(CreateCollectionViewData(collection));
                }
            }

            Changed?.Invoke();
        }
    }
}
