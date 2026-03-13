using System.Collections.ObjectModel;
namespace MeteoApp
{
    public class MeteoListViewModel : BaseViewModel
    {
        ObservableCollection<Entry> _entries;
        public ObservableCollection<Entry> Entries
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
            Entries =
            [
                new() { Name = "Roma" },
                new() { Name = "Milano" },
                new() { Name = "Napoli" },
                new() { Name = "Torino" },
                new() { Name = "Palermo" },
                new() { Name = "Lugano" }
            ];
        }
    }
}