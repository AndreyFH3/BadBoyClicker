using DG.Tweening;
using DailyQuests;
using GameLocalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace DailyQuestMVP
{
    public class DailyQuestViewElement : MonoBehaviour
    {
        private enum CardState
        {
            InProgress,
            Completed,
            Claimed
        }

        private const string PointsSuffixKey = "daily_quest.points_suffix";
        private const string ClaimButtonKey = "daily_quest.claim_button";
        private const string ClaimedButtonKey = "daily_quest.claimed_button";

        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _description;
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField] private TextMeshProUGUI _pointsText;
        [SerializeField] private Image _icon;
        [SerializeField] private Image _progressFill;
        [SerializeField] private GameObject _completedMarker;
        [SerializeField] private Button _claimPointsButton;
        [SerializeField] private TextMeshProUGUI _claimPointsButtonText;

        [Header("State Roots")]
        [SerializeField] private GameObject _inProgressStateRoot;
        [SerializeField] private GameObject _completedStateRoot;
        [SerializeField] private GameObject _claimedStateRoot;

        [SerializeField] private float _fillDuration = 0.2f;

        private Tween _fillTween;
        private string _id;
        public event Action<string> ClaimPointsRequested;

        private void Awake()
        {
            if (_claimPointsButton != null)
            {
                _claimPointsButton.onClick.AddListener(RequestClaimPoints);
            }
        }

        private void OnDestroy()
        {
            _fillTween?.Kill();
            if (_claimPointsButton != null)
            {
                _claimPointsButton.onClick.RemoveListener(RequestClaimPoints);
            }
        }

        public void SetData(DailyQuestViewData data)
        {
            _id = data.Id;

            if (_title != null)
            {
                _title.text = data.Title;
            }

            if (_description != null)
            {
                _description.text = data.Description;
            }

            if (_icon != null)
            {
                _icon.sprite = data.Icon;
                _icon.enabled = data.Icon != null;
            }

            if (_progressText != null)
            {
                _progressText.text = data.ProgressText;
            }

            if (_pointsText != null)
            {
                _pointsText.text = $"+{data.Points} {Localization.Tr(PointsSuffixKey)}";
            }

            ApplyState(GetState(data));

            if (_progressFill != null)
            {
                _fillTween?.Kill();
                _fillTween = _progressFill
                    .DOFillAmount(Mathf.Clamp01(data.Progress), _fillDuration)
                    .SetEase(Ease.OutQuad);
            }
        }

        private static CardState GetState(DailyQuestViewData data)
        {
            if (data.IsPointsClaimed)
            {
                return CardState.Claimed;
            }

            return data.CanClaimPoints ? CardState.Completed : CardState.InProgress;
        }

        private void ApplyState(CardState state)
        {
            if (_inProgressStateRoot != null)
            {
                _inProgressStateRoot.SetActive(state == CardState.InProgress);
            }

            if (_completedStateRoot != null)
            {
                _completedStateRoot.SetActive(state == CardState.Completed);
            }

            if (_claimedStateRoot != null)
            {
                _claimedStateRoot.SetActive(state == CardState.Claimed);
            }

            if (_completedMarker != null)
            {
                _completedMarker.SetActive(state == CardState.Claimed);
            }

            if (_claimPointsButton != null)
            {
                bool showButton = state != CardState.InProgress;
                _claimPointsButton.gameObject.SetActive(showButton);
                _claimPointsButton.interactable = state == CardState.Completed;
            }

            if (_claimPointsButtonText != null)
            {
                _claimPointsButtonText.text = state == CardState.Claimed
                    ? Localization.Tr(ClaimedButtonKey)
                    : Localization.Tr(ClaimButtonKey);
            }
        }

        private void RequestClaimPoints()
        {
            ClaimPointsRequested?.Invoke(_id);
        }
    }
}
