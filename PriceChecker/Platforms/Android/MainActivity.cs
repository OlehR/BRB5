using Android.App;
using Android.Content.PM;
using Android.OS;
using Utils;

namespace PriceChecker
{
    [Activity(
        Theme = "@style/Maui.SplashTheme",
        MainLauncher = true,
        LaunchMode = LaunchMode.SingleTop,
        ScreenOrientation = ScreenOrientation.Landscape,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density
    )]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            FileLogger.PathLog = Path.Combine(Android.App.Application.Context.GetExternalFilesDir("").AbsolutePath, Android.OS.Environment.DirectoryDownloads,"Logs");
        }
    }
}
