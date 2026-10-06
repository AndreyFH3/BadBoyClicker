using System;
using DailyQuests;
using GameLocalization;
using Zenject;

namespace DailyQuestMVP
{
    public class DailyQuestPresenter : IInitializable, IDisposable
    {
        private IDailyQuestService _service;
        private IDailyQuestView _view;

        [Inject]
        public void Construct(
            IDailyQuestService service,
            IDailyQuestView view)
        {
            _service = service;
            _view = view;
        }

        public void Initialize()
        {
            _service.Changed += UpdateView;
            _view.ClaimQuestPointsRequested += OnClaimQuestPointsRequested;
            _view.ClaimMilestoneRequested += OnClaimMilestoneRequested;
            Localization.LanguageChanged += UpdateView;
            UpdateView();
        }

        public void Dispose()
        {
            _service.Changed -= UpdateView;
            _view.ClaimQuestPointsRequested -= OnClaimQuestPointsRequested;
            _view.ClaimMilestoneRequested -= OnClaimMilestoneRequested;
            Localization.LanguageChanged -= UpdateView;
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
    }
}
