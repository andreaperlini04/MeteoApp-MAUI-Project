using Microsoft.Maui.ApplicationModel;

namespace MeteoApp;

public partial class MeteoListPage : ContentPage
{
    private bool _isRequestingLocationPermission;
    private bool _locationLoaded;

    public MeteoListPage()
    {
        InitializeComponent();
        BindingContext = new MeteoListViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isRequestingLocationPermission) return;

        _isRequestingLocationPermission = true;

        try
        {
            await Task.Delay(500);
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            if (status == PermissionStatus.Granted && !_locationLoaded && BindingContext is MeteoListViewModel vm)
            {
                _locationLoaded = true;
                await vm.LoadCurrentLocationAsync();
            }
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

            if (BindingContext is MeteoListViewModel vm)
            {
                var (name, lat, lon) = await vm.GetCityInfoAsync(newEntry.Name);
                newEntry.Name = name;
                newEntry.Latitude = lat;
                newEntry.Longitude = lon;

                await App.Database.SaveLocationAsync(newEntry);
                vm.Entries.Add(newEntry);
            }
        }
    }

    private async void OnDeleteItemInvoked(object sender, EventArgs e)
    {
        if (sender is SwipeItemView swipeItem && swipeItem.CommandParameter is MeteoLocation location)
        {
            await App.Database.DeleteLocationAsync(location);
            
            if (BindingContext is MeteoListViewModel vm)
            {
                vm.Entries.Remove(location);
            }
        }
    }
}
