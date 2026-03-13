namespace MeteoApp;

public partial class MeteoListPage : Shell
{
    public Dictionary<string, Type> Routes { get; private set; } = new Dictionary<string, Type>();

    public MeteoListPage()
	{
		InitializeComponent();
        RegisterRoutes();

        BindingContext = new MeteoListViewModel();
    }

    private void RegisterRoutes()
    {
        Routes.Add("entrydetails", typeof(MeteoItemPage));

        foreach (var item in Routes)
            Routing.RegisterRoute(item.Key, item.Value);
    }

    private void OnListItemSelected(object sender, SelectionChangedEventArgs e)
{
    if (e.CurrentSelection.FirstOrDefault() is Entry entry)
    {
        var navigationParameter = new Dictionary<string, object>
        {
            { "Entry", entry }
        };
        Shell.Current.GoToAsync($"entrydetails", navigationParameter);
        
        // Deseleziono l'elemento così posso cliccarlo di nuovo tornando indietro
        ((CollectionView)sender).SelectedItem = null;
    }
}

    private void OnItemAdded(object sender, EventArgs e)
    {
         _ = ShowPrompt();
    }

    private async Task ShowPrompt()
    {
        string result = await DisplayPromptAsync("Aggiungi Città", "Inserisci il nome della località:");
        
        if (!string.IsNullOrWhiteSpace(result))
        {
            var newEntry = new Entry
            {
                Name = result.Trim()
            };

            if (BindingContext is MeteoListViewModel vm)
            {
                vm.Entries.Add(newEntry);
            }
        }
    }
}