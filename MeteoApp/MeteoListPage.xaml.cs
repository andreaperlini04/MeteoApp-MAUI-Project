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
        _ = ShowAddLocationPage();
    }

    private async Task ShowAddLocationPage()
    {
        if (BindingContext is not MeteoListViewModel vm) return;

        var tcs = new TaskCompletionSource<MeteoLocation>();
        await Navigation.PushModalAsync(new NavigationPage(new AddLocationPage(vm, tcs)));
        var result = await tcs.Task;

        if (result == null) return;

        // Se l'utente ha scritto solo il nome senza cliccare la mappa,
        // recupera le coordinate e il nome corretto da OpenWeather
        if (result.Latitude == 0 && result.Longitude == 0)
        {
            var (name, lat, lon) = await vm.GetCityInfoAsync(result.Name);
            result.Name = name;
            result.Latitude = lat;
            result.Longitude = lon;
        }

        await App.Database.SaveLocationAsync(result);
        vm.Entries.Add(result);
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
