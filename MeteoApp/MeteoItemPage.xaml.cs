using System.Net.Http.Json;

namespace MeteoApp;

[QueryProperty(nameof(Entry), "Entry")]
public partial class MeteoItemPage : ContentPage
{
    Entry entry;
    public Entry Entry
    {
        get => entry;
        set
        {
            entry = value;
            OnPropertyChanged();
        }
    }

    // Nuova proprietà per mostrare la temperatura
    private string _temperatureText;
    public string TemperatureText
    {
        get => _temperatureText;
        set
        {
            _temperatureText = value;
            OnPropertyChanged();
        }
    }

    public MeteoItemPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadWeatherDataAsync();
    }

    private async Task LoadWeatherDataAsync()
    {
        if (Entry == null || string.IsNullOrWhiteSpace(Entry.CityName)) return;

        TemperatureText = "Caricamento...";

        // INSERISCI QUI LA TUA CHIAVE API DI OPENWEATHER
        string apiKey = Config.OpenWeatherApiKey;
        
        // units=metric serve per avere i gradi Celsius
        string url = $"https://api.openweathermap.org/data/2.5/weather?q={Entry.CityName}&appid={apiKey}&units=metric&lang=it";

        using HttpClient client = new HttpClient();
        try
        {
            var response = await client.GetFromJsonAsync<WeatherApiResponse>(url);
            if (response != null && response.main != null)
            {
                TemperatureText = $"{response.main.temp} °C";
            }
        }
        catch (Exception)
        {
            TemperatureText = "Errore durante il recupero dei dati";
        }
    }
}

// Classi di supporto per la deserializzazione del JSON di OpenWeather
public class WeatherApiResponse
{
    public MainData main { get; set; }
}

public class MainData
{
    public float temp { get; set; }
}