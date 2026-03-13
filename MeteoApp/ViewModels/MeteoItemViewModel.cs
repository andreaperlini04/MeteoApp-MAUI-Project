using System;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace MeteoApp
{
    public class MeteoItemViewModel : BaseViewModel
    {
        private Entry _entry;
        public Entry Entry
        {
            get => _entry;
            set
            {
                _entry = value;
                OnPropertyChanged();
            }
        }

        private string _temperatureText;
        public string TemperatureText
        {
            get => _temperatureText;
            set
            {
                _temperatureText = value;
                OnPropertyChanged();
            }
        }

        // Il costruttore riceve l'oggetto Entry dalla pagina
        public MeteoItemViewModel(Entry entry)
        {
            Entry = entry;
        }

        // Tutta la logica API spostata qui dentro
        public async Task LoadWeatherDataAsync()
        {
            if (Entry == null || string.IsNullOrWhiteSpace(Entry.Name)) return;

            TemperatureText = "Caricamento...";

            string apiKey = Config.OpenWeatherApiKey;
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={Entry.Name}&appid={apiKey}&units=metric&lang=it";

            using HttpClient client = new HttpClient();
            try
            {
                var response = await client.GetFromJsonAsync<WeatherApiResponse>(url);
                if (response != null && response.main != null)
                {
                    TemperatureText = $"{response.main.temp} °C";
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
        }

        public class MainData
        {
            public float temp { get; set; }
        }
    }
}