using System;
using DailyQuests;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DailyQuestMVP
{
    public class DailyQuestMilestoneViewElement : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _pointsText;
        [SerializeField] private TextMeshProUGUI _rewardText;
        [SerializeField] private Button _claimButton;
        [SerializeField] private GameObject _claimedMarker;
        [SerializeField] private GameObject _lockedMarker;

        private int _requiredPoints;
        public event Action<int> ClaimRequested;

        private void Awake()
        {
            if (_claimButton != null)
            {
                _claimButton.onClick.AddListener(RequestClaim);
            }
        }

        private void OnDestroy()
        {
            if (_claimButton != null)
            {
                _claimButton.onClick.RemoveListener(RequestClaim);
            }
        }

        public void SetData(DailyQuestMilestoneViewData data)
        {
            _requiredPoints = data.RequiredPoints;

            if (_pointsText != null)
            {
                _pointsText.text = data.RequiredPoints.ToString();
            }

            if (_rewardText != null)
            {
                _rewardText.text = data.RewardText;
            }

            if (_claimButton != null)
            {
                _claimButton.gameObject.SetActive(!data.IsClaimed);
                _claimButton.interactable = data.IsUnlocked && !data.IsClaimed;
            }

            if (_claimedMarker != null)
            {
                _claimedMarker.SetActive(data.IsClaimed);
            }

            if (_lockedMarker != null)
            {
                _lockedMarker.SetActive(!data.IsUnlocked && !data.IsClaimed);
            }
        }

        private void RequestClaim()
        {
            ClaimRequested?.Invoke(_requiredPoints);
        }
    }
}
