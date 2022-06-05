using System;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using System.IO;
using Utils;
using Android.Content;
using AndroidX.Core.Content;
using BRB5.Model;

namespace BRB5.Droid
{
    [Activity(Label = "BRB5", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Config.PathDownloads  = Path.Combine(Android.App.Application.Context.GetExternalFilesDir("").AbsolutePath, Android.OS.Environment.DirectoryDownloads);= Path.Combine(Android.App.Application.Context.GetExternalFilesDir("").AbsolutePath, Android.OS.Environment.DirectoryDownloads);
            FileLogger.PathLog = Path.Combine(Config.PathDownloads, "Log");

            Utils Util = Utils.GetInstance();
            if (Util.LoadAPK(Config.PathDownloads, "ua.UniCS.TM.brb5.apk", null, VerCode))
                InstallAPK(Path.Combine(Config.PathDownloads, "ua.UniCS.TM.brb5.apk"));

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            ZXing.Net.Mobile.Forms.Android.Platform.Init();
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            global::ZXing.Net.Mobile.Forms.Android.Platform.Init();
            LoadApplication(new App());
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        void InstallAPK(string filepath)
        {
            Java.IO.File file = new Java.IO.File(filepath);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
            {
                Android.Net.Uri URIAPK = FileProvider.GetUriForFile(Android.App.Application.Context, Android.App.Application.Context.ApplicationContext.PackageName + ".provider", file);
                Intent intS = new Intent(Intent.ActionInstallPackage);
                intS.SetData(URIAPK);
                intS.SetFlags(ActivityFlags.GrantReadUriPermission);
                Android.App.Application.Context.StartActivity(intS);
            }
            else
            {
                Android.Net.Uri URIAPK = Android.Net.Uri.FromFile(file);
                Intent intS = new Intent(Intent.ActionView);
                intS.SetDataAndType(URIAPK, "application/vnd.android.package-archive");
                intS.SetFlags(ActivityFlags.NewTask);
                Android.App.Application.Context.StartActivity(intS);
            }
        }
        public int VerCode
        {
            get {
            int VerCode;
            var pInfo = Android.App.Application.Context.ApplicationContext.PackageManager.GetPackageInfo(Android.App.Application.Context.ApplicationContext.PackageName, 0);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.P)
            {
                VerCode = (int)pInfo.LongVersionCode; // avoid huge version numbers and you will be ok
            }
            else
            {
                //noinspection deprecation
                VerCode = pInfo.VersionCode;
            }
            return VerCode;
        }
}
    }
}