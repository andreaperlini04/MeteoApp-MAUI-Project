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
                    CurrentLocationEntries.Add(new MeteoLocation { Name = response.name });
                }
            }
            catch (Exception)
            {
            }
        }

        private class LocationWeatherResponse
        {
            public string name { get; set; }
        }
    }
}