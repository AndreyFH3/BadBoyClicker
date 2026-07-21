using System;
using CardCollections;
using Chests;
using Core;
using Core.Ads;
using GameLocalization;
using QuestSystem;
using Utils;
using Zenject;

namespace ChestsMVP
{
    public class ChestOpenPresenter : IInitializable, IDisposable
    {
        private const string OpenHintLocalizationKey = "chest_open_hint";

        private readonly IChestService _chestService;
        private readonly IChestOpenView _view;
        private readonly ILocalizationService _localization;
        private readonly IRewardOfferUiGate _rewardOfferUiGate;
        private ChestOpenResult _pendingResult;

        public ChestOpenPresenter(
            IChestService chestService,
            IChestOpenView view,
            ILocalizationService localization,
            IRewardOfferUiGate rewardOfferUiGate)
        {
            _chestService = chestService;
            _view = view;
            _localization = localization;
            _rewardOfferUiGate = rewardOfferUiGate;
        }

        public void Initialize()
        {
            _chestService.ChestOpeningPrepared += OnChestOpened;
            _view.CloseRequested += OnCloseRequested;
            _view.RewardRevealed += OnRewardRevealed;
        }

        public void Dispose()
        {
            _chestService.ChestOpeningPrepared -= OnChestOpened;
            _view.CloseRequested -= OnCloseRequested;
            _view.RewardRevealed -= OnRewardRevealed;
        }

        private void OnChestOpened(ChestOpenResult result)
        {
            if (result == null)
            {
                return;
            }

            _rewardOfferUiGate.Block(this);
            _pendingResult = result;
            _view.Show(new ChestOpenViewData
            {
                ChestTitle = _localization.Localize(result.Chest.TitleLocalizationKey),
                ChestIcon = result.Chest.Icon,
                OpenHintText = _localization.Localize(OpenHintLocalizationKey),
                RewardText = result.Card != null ? CreateCardRewardText(result.Card) : CreateRewardText(result.Reward),
                RewardIcon = result.Card != null ? result.Card.Icon : result.Reward?.Icon
            });
        }

        private void OnRewardRevealed()
        {
            if (_pendingResult == null)
            {
                return;
            }

            ChestOpenResult result = _pendingResult;
            _pendingResult = null;
            _chestService.TryClaimChestReward(result);
        }

        private string CreateCardRewardText(CardCollectionConfig.CardData card)
        {
            return $"{_localization.Localize(card.TitleLocalizationKey)} x1";
        }

        private string CreateRewardText(QuestReward reward)
        {
            if (reward == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(reward.DisplayTextLocalizationKey))
            {
                return _localization.Localize(reward.DisplayTextLocalizationKey);
            }

            if (!string.IsNullOrEmpty(reward.DisplayText))
            {
                return reward.DisplayText;
            }

            switch (reward.RewardType)
            {
                case QuestRewardType.Currency:
                    return $"+{reward.Amount.ConvertFromLongToString()} {_localization.Localize(GetCurrencyNameKey(reward.CurrencyType))}";
                case QuestRewardType.PlayerBackground:
                case QuestRewardType.Boost:
                case QuestRewardType.Chest:
                case QuestRewardType.Custom:
                    return reward.RewardId;
                default:
                    return string.Empty;
            }
        }

        private static string GetCurrencyNameKey(QuestRewardCurrencyType currencyType)
        {
            switch (currencyType)
            {
                case QuestRewardCurrencyType.Soft:
                    return "currency.soft";
                case QuestRewardCurrencyType.Decor:
                    return "currency.decor";
                case QuestRewardCurrencyType.Hard:
                    return "currency.hard";
                case QuestRewardCurrencyType.Yan:
                    return "currency.yan";
                default:
                    return "currency.soft";
            }
        }

        private void OnCloseRequested()
        {
            _pendingResult = null;
            _rewardOfferUiGate.Unblock(this);
            _view.Hide();
        }
    }
}
