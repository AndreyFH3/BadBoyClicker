using System;
using System.Collections.Generic;
using DG.Tweening;
using GameAudio;
using GameLocalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GameSettings
{
    public class SettingsWindow : MonoBehaviour
    {
        private const string AudioEnabledKey = "settings_audio_enabled";

        [Header("Window")]
        [SerializeField] private GameObject _windowRoot;
        [SerializeField] private bool _hideOnAwake = true;
        [SerializeField] private Button _openButton;
        [SerializeField] private Button _closeButton;
        [Min(0f)] [SerializeField] private float _fadeDuration = 0.2f;

        [Header("Sound")]
        [SerializeField] private Button _soundToggleButton;
        [SerializeField] private Image _soundStateText;

        [Header("Language")]
        [SerializeField] private List<LanguageOption> _languageOptions = new();

        [Header("Optional localized labels")]
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _soundLabelText;
        [SerializeField] private TMP_Text _languageLabelText;

        private bool _audioEnabled = true;
        private IAudioService _audioService;
        private CanvasGroup _canvasGroup;
        private Tween _fadeTween;

        [Inject]
        private void Construct(IAudioService audioService)
        {
            _audioService = audioService;
        }

        private void Awake()
        {
            if (_windowRoot == null)
            {
                _windowRoot = gameObject;
            }

            _canvasGroup = GetOrAddCanvasGroup(_windowRoot);

            _audioEnabled = PlayerPrefs.GetInt(AudioEnabledKey, 1) == 1;
            ApplyAudio();
            BindButtons();
            RefreshTexts();

            if (_hideOnAwake)
            {
                HideImmediately();
            }
            else
            {
                SetVisibleState(1f, true);
            }
        }

        private void OnEnable()
        {
            Localization.LanguageChanged += RefreshTexts;
            Localization.ServiceChanged += RefreshTexts;
            RefreshTexts();
        }

        private void OnDisable()
        {
            Localization.LanguageChanged -= RefreshTexts;
            Localization.ServiceChanged -= RefreshTexts;
        }

        private void OnDestroy()
        {
            KillFadeTween();
            UnbindButtons();
        }

        public void Show()
        {
            KillFadeTween();

            bool wasActive = _windowRoot.activeSelf;
            _windowRoot.SetActive(true);
            RefreshTexts();

            if (_canvasGroup == null || _fadeDuration <= 0f)
            {
                SetVisibleState(1f, true);
                return;
            }

            if (!wasActive)
            {
                _canvasGroup.alpha = 0f;
            }

            SetInteraction(false);
            _fadeTween = _canvasGroup
                .DOFade(1f, _fadeDuration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    _fadeTween = null;
                    SetVisibleState(1f, true);
                });
        }

        public void Hide()
        {
            KillFadeTween();
            SetInteraction(false);

            if (!_windowRoot.activeSelf || _canvasGroup == null || _fadeDuration <= 0f)
            {
                HideImmediately();
                return;
            }

            _fadeTween = _canvasGroup
                .DOFade(0f, _fadeDuration)
                .SetEase(Ease.InQuad)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    _fadeTween = null;
                    HideImmediately();
                });
        }

        public void ToggleAudio()
        {
            SetAudioEnabled(!_audioEnabled);
        }

        public void SetRussian()
        {
            SetLanguage("ru");
        }

        public void SetEnglish()
        {
            SetLanguage("en");
        }

        public void SetLanguage(string language)
        {
            Localization.SetLanguage(language);
            RefreshTexts();
        }

        public void SetAudioEnabled(bool enabled)
        {
            _audioEnabled = enabled;
            PlayerPrefs.SetInt(AudioEnabledKey, _audioEnabled ? 1 : 0);
            PlayerPrefs.Save();
            ApplyAudio();
            RefreshTexts();
        }

        private void BindButtons()
        {
            if (_openButton != null)
            {
                _openButton.onClick.AddListener(Show);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(Hide);
            }

            if (_soundToggleButton != null)
            {
                _soundToggleButton.onClick.AddListener(ToggleAudio);
            }

            foreach (LanguageOption option in _languageOptions)
            {
                option.Bind(SetLanguage);
            }
        }

        private void UnbindButtons()
        {
            if (_openButton != null)
            {
                _openButton.onClick.RemoveListener(Show);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveListener(Hide);
            }

            if (_soundToggleButton != null)
            {
                _soundToggleButton.onClick.RemoveListener(ToggleAudio);
            }

            foreach (LanguageOption option in _languageOptions)
            {
                option.Unbind(SetLanguage);
            }
        }

        private void ApplyAudio()
        {
            IAudioService audioService = _audioService ?? AudioServices.Current;

            if (audioService != null)
            {
                audioService.SetEnabled(_audioEnabled);
                return;
            }

            AudioListener.volume = _audioEnabled ? 1f : 0f;
        }

        private void HideImmediately()
        {
            KillFadeTween();
            SetVisibleState(0f, false);
            _windowRoot.SetActive(false);
        }

        private void SetVisibleState(float alpha, bool interactable)
        {
            if (_canvasGroup == null)
            {
                return;
            }

            _canvasGroup.alpha = alpha;
            SetInteraction(interactable);
        }

        private void SetInteraction(bool enabled)
        {
            if (_canvasGroup == null)
            {
                return;
            }

            _canvasGroup.interactable = enabled;
            _canvasGroup.blocksRaycasts = enabled;
        }

        private void KillFadeTween()
        {
            if (_fadeTween == null)
            {
                return;
            }

            _fadeTween.Kill();
            _fadeTween = null;
        }

        private static CanvasGroup GetOrAddCanvasGroup(GameObject target)
        {
            if (target.TryGetComponent(out CanvasGroup canvasGroup))
            {
                return canvasGroup;
            }

            return target.AddComponent<CanvasGroup>();
        }

        private void RefreshTexts()
        {
            if (_titleText != null)
            {
                _titleText.text = Localization.Tr("ui.settings");
            }

            if (_soundLabelText != null)
            {
                _soundLabelText.text = Localization.Tr("settings.sound");
            }

            if (_languageLabelText != null)
            {
                _languageLabelText.text = Localization.Tr("settings.language");
            }

            if (_soundStateText != null)
            {
                _soundStateText.gameObject.SetActive(_audioEnabled);
            }

            foreach (LanguageOption option in _languageOptions)
            {
                option.Refresh(Localization.CurrentLanguage);
            }
        }

        [Serializable]
        private class LanguageOption
        {
            [SerializeField] private string _language = "ru";
            [SerializeField] private Button _button;
            [SerializeField] private TMP_Text _label;
            [SerializeField] private GameObject _selectedMarker;

            private Action<string> _onClick;

            public void Bind(Action<string> onClick)
            {
                if (_button == null)
                {
                    return;
                }

                _onClick = onClick;
                _button.onClick.AddListener(Click);
            }

            public void Unbind(Action<string> onClick)
            {
                if (_button != null)
                {
                    _button.onClick.RemoveListener(Click);
                }

                if (_onClick == onClick)
                {
                    _onClick = null;
                }
            }

            public void Refresh(string currentLanguage)
            {
                bool isSelected = string.Equals(_language, currentLanguage, StringComparison.OrdinalIgnoreCase);

                if (_label != null)
                {
                    _label.text = _language.ToUpperInvariant();
                }

                if (_selectedMarker != null)
                {
                    _selectedMarker.SetActive(isSelected);
                }
            }

            private void Click()
            {
                _onClick?.Invoke(_language);
            }
        }
    }
}
