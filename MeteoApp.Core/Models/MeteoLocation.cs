using SQLite;

namespace MeteoApp.Core.Models
{
    public class MeteoLocation
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}