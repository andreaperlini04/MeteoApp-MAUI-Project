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
            Entries = new ObservableCollection<Entry>
            {
                new Entry { CityName = "Roma" },
                new Entry { CityName = "Milano" },
                new Entry { CityName = "Napoli" },
                new Entry { CityName = "Torino" },
                new Entry { CityName = "Palermo" },
                new Entry { CityName = "Lugano" }
            };
        }
    }
}