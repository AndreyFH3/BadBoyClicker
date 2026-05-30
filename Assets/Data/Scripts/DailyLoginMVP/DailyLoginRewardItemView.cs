using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DailyLoginMVP
{
    public class DailyLoginRewardItemView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private RectTransform _currentRewardMarker;

        public void SetData(DailyLoginRewardViewData data)
        {
            if (_icon != null)
            {
                _icon.sprite = data.Icon;
                _icon.enabled = data.Icon != null;
            }

            if (_text != null)
            {
                _text.text = data.Text;
                _text.gameObject.SetActive(!string.IsNullOrEmpty(data.Text));
            }

            if (_currentRewardMarker != null)
            {
                _currentRewardMarker.gameObject.SetActive(data.IsCurrent);
            }
        }
    }
}
