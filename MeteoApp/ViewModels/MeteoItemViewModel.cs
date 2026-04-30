using System;
using System.Threading.Tasks;
using MeteoApp.Core.Models;
using MeteoApp.Core.Services;
using MeteoApp.Resources.Strings;

namespace MeteoApp
{
    public class MeteoItemViewModel : BaseViewModel
    {
        private readonly WeatherService _weatherService;
        private MeteoLocation _entry;

        public MeteoLocation MeteoLocation
        {
            get => _entry;
            set
            {
                _entry = value;
                OnPropertyChanged();
            }
        }

        private string _temperatureText;
        private string _temperatureMinText;
        private string _temperatureMaxText;
        private string _description;
        private string _iconUrl;

        public string IconUrl
        {
            get => _iconUrl;
            set { _iconUrl = value; OnPropertyChanged(); }
        }

        public string TemperatureText
        {
            get => _temperatureText;
            set { _temperatureText = value; OnPropertyChanged(); }
        }

        public string TemperatureMinText
        {
            get => _temperatureMinText;
            set { _temperatureMinText = value; OnPropertyChanged(); }
        }

        public string TemperatureMaxText
        {
            get => _temperatureMaxText;
            set { _temperatureMaxText = value; OnPropertyChanged(); }
        }

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        public MeteoItemViewModel(WeatherService weatherService)
        {
            _weatherService = weatherService;
        }

        public void Initialize(MeteoLocation entry)
        {
            MeteoLocation = entry;
        }

        private static double ToFahrenheit(double celsius) => celsius * 9.0 / 5.0 + 32;

        public async Task LoadWeatherDataAsync()
        {
            if (MeteoLocation == null || string.IsNullOrWhiteSpace(MeteoLocation.Name)) return;

            // Testi di "caricamento" localizzati
            TemperatureText    = AppResources.Weather_Loading;
            TemperatureMaxText = "...";
            TemperatureMinText = "...";
            Description        = AppResources.Weather_Loading;
            IconUrl            = string.Empty;

            var weather = await _weatherService.GetWeatherDetailsAsync(MeteoLocation.Name);

            if (weather != null)
            {
                var settings = App.SettingsService.Load();
                bool useFahrenheit = settings.TemperatureUnit == "fahrenheit";

                double temp    = useFahrenheit ? ToFahrenheit(weather.Temperature)    : weather.Temperature;
                double tempMin = useFahrenheit ? ToFahrenheit(weather.TemperatureMin) : weather.TemperatureMin;
                double tempMax = useFahrenheit ? ToFahrenheit(weather.TemperatureMax) : weather.TemperatureMax;
                string unit    = useFahrenheit ? "°F" : "°C";

                TemperatureText    = $"{Math.Round(temp)} {unit}";
                TemperatureMinText = $"{Math.Round(tempMin)} {unit}";
                TemperatureMaxText = $"{Math.Round(tempMax)} {unit}";
                Description        = weather.Description;
                IconUrl            = weather.IconUrl;
            }
            else
            {
                TemperatureText = AppResources.Weather_Error;
            }
        }
    }
}
