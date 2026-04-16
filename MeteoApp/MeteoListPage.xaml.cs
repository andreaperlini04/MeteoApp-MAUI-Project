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
        catch (Exception ex)
        {
            // Aggiungiamo questo blocco per catturare l'eccezione dei permessi!
            // Così l'app non crasha, ma eviterà solo di caricare la posizione.
            Console.WriteLine($"Errore permessi: {ex.Message}");
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

        // Se result è null significa che l'utente ha premuto "Annulla"
        if (result == null) return;

        // Arrivati a questo punto, sappiamo già che i dati in "result" sono validi
        // perché il controllo API è stato fatto all'interno di AddLocationPage.
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
