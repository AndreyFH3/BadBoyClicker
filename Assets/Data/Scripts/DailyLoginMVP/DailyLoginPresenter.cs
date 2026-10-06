using System;
using Core.Ads;
using GameLocalization;
using Zenject;

namespace DailyLoginMVP
{
    public class DailyLoginPresenter : IInitializable, IDisposable
    {
        private readonly DiContainer _container;
        private readonly DailyLoginModel _model;
        private readonly IDailyLoginView _view;
        private readonly IDailyLoginStartupGate _startupGate;
        private readonly IRewardOfferUiGate _rewardOfferUiGate;
        private bool _isCompleted;

        public DailyLoginPresenter(
            DiContainer container,
            DailyLoginModel model,
            IDailyLoginView view,
            IDailyLoginStartupGate startupGate,
            IRewardOfferUiGate rewardOfferUiGate)
        {
            _container = container;
            _model = model;
            _view = view;
            _startupGate = startupGate;
            _rewardOfferUiGate = rewardOfferUiGate;
        }

        public void Initialize()
        {
            _view.ClaimRequested += Claim;
            Localization.LanguageChanged += OnLanguageChanged;

            TryShowReward();
        }

        private void TryShowReward()
        {
            if (_model.HasReward)
            {
                _rewardOfferUiGate.Block(this);
                _view.Show(_model.CreateViewData());
                return;
            }

            Complete();
        }

        public void Dispose()
        {
            _view.ClaimRequested -= Claim;
            Localization.LanguageChanged -= OnLanguageChanged;
        }

        private void Claim()
        {
            _model.Claim();
            Complete();
        }

        private void Complete()
        {
            if (_isCompleted)
            {
                return;
            }

            _isCompleted = true;
            _rewardOfferUiGate.Unblock(this);
            _view.Hide();
            Dispose();

            _container.Unbind<DailyLoginPresenter>();
            _container.Unbind<DailyLoginModel>();
            _startupGate.Complete();
        }

        private void OnLanguageChanged()
        {
            if (!_isCompleted && _model.HasReward)
            {
                _view.Show(_model.CreateViewData());
            }
        }
    }
}
