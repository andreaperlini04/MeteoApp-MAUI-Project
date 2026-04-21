using Microsoft.Extensions.Logging;
using MeteoApp.Core.Data;
using MeteoApp.Core.Services;
using Microsoft.Maui.Controls.Hosting;

namespace MeteoApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiMaps()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<MeteoDatabase>();
            builder.Services.AddSingleton<WeatherService>();

            builder.Services.AddTransient<MeteoListViewModel>();
            builder.Services.AddTransient<MeteoListPage>();
            builder.Services.AddTransient<MeteoItemViewModel>();
            builder.Services.AddTransient<MeteoItemPage>();


            return builder.Build();
        }
    }
}