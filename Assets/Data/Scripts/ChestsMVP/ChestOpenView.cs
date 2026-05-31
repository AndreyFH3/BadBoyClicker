using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChestsMVP
{
    public class ChestOpenView : MonoBehaviour, IChestOpenView
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Image _chestIcon;
        [SerializeField] private TextMeshProUGUI _chestTitle;
        [SerializeField] private Image _rewardIcon;
        [SerializeField] private TextMeshProUGUI _rewardText;
        [SerializeField] private Button _closeButton;

        public event Action CloseRequested;

        private void Awake()
        {
            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(RequestClose);
            }

            Hide();
        }

        private void OnDestroy()
        {
            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveListener(RequestClose);
            }
        }

        public void Show(ChestOpenViewData data)
        {
            Root.SetActive(true);

            SetImage(_chestIcon, data?.ChestIcon);
            SetText(_chestTitle, data?.ChestTitle);
            SetImage(_rewardIcon, data?.RewardIcon);
            SetText(_rewardText, data?.RewardText);
        }

        public void Hide()
        {
            Root.SetActive(false);
        }

        private GameObject Root => _root != null ? _root : gameObject;

        private static void SetImage(Image image, Sprite sprite)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = sprite;
            image.enabled = sprite != null;
        }

        private static void SetText(TextMeshProUGUI text, string value)
        {
            if (text == null)
            {
                return;
            }

            text.text = value ?? string.Empty;
            text.gameObject.SetActive(!string.IsNullOrEmpty(value));
        }

        private void RequestClose()
        {
            CloseRequested?.Invoke();
        }
    }
}
