using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameLocalization
{
    public class LocalizationService : ILocalizationService
    {
        private readonly LocalizationConfig _config;
        private readonly Dictionary<string, Dictionary<string, string>> _values = new();
        private bool _isBuilt;

        public string CurrentLanguage => _config?.CurrentLanguage ?? "ru";

        public LocalizationService(LocalizationConfig config)
        {
            _config = config;
            Rebuild();
        }

        public string Localize(string key, string fallback = null)
        {
            if (string.IsNullOrEmpty(key))
            {
                return fallback ?? string.Empty;
            }

            EnsureBuilt();

            if (_values.TryGetValue(key, out var translations))
            {
                if (translations.TryGetValue(CurrentLanguage, out string value) && !string.IsNullOrEmpty(value))
                {
                    return value;
                }

                string defaultLanguage = _config?.DefaultLanguage ?? CurrentLanguage;
                if (translations.TryGetValue(defaultLanguage, out value) && !string.IsNullOrEmpty(value))
                {
                    return value;
                }
            }

            if (TryRebuildAndLocalize(key, out string rebuiltValue))
            {
                return rebuiltValue;
            }

            if (!_values.ContainsKey(key))
            {
                Debug.LogError($"key <color=red>{key}</color> is not in Dictionary!");
            }

            return fallback ?? key;
        }

        public string Format(string key, string fallback, params object[] args)
        {
            string format = Localize(key, fallback);
            return args == null || args.Length == 0 ? format : string.Format(format, args);
        }

        private void Rebuild()
        {
            _values.Clear();

            var entries = _config?.Entries;
            if (entries == null)
            {
                _isBuilt = true;
                return;
            }

            foreach (var entry in entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.Key))
                {
                    continue;
                }

                if (!_values.TryGetValue(entry.Key, out var translations))
                {
                    translations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    _values.Add(entry.Key, translations);
                }

                var values = entry.Values;
                if (values == null)
                {
                    continue;
                }

                foreach (var value in values)
                {
                    if (value == null || string.IsNullOrEmpty(value.Language))
                    {
                        continue;
                    }

                    translations[value.Language] = value.Text;
                }
            }

            _isBuilt = true;
        }

        private void EnsureBuilt()
        {
            if (_isBuilt && _values.Count > 0)
            {
                return;
            }

            Rebuild();
        }

        private bool TryRebuildAndLocalize(string key, out string value)
        {
            value = null;
            Rebuild();

            if (!_values.TryGetValue(key, out var translations))
            {
                return false;
            }

            if (translations.TryGetValue(CurrentLanguage, out value) && !string.IsNullOrEmpty(value))
            {
                return true;
            }

            string defaultLanguage = _config?.DefaultLanguage ?? CurrentLanguage;
            return translations.TryGetValue(defaultLanguage, out value) && !string.IsNullOrEmpty(value);
        }
    }
}
