namespace MeteoApp;

[QueryProperty(nameof(Entry), "Entry")]
public partial class MeteoItemPage : ContentPage
{
    private Entry _entry;
    public Entry Entry
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