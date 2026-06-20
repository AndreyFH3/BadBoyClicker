using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shop
{
    public class ShopPurchaseConfirmationView : MonoBehaviour, IShopPurchaseConfirmationView
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;

        public event Action ConfirmRequested;
        public event Action CancelRequested;

        private void Awake()
        {
            AddListeners();
            Hide();
        }

        private void OnDestroy()
        {
            RemoveListeners();
        }

        public void Show(ShopPurchaseConfirmationData data)
        {
            if (_descriptionText != null)
                _descriptionText.text = data?.Description ?? string.Empty;

            Root.SetActive(true);
        }

        public void Hide()
        {
            Root.SetActive(false);
        }

        private GameObject Root => _root != null ? _root : gameObject;

        private void AddListeners()
        {
            if (_confirmButton != null)
                _confirmButton.onClick.AddListener(RequestConfirm);
            if (_cancelButton != null)
                _cancelButton.onClick.AddListener(RequestCancel);
        }

        private void RemoveListeners()
        {
            if (_confirmButton != null)
                _confirmButton.onClick.RemoveListener(RequestConfirm);
            if (_cancelButton != null)
                _cancelButton.onClick.RemoveListener(RequestCancel);
        }

        private void RequestConfirm()
        {
            ConfirmRequested?.Invoke();
        }

        private void RequestCancel()
        {
            CancelRequested?.Invoke();
        }
    }
}
