using MeteoApp.Core.Models;
using SQLite;
using Appwrite;
using Appwrite.Models;
using Appwrite.Services;

namespace MeteoApp.Core.Data
{
    public class MeteoDatabase
    {
        private SQLiteAsyncConnection _sqliteDatabase;
        private readonly Databases _appwriteDatabases;

        private const string DatabaseId = Config.DatabaseId;
        private const string CollectionId = Config.CollectionId;

        public MeteoDatabase()
        {
            var client = new Client();
            client
                .SetEndpoint(Config.AppwriteEndPoint)
                .SetProject(Config.ProjectId)
                .SetKey(Config.AppwriteKey);

            _appwriteDatabases = new Databases(client);
        }

        private async Task InitSQLiteAsync()
        {
            if (_sqliteDatabase is not null)
                return;

            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MeteoSQLite.db3");
            _sqliteDatabase = new SQLiteAsyncConnection(dbPath);
            await _sqliteDatabase.CreateTableAsync<MeteoLocation>();
        }

        public async Task<List<MeteoLocation>> GetLocationsAsync()
        {
            await InitSQLiteAsync();
            return await _sqliteDatabase.Table<MeteoLocation>().ToListAsync();
        }

        public async Task<int> SaveLocationAsync(MeteoLocation item)
        {
            await InitSQLiteAsync();

            int result;
            if (item.Id != 0)
                result = await _sqliteDatabase.UpdateAsync(item);
            else
                result = await _sqliteDatabase.InsertAsync(item);

            _ = Task.Run(async () =>
            {
                try
                {
                    // Usa il nome della città per creare un ID univoco (es. "lugano")
                    string docId = item.Name.Replace(" ", "").ToLower();

                    // Inseriamo Nome, Latitudine e Longitudine
                    var data = new Dictionary<string, object>
                    {
                        { "name", item.Name },
                        { "latitude", item.Latitude },
                        { "longitude", item.Longitude }
                    };

                    try
                    {
                        await _appwriteDatabases.GetDocument(DatabaseId, CollectionId, docId);
                        // Se vuoi che aggiorni le coordinate esistenti puoi chiamare UpdateDocument qui
                    }
                    catch
                    {
                        await _appwriteDatabases.CreateDocument(DatabaseId, CollectionId, docId, data);
                    }
                }
                catch { /* Ignora errori di connessione */ }
            });

            return result;
        }

        public async Task<int> DeleteLocationAsync(MeteoLocation item)
        {
            await InitSQLiteAsync();

            int result = await _sqliteDatabase.DeleteAsync(item);

            try
            {
                string docId = item.Name.Replace(" ", "").ToLower();
                await _appwriteDatabases.DeleteDocument(DatabaseId, CollectionId, docId);
            }
            catch { /* Ignora errori */ }

            return result;
        }

        public async Task SyncWithAppwriteAsync()
        {
            await InitSQLiteAsync();

            try
            {
                var response = await _appwriteDatabases.ListDocuments(DatabaseId, CollectionId);
                var localLocations = await GetLocationsAsync();

                foreach (var doc in response.Documents)
                {
                    // Leggiamo tutti i campi salvati su Appwrite
                    var cloudName = doc.Data["name"].ToString();
                    var cloudLat = Convert.ToDouble(doc.Data["latitude"]);
                    var cloudLon = Convert.ToDouble(doc.Data["longitude"]);

                    bool existsLocally = localLocations.Any(l => l.Name.Equals(cloudName, StringComparison.OrdinalIgnoreCase));
                    if (!existsLocally)
                    {
                        // Inseriamo l'oggetto completo con le coordinate nel DB locale
                        await _sqliteDatabase.InsertAsync(new MeteoLocation
                        {
                            Name = cloudName,
                            Latitude = cloudLat,
                            Longitude = cloudLon
                        });
                    }
                }
            }
            catch
            {
                // Gestione silenziosa in caso di mancanza di rete
            }
        }
    }
}