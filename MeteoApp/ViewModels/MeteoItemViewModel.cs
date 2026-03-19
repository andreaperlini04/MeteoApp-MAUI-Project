using System;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace MeteoApp
{
    public class MeteoItemViewModel : BaseViewModel
    {
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

        public MeteoItemViewModel(MeteoLocation entry)
        {
            MeteoLocation = entry;
        }

        public async Task LoadWeatherDataAsync()
        {
            if (MeteoLocation == null || string.IsNullOrWhiteSpace(MeteoLocation.Name)) return;

            TemperatureText = "Caricamento...";
            TemperatureMaxText = "Caricamento...";
            TemperatureMinText = "Caricamento...";
            Description = "Caricamento...";
            IconUrl = string.Empty;

            string apiKey = Config.OpenWeatherApiKey;
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={MeteoLocation.Name}&appid={apiKey}&units=metric&lang=it";

            using HttpClient client = new HttpClient();
            try
            {
                var response = await client.GetFromJsonAsync<WeatherApiResponse>(url);
                if (response != null && response.main != null)
                {
                    TemperatureText = $"{response.main.temp} °C";
                    TemperatureMinText = $"{response.main.temp_min} °C";
                    TemperatureMaxText = $"{response.main.temp_max} °C";
                    
                }

                if (response.weather != null && response.weather.Length > 0)
            {
                string desc = response.weather[0].description;
                Description = char.ToUpper(desc[0]) + desc.Substring(1); 
                
                
                IconUrl = $"https://openweathermap.org/img/wn/{response.weather[0].icon}@4x.png";
            }
            }
            catch (Exception)
            {
                TemperatureText = "Errore durante il recupero dei dati";
            }
        }

        // Classi di supporto per deserializzare il JSON di OpenWeather (spostate qui)
        public class WeatherApiResponse
        {
            public MainData main { get; set; }
            public WeatherData[] weather { get; set; }
        }

        public class MainData
        {
            public float temp { get; set; }
            public float temp_min { get; set; }
            public float temp_max { get; set; }

        }

        public class WeatherData 
        {
            public string description { get; set; }
            public string icon { get; set; }
        }
    }
}