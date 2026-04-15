using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace MeteoApp
{
    public class MeteoListViewModel : BaseViewModel
    {
        ObservableCollection<MeteoLocation> _entries;
        public ObservableCollection<MeteoLocation> Entries
        {
            get { return _entries; }
            set
            {
                _entries = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<MeteoLocation> CurrentLocationEntries { get; set; }

        public MeteoListViewModel()
        {
            Entries = new ObservableCollection<MeteoLocation>();
            CurrentLocationEntries = new ObservableCollection<MeteoLocation>();
            _ = LoadCitiesAsync();
        }

        public async Task LoadCitiesAsync()
        {
            var locations = await App.Database.GetLocationsAsync();

            Entries.Clear();
            foreach (var location in locations)
            {
                Entries.Add(location);
            }
        }

        public async Task LoadCurrentLocationAsync()
        {
            try
            {
                var gpsLocation = await Geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Medium,
                    Timeout = TimeSpan.FromSeconds(10)
                });

                if (gpsLocation == null) return;

                string lat = gpsLocation.Latitude.ToString(CultureInfo.InvariantCulture);
                string lon = gpsLocation.Longitude.ToString(CultureInfo.InvariantCulture);
                string apiKey = Config.OpenWeatherApiKey;
                string url = $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={apiKey}&units=metric&lang=it";

                using HttpClient client = new HttpClient();
                var response = await client.GetFromJsonAsync<LocationWeatherResponse>(url);

                if (response != null && !string.IsNullOrWhiteSpace(response.name))
                {
                    CurrentLocationEntries.Clear();
                    CurrentLocationEntries.Add(new MeteoLocation
                    {
                        Name = response.name,
                        Latitude = gpsLocation.Latitude,
                        Longitude = gpsLocation.Longitude
                    });
                }
            }
            catch (Exception)
            {
            }

        }

        public async Task<(string Name, double Latitude, double Longitude)> GetCityInfoAsync(string cityName)
        {
            string apiKey = Config.OpenWeatherApiKey;
            string cityQuery = cityName.Trim().Replace(" ", "+");
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={cityQuery}&appid={apiKey}";

            using HttpClient client = new HttpClient();
            try
            {
                var response = await client.GetFromJsonAsync<CityWeatherResponse>(url);
                if (response?.coord != null && !string.IsNullOrWhiteSpace(response.name))
                    return (response.name, response.coord.lat, response.coord.lon);
            }
            catch (Exception)
            {
                // errore (es. città non trovata, l'API restituisce 404).
            }

            return (null, 0, 0);
        }

        public async Task<string> GetCityNameFromCoordinatesAsync(double lat, double lon)
        {
            string apiKey = Config.OpenWeatherApiKey;
            string latStr = lat.ToString(CultureInfo.InvariantCulture);
            string lonStr = lon.ToString(CultureInfo.InvariantCulture);
            string url = $"https://api.openweathermap.org/data/2.5/weather?lat={latStr}&lon={lonStr}&appid={apiKey}";

            using HttpClient client = new();
            try
            {
                var response = await client.GetFromJsonAsync<LocationWeatherResponse>(url);
                if (response != null && !string.IsNullOrWhiteSpace(response.name))
                    return response.name;
            }
            catch (Exception)
            {
            }
            return string.Empty;
        }

        private class LocationWeatherResponse
        {
            public string name { get; set; }
        }

        private class CityWeatherResponse
        {
            public string name { get; set; }
            public CoordData coord { get; set; }
        }

        private class CoordData
        {
            public double lat { get; set; }
            public double lon { get; set; }
        }
    }
}