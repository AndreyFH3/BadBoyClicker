using System;
using System.Collections.Generic;
using GameLocalization;
using QuestSystem;
using Zenject;

namespace RewardActivation
{
    public class RewardActivationService : IRewardActivationService, IInitializable, IDisposable
    {
        private readonly IRewardActivationView _view;
        private readonly ILocalizationService _localization;
        private readonly Queue<(QuestReward Reward, Action Grant)> _pending = new();

        private Action _currentGrant;

        public RewardActivationService(IRewardActivationView view, ILocalizationService localization)
        {
            _view = view;
            _localization = localization;
        }

        public void Initialize()
        {
            _view.ActivateRequested += OnActivateRequested;
        }

        public void Dispose()
        {
            _view.ActivateRequested -= OnActivateRequested;
        }

        public void Enqueue(QuestReward reward, Action grantAction)
        {
            if (reward == null || grantAction == null)
            {
                return;
            }

            _pending.Enqueue((reward, grantAction));

            if (_currentGrant == null)
            {
                ShowNext();
            }
        }

        private void OnActivateRequested()
        {
            var grant = _currentGrant;
            _currentGrant = null;
            _view.Hide();

            grant?.Invoke();

            ShowNext();
        }

        private void ShowNext()
        {
            if (_pending.Count == 0)
            {
                _currentGrant = null;
                return;
            }

            var (reward, grant) = _pending.Dequeue();
            _currentGrant = grant;

            var data = new RewardActivationViewData(
                reward.Icon,
                LocalizeOrFallback(reward.ActivationTitleLocalizationKey, reward.ActivationTitle),
                LocalizeOrFallback(reward.ActivationDescriptionLocalizationKey, reward.ActivationDescription));

            _view.Show(data);
        }

        private string LocalizeOrFallback(string key, string fallback)
        {
            if (string.IsNullOrEmpty(key))
            {
                return fallback;
            }

            string localized = _localization.Localize(key);
            return string.IsNullOrEmpty(localized) || localized == key ? fallback : localized;
        }
    }
}
