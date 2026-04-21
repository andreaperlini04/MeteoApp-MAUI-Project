using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MeteoApp.Core.Models;
using MeteoApp.Core.Services;

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
            set
            {
                _iconUrl = value;
                OnPropertyChanged();
            }
        }
        public string TemperatureText
        {
            get => _temperatureText;
            set
            {
                _temperatureText = value;
                OnPropertyChanged();
            }
        }

        public string TemperatureMinText
        {
            get => _temperatureMinText;
            set
            {
                _temperatureMinText = value;
                OnPropertyChanged();
            }
        }
        public string TemperatureMaxText
        {
            get => _temperatureMaxText;
            set
            {
                _temperatureMaxText = value;
                OnPropertyChanged();
            }
        }
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        public MeteoItemViewModel(WeatherService weatherService)
        {
            _weatherService = weatherService;
        }

        public void Initialize(MeteoLocation entry)
        {
            MeteoLocation = entry;
        }

        public async Task LoadWeatherDataAsync()
        {
            if (MeteoLocation == null || string.IsNullOrWhiteSpace(MeteoLocation.Name)) return;

            TemperatureText = "Caricamento...";
            TemperatureMaxText = "...";
            TemperatureMinText = "...";
            Description = "Caricamento...";
            IconUrl = string.Empty;

            var weather = await _weatherService.GetWeatherDetailsAsync(MeteoLocation.Name);

            if (weather != null)
            {
                TemperatureText = $"{Math.Round(weather.Temperature)} °C";
                TemperatureMinText = $"{Math.Round(weather.TemperatureMin)} °C";
                TemperatureMaxText = $"{Math.Round(weather.TemperatureMax)} °C";
                Description = weather.Description;
                IconUrl = weather.IconUrl;
            }
            else
            {
                TemperatureText = "Errore durante il recupero dei dati";
            }
        }

        
    }
}