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
            _chestService.ChestOpened += OnChestOpened;
            _view.CloseRequested += OnCloseRequested;
        }

        public void Dispose()
        {
            _chestService.ChestOpened -= OnChestOpened;
            _view.CloseRequested -= OnCloseRequested;
        }

        private void OnChestOpened(ChestOpenResult result)
        {
            if (result == null)
            {
                return;
            }

            _rewardOfferUiGate.Block(this);
            _view.Show(new ChestOpenViewData
            {
                ChestTitle = _localization.Localize(result.Chest.TitleLocalizationKey),
                ChestIcon = result.Chest.Icon,
                OpenHintText = _localization.Localize(OpenHintLocalizationKey),
                RewardText = result.Card != null ? CreateCardRewardText(result.Card) : CreateRewardText(result.Reward),
                RewardIcon = result.Card != null ? result.Card.Icon : result.Reward?.Icon
            });
        }

        private string CreateCardRewardText(CardCollectionConfig.CardData card)
        {
            return _localization.Localize(card.TitleLocalizationKey);
        }

        private string CreateRewardText(QuestReward reward)
        {
            if (reward == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(reward.DisplayTextLocalizationKey) || !string.IsNullOrEmpty(reward.DisplayText))
            {
                return _localization.Localize(reward.DisplayTextLocalizationKey);
            }

            switch (reward.RewardType)
            {
                case QuestRewardType.Currency:
                    return reward.Amount.ConvertFromLongToString();
                case QuestRewardType.PlayerBackground:
                case QuestRewardType.Boost:
                case QuestRewardType.Chest:
                case QuestRewardType.Custom:
                    return reward.RewardId;
                default:
                    return string.Empty;
            }
        }

        private void OnCloseRequested()
        {
            _rewardOfferUiGate.Unblock(this);
            _view.Hide();
        }
    }
}
