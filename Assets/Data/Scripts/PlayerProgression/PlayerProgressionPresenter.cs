using System;
using GameLocalization;
using Zenject;

namespace PlayerProgression
{
    public class PlayerProgressionPresenter : IInitializable, IDisposable
    {
        private IPlayerProgressionService _service;
        private IPlayerProgressionView _view;

        [Inject]
        public void Construct(IPlayerProgressionService service, IPlayerProgressionView view)
        {
            _service = service;
            _view = view;
        }

        public void Initialize()
        {
            _service.Changed += UpdateView;
            _service.ExperienceAdded += OnExperienceAdded;
            _view.NewLevelRequested += OnNewLevelRequested;
            Localization.LanguageChanged += UpdateView;
            UpdateView();
        }

        public void Dispose()
        {
            _service.Changed -= UpdateView;
            _service.ExperienceAdded -= OnExperienceAdded;
            _view.NewLevelRequested -= OnNewLevelRequested;
            Localization.LanguageChanged -= UpdateView;
        }

        private void UpdateView()
        {
            _view.UpdateState(
                _service.CurrentLevel,
                _service.CurrentExperience,
                _service.ExperienceToNextLevel,
                _service.CurrentProgress);
            _view.SetNewLevelAvailable(_service.CanCompleteLevel);
        }

        private void OnExperienceAdded(long amount)
        {
            _view.ShowAddedExperience(amount);
        }

        private void OnNewLevelRequested()
        {
            if (!_service.CanCompleteLevel)
            {
                return;
            }

            _view.ShowLevelUpOffer(ConfirmLevelUp, _service.NextLevelRewardDescription, _service.NextLevelRewardIcon);
        }

        private void ConfirmLevelUp()
        {
            string rewardDescription = _service.NextLevelRewardDescription;
            var rewardIcon = _service.NextLevelRewardIcon;

            if (_service.CompleteLevel())
            {
                _view.ShowLevelUpResult(rewardDescription, rewardIcon);
            }
        }
    }
}
