using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.AppCompat.App;
using BL;
using BRB5;
using System.Globalization;
using Utils;
using BRB5.Model;
using Android.Runtime;
using Android.Views;
using BRB6.View;
using BRB6.PlatformDependency;
using Android.Content;

namespace BRB6
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        MyBroadcastReceiver BR;
        public static Action<Keycode, KeyEvent> Key;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;
            base.OnCreate(savedInstanceState);
            
            //Config.Company = db.GetConfig<eCompany>("Company");
            //DB db = DB.GetDB(ProtoBRB.GetPathDB);
            // Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);
            ProtoBRB.SetPath(Path.Combine(Android.App.Application.Context.GetExternalFilesDir("").AbsolutePath, Android.OS.Environment.DirectoryDownloads));
            //UtilAndroid.InstallAPK();
            //Config.PathDownloads = Path.Combine(Android.App.Application.Context.GetExternalFilesDir("").AbsolutePath, Android.OS.Environment.DirectoryDownloads);
            Config.SN = GetDeviceId();
            //Config.Ver = int.Parse(AppInfo.VersionString.Replace(".", ""));
            //Config.Manufacturer = DeviceInfo.Manufacturer;
            //Config.Model = DeviceInfo.Model;
            
            Config.NativeBase = new Native();
            //FileLogger.PathLog = Path.Combine(Config.PathDownloads, "Log");
            FileLogger.WriteLogMessage("Start", eTypeLog.Expanded);
            if (Config.TypeScaner == eTypeScaner.PM351)
                BR = new MyBroadcastReceiver();

            /*
            try
            {
            ///!!!!=TMP копіювання бази
                string path1 = Config.PathDownloads; //Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);

                var FileDestination = Path.Combine(path1, "brb5.db");
                if (File.Exists(FileDestination)) File.Delete(FileDestination);

                byte[] buffer = File.ReadAllBytes(db.PathNameDB);
                File.WriteAllBytes(FileDestination, buffer);
                //File.Copy(db.PathNameDB, FileDestination, true);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(e.Message);
            }
            */
            //Config.TypeScaner = App.GetTypeScaner();
            //if (Config.TypeScaner == eTypeScaner.PM351)
            //    BR = new MyBroadcastReceiver();
            //LoadApplication(new App());
        }
        public string GetDeviceId()
        {
            string deviceID = Android.OS.Build.Serial?.ToString();
            if (string.IsNullOrEmpty(deviceID) || deviceID.ToUpper() == "UNKNOWN") // Android 9 returns "Unknown"
            {
                //ContentResolver myContentResolver = MainActivity.myContentResolver;
                deviceID = Android.Provider.Settings.Secure.GetString(ContentResolver, Android.Provider.Settings.Secure.AndroidId);
            }
            return deviceID;
        }
        public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            Key?.Invoke(keyCode, e);
            return base.OnKeyDown(keyCode, e);
        }

        protected override void OnResume()
        {
            base.OnResume();           
            if (BR != null)
                RegisterReceiver(BR, new IntentFilter(MyBroadcastReceiver.IntentEvent));
           
        }
    }
}
