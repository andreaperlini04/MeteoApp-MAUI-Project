using System.Globalization;

namespace MeteoApp.Services
{
    public class LanguageService
    {
        public event Action? LanguageChanged;

        public string CurrentLanguageCode =>
            CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        public void SetLanguage(string cultureCode)
        {
            var culture = new CultureInfo(cultureCode);

            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            LanguageChanged?.Invoke();
        }

        public void ToggleLanguage()
        {
            var next = CurrentLanguageCode == "it" ? "en" : "it";
            SetLanguage(next);
        }
    }
}