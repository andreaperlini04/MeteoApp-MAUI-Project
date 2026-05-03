using MeteoApp.Core.Models;
using MeteoApp.Resources.Strings;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using MeteoApp.Core.Services;

namespace MeteoApp;

public partial class AddLocationPage : ContentPage
{
    private readonly MeteoListViewModel _viewModel;
    private readonly TaskCompletionSource<MeteoLocation> _tcs;
    private readonly WeatherService _weatherService;
    private double _selectedLat = 0;
    private double _selectedLon = 0;
    private bool _isSettingTextFromMap = false;

    public AddLocationPage(MeteoListViewModel viewModel, TaskCompletionSource<MeteoLocation> tcs, WeatherService weatherService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _tcs = tcs;
        _weatherService = weatherService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CenterOnCurrentLocationAsync();
    }

    private async Task CenterOnCurrentLocationAsync()
    {
        try
        {
            var location = await Geolocation.GetLastKnownLocationAsync();
            location ??= await Geolocation.GetLocationAsync(new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Medium,
                Timeout = TimeSpan.FromSeconds(5)
            });

            if (location != null)
            {
                MyMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                    new Location(location.Latitude, location.Longitude),
                    Distance.FromKilometers(10)));
            }
        }
        catch { }
    }

    private void OnCityEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        if (!_isSettingTextFromMap)
        {
            _selectedLat = 0;
            _selectedLon = 0;
            MyMap.Pins.Clear();
        }
    }

    private async void OnMapClicked(object sender, MapClickedEventArgs e)
    {
        _selectedLat = e.Location.Latitude;
        _selectedLon = e.Location.Longitude;

        MyMap.Pins.Clear();
        MyMap.Pins.Add(new Pin
        {
            Label = "...",
            Location = e.Location
        });

        _isSettingTextFromMap = true;
        CityEntry.Text = string.Empty;
        _isSettingTextFromMap = false;

        string cityName = await _weatherService.GetCityNameFromCoordinatesAsync(_selectedLat, _selectedLon);

        MyMap.Pins.Clear();
        if (!string.IsNullOrWhiteSpace(cityName))
        {
            _isSettingTextFromMap = true;
            CityEntry.Text = cityName;
            _isSettingTextFromMap = false;
            MyMap.Pins.Add(new Pin { Label = cityName, Location = e.Location });
        }
        else
        {
            // Stringa localizzata per "Posizione sconosciuta"
            MyMap.Pins.Add(new Pin
            {
                Label = AppResources.AddLocation_UnknownPlace,
                Location = e.Location
            });
        }
    }

    private async void OnSave(object sender, EventArgs e)
    {
        string name = CityEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            await DisplayAlertAsync(
                AppResources.AddLocation_ErrorTitle,
                AppResources.AddLocation_ErrorEmptyCity,
                AppResources.AddLocation_OK);
            return;
        }

        double finalLat = _selectedLat;
        double finalLon = _selectedLon;
        string finalName = name;

        if (finalLat == 0 && finalLon == 0)
        {
            var (apiName, lat, lon) = await _weatherService.GetCityInfoAsync(name);

            if (string.IsNullOrEmpty(apiName))
            {
                await DisplayAlertAsync(
                    AppResources.AddLocation_ErrorTitle,
                    AppResources.AddLocation_ErrorCityNotFound,
                    AppResources.AddLocation_OK);
                return;
            }

            finalName = apiName;
            finalLat = lat;
            finalLon = lon;
        }

        _tcs.SetResult(new MeteoLocation
        {
            Name = finalName,
            Latitude = finalLat,
            Longitude = finalLon
        });

        await Navigation.PopModalAsync();
    }

    private async void OnCancel(object sender, EventArgs e)
    {
        _tcs.SetResult(null);
        await Navigation.PopModalAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (!_tcs.Task.IsCompleted)
            _tcs.SetResult(null);
    }
}
