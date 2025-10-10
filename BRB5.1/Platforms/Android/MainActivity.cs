using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.App;
using BL;
using BRB5;
using BRB5.Model;
using BRB6.PlatformDependency;
using BRB6.View;
using System.Globalization;
using Utils;
using static Android.Graphics.ColorSpace;

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
            //string path1 = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);
            ProtoBRB.SetPath(Path.Combine(Android.App.Application.Context.GetExternalFilesDir("").AbsolutePath, Android.OS.Environment.DirectoryDownloads));            
            Config.SN = GetDeviceId();         
            Config.NativeBase = new Native();
            //FileLogger.PathLog = Path.Combine(Config.PathDownloads, "Log");
            FileLogger.WriteLogMessage("Start", eTypeLog.Expanded);
            if (Config.TypeScaner == eTypeScaner.NLS_MT67 || Config.TypeScaner == eTypeScaner.PM351 || Config.TypeScaner == eTypeScaner.PM84  || Config.TypeScaner == eTypeScaner.Zebra  || Config.TypeScaner == eTypeScaner.MetapaceM_K4) //|| Config.TypeScaner == eTypeScaner.BitaHC61
                BR = new MyBroadcastReceiver();
        }
        public string GetDeviceId()
        {
            string deviceID = Android.OS.Build.Serial?.ToString();
            if (string.IsNullOrEmpty(deviceID) || deviceID.ToUpper() == "UNKNOWN") // Android 9 returns "Unknown"         
                deviceID =$"{ Android.Provider.Settings.Secure.GetString(ContentResolver, Android.Provider.Settings.Secure.AndroidId)}({DeviceInfo.Current.Name})"; 
           
            if (deviceID.Length > 32)
                deviceID = deviceID[..32];
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
            {
                var I = new IntentFilter(MyBroadcastReceiver.IntentEvent);
                I.AddCategory(Intent.CategoryDefault);
                RegisterReceiver(BR, I);// "android.intent.category.DEFAULT"
            }

        }
        protected override void OnPause()
        {
            if(BR != null) UnregisterReceiver(BR);         
            base.OnPause();
        }

        public override void OnUserInteraction()
        {
            base.OnUserInteraction();
            (App.Current as App)?.ResetInactivityTimer();
        }
    }
}
