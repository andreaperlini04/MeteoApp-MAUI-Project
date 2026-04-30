using System.Text.Json;
using MeteoApp.Models;

namespace MeteoApp.Services
{
    public class SettingsService
    {
        private const string Key = "app_settings";

        public AppSettings Load()
        {
            var json = Preferences.Default.Get(Key, string.Empty);
            if (string.IsNullOrEmpty(json))
                return new AppSettings();
            try
            {
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        public void Save(AppSettings settings)
        {
            Preferences.Default.Set(Key, JsonSerializer.Serialize(settings));
        }
    }
}
