using CardCollections;
using UnityEngine;
using UnityEngine.UI;

namespace CardCollectionMVP
{
    public class CardStateView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private GameObject _missingIndicator;

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

            if (_missingIndicator != null)
            {
                _missingIndicator.SetActive(!data.IsCollected);
            }
        }
    }
}
