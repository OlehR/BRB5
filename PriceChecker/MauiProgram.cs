using FFImageLoading.Maui;
using Microsoft.Extensions.Logging;
using BRB5.Model;
using BRB5;
namespace PriceChecker
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseFFImageLoading()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            Config.ApiUrl1 = "https://api.spar.uz.ua/";
            Config.Company = eCompany.PSU;
            Config.CodeWarehouse = 9;

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
