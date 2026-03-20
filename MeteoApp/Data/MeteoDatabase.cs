using SQLite;

namespace MeteoApp
{
    public class MeteoDatabase
    {
        private SQLiteAsyncConnection _database;

        public MeteoDatabase()
        {
        }

        async Task Init()
        {
            if (_database is not null)
                return;

            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MeteoSQLite.db3");
            
            // Inizializza la connessione 
            _database = new SQLiteAsyncConnection(dbPath);

            // Crea la tabella se non esiste, sfruttando l'ORM 
            await _database.CreateTableAsync<MeteoLocation>(); 
        }

        public async Task<List<MeteoLocation>> GetLocationsAsync()
        {
            await Init();
            return await _database.Table<MeteoLocation>().ToListAsync(); 
        }

        public async Task<int> SaveLocationAsync(MeteoLocation item)
        {
            await Init();
            if (item.Id != 0)
                return await _database.UpdateAsync(item);
            else
                return await _database.InsertAsync(item); 
        }
        
        public async Task<int> DeleteLocationAsync(MeteoLocation item)
        {
            await Init();
            return await _database.DeleteAsync(item);
        }
    }
}