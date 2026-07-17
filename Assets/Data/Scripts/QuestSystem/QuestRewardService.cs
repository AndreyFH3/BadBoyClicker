using System.Collections.Generic;
using Core;
using DailyLogin;
using RewardActivation;
using UnityEngine;
using Zenject;

namespace QuestSystem
{
    public class QuestRewardService : IQuestRewardService
    {
        private readonly Wallet _wallet;
        private readonly DiContainer _container;

        [InjectOptional] private IBoostRewardService _boostRewardService;
        [InjectOptional] private IQuestBackgroundRewardService _backgroundRewardService;
        [InjectOptional] private IQuestCustomRewardService _customRewardService;
        [InjectOptional] private IRewardActivationService _activationService;

        public QuestRewardService(Wallet wallet, DiContainer container)
        {
            _wallet = wallet;
            _container = container;
        }

        public void GiveRewards(IReadOnlyList<QuestReward> rewards)
        {
            if (rewards == null)
            {
                return;
            }

            foreach (var reward in rewards)
            {
                GiveReward(reward);
            }
        }

        public void GiveReward(QuestReward reward)
        {
            if (reward == null)
            {
                return;
            }

            if (reward.RequiresActivation && _activationService != null)
            {
                _activationService.Enqueue(reward, () => GrantNow(reward));
                return;
            }

            GrantNow(reward);
        }

        private void GrantNow(QuestReward reward)
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
                    Debug.LogWarning($"Unsupported quest reward type: {reward.RewardType}");
                    break;
            }
        }

        private void GiveCurrency(QuestRewardCurrencyType currencyType, long amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"Quest currency reward amount should be positive: {amount}");
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
                    Debug.LogWarning($"Unsupported quest currency type: {currencyType}");
                    break;
            }
        }

        private void GiveBackground(string backgroundId)
        {
            if (_backgroundRewardService == null)
            {
                Debug.LogWarning($"Background reward service is not bound. Background was not granted: {backgroundId}");
                return;
            }

            _backgroundRewardService.GiveBackground(backgroundId);
        }

        private void GiveBoost(string boostId)
        {
            if (_boostRewardService == null)
            {
                Debug.LogWarning($"Boost reward service is not bound. Boost was not granted: {boostId}");
                return;
            }

            _boostRewardService.GiveBoost(boostId);
        }

        private void GiveChest(string chestId)
        {
            var chestRewardService = _container.TryResolve<IChestRewardService>();
            if (chestRewardService == null)
            {
                Debug.LogWarning($"Chest reward service is not bound. Chest was not granted: {chestId}");
                return;
            }

            chestRewardService.GiveChest(chestId);
        }

        private void GiveCustom(string rewardId)
        {
            if (_customRewardService == null)
            {
                Debug.LogWarning($"Custom quest reward service is not bound. Reward was not granted: {rewardId}");
                return;
            }

            _customRewardService.GiveCustomReward(rewardId);
        }
    }
}
