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
            // Assegniamo il nostro ViewModel come BindingContext della pagina
            BindingContext = new MeteoItemViewModel(_entry);
            OnPropertyChanged();
        }
    }

    public MeteoItemPage()
    {
        InitializeComponent();
        // NON mettiamo più BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Recuperiamo il ViewModel e gli diciamo di caricare i dati
        if (BindingContext is MeteoItemViewModel vm)
        {
            await vm.LoadWeatherDataAsync();
        }
    }
}