using System;

namespace GameLocalization
{
    public static class Localization
    {
        private static ILocalizationService _service;

        public static bool IsReady => _service != null;
        public static string CurrentLanguage => _service?.CurrentLanguage ?? string.Empty;
        public static event Action ServiceChanged;
        public static event Action LanguageChanged;

        public static void SetService(ILocalizationService service)
        {
            if (ReferenceEquals(_service, service))
            {
                return;
            }

            _service = service;
            ServiceChanged?.Invoke();
        }

        public static string Tr(string key)
        {
            return _service == null ? key ?? string.Empty : _service.Localize(key);
        }

        public static void SetLanguage(string language)
        {
            if (_service == null)
            {
                return;
            }

            string currentLanguage = _service.CurrentLanguage;
            _service.SetLanguage(language);

            if (!string.Equals(currentLanguage, _service.CurrentLanguage, StringComparison.OrdinalIgnoreCase))
            {
                LanguageChanged?.Invoke();
            }
        }

        public static string Format(string key, params object[] args)
        {
            return _service == null
                ? string.Format(key ?? string.Empty, args)
                : _service.Format(key, args);
        }
    }
}
