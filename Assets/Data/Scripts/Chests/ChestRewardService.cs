using System;
using System.Collections.Generic;
using CardCollections;
using Core;
using DailyLogin;
using PlayerFeatures;
using PlayerProgression;
using QuestSystem;
using Shop;
using UnityEngine;
using Zenject;

namespace Chests
{
    public class ChestRewardService : IChestService, IInitializable
    {
        // Soft-currency chest rewards ignore the per-entry min/max and use the
        // configurable mixed-income time window from ChestConfig instead.
        private readonly ChestConfig _config;
        private readonly Wallet _wallet;
        private readonly ICardCollectionService _cardCollectionService;
        private readonly IPlayerProgressionService _playerProgression;
        private readonly IPlayerFeatureUnlockService _featureUnlockService;
        private readonly IShopRuntimeSave _shopSave;
        private readonly Dictionary<string, ChestConfig.ChestData> _chestsById = new();

        [InjectOptional] private IBoostRewardService _boostRewardService;
        [InjectOptional] private IQuestBackgroundRewardService _backgroundRewardService;
        [InjectOptional] private IQuestCustomRewardService _customRewardService;

        public event System.Action<ChestOpenResult> ChestOpeningPrepared;
        public event System.Action<ChestOpenResult> ChestOpened;

        public ChestRewardService(
            ChestConfig config,
            Wallet wallet,
            ICardCollectionService cardCollectionService,
            IPlayerProgressionService playerProgression,
            IPlayerFeatureUnlockService featureUnlockService,
            IShopRuntimeSave shopSave)
        {
            _config = config;
            _wallet = wallet;
            _cardCollectionService = cardCollectionService;
            _playerProgression = playerProgression;
            _featureUnlockService = featureUnlockService;
            _shopSave = shopSave;
        }

        public void Initialize()
        {
            RebuildIndex();
        }

        public void GiveChest(string chestId)
        {
            TryOpenChest(chestId, out _);
        }

        public ChestConfig.ChestData GetChest(string chestId)
        {
            return !string.IsNullOrEmpty(chestId) && _chestsById.TryGetValue(chestId, out var chest)
                ? chest
                : null;
        }

        public bool TryOpenChest(string chestId, out ChestOpenResult result)
        {
            result = null;

            if (!_featureUnlockService.IsUnlocked(PlayerFeatureType.Chests))
            {
                Debug.Log($"Chests are locked. Chest reward was not granted: {chestId}");
                return false;
            }

            var chest = GetChest(chestId);
            if (chest == null)
            {
                Debug.LogWarning($"Chest config was not found: {chestId}");
                return false;
            }

            var entry = RollReward(chest);
            if (entry == null)
            {
                Debug.LogWarning($"Chest has no configured rewards: {chestId}");
                return false;
            }

            QuestReward reward = null;
            CardCollectionConfig.CardData card = null;

            switch (entry.RewardKind)
            {
                case ChestConfig.ChestRewardKind.ConfiguredReward:
                    if (entry.Reward == null)
                    {
                        Debug.LogWarning($"Chest reward entry is empty: {chestId}");
                        return false;
                    }

                    reward = entry.Reward;
                    ApplyAmountOverride(entry, reward);
                    break;
                case ChestConfig.ChestRewardKind.RandomCard:
                    if (!TrySelectRandomCard(entry, out card))
                    {
                        Debug.LogWarning($"Chest random card reward could not be granted: {chestId}");
                        return false;
                    }

                    break;
                default:
                    Debug.LogWarning($"Unsupported chest reward kind: {entry.RewardKind}");
                    return false;
            }

            result = new ChestOpenResult
            {
                Chest = chest,
                Reward = reward,
                Card = card
            };

            ChestOpeningPrepared?.Invoke(result);
            return true;
        }

        public bool TryClaimChestReward(ChestOpenResult result)
        {
            if (result == null || result.IsClaimed)
            {
                return false;
            }

            bool granted = result.Card != null
                ? _cardCollectionService != null && _cardCollectionService.TryAddCard(result.Card.Id)
                : GiveReward(result.Reward);

            if (!granted)
            {
                Debug.LogWarning($"Chest reward could not be granted: {result.Chest?.Id}");
                return false;
            }

            result.IsClaimed = true;
            ChestOpened?.Invoke(result);
            return true;
        }

        private void RebuildIndex()
        {
            _chestsById.Clear();

            if (_config?.Chests == null)
            {
                return;
            }

            foreach (var chest in _config.Chests)
            {
                if (chest == null || string.IsNullOrEmpty(chest.Id) || _chestsById.ContainsKey(chest.Id))
                {
                    continue;
                }

                _chestsById.Add(chest.Id, chest);
            }
        }

        private ChestConfig.ChestRewardEntry RollReward(ChestConfig.ChestData chest)
        {
            if (chest?.Rewards == null || chest.Rewards.Count == 0)
            {
                return null;
            }

            int totalWeight = 0;
            foreach (var reward in chest.Rewards)
            {
                if (reward != null && IsRewardEntryValid(reward))
                {
                    totalWeight += reward.RarityWeight;
                }
            }

            if (totalWeight <= 0)
            {
                return null;
            }

            int roll = UnityEngine.Random.Range(0, totalWeight);
            foreach (var reward in chest.Rewards)
            {
                if (reward == null || !IsRewardEntryValid(reward))
                {
                    continue;
                }

                roll -= reward.RarityWeight;
                if (roll < 0)
                {
                    return reward;
                }
            }

            return null;
        }

        private bool IsRewardEntryValid(ChestConfig.ChestRewardEntry entry)
        {
            switch (entry.RewardKind)
            {
                case ChestConfig.ChestRewardKind.ConfiguredReward:
                    return entry.Reward != null &&
                           RewardFeatureGate.IsAvailable(entry.Reward, _featureUnlockService);
                case ChestConfig.ChestRewardKind.RandomCard:
                    return _featureUnlockService == null ||
                           _featureUnlockService.IsUnlocked(PlayerFeatureType.CardCollection);
                default:
                    return false;
            }
        }

        private bool TrySelectRandomCard(
            ChestConfig.ChestRewardEntry entry,
            out CardCollectionConfig.CardData selectedCard)
        {
            selectedCard = null;

            if (_cardCollectionService == null)
            {
                Debug.LogWarning("Card collection service is not bound. Chest card reward was not granted.");
                return false;
            }

            var candidates = GetCardCandidates(entry, requireMissingCard: !entry.AllowDuplicateCards);
            if (candidates.Count == 0 && !entry.AllowDuplicateCards)
            {
                candidates = GetCardCandidates(entry, requireMissingCard: false);
            }

            if (candidates.Count == 0)
            {
                Debug.LogWarning(
                    $"No card candidates for chest reward. Collection: {entry.CardCollectionId}");
                return false;
            }

            selectedCard = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            return true;
        }

        private List<CardCollectionConfig.CardData> GetCardCandidates(
            ChestConfig.ChestRewardEntry entry,
            bool requireMissingCard)
        {
            var result = new List<CardCollectionConfig.CardData>();

            foreach (var collection in _cardCollectionService.Collections)
            {
                if (collection == null ||
                    (!string.IsNullOrEmpty(entry.CardCollectionId) && collection.Id != entry.CardCollectionId) ||
                    collection.Cards == null)
                {
                    continue;
                }

                foreach (var card in collection.Cards)
                {
                    if (card == null ||
                        requireMissingCard && _cardCollectionService.HasCard(card.Id))
                    {
                        continue;
                    }

                    result.Add(card);
                }
            }

            return result;
        }

        private void ApplyAmountOverride(ChestConfig.ChestRewardEntry entry, QuestReward reward)
        {
            if (reward.RewardType != QuestRewardType.Currency)
            {
                return;
            }

            if (reward.CurrencyType == QuestRewardCurrencyType.Soft)
            {
                reward.SetAmount(RollSoftRewardAmount());
                return;
            }

            if (!entry.HasAmountRange)
            {
                return;
            }

            long amount = UnityEngine.Random.Range((int)entry.MinAmount, (int)entry.MaxAmount + 1);
            if (entry.ScaleWithIncomeMultiplier)
            {
                float multiplier = _playerProgression?.ClickIncomeMultiplier ?? 1f;
                amount = Math.Max(1, (long)Math.Ceiling(amount * Math.Max(0f, multiplier)));
            }

            reward.SetAmount(amount);
        }

        private long RollSoftRewardAmount()
        {
            double incomePerSecond = Math.Max(0, _shopSave?.AutoIncomePerSecond ?? 0) +
                                     Math.Max(0, _shopSave?.ClickValue ?? 0) *
                                     (_config?.SoftRewardAssumedClicksPerSecond ?? 0f);
            long configuredMin = _config?.SoftRewardMinAmount ?? 250;
            long min = ToLongSaturated(Math.Max(configuredMin,
                incomePerSecond * 60d * (_config?.SoftRewardMinIncomeMinutes ?? 1f)));
            long max = ToLongSaturated(Math.Max(min,
                incomePerSecond * 60d * (_config?.SoftRewardMaxIncomeMinutes ?? 3f)));
            return RollRandomLong(min, max);
        }

        private static long ToLongSaturated(double value)
        {
            return value >= long.MaxValue ? long.MaxValue : Math.Max(0, (long)Math.Ceiling(value));
        }

        // Random.Range(int,int) can't safely cover this range at high incomes
        // (late-game soft currency routinely exceeds int.MaxValue), so roll in
        // double space instead.
        private static long RollRandomLong(long min, long max)
        {
            if (max <= min)
            {
                return min;
            }

            double t = UnityEngine.Random.value;
            return min + (long)(t * (max - min + 1));
        }

        private bool GiveReward(QuestReward reward)
        {
            switch (reward.RewardType)
            {
                case QuestRewardType.Currency:
                    return GiveCurrency(reward.CurrencyType, reward.Amount);
                case QuestRewardType.PlayerBackground:
                    return GiveBackground(reward.RewardId);
                case QuestRewardType.Boost:
                    return GiveBoost(reward.RewardId);
                case QuestRewardType.Chest:
                    return TryOpenChest(reward.RewardId, out _);
                case QuestRewardType.Custom:
                    return GiveCustom(reward.RewardId);
                default:
                    Debug.LogWarning($"Unsupported chest reward type: {reward.RewardType}");
                    return false;
            }
        }

        private bool GiveCurrency(QuestRewardCurrencyType currencyType, long amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"Chest currency reward amount should be positive: {amount}");
                return false;
            }

            switch (currencyType)
            {
                case QuestRewardCurrencyType.Soft:
                    _wallet.AddSoft(amount);
                    return true;
                case QuestRewardCurrencyType.Decor:
                    _wallet.AddMiddle(amount);
                    return true;
                case QuestRewardCurrencyType.Hard:
                    _wallet.AddHard(amount);
                    return true;
                default:
                    Debug.LogWarning($"Unsupported chest currency type: {currencyType}");
                    return false;
            }
        }

        private bool GiveBackground(string backgroundId)
        {
            if (_backgroundRewardService == null)
            {
                Debug.LogWarning($"Background reward service is not bound. Chest reward was not granted: {backgroundId}");
                return false;
            }

            _backgroundRewardService.GiveBackground(backgroundId);
            return true;
        }

        private bool GiveBoost(string boostId)
        {
            if (_boostRewardService == null)
            {
                Debug.LogWarning($"Boost reward service is not bound. Chest reward was not granted: {boostId}");
                return false;
            }

            _boostRewardService.GiveBoost(boostId);
            return true;
        }

        private bool GiveCustom(string rewardId)
        {
            if (_customRewardService == null)
            {
                Debug.LogWarning($"Custom reward service is not bound. Chest reward was not granted: {rewardId}");
                return false;
            }

            _customRewardService.GiveCustomReward(rewardId);
            return true;
        }
    }
}
