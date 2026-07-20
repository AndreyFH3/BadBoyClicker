using System;
using Core.Ads;
using GameLocalization;
using Zenject;

namespace AdBonusOffers
{
    public class AdBonusOfferPresenter : IInitializable, ITickable, IDisposable
    {
        private IAdBonusOfferService _service;
        private IAdBonusOfferView _view;
        private IRewardedAdErrorView _adErrorView;
        private IRewardOfferUiGate _uiGate;
        private bool _isConfirmationOpen;
        private bool _isResultOpen;
        private bool _isAdClaimBlockedAfterFailure;

        [Inject]
        public void Construct(
            IAdBonusOfferService service,
            IAdBonusOfferView view,
            IRewardedAdErrorView adErrorView,
            IRewardOfferUiGate uiGate)
        {
            _service = service;
            _view = view;
            _adErrorView = adErrorView;
            _uiGate = uiGate;
        }

        public void Initialize()
        {
            _service.OfferShown += OnOfferShown;
            _service.OfferHidden += OnOfferHidden;
            _service.RewardGranted += OnRewardGranted;
            _service.RewardFailed += OnRewardFailed;
            _view.CardClicked += OnCardClicked;
            _view.ConfirmRequested += OnConfirmRequested;
            _view.HardClaimRequested += OnHardClaimRequested;
            _view.ClosedRequested += OnClosedRequested;
            _view.ResultClosedRequested += OnResultClosedRequested;
            _uiGate.Changed += OnUiGateChanged;
            Localization.LanguageChanged += OnLanguageChanged;
        }

        public void Tick()
        {
            if (_service.HasActiveOffer)
            {
                _view.SetCardRemainingSeconds(_service.CurrentOfferRemainingSeconds);
            }
        }

        public void Dispose()
        {
            _service.OfferShown -= OnOfferShown;
            _service.OfferHidden -= OnOfferHidden;
            _service.RewardGranted -= OnRewardGranted;
            _service.RewardFailed -= OnRewardFailed;
            _view.CardClicked -= OnCardClicked;
            _view.ConfirmRequested -= OnConfirmRequested;
            _view.HardClaimRequested -= OnHardClaimRequested;
            _view.ClosedRequested -= OnClosedRequested;
            _view.ResultClosedRequested -= OnResultClosedRequested;
            _uiGate.Changed -= OnUiGateChanged;
            Localization.LanguageChanged -= OnLanguageChanged;
        }

        private void OnOfferShown(AdBonusOfferViewData data)
        {
            _isAdClaimBlockedAfterFailure = false;
            if (IsOfferUiBlocked)
            {
                _view.HideCard();
                _service.SetCurrentOfferTimerPaused(true);
                return;
            }

            if (!IsOfferUiBlocked)
            {
                _view.ShowCard(data);
            }
        }

        private void OnOfferHidden()
        {
            _isConfirmationOpen = false;
            _isAdClaimBlockedAfterFailure = false;
            _service.SetCurrentOfferTimerPaused(false);
            _view.HideCard();
            _view.HideConfirmation();
        }

        private void OnRewardGranted(AdBonusOfferViewData data)
        {
            _isConfirmationOpen = false;
            _isResultOpen = true;
            _service.SetCurrentOfferTimerPaused(false);
            _view.HideCard();
            _view.HideConfirmation();
            _view.ShowRewardResult(data);
        }

        private void OnRewardFailed(AdBonusOfferViewData data)
        {
            _adErrorView.Show();
            _isAdClaimBlockedAfterFailure = true;

            if (_service.HasActiveOffer)
            {
                var currentOffer = GetCurrentOfferViewData();

                if (!currentOffer.CanClaimForHard)
                {
                    _isConfirmationOpen = false;
                    _service.HideCurrentOffer();
                    return;
                }

                _isConfirmationOpen = true;
                _service.SetCurrentOfferTimerPaused(true);
                _view.HideCard();
                _view.ShowConfirmation(currentOffer);
            }
        }

        private void OnCardClicked()
        {
            if (_service.HasActiveOffer)
            {
                _isConfirmationOpen = true;
                _service.SetCurrentOfferTimerPaused(true);
                _view.HideCard();
                _view.ShowConfirmation(GetCurrentOfferViewData());
            }
        }

        private void OnConfirmRequested()
        {
            _service.ClaimCurrentOffer();
        }

        private void OnHardClaimRequested()
        {
            _service.ClaimCurrentOfferForHard();
        }

        private void OnClosedRequested()
        {
            _isConfirmationOpen = false;
            _service.SetCurrentOfferTimerPaused(false);
            _view.HideConfirmation();

            if (_service.HasActiveOffer && IsOfferUiBlocked)
            {
                _service.SetCurrentOfferTimerPaused(true);
            }

            if (_service.HasActiveOffer && !IsOfferUiBlocked)
            {
                _view.ShowCard(GetCurrentOfferViewData());
            }
        }

        private void OnResultClosedRequested()
        {
            _isResultOpen = false;
            _view.HideRewardResult();
        }

        private void OnLanguageChanged()
        {
            if (_service.HasActiveOffer && !IsOfferUiBlocked)
            {
                _view.ShowCard(GetCurrentOfferViewData());
            }
        }

        private void OnUiGateChanged()
        {
            if (!_service.HasActiveOffer)
            {
                return;
            }

            if (IsOfferUiBlocked)
            {
                _view.HideCard();
                _service.SetCurrentOfferTimerPaused(true);
                return;
            }

            _service.SetCurrentOfferTimerPaused(false);
            _view.ShowCard(GetCurrentOfferViewData());
        }

        private bool IsOfferUiBlocked => _isConfirmationOpen || _isResultOpen || _uiGate.IsBlocked;

        private AdBonusOfferViewData GetCurrentOfferViewData()
        {
            var data = _service.CurrentOffer;
            return _isAdClaimBlockedAfterFailure ? data.WithAdClaimAvailability(false) : data;
        }
    }
}
