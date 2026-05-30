using System;
using DailyQuests;
using PlayerFeatures;
using Zenject;

namespace DailyQuestMVP
{
    public class DailyQuestPresenter : IInitializable, IDisposable
    {
        private IDailyQuestService _service;
        private IDailyQuestView _view;
        private IPlayerFeatureUnlockService _featureUnlockService;
        private bool _isUnlocked;

        [Inject]
        public void Construct(
            IDailyQuestService service,
            IDailyQuestView view,
            IPlayerFeatureUnlockService featureUnlockService)
        {
            _service = service;
            _view = view;
            _featureUnlockService = featureUnlockService;
        }

        public void Initialize()
        {
            _isUnlocked = _featureUnlockService.IsUnlocked(PlayerFeatureType.DailyQuest);
            if (!_isUnlocked)
            {
                _featureUnlockService.FeatureUnlocked += OnFeatureUnlocked;
                return;
            }

            StartUnlockedFlow();
        }

        public void Dispose()
        {
            _service.Changed -= UpdateView;
            _view.ClaimQuestPointsRequested -= OnClaimQuestPointsRequested;
            _view.ClaimMilestoneRequested -= OnClaimMilestoneRequested;
            _featureUnlockService.FeatureUnlocked -= OnFeatureUnlocked;
        }

        private void StartUnlockedFlow()
        {
            _service.Changed += UpdateView;
            _view.ClaimQuestPointsRequested += OnClaimQuestPointsRequested;
            _view.ClaimMilestoneRequested += OnClaimMilestoneRequested;
            UpdateView();
        }

        private void UpdateView()
        {
            _view.SetData(_service.GetViewData());
        }

        private void OnClaimQuestPointsRequested(string questId)
        {
            if (_service.ClaimQuestPoints(questId))
            {
                UpdateView();
            }
        }

        private void OnClaimMilestoneRequested(int requiredPoints)
        {
            if (_service.ClaimMilestone(requiredPoints))
            {
                UpdateView();
            }
        }

        private void OnFeatureUnlocked(PlayerFeatureType feature)
        {
            if (feature != PlayerFeatureType.DailyQuest || _isUnlocked)
            {
                return;
            }

            _isUnlocked = true;
            _featureUnlockService.FeatureUnlocked -= OnFeatureUnlocked;
            StartUnlockedFlow();
        }
    }
}
