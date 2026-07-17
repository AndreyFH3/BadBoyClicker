using System;
using System.Collections.Generic;
using CardCollections;
using Core;
using DailyLogin;
using PlayerProgression;
using QuestSystem;
using UnityEngine;
using Zenject;

namespace Chests
{
    public class ChestRewardService : IChestService, IInitializable
    {
        private readonly ChestConfig _config;
        private readonly Wallet _wallet;
        private readonly ICardCollectionService _cardCollectionService;
        private readonly IPlayerProgressionService _playerProgression;
        private readonly Dictionary<string, ChestConfig.ChestData> _chestsById = new();

        [InjectOptional] private IBoostRewardService _boostRewardService;
        [InjectOptional] private IQuestBackgroundRewardService _backgroundRewardService;
        [InjectOptional] private IQuestCustomRewardService _customRewardService;

        public event System.Action<ChestOpenResult> ChestOpened;

        public ChestRewardService(
            ChestConfig config,
            Wallet wallet,
            ICardCollectionService cardCollectionService,
            IPlayerProgressionService playerProgression)
        {
            _config = config;
            _wallet = wallet;
            _cardCollectionService = cardCollectionService;
            _playerProgression = playerProgression;
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
                    GiveReward(reward);
                    break;
                case ChestConfig.ChestRewardKind.RandomCard:
                    if (!TryGiveRandomCard(entry, out card))
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
                    return entry.Reward != null;
                case ChestConfig.ChestRewardKind.RandomCard:
                    return true;
                default:
                    return false;
            }
        }

        private bool TryGiveRandomCard(
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
                    $"No card candidates for chest reward. Collection: {entry.CardCollectionId}, stars: {entry.CardStars}");
                return false;
            }

            selectedCard = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            return _cardCollectionService.TryAddCard(selectedCard.Id);
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
                        card.Stars != entry.CardStars ||
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
            if (reward.RewardType != QuestRewardType.Currency || !entry.HasAmountRange)
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

        private void GiveReward(QuestReward reward)
        {
            switch (reward.RewardType)
            {
                case QuestRewardType.Currency:
                    GiveCurrency(reward.CurrencyType, reward.Amount);
                    break;
                case QuestRewardType.PlayerBackground:
                    GiveBackground(reward.RewardId);
                    break;
                case QuestRewardType.Boost:
                    GiveBoost(reward.RewardId);
                    break;
                case QuestRewardType.Chest:
                    GiveChest(reward.RewardId);
                    break;
                case QuestRewardType.Custom:
                    GiveCustom(reward.RewardId);
                    break;
                default:
                    Debug.LogWarning($"Unsupported chest reward type: {reward.RewardType}");
                    break;
            }
        }

        private void GiveCurrency(QuestRewardCurrencyType currencyType, long amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"Chest currency reward amount should be positive: {amount}");
                return;
            }

            switch (currencyType)
            {
                case QuestRewardCurrencyType.Soft:
                    _wallet.AddSoft(amount);
                    break;
                case QuestRewardCurrencyType.Decor:
                    _wallet.AddMiddle(amount);
                    break;
                case QuestRewardCurrencyType.Hard:
                    _wallet.AddHard(amount);
                    break;
                default:
                    Debug.LogWarning($"Unsupported chest currency type: {currencyType}");
                    break;
            }
        }

        private void GiveBackground(string backgroundId)
        {
            if (_backgroundRewardService == null)
            {
                Debug.LogWarning($"Background reward service is not bound. Chest reward was not granted: {backgroundId}");
                return;
            }

            _backgroundRewardService.GiveBackground(backgroundId);
        }

        private void GiveBoost(string boostId)
        {
            if (_boostRewardService == null)
            {
                Debug.LogWarning($"Boost reward service is not bound. Chest reward was not granted: {boostId}");
                return;
            }

            _boostRewardService.GiveBoost(boostId);
        }

        private void GiveCustom(string rewardId)
        {
            if (_customRewardService == null)
            {
                Debug.LogWarning($"Custom reward service is not bound. Chest reward was not granted: {rewardId}");
                return;
            }

            _customRewardService.GiveCustomReward(rewardId);
        }
    }
}
