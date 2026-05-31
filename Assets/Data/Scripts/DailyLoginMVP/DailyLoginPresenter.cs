using System;
using GameLocalization;
using PlayerFeatures;
using Zenject;

namespace DailyLoginMVP
{
    public class DailyLoginPresenter : IInitializable, IDisposable
    {
        private readonly DiContainer _container;
        private readonly DailyLoginModel _model;
        private readonly IDailyLoginView _view;
        private readonly IDailyLoginStartupGate _startupGate;
        private readonly IPlayerFeatureUnlockService _featureUnlockService;
        private bool _isCompleted;
        private bool _isWaitingForUnlock;

        public DailyLoginPresenter(
            DiContainer container,
            DailyLoginModel model,
            IDailyLoginView view,
            IDailyLoginStartupGate startupGate,
            IPlayerFeatureUnlockService featureUnlockService)
        {
            _container = container;
            _model = model;
            _view = view;
            _startupGate = startupGate;
            _featureUnlockService = featureUnlockService;
        }

        public void Initialize()
        {
            _view.ClaimRequested += Claim;
            Localization.LanguageChanged += OnLanguageChanged;

            if (!_featureUnlockService.IsUnlocked(PlayerFeatureType.DailyLoginReward))
            {
                _isWaitingForUnlock = true;
                _featureUnlockService.FeatureUnlocked += OnFeatureUnlocked;
                _startupGate.Complete();
                return;
            }

            TryShowReward();
        }

        private void TryShowReward()
        {
            if (_model.HasReward)
            {
                _view.Show(_model.CreateViewData());
                return;
            }

            Complete();
        }

        public void Dispose()
        {
            _view.ClaimRequested -= Claim;
            Localization.LanguageChanged -= OnLanguageChanged;
            _featureUnlockService.FeatureUnlocked -= OnFeatureUnlocked;
        }

        private void Claim()
        {
            _model.Claim();
            Complete();
        }

        private void OnFeatureUnlocked(PlayerFeatureType feature)
        {
            if (feature != PlayerFeatureType.DailyLoginReward || !_isWaitingForUnlock)
            {
                return;
            }

            _isWaitingForUnlock = false;
            _featureUnlockService.FeatureUnlocked -= OnFeatureUnlocked;
            TryShowReward();
        }

        private void Complete()
        {
            if (_isCompleted)
            {
                return;
            }

            _isCompleted = true;
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
