using System;
using Zenject;

namespace GameLocalization
{
    public class LocalizationInitializer : IInitializable, IDisposable
    {
        private readonly ILocalizationService _localization;

        public LocalizationInitializer(ILocalizationService localization)
        {
            _localization = localization;
        }

        public void Initialize()
        {
            Localization.SetService(_localization);
        }

        public void Dispose()
        {
            Localization.SetService(null);
        }
    }
}
