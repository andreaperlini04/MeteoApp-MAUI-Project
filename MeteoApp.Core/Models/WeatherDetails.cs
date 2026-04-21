namespace MeteoApp.Core.Models
{
    public class WeatherDetails
    {
        public double Temperature { get; set; }
        public double TemperatureMin { get; set; }
        public double TemperatureMax { get; set; }
        public string Description { get; set; }
        public string IconUrl { get; set; }
    }
}