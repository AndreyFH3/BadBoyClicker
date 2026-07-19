using System;
using QuestSystem;
using UnityEngine;
using Zenject;

namespace CardCollections
{
    // Permanent percent bonuses granted by fully-claimed card collections
    // (e.g. "card_collection_bonus_passive_income_10"). Recomputed from the
    // claimed-collection state rather than stored separately, so it stays in
    // sync automatically whenever collection save data changes.
    public class CardCollectionBonusService : ICardCollectionBonusService, IInitializable, IDisposable
    {
        private const string PassiveIncomeBonusPrefix = "card_collection_bonus_passive_income_";
        private const string ClickIncomeBonusPrefix = "card_collection_bonus_click_power_";
        private const string ShopDiscountBonusPrefix = "card_collection_bonus_shop_discount_";
        private const string OfflineIncomeBonusPrefix = "card_collection_bonus_offline_income_";

        private readonly ICardCollectionService _collectionService;
        private float _passiveIncomePercent;
        private float _clickIncomePercent;
        private float _shopDiscountPercent;
        private float _offlineIncomePercent;

        public event Action Changed;

        public float PassiveIncomeMultiplier => 1f + _passiveIncomePercent / 100f;
        public float ClickIncomeMultiplier => 1f + _clickIncomePercent / 100f;
        public float ShopPriceMultiplier => Mathf.Max(0f, 1f - _shopDiscountPercent / 100f);
        public float OfflineIncomeMultiplier => 1f + _offlineIncomePercent / 100f;

        public CardCollectionBonusService(ICardCollectionService collectionService)
        {
            _collectionService = collectionService;
        }

        public void Initialize()
        {
            Recalculate();
            _collectionService.Changed += OnCollectionServiceChanged;
            _collectionService.CollectionRewardClaimed += OnCollectionRewardClaimed;
        }

        public void Dispose()
        {
            _collectionService.Changed -= OnCollectionServiceChanged;
            _collectionService.CollectionRewardClaimed -= OnCollectionRewardClaimed;
        }

        private void OnCollectionServiceChanged()
        {
            Recalculate();
        }

        private void OnCollectionRewardClaimed(CardCollectionViewData _)
        {
            Recalculate();
        }

        private void Recalculate()
        {
            float passivePercent = 0f;
            float clickPercent = 0f;
            float shopDiscountPercent = 0f;
            float offlinePercent = 0f;

            foreach (var collection in _collectionService.Collections)
            {
                if (collection == null || string.IsNullOrEmpty(collection.Id) ||
                    !_collectionService.IsRewardClaimed(collection.Id))
                {
                    continue;
                }

                foreach (var reward in collection.Rewards)
                {
                    if (reward == null || reward.RewardType != QuestRewardType.Custom ||
                        string.IsNullOrEmpty(reward.RewardId))
                    {
                        continue;
                    }

                    if (reward.RewardId.StartsWith(PassiveIncomeBonusPrefix, StringComparison.Ordinal))
                    {
                        passivePercent += reward.Amount;
                    }
                    else if (reward.RewardId.StartsWith(ClickIncomeBonusPrefix, StringComparison.Ordinal))
                    {
                        clickPercent += reward.Amount;
                    }
                    else if (reward.RewardId.StartsWith(ShopDiscountBonusPrefix, StringComparison.Ordinal))
                    {
                        shopDiscountPercent += reward.Amount;
                    }
                    else if (reward.RewardId.StartsWith(OfflineIncomeBonusPrefix, StringComparison.Ordinal))
                    {
                        offlinePercent += reward.Amount;
                    }
                }
            }

            _passiveIncomePercent = passivePercent;
            _clickIncomePercent = clickPercent;
            _shopDiscountPercent = shopDiscountPercent;
            _offlineIncomePercent = offlinePercent;
            Changed?.Invoke();
        }
    }
}
