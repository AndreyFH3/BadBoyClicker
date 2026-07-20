using System;
using DG.Tweening;
using GameLocalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace OfflineIncome
{
    public class OfflineIncomeView : MonoBehaviour, IOfflineIncomeView
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TextMeshProUGUI _rewardText;
        [SerializeField] private TextMeshProUGUI _rewardTextButton;
        [SerializeField] private TextMeshProUGUI _adRewardText;
        [SerializeField] private TextMeshProUGUI _hardCostText;
        [SerializeField] private TextMeshProUGUI _stayDurationText;
        [SerializeField] private Button _claimButton;
        [SerializeField] private Button _claimForHardButton;
        [SerializeField] private Button _claimWithAdButton;
        [Tooltip("Holds the ad-reward preview, the hard-currency cost and the x2/WatchAd buttons. Hidden once one of those offers is claimed, leaving just the reward amount and the claim button.")]
        [SerializeField] private GameObject _doubleOfferRoot;
        [Tooltip("Fade duration for the whole popup on Show()/Hide(). Uses the root CanvasGroup (added automatically if missing).")]
        [SerializeField] private float _fadeDuration = 0.25f;

        private CanvasGroup _rootCanvasGroup;
        private AwaitableCompletionSource _claimAcknowledgedSource;
        private bool _isVisible;

        public event Action ClaimRequested;
        public event Action ClaimForHardRequested;
        public event Action ClaimWithAdRequested;

        private void Awake()
        {
            _rootCanvasGroup = GetOrAddCanvasGroup(Root);
            AddListeners();
            HideImmediate();
        }

        private void OnDestroy()
        {
            RemoveListeners();
            DOTween.Kill(_rootCanvasGroup);
        }

        public async void Show(OfflineIncomeViewData data)
        {
            Root.SetActive(true);
            SetActive(_doubleOfferRoot, true);
            ApplyData(data);

            if (_isVisible)
            {
                // Already on screen (e.g. re-shown after an insufficient-funds or failed-ad
                // retry) - just refresh the data, don't replay the fade-in.
                SetCanvasGroupAlpha(1f);
                return;
            }

            _isVisible = true;
            SetCanvasGroupInteractable(false);
            SetCanvasGroupAlpha(0f);

            await FadeAsync(1f);

            SetCanvasGroupInteractable(true);
        }

        public async void ShowClaimedReward(long finalReward, Action onHidden = null)
        {
            // The offer (ad preview, hard cost, x2/WatchAd buttons) no longer applies once one
            // of them has been claimed - only the updated total and the claim/close button remain.
            SetActive(_doubleOfferRoot, false);

            if (_rewardText != null)
                _rewardText.text = finalReward.ConvertFromLongToString();
            if (_rewardTextButton != null)
                _rewardTextButton.text = $"{Localization.Tr("common.take")} {finalReward.ConvertFromLongToString()}";

            await WaitForClaimAcknowledgedAsync();
            await HideInternalAsync();

            onHidden?.Invoke();
        }

        public async void Hide(Action onHidden = null)
        {
            await HideInternalAsync();

            onHidden?.Invoke();
        }

        public void DestroyView()
        {
            Destroy(gameObject);
        }

        private void ApplyData(OfflineIncomeViewData data)
        {
            if (_rewardText != null)
                _rewardText.text = data.Reward.ConvertFromLongToString();
            if (_rewardTextButton != null)
                _rewardTextButton.text = $"{Localization.Tr("common.take")} {data.Reward.ConvertFromLongToString()}";
            if (_adRewardText != null)
                _adRewardText.text = data.DoubledReward.ConvertFromLongToString();
            if (_hardCostText != null)
                _hardCostText.text = data.HardClaimCost.ToString();
            if (_stayDurationText != null)
                _stayDurationText.text = $"{Localization.Tr("offlie_stay")} {FormatDuration(data.ElapsedSeconds)}";
            if (_claimForHardButton != null)
                _claimForHardButton.interactable = data.CanClaimForHard;
        }

        private static string FormatDuration(long totalSeconds)
        {
            totalSeconds = Math.Max(0, totalSeconds);
            long hours = totalSeconds / 3600;
            long minutes = totalSeconds % 3600 / 60;
            long seconds = totalSeconds % 60;

            string result = string.Empty;
            if (hours > 0)
                result += hours.ToString() + Localization.Format("hours", hours);
            if (minutes > 0)
                result += (result.Length > 0 ? " " : string.Empty) + minutes.ToString() + Localization.Format("minutes");
            if (seconds > 0 || result.Length == 0)
                result += (result.Length > 0 ? " " : string.Empty) +  seconds.ToString() + Localization.Format("seconds", seconds);

            return result;
        }

        private async Awaitable HideInternalAsync()
        {
            _isVisible = false;
            SetCanvasGroupInteractable(false);

            await FadeAsync(0f);

            Root.SetActive(false);
        }

        private void HideImmediate()
        {
            _isVisible = false;
            SetCanvasGroupAlpha(0f);
            SetCanvasGroupInteractable(false);
            Root.SetActive(false);
        }

        private GameObject Root => _root != null ? _root : gameObject;

        private Awaitable FadeAsync(float targetAlpha)
        {
            var completionSource = new AwaitableCompletionSource();

            if (_rootCanvasGroup == null)
            {
                completionSource.SetResult();
                return completionSource.Awaitable;
            }

            // Finish (not abandon) any fade already running so an interrupted Show()/Hide()
            // always resolves its waiter instead of leaving it hanging.
            DOTween.Kill(_rootCanvasGroup, complete: true);

            _rootCanvasGroup.DOFade(targetAlpha, _fadeDuration)
                .SetTarget(_rootCanvasGroup)
                .OnComplete(() => completionSource.SetResult());

            return completionSource.Awaitable;
        }

        private void SetCanvasGroupAlpha(float alpha)
        {
            if (_rootCanvasGroup != null)
                _rootCanvasGroup.alpha = alpha;
        }

        private void SetCanvasGroupInteractable(bool interactable)
        {
            if (_rootCanvasGroup == null)
                return;

            _rootCanvasGroup.interactable = interactable;
            _rootCanvasGroup.blocksRaycasts = interactable;
        }

        private static CanvasGroup GetOrAddCanvasGroup(GameObject go)
        {
            return go.TryGetComponent(out CanvasGroup canvasGroup) ? canvasGroup : go.AddComponent<CanvasGroup>();
        }

        private static void SetActive(GameObject go, bool active)
        {
            if (go != null)
                go.SetActive(active);
        }

        // Reuses the claim button as the "acknowledge and close" control once a doubled
        // reward is on screen, instead of wiring up a separate close button.
        private Awaitable WaitForClaimAcknowledgedAsync()
        {
            _claimAcknowledgedSource = new AwaitableCompletionSource();
            return _claimAcknowledgedSource.Awaitable;
        }

        private void AddListeners()
        {
            if (_claimButton != null)
                _claimButton.onClick.AddListener(RequestClaim);
            if (_claimForHardButton != null)
                _claimForHardButton.onClick.AddListener(RequestClaimForHard);
            if (_claimWithAdButton != null)
                _claimWithAdButton.onClick.AddListener(RequestClaimWithAd);
        }

        private void RemoveListeners()
        {
            if (_claimButton != null)
                _claimButton.onClick.RemoveListener(RequestClaim);
            if (_claimForHardButton != null)
                _claimForHardButton.onClick.RemoveListener(RequestClaimForHard);
            if (_claimWithAdButton != null)
                _claimWithAdButton.onClick.RemoveListener(RequestClaimWithAd);
        }

        private void RequestClaim()
        {
            if (_claimAcknowledgedSource != null)
            {
                AwaitableCompletionSource source = _claimAcknowledgedSource;
                _claimAcknowledgedSource = null;
                source.SetResult();
                return;
            }

            ClaimRequested?.Invoke();
        }

        private void RequestClaimForHard()
        {
            ClaimForHardRequested?.Invoke();
        }

        private void RequestClaimWithAd()
        {
            ClaimWithAdRequested?.Invoke();
        }
    }
}
