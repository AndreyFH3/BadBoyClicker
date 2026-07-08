using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DailyLoginMVP
{
    public class DailyLoginRewardItemView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private TextMeshProUGUI _dayText;
        [SerializeField] private RectTransform _currentRewardMarker;
        [SerializeField] private GameObject _lockedMarker;
        [SerializeField] private GameObject _claimedMarker;
        [SerializeField] private GameObject _milestoneMarker;

        public void SetData(DailyLoginRewardViewData data)
        {
            if (_icon != null)
            {
                _icon.sprite = data.Icon;
                _icon.enabled = data.Icon != null;
            }

            if (_text != null)
            {
                bool showText = data.IsResource && !string.IsNullOrEmpty(data.Text);
                _text.text = data.Text;
                _text.gameObject.SetActive(showText);
            }

            if (_dayText != null)
            {
                _dayText.text = data.DayText;
                _dayText.gameObject.SetActive(!string.IsNullOrEmpty(data.DayText));
            }

            if (_currentRewardMarker != null)
            {
                _currentRewardMarker.gameObject.SetActive(data.IsCurrent);
            }

            if (_lockedMarker != null)
            {
                _lockedMarker.SetActive(data.IsLocked);
            }

            if (_claimedMarker != null)
            {
                _claimedMarker.SetActive(data.IsClaimed);
            }

            if (_milestoneMarker != null)
            {
                _milestoneMarker.SetActive(data.IsMilestone);
            }
        }
    }
}
