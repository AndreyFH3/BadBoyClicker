using System;
using CardCollections;
using GameLocalization;
using Zenject;
using Shop;
using Chests;
using Core.Ads;
using Core.Time;
using UnityEngine;

namespace CardCollectionMVP
{
    public class CardCollectionsPresenter : IInitializable, ITickable, IDisposable
    {
        private const string CardChestAdPlacementId = "card_collection_chest";
        private static readonly long CardChestAdCooldownTicks = TimeSpan.FromHours(8).Ticks;
        private ICardCollectionService _service;
        private ICardCollectionsView _view;
        private CardCollectionSelectionModel _selectionModel;
        private IShopModel _shopModel;
        private IShopPurchaseConfirmationView _purchaseConfirmationView;
        private IRewardedAdsService _adsService;
        private IRewardedAdErrorView _adErrorView;
        private ITimeService _timeService;
        private IChestService _chestService;
        private ICardCollectionRewardWindow _rewardWindow;
        private string _pendingCardChestPurchaseId;
        private bool _isCardChestAdInProgress;
        private float _nextAdUiUpdateTime;

        [Inject]
        public void Construct(
            ICardCollectionService service,
            ICardCollectionsView view,
            CardCollectionSelectionModel selectionModel,
            IShopModel shopModel,
            IShopPurchaseConfirmationView purchaseConfirmationView,
            IRewardedAdsService adsService,
            IRewardedAdErrorView adErrorView,
            ITimeService timeService,
            IChestService chestService,
            ICardCollectionRewardWindow rewardWindow)
        {
            _service = service;
            _view = view;
            _selectionModel = selectionModel;
            _shopModel = shopModel;
            _purchaseConfirmationView = purchaseConfirmationView;
            _adsService = adsService;
            _adErrorView = adErrorView;
            _timeService = timeService;
            _chestService = chestService;
            _rewardWindow = rewardWindow;
        }

        public void Initialize()
        {
            _service.Changed += UpdateView;
            _shopModel.StateChanged += UpdateView;
            _service.CollectionChanged += OnCollectionChanged;
            _service.CollectionRewardClaimed += OnCollectionRewardClaimed;
            _view.CollectionSelected += OnCollectionSelected;
            _view.CollectRequested += OnCollectRequested;
            _view.CardChestPurchaseRequested += OnCardChestPurchaseRequested;
            _view.CardChestAdRequested += OnCardChestAdRequested;
            _purchaseConfirmationView.ConfirmRequested += OnPurchaseConfirmed;
            _purchaseConfirmationView.CancelRequested += OnPurchaseCanceled;
            _selectionModel.SelectedCollectionChanged += OnSelectedCollectionChanged;
            Localization.LanguageChanged += UpdateView;

            UpdateView();
        }

        public void Dispose()
        {
            _service.Changed -= UpdateView;
            _shopModel.StateChanged -= UpdateView;
            _service.CollectionChanged -= OnCollectionChanged;
            _service.CollectionRewardClaimed -= OnCollectionRewardClaimed;
            _view.CollectionSelected -= OnCollectionSelected;
            _view.CollectRequested -= OnCollectRequested;
            _view.CardChestPurchaseRequested -= OnCardChestPurchaseRequested;
            _view.CardChestAdRequested -= OnCardChestAdRequested;
            _purchaseConfirmationView.ConfirmRequested -= OnPurchaseConfirmed;
            _purchaseConfirmationView.CancelRequested -= OnPurchaseCanceled;
            _selectionModel.SelectedCollectionChanged -= OnSelectedCollectionChanged;
            Localization.LanguageChanged -= UpdateView;
        }

        public void Tick()
        {
            if (Time.unscaledTime < _nextAdUiUpdateTime)
            {
                return;
            }

            _nextAdUiUpdateTime = Time.unscaledTime + 1f;
            UpdateCardChestAdView();
        }

        private void UpdateView()
        {
            _view.SetVisible(string.IsNullOrEmpty(_selectionModel.SelectedCollectionId));

            var collections = _service.GetAllViewData();
            _view.SetData(collections);
            _view.SetCardChestPurchaseData(_shopModel.GetCardChestPurchaseData());
            UpdateCardChestAdView();
        }

        private void OnCollectionChanged(CardCollectionViewData collection)
        {
            _view.UpdateCollection(collection);
        }

        private void OnCollectionRewardClaimed(CardCollectionViewData collection)
        {
            if (collection?.Rewards != null)
            {
                _rewardWindow.Show(collection.Rewards);
            }
        }

        private void OnCollectionSelected(string collectionId)
        {
            if (_service.GetViewData(collectionId) == null)
            {
                return;
            }

            _selectionModel.Select(collectionId);
        }

        private void OnSelectedCollectionChanged(string collectionId)
        {
            _view.SetVisible(string.IsNullOrEmpty(collectionId));
        }

        private void OnCollectRequested(string collectionId)
        {
            _service.TryClaimReward(collectionId);
        }

        private void OnCardChestPurchaseRequested()
        {
            var offer = _shopModel.GetCardChestPurchaseData();
            if (offer == null)
            {
                return;
            }

            var confirmation = _shopModel.GetPurchaseConfirmationData(offer.Id);
            if (confirmation == null)
            {
                return;
            }

            _pendingCardChestPurchaseId = offer.Id;
            _purchaseConfirmationView.Show(confirmation);
        }

        private void OnCardChestAdRequested()
        {
            if (_isCardChestAdInProgress || GetCardChestAdRemainingTicks() > 0 ||
                _service.AreAllCollectionsCompleted || string.IsNullOrEmpty(_shopModel.GetCardChestId()))
            {
                return;
            }

            if (!_adsService.IsAvailable(CardChestAdPlacementId))
            {
                _adErrorView.Show();
                return;
            }

            _isCardChestAdInProgress = true;
            UpdateCardChestAdView();
            _adsService.Show(CardChestAdPlacementId, OnCardChestAdRewarded, OnCardChestAdFailed);
        }

        private void OnCardChestAdRewarded()
        {
            _isCardChestAdInProgress = false;
            _service.SetLastCardChestAdUtcTicks(_timeService.CurrentUtcTicks);

            string chestId = _shopModel.GetCardChestId();
            if (!string.IsNullOrEmpty(chestId))
            {
                _chestService.TryOpenChest(chestId, out _);
            }

            UpdateCardChestAdView();
        }

        private void OnCardChestAdFailed(Core.Ads.RewardedAdFailureReason reason)
        {
            _isCardChestAdInProgress = false;
            _adErrorView.Show();
            UpdateCardChestAdView();
        }

        private void UpdateCardChestAdView()
        {
            string chestId = _shopModel.GetCardChestId();
            bool visible = !_service.AreAllCollectionsCompleted && !string.IsNullOrEmpty(chestId);
            bool isAvailable = visible && !_isCardChestAdInProgress &&
                               GetCardChestAdRemainingTicks() <= 0 &&
                               _adsService.IsAvailable(CardChestAdPlacementId);

            _view.SetCardChestAdAvailable(isAvailable);
        }

        private long GetCardChestAdRemainingTicks()
        {
            long lastClaimTicks = _service.LastCardChestAdUtcTicks;
            if (lastClaimTicks <= 0)
            {
                return 0;
            }

            long elapsedTicks = Math.Max(0, _timeService.CurrentUtcTicks - lastClaimTicks);
            return Math.Max(0, CardChestAdCooldownTicks - elapsedTicks);
        }

        private void OnPurchaseConfirmed()
        {
            if (string.IsNullOrEmpty(_pendingCardChestPurchaseId))
            {
                return;
            }

            string id = _pendingCardChestPurchaseId;
            _pendingCardChestPurchaseId = null;
            _purchaseConfirmationView.Hide();
            _shopModel.Buy(id);
        }

        private void OnPurchaseCanceled()
        {
            _pendingCardChestPurchaseId = null;
        }
    }
}
