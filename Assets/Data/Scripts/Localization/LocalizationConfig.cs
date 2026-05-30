using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameLocalization
{
    [CreateAssetMenu(fileName = "LocalizationConfig", menuName = "Configs/Localization")]
    public class LocalizationConfig : ScriptableObject
    {
        [SerializeField] private string _defaultLanguage = "ru";
        [SerializeField] private string _currentLanguage = "ru";
        [SerializeField] private List<LocalizationEntry> _entries = new();

        public string DefaultLanguage => string.IsNullOrWhiteSpace(_defaultLanguage) ? "ru" : _defaultLanguage;
        public string CurrentLanguage => string.IsNullOrWhiteSpace(_currentLanguage) ? DefaultLanguage : _currentLanguage;
        public IReadOnlyList<LocalizationEntry> Entries => _entries;

        [Serializable]
        public class LocalizationEntry
        {
            [SerializeField] private string _key;
            [SerializeField] private List<LocalizedValue> _values = new();

            public string Key => _key;
            public IReadOnlyList<LocalizedValue> Values => _values;
        }

        [Serializable]
        public class LocalizedValue
        {
            [SerializeField] private string _language = "ru";
            [TextArea]
            [SerializeField] private string _text;

            public string Language => _language;
            public string Text => _text;
        }
    }
}
