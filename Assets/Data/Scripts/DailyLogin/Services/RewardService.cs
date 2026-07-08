using Core;
using QuestSystem;
using UnityEngine;
using Zenject;

namespace DailyLogin
{
    public class RewardService : IRewardService
    {
        private readonly Wallet _wallet;

        [InjectOptional] private IBoostRewardService _boostRewardService;
        [InjectOptional] private IChestRewardService _chestRewardService;
        [InjectOptional] private IQuestBackgroundRewardService _backgroundRewardService;

        public RewardService(Wallet wallet)
        {
            _wallet = wallet;
        }

        public void GiveReward(RewardConfig reward)
        {
            if (reward == null)
            {
                return;
            }

            switch (reward.RewardType)
            {
                case RewardType.Currency:
                    GiveCurrency(reward.CurrencyType, reward.RollAmount());
                    break;
                case RewardType.Boost:
                    GiveBoost(reward.RewardId);
                    break;
                case RewardType.Chest:
                    GiveChest(reward.RewardId);
                    break;
                case RewardType.Cosmetic:
                    GiveCosmetic(reward.RewardId);
                    break;
                default:
                    Debug.LogWarning($"Unsupported reward type: {reward.RewardType}");
                    break;
            }
        }

        private void GiveCurrency(CurrencyType currencyType, long amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"Daily login currency reward amount should be positive: {amount}");
                return;
            }

            switch (currencyType)
            {
                case CurrencyType.Soft:
                    _wallet.AddSoft(amount);
                    break;
                case CurrencyType.Decor:
                    _wallet.AddMiddle(amount);
                    break;
                case CurrencyType.Hard:
                    _wallet.AddHard(amount);
                    break;
                default:
                    Debug.LogWarning($"Unsupported currency type: {currencyType}");
                    break;
            }
        }

        private void GiveBoost(string boostId)
        {
            if (_boostRewardService == null)
            {
                // Bind this port to the real BoostService when boosts are implemented.
                Debug.LogWarning($"Boost reward service is not bound. Boost was not granted: {boostId}");
                return;
            }

            _boostRewardService.GiveBoost(boostId);
        }

        private void GiveChest(string chestId)
        {
            if (_chestRewardService == null)
            {
                // Bind this port to the real ChestService when chests are implemented.
                Debug.LogWarning($"Chest reward service is not bound. Chest was not granted: {chestId}");
                return;
            }

            _chestRewardService.GiveChest(chestId);
        }

        private void GiveCosmetic(string cosmeticId)
        {
            if (_backgroundRewardService == null)
            {
                Debug.LogWarning($"Cosmetic reward service is not bound. Cosmetic was not granted: {cosmeticId}");
                return;
            }

            _backgroundRewardService.GiveBackground(cosmeticId);
        }
    }
}
