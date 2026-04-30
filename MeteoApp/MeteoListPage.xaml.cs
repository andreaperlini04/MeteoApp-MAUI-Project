using MeteoApp.Core.Models;
using MeteoApp.Core.Services;

namespace MeteoApp;

public partial class MeteoListPage : ContentPage
{
    private bool _isRequestingLocationPermission;
    private readonly MeteoListViewModel _viewModel;
    private bool _locationLoaded;

    public MeteoListPage(MeteoListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var settings = App.SettingsService.Load();
        ThemeButton.Text = settings.Theme == "dark" ? "🌙" : "☀️";
        UnitButton.Text = settings.TemperatureUnit == "fahrenheit" ? "°F" : "°C";

        await _viewModel.LoadCitiesAsync();

        if (_isRequestingLocationPermission || _locationLoaded) return;

        _isRequestingLocationPermission = true;

        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (status == PermissionStatus.Granted)
            {
                _locationLoaded = true;
                await _viewModel.LoadCurrentLocationAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Errore permessi: {ex.Message}");
        }
        finally
        {
            _isRequestingLocationPermission = false;
        }
    }

    private void OnChangeLanguageClicked(object sender, EventArgs e)
    {
        App.LanguageService.ToggleLanguage();
        var settings = App.SettingsService.Load();
        settings.Language = App.LanguageService.CurrentLanguageCode;
        App.SettingsService.Save(settings);
    }

    private void OnToggleThemeClicked(object sender, EventArgs e)
    {
        var settings = App.SettingsService.Load();
        settings.Theme = settings.Theme == "light" ? "dark" : "light";
        App.SettingsService.Save(settings);
        Application.Current.UserAppTheme = settings.Theme == "dark" ? AppTheme.Dark : AppTheme.Light;
        ThemeButton.Text = settings.Theme == "dark" ? "🌙" : "☀️";
    }

    private void OnToggleUnitClicked(object sender, EventArgs e)
    {
        var settings = App.SettingsService.Load();
        settings.TemperatureUnit = settings.TemperatureUnit == "celsius" ? "fahrenheit" : "celsius";
        App.SettingsService.Save(settings);
        UnitButton.Text = settings.TemperatureUnit == "fahrenheit" ? "°F" : "°C";
    }

    private void OnListItemSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is MeteoLocation location)
        {
            NavigateToDetails(location);
            ((CollectionView)sender).SelectedItem = null;
        }
    }

    private void OnCurrentLocationTapped(object sender, TappedEventArgs e)
    {
        if (sender is View view && view.BindingContext is MeteoLocation location)
            NavigateToDetails(location);
    }

    private void NavigateToDetails(MeteoLocation location)
    {
        var navigationParameter = new Dictionary<string, object>
        {
            { "MeteoLocation", location }
        };
        Shell.Current.GoToAsync("entrydetails", navigationParameter);
    }

    private void OnItemAdded(object sender, EventArgs e)
    {
        _ = ShowAddLocationPage();
    }

    private async Task ShowAddLocationPage()
    {
        var tcs = new TaskCompletionSource<MeteoLocation>();
        var weatherService = Handler.MauiContext.Services.GetService<WeatherService>();
        await Navigation.PushModalAsync(new NavigationPage(new AddLocationPage(_viewModel, tcs, weatherService)));

        var result = await tcs.Task;
        if (result != null)
            await _viewModel.SaveNewLocationAsync(result);
    }

    private async void OnDeleteItemInvoked(object sender, EventArgs e)
    {
        if (sender is SwipeItemView swipeItem && swipeItem.CommandParameter is MeteoLocation location)
            await _viewModel.DeleteLocationAsync(location);
    }

    private void OnShowTokenClicked(object sender, EventArgs e)
    {
        #if ANDROID
            _ = MeteoApp.MainActivity.Instance?.FetchFcmToken();
        #endif
    }
}
