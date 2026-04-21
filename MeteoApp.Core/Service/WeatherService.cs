using System;
using System.Globalization;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MeteoApp.Core.Models;

namespace MeteoApp.Core.Services
{
    public class WeatherService
    {
        private readonly HttpClient _client;

        public WeatherService()
        {
            _client = new HttpClient();
        }

        public async Task<(string Name, double Latitude, double Longitude)> GetCityInfoAsync(string cityName)
        {
            // Assicurati che Config sia accessibile (deve essere 'public class Config' in MeteoApp.Core)
            string apiKey = Config.OpenWeatherApiKey;
            string cityQuery = cityName.Trim().Replace(" ", "+");
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={cityQuery}&appid={apiKey}";

            try
            {
                var response = await _client.GetFromJsonAsync<CityWeatherResponse>(url);
                if (response?.coord != null && !string.IsNullOrWhiteSpace(response.name))
                    return (response.name, response.coord.lat, response.coord.lon);
            }
            catch (Exception)
            {
                // errore (es. città non trovata, l'API restituisce 404).
            }

            return (null, 0, 0);
        }

        public async Task<string> GetCityNameFromCoordinatesAsync(double lat, double lon)
        {
            string apiKey = Config.OpenWeatherApiKey;
            string latStr = lat.ToString(CultureInfo.InvariantCulture);
            string lonStr = lon.ToString(CultureInfo.InvariantCulture);
            // Uso le stesse unità della tua altra chiamata
            string url = $"https://api.openweathermap.org/data/2.5/weather?lat={latStr}&lon={lonStr}&appid={apiKey}&units=metric&lang=it";

            try
            {
                var response = await _client.GetFromJsonAsync<LocationWeatherResponse>(url);
                if (response != null && !string.IsNullOrWhiteSpace(response.name))
                    return response.name;
            }
            catch (Exception)
            {
            }
            return string.Empty;
        }

        private class LocationWeatherResponse
        {
            public string name { get; set; }
        }

        private class CityWeatherResponse
        {
            public string name { get; set; }
            public CoordData coord { get; set; }
        }

        private class CoordData
        {
            public double lat { get; set; }
            public double lon { get; set; }
        }
        public async Task<WeatherDetails> GetWeatherDetailsAsync(string cityName)
        {
            string apiKey = Config.OpenWeatherApiKey;
            string cityQuery = cityName.Trim().Replace(" ", "+");
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={cityQuery}&appid={apiKey}&units=metric&lang=it";

            try
            {
                var response = await _client.GetFromJsonAsync<WeatherApiResponse>(url);
                if (response?.main != null && response.weather?.Length > 0)
                {
                    string desc = response.weather[0].description;
                    return new WeatherDetails
                    {
                        Temperature = response.main.temp,
                        TemperatureMin = response.main.temp_min,
                        TemperatureMax = response.main.temp_max,
                        Description = char.ToUpper(desc[0]) + desc.Substring(1),
                        IconUrl = $"https://openweathermap.org/img/wn/{response.weather[0].icon}@4x.png"
                    };
                }
            }
            catch (Exception ex)
            {
                // Loggare l'errore se necessario
            }
            return null;
        }

   
        private class WeatherApiResponse
        {
            public MainData main { get; set; }
            public WeatherData[] weather { get; set; }
        }

        private class MainData
        {
            public double temp { get; set; }
            public double temp_min { get; set; }
            public double temp_max { get; set; }
        }

        private class WeatherData
        {
            public string description { get; set; }
            public string icon { get; set; }
        }
    }





}