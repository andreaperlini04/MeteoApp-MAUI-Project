using MeteoApp; 

namespace MeteoApp;

public partial class App : Application
{
    private static MeteoDatabase database;

    // Proprietà statica globale per accedere al database
    public static MeteoDatabase Database
    {
        get
        {
            if (database == null)
                database = new MeteoDatabase();
            return database;
        }
    }

    public App()
    {
        InitializeComponent();
        
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}