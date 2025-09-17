using BRB5;
using BRB5.Model;
using CommunityToolkit.Maui;
using FFImageLoading.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
#endif
namespace PriceChecker
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            Config.Company = eCompany.Universal;

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseFFImageLoading()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Montserrat-ExtraBold.ttf", "MontserratExtraBold");
                })
                .ConfigureLifecycleEvents(events =>
                {
#if WINDOWS
    events.AddWindows(w =>
    {
        w.OnWindowCreated(window =>
        {
            window.ExtendsContentIntoTitleBar = false; //If you need to completely hide the minimized maximized close button, you need to set this value to false.
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WindowId myWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            var _appWindow = AppWindow.GetFromWindowId(myWndId);
            _appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
        });
    });
#endif
                });


#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
