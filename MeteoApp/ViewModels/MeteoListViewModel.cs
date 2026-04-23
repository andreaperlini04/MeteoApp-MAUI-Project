using MeteoApp.Core.Services;
using MeteoApp.Core.Models;
using System.Collections.ObjectModel;
using MeteoApp.Core.Data;

namespace MeteoApp
{
    public class MeteoListViewModel : BaseViewModel
    {
        private readonly MeteoDatabase _database;
        private readonly WeatherService _weatherService;

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

        public MeteoListViewModel(MeteoDatabase database, WeatherService weatherService)
        {
            _weatherService = weatherService;
            _database = database;


            Entries = new ObservableCollection<MeteoLocation>();
            CurrentLocationEntries = new ObservableCollection<MeteoLocation>();
           
        }

        public async Task LoadCitiesAsync()
        {
            var locations = await _database.GetLocationsAsync();

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

                string cityName = await _weatherService.GetCityNameFromCoordinatesAsync(gpsLocation.Latitude, gpsLocation.Longitude);

                if (!string.IsNullOrWhiteSpace(cityName))
                {
                    CurrentLocationEntries.Clear();
                    CurrentLocationEntries.Add(new MeteoLocation
                    {
                        Name = cityName,
                        Latitude = gpsLocation.Latitude,
                        Longitude = gpsLocation.Longitude
                    });
                }
            }
            catch (Exception)
            {
            }
        }


        public async Task SaveNewLocationAsync(MeteoLocation location)
        {
            await _database.SaveLocationAsync(location);

            await LoadCitiesAsync();
        }

        public async Task DeleteLocationAsync(MeteoLocation location)
        {
            await _database.DeleteLocationAsync(location);

            await LoadCitiesAsync();
        }
    }
}