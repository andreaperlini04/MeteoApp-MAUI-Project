using MeteoApp.Core.Models;

namespace MeteoApp;

[QueryProperty(nameof(MeteoLocation), "MeteoLocation")]
public partial class MeteoItemPage : ContentPage
{
    private readonly MeteoItemViewModel _viewModel;
    private readonly ForecastStateService _forecastStateService;

    public MeteoLocation MeteoLocation
    {
        set
        {
            _viewModel.Initialize(value);
        }
    }

    public MeteoItemPage(MeteoItemViewModel viewModel, ForecastStateService forecastStateService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _forecastStateService = forecastStateService;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadWeatherDataAsync();
    }

    private async void OnViewForecastClicked(object sender, EventArgs e)
    {
        _forecastStateService.CityName = _viewModel.MeteoLocation.Name;
        await Shell.Current.GoToAsync("forecast");
    }
}
