using System;
using CardCollections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardCollectionMVP
{
    public class CardCollectionItemView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _description;
        [SerializeField] private TextMeshProUGUI _cardsProgressText;
        [SerializeField] private TextMeshProUGUI _starsProgressText;
        [SerializeField] private Image _progressFill;
        [SerializeField] private GameObject _completedMarker;
        [SerializeField] private GameObject _rewardClaimedMarker;
        [SerializeField] private Button _selectButton;

        private string _id;

        public event Action<string> Selected;

        private void Awake()
        {
            if (_selectButton != null)
            {
                _selectButton.onClick.AddListener(RequestSelect);
            }
        }

        private void OnDestroy()
        {
            if (_selectButton != null)
            {
                _selectButton.onClick.RemoveListener(RequestSelect);
            }
        }

        public void SetData(CardCollectionViewData data)
        {
            if (data == null)
            {
                return;
            }

            _id = data.Id;

            if (_title != null)
            {
                _title.text = data.Title;
            }

            if (_description != null)
            {
                _description.text = data.Description;
            }

            if (_cardsProgressText != null)
            {
                _cardsProgressText.text = $"{data.CollectedCards}/{data.TotalCards}";
            }

            if (_starsProgressText != null)
            {
                _starsProgressText.text = $"{data.CollectedStars}/{data.TotalStars}";
            }

            if (_progressFill != null)
            {
                _progressFill.fillAmount = data.TotalCards > 0
                    ? Mathf.Clamp01((float)data.CollectedCards / data.TotalCards)
                    : 0f;
            }

            if (_completedMarker != null)
            {
                _completedMarker.SetActive(data.IsCompleted);
            }

            if (_rewardClaimedMarker != null)
            {
                _rewardClaimedMarker.SetActive(data.IsRewardClaimed);
            }
        }

        private void RequestSelect()
        {
            Selected?.Invoke(_id);
        }
    }
}
