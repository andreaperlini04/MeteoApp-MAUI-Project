namespace MeteoApp
{
    public class ForecastStateService
    {
        private string _cityName = string.Empty;

        public string CityName
        {
            get => _cityName;
            set
            {
                _cityName = value;
                OnCityNameChanged?.Invoke();
            }
        }

        public event Action? OnCityNameChanged;
    }
}