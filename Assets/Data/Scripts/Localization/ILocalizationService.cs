namespace GameLocalization
{
    public interface ILocalizationService
    {
        string CurrentLanguage { get; }
        string Localize(string key, string fallback = null);
        string Format(string key, string fallback, params object[] args);
    }
}
