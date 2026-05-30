using CardCollections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardCollectionMVP
{
    public class CardItemView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _description;
        [SerializeField] private TextMeshProUGUI _starsText;
        [SerializeField] private TextMeshProUGUI _amountText;
        [SerializeField] private Image _progressFill;
        [SerializeField] private GameObject _collectedMarker;
        [SerializeField] private CanvasGroup _lockedCanvasGroup;
        [SerializeField] private float _lockedAlpha = 0.45f;

        public void SetData(CardViewData data)
        {
            if (data == null)
            {
                return;
            }

            if (_icon != null)
            {
                _icon.sprite = data.Icon;
                _icon.enabled = data.Icon != null;
            }

            if (_title != null)
            {
                _title.text = data.Title;
            }

            if (_description != null)
            {
                _description.text = data.Description;
            }

            if (_starsText != null)
            {
                _starsText.text = data.Stars.ToString();
            }

            if (_amountText != null)
            {
                _amountText.text = $"{data.CurrentAmount}/{data.RequiredAmount}";
            }

            if (_progressFill != null)
            {
                _progressFill.fillAmount = data.RequiredAmount > 0
                    ? Mathf.Clamp01((float)data.CurrentAmount / data.RequiredAmount)
                    : 0f;
            }

            if (_collectedMarker != null)
            {
                _collectedMarker.SetActive(data.IsCollected);
            }

            if (_lockedCanvasGroup != null)
            {
                _lockedCanvasGroup.alpha = data.IsCollected ? 1f : Mathf.Clamp01(_lockedAlpha);
            }
        }
    }
}
