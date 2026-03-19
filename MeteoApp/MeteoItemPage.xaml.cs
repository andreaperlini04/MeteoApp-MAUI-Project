namespace MeteoApp;

[QueryProperty(nameof(MeteoLocation), "MeteoLocation")]
public partial class MeteoItemPage : ContentPage
{
    private MeteoLocation _entry;
    public MeteoLocation MeteoLocation
    {
        get => _entry;
        set
        {
            _entry = value;
            BindingContext = new MeteoItemViewModel(_entry);
            OnPropertyChanged();
        }
    }

    public MeteoItemPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is MeteoItemViewModel vm)
        {
            await vm.LoadWeatherDataAsync();
        }
    }
}