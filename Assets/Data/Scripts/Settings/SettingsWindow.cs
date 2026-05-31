using System;
using System.Collections.Generic;
using GameLocalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        [Header("Sound")]
        [SerializeField] private Button _soundToggleButton;
        [SerializeField] private TMP_Text _soundStateText;

        [Header("Language")]
        [SerializeField] private List<LanguageOption> _languageOptions = new();

        [Header("Optional localized labels")]
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _soundLabelText;
        [SerializeField] private TMP_Text _languageLabelText;

        private bool _audioEnabled = true;

        private void Awake()
        {
            if (_windowRoot == null)
            {
                _windowRoot = gameObject;
            }

            _audioEnabled = PlayerPrefs.GetInt(AudioEnabledKey, 1) == 1;
            ApplyAudio();
            BindButtons();
            RefreshTexts();

            if (_hideOnAwake)
            {
                Hide();
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
            UnbindButtons();
        }

        public void Show()
        {
            _windowRoot.SetActive(true);
            RefreshTexts();
        }

        public void Hide()
        {
            _windowRoot.SetActive(false);
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
            AudioListener.volume = _audioEnabled ? 1f : 0f;
        }

        private void RefreshTexts()
        {
            if (_titleText != null)
            {
                _titleText.text = Localization.Tr("ui.settings", "Settings");
            }

            if (_soundLabelText != null)
            {
                _soundLabelText.text = Localization.Tr("settings.sound", "Sound");
            }

            if (_languageLabelText != null)
            {
                _languageLabelText.text = Localization.Tr("settings.language", "Language");
            }

            if (_soundStateText != null)
            {
                _soundStateText.text = Localization.Tr(_audioEnabled ? "common.on" : "common.off", _audioEnabled ? "On" : "Off");
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
