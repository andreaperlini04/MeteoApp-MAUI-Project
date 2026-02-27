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

    public MeteoItemPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
    }
}