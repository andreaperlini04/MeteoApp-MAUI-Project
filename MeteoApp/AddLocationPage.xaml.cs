using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace MeteoApp;

public partial class AddLocationPage : ContentPage
{
    private readonly MeteoListViewModel _viewModel;
    private readonly TaskCompletionSource<MeteoLocation> _tcs;
    private double _selectedLat = 0;
    private double _selectedLon = 0;
    private bool _isSettingTextFromMap = false;

    public AddLocationPage(MeteoListViewModel viewModel, TaskCompletionSource<MeteoLocation> tcs)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _tcs = tcs;
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

        // Metti subito un pin provvisorio mentre aspetti OpenWeather
        MyMap.Pins.Clear();
        MyMap.Pins.Add(new Pin
        {
            Label = "...",
            Location = e.Location
        });

        _isSettingTextFromMap = true;
        CityEntry.Text = string.Empty;
        _isSettingTextFromMap = false;

        string cityName = await _viewModel.GetCityNameFromCoordinatesAsync(_selectedLat, _selectedLon);

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
            MyMap.Pins.Add(new Pin { Label = "Posizione sconosciuta", Location = e.Location });
        }
    }

    private async void OnSave(object sender, EventArgs e)
    {
        string name = CityEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            await DisplayAlert("Errore", "Clicca sulla mappa o scrivi il nome di una città.", "OK");
            return;
        }

        _tcs.SetResult(new MeteoLocation
        {
            Name = name,
            Latitude = _selectedLat,
            Longitude = _selectedLon
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
        // Gestisce il caso in cui l'utente chiude la pagina con il tasto back di Android
        if (!_tcs.Task.IsCompleted)
            _tcs.SetResult(null);
    }
}
