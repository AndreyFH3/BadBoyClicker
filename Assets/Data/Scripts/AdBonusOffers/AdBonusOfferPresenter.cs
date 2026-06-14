using System;
using GameLocalization;
using Zenject;

namespace AdBonusOffers
{
    public class AdBonusOfferPresenter : IInitializable, ITickable, IDisposable
    {
        private IAdBonusOfferService _service;
        private IAdBonusOfferView _view;

        [Inject]
        public void Construct(IAdBonusOfferService service, IAdBonusOfferView view)
        {
            _service = service;
            _view = view;
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
            Localization.LanguageChanged -= OnLanguageChanged;
        }

        private void OnOfferShown(AdBonusOfferViewData data)
        {
            _view.ShowCard(data);
        }

        private void OnOfferHidden()
        {
            _view.HideCard();
            _view.HideConfirmation();
        }

        private void OnRewardGranted(AdBonusOfferViewData data)
        {
            _view.HideCard();
            _view.HideConfirmation();
            _view.ShowRewardResult(data);
        }

        private void OnRewardFailed(AdBonusOfferViewData data)
        {
            if (_service.HasActiveOffer)
            {
                _view.ShowConfirmation(_service.CurrentOffer);
            }
        }

        private void OnCardClicked()
        {
            if (_service.HasActiveOffer)
            {
                _view.ShowConfirmation(_service.CurrentOffer);
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
            _view.HideConfirmation();
            if (_service.HasActiveOffer)
            {
                _view.ShowCard(_service.CurrentOffer);
            }
        }

        private void OnResultClosedRequested()
        {
            _view.HideRewardResult();
            _service.CompleteRewardPresentation();
        }

        private void OnLanguageChanged()
        {
            if (_service.HasActiveOffer)
            {
                _view.ShowCard(_service.CurrentOffer);
            }
        }
    }
}
