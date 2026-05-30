using DG.Tweening;
using DailyQuests;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace DailyQuestMVP
{
    public class DailyQuestViewElement : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _description;
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField] private TextMeshProUGUI _pointsText;
        [SerializeField] private Image _progressFill;
        [SerializeField] private GameObject _completedMarker;
        [SerializeField] private Button _claimPointsButton;
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

            if (_progressText != null)
            {
                _progressText.text = data.ProgressText;
            }

            if (_pointsText != null)
            {
                _pointsText.text = $"+{data.Points}";
            }

            if (_completedMarker != null)
            {
                _completedMarker.SetActive(data.IsPointsClaimed);
            }

            if (_claimPointsButton != null)
            {
                _claimPointsButton.gameObject.SetActive(data.CanClaimPoints);
                _claimPointsButton.interactable = data.CanClaimPoints;
            }

            if (_progressFill != null)
            {
                _fillTween?.Kill();
                _fillTween = _progressFill
                    .DOFillAmount(Mathf.Clamp01(data.Progress), _fillDuration)
                    .SetEase(Ease.OutQuad);
            }
        }

        private void RequestClaimPoints()
        {
            ClaimPointsRequested?.Invoke(_id);
        }
    }
}
