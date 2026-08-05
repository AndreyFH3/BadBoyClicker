using System;
using GameLocalization;
using TMPro;
using UIAnimations;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    public sealed class StartupLoadingView : MonoBehaviour
    {
        public event Action StartRequested;

        [Tooltip("Container with the loading status only. It must not contain the Start button.")]
        [SerializeField] private GameObject _loadingStateRoot;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private Button _startButton;
        [SerializeField] private TextMeshProUGUI _buttonText;
        [SerializeField] private CanvasGroupFade _fade;

        private bool _canStart;
        private bool _isClosing;

        private void Awake()
        {
            if (_fade == null)
            {
                _fade = GetComponent<CanvasGroupFade>();
            }

            if (_startButton != null)
            {
                _startButton.onClick.AddListener(StartGame);
                _startButton.gameObject.SetActive(false);
            }

            if (_loadingStateRoot != null)
            {
                _loadingStateRoot.SetActive(true);
            }

            Localization.ServiceChanged += RefreshTexts;
            Localization.LanguageChanged += RefreshTexts;
            RefreshTexts();
        }

        private void OnDestroy()
        {
            Localization.ServiceChanged -= RefreshTexts;
            Localization.LanguageChanged -= RefreshTexts;

            if (_startButton != null)
            {
                _startButton.onClick.RemoveListener(StartGame);
            }
        }

        public void AllowStart()
        {
            if (_isClosing)
            {
                return;
            }

            _canStart = true;
            if (_loadingStateRoot != null)
            {
                _loadingStateRoot.SetActive(false);
            }

            if (_startButton != null)
            {
                _startButton.gameObject.SetActive(true);
                _startButton.interactable = true;
            }

            RefreshTexts();
        }

        private void StartGame()
        {
            if (!_canStart || _isClosing)
            {
                return;
            }

            _isClosing = true;
            _startButton.interactable = false;
            StartRequested?.Invoke();

            if (_fade != null)
            {
                _fade.Hide(() => Destroy(gameObject));
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void RefreshTexts()
        {
            bool isRussian = string.Equals(Localization.CurrentLanguage, "ru", StringComparison.OrdinalIgnoreCase);

            if (_statusText != null)
            {
                _statusText.text = isRussian
                    ? "\u041f\u043e\u043b\u0443\u0447\u0435\u043d\u0438\u0435 \u0434\u0430\u043d\u043d\u044b\u0445..."
                    : "Receiving data...";
            }

            if (_buttonText != null)
            {
                _buttonText.text = isRussian
                    ? "\u041d\u0430\u0447\u0430\u0442\u044c \u0438\u0433\u0440\u0443"
                    : "Start game";
            }
        }

    }
}
