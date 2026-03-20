using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MeteoApp
{
    public class MeteoListViewModel : BaseViewModel
    {
        ObservableCollection<MeteoLocation> _entries;
        public ObservableCollection<MeteoLocation> Entries
        {
            get { return _entries; }
            set
            {
                _entries = value;
                OnPropertyChanged();
            }
        }

        public MeteoListViewModel()
        {
            Entries = new ObservableCollection<MeteoLocation>();
            _ = LoadCitiesAsync(); // Carica le città all'avvio
        }

        public async Task LoadCitiesAsync()
        {
            // Recupera le voci dal database locale
            var locations = await App.Database.GetLocationsAsync();
            
            Entries.Clear();
            foreach (var location in locations)
            {
                Entries.Add(location);
            }
        }
    }
}