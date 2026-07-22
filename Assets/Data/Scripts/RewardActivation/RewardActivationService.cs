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
        private readonly Queue<(RewardActivationViewData Data, Action Grant, Action Postpone)> _pending = new();

        private Action _currentGrant;
        private Action _currentPostpone;

        public RewardActivationService(IRewardActivationView view, ILocalizationService localization)
        {
            _view = view;
            _localization = localization;
        }

        public void Initialize()
        {
            _view.ActivateRequested += OnActivateRequested;
            _view.PostponeRequested += OnPostponeRequested;
        }

        public void Dispose()
        {
            _view.ActivateRequested -= OnActivateRequested;
            _view.PostponeRequested -= OnPostponeRequested;
        }

        public void Enqueue(QuestReward reward, Action grantAction, Action postponeAction = null)
        {
            if (reward == null || grantAction == null)
            {
                return;
            }

            var data = new RewardActivationViewData(
                reward.Icon,
                ResolveTitle(reward),
                ResolveDescription(reward),
                _localization.Localize("common.take"),
                _localization.Localize("common.later"),
                postponeAction != null);

            EnqueueInternal(data, grantAction, postponeAction);
        }

        public void Enqueue(RewardActivationViewData data, Action closeAction = null)
        {
            EnqueueInternal(data, closeAction ?? (() => { }), null);
        }

        private void EnqueueInternal(RewardActivationViewData data, Action grantAction, Action postponeAction)
        {
            _pending.Enqueue((data, grantAction, postponeAction));

            if (_currentGrant == null)
            {
                ShowNext();
            }
        }

        private void OnActivateRequested()
        {
            var grant = _currentGrant;
            _currentGrant = null;
            _currentPostpone = null;
            _view.Hide();

            grant?.Invoke();

            ShowNext();
        }

        private void OnPostponeRequested()
        {
            var postpone = _currentPostpone;
            _currentGrant = null;
            _currentPostpone = null;
            _view.Hide();

            postpone?.Invoke();
            ShowNext();
        }

        private void ShowNext()
        {
            if (_pending.Count == 0)
            {
                _currentGrant = null;
                return;
            }

            var (data, grant, postpone) = _pending.Dequeue();
            _currentGrant = grant;
            _currentPostpone = postpone;

            _view.Show(data);
        }

        private string ResolveTitle(QuestReward reward)
        {
            string activationTitle = LocalizeOrFallback(reward.ActivationTitleLocalizationKey, reward.ActivationTitle);
            if (!string.IsNullOrEmpty(activationTitle))
            {
                return activationTitle;
            }

            string displayTitle = LocalizeOrFallback(reward.DisplayTextLocalizationKey, reward.DisplayText);
            return string.IsNullOrEmpty(displayTitle)
                ? _localization.Localize("daily_quest.milestone.reward.title")
                : displayTitle;
        }

        private string ResolveDescription(QuestReward reward)
        {
            string description = LocalizeOrFallback(reward.ActivationDescriptionLocalizationKey, reward.ActivationDescription);
            if (!string.IsNullOrEmpty(description))
            {
                return description;
            }

            string rewardName = LocalizeOrFallback(reward.DisplayTextLocalizationKey, reward.DisplayText);
            if (string.IsNullOrEmpty(rewardName))
            {
                rewardName = reward.RewardType == QuestRewardType.Currency && reward.Amount > 0
                    ? reward.Amount.ToString()
                    : reward.RewardId;
            }

            return _localization.Format("daily_quest.milestone.reward.claim_description", rewardName);
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
