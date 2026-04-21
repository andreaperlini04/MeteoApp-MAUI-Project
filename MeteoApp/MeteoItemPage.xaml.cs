using MeteoApp.Core.Models;

namespace MeteoApp;

[QueryProperty(nameof(MeteoLocation), "MeteoLocation")]
public partial class MeteoItemPage : ContentPage
{
    private readonly MeteoItemViewModel _viewModel;

    public MeteoLocation MeteoLocation
    {
        set
        {
            _viewModel.Initialize(value);
        }
    }

    public MeteoItemPage(MeteoItemViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadWeatherDataAsync();
    }
}