using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using FFImageLoading.Maui;
using BarcodeScanning;
using BRB51.Handlers;

namespace BRB51
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseFFImageLoading()
                .UseBarcodeScanning()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            FormHandler.RemoveBorders();
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
