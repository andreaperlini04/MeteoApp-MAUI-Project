using System.Globalization;
using MeteoApp.Services;

namespace MeteoApp;

public partial class App : Application
{
    public static readonly LanguageService LanguageService = new();

    public App()
    {
        InitializeComponent();

        // Assicura che i nuovi thread usino la stessa cultura UI.
        CultureInfo.DefaultThreadCurrentCulture   = CultureInfo.CurrentCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CurrentUICulture;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());

        // Ogni volta che la lingua cambia, ricrea l'AppShell da zero:
        // questo forza tutti i binding {x:Static} a leggere i nuovi valori.
        LanguageService.LanguageChanged += () =>
        {
            window.Page = new AppShell();
        };

        return window;
    }
}
