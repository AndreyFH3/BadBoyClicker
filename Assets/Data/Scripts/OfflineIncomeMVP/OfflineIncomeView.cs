using System;
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

        public event Action ClaimRequested;
        public event Action ClaimForHardRequested;
        public event Action ClaimWithAdRequested;

        private void Awake()
        {
            AddListeners();
            Hide();
        }

        private void OnDestroy()
        {
            RemoveListeners();
        }

        public void Show(OfflineIncomeViewData data)
        {
            Root.SetActive(true);

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

        public void Hide()
        {
            Root.SetActive(false);
        }

        private GameObject Root => _root != null ? _root : gameObject;

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
