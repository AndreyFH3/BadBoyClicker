namespace GameLocalization
{
    public interface ILocalizationService
    {
        System.Collections.Generic.IReadOnlyList<string> AvailableLanguages { get; }
        string CurrentLanguage { get; }
        void SetLanguage(string language);
        string Localize(string key);
        string Format(string key, params object[] args);
    }
}
