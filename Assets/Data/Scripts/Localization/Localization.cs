using System;

namespace GameLocalization
{
    public static class Localization
    {
        private static ILocalizationService _service;

        public static bool IsReady => _service != null;
        public static event Action ServiceChanged;

        public static void SetService(ILocalizationService service)
        {
            if (ReferenceEquals(_service, service))
            {
                return;
            }

            _service = service;
            ServiceChanged?.Invoke();
        }

        public static string Tr(string key, string fallback = null)
        {
            return _service == null ? fallback ?? key ?? string.Empty : _service.Localize(key, fallback);
        }

        public static string Format(string key, string fallback, params object[] args)
        {
            return _service == null
                ? string.Format(fallback ?? key ?? string.Empty, args)
                : _service.Format(key, fallback, args);
        }
    }
}
