using System;
using System.Collections.Generic;
using DailyQuests;
using Rewards;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DailyQuestMVP
{
    public class DailyQuestMilestoneViewElement : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _pointsText;

        [Header("Reward")]
        [SerializeField] private Transform _rewardsRoot;
        [SerializeField] private RewardView _rewardReference;

        [Header("State")]
        [SerializeField] private Button _claimButton;
        [Tooltip("Shown once the reward has been collected (e.g. a checkmark).")]
        [SerializeField] private GameObject _claimedMarker;
        [Tooltip("Shown when the reward is ready to be collected, to draw attention.")]
        [SerializeField] private GameObject _claimAvailableMarker;

        private readonly List<RewardView> _rewardViews = new();
        private int _requiredPoints;

        public event Action<int> ClaimRequested;

        private void Awake()
        {
            if (_rewardReference != null)
            {
                _rewardReference.gameObject.SetActive(false);
            }

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

            // Hide the reward once it has been collected: the claimed marker
            // communicates the state on its own.
            RebuildRewards(data.IsClaimed ? null : data.Rewards);

            if (_claimButton != null)
            {
                _claimButton.interactable = data.CanClaim;
            }

            if (_claimedMarker != null)
            {
                _claimedMarker.SetActive(data.IsClaimed);
            }

            if (_claimAvailableMarker != null)
            {
                _claimAvailableMarker.SetActive(data.CanClaim);
            }
        }

        private void RebuildRewards(IReadOnlyList<RewardDisplay> rewards)
        {
            if (_rewardsRoot == null || _rewardReference == null)
            {
                return;
            }

            int count = rewards?.Count ?? 0;

            for (int i = 0; i < _rewardViews.Count; i++)
            {
                _rewardViews[i].gameObject.SetActive(i < count);
            }

            for (int i = 0; i < count; i++)
            {
                RewardView view;
                if (i < _rewardViews.Count)
                {
                    view = _rewardViews[i];
                }
                else
                {
                    view = Instantiate(_rewardReference, _rewardsRoot);
                    _rewardViews.Add(view);
                }

                view.gameObject.SetActive(true);
                view.Set(rewards[i]);
            }
        }

        private void RequestClaim()
        {
            ClaimRequested?.Invoke(_requiredPoints);
        }
    }
}
