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
        [SerializeField] private TextMeshProUGUI _starsText;
        [SerializeField] private GameObject _lockedIndicator;

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

            if (_starsText != null)
            {
                _starsText.text = data.Stars.ToString();
            }

            if (_lockedIndicator != null)
            {
                _lockedIndicator.SetActive(!data.IsCollected);
            }
        }
    }
}
