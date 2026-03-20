using Microsoft.Maui.ApplicationModel;

namespace MeteoApp;

public partial class MeteoListPage : ContentPage
{
    private bool _isRequestingLocationPermission;
    public MeteoListPage()
    {
        InitializeComponent();
        BindingContext = new MeteoListViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (_isRequestingLocationPermission)
        {
            return;
        }

        _isRequestingLocationPermission = true;

        try
        {
            await Task.Delay(500);
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            // if denied: non fare nulla
        }
        finally
        {
            _isRequestingLocationPermission = false;
        }
        
    }

    private void OnListItemSelected(object sender, SelectionChangedEventArgs e)
    {
         if (e.CurrentSelection.FirstOrDefault() is MeteoLocation location)
        {
            var navigationParameter = new Dictionary<string, object>
            {
                { "MeteoLocation", location }
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
            var newEntry = new MeteoLocation
            {
                Name = result.Trim()
            };

            await App.Database.SaveLocationAsync(newEntry);

            if (BindingContext is MeteoListViewModel vm)
            {
                vm.Entries.Add(newEntry);
            }
        }
    }
}
