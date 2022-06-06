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
using BRB5.Connector;
using System.Net;

namespace BRB5.Droid
{
    [Activity(Label = "BRB5", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            Config.PathDownloads = Path.Combine(Android.App.Application.Context.GetExternalFilesDir("").AbsolutePath, Android.OS.Environment.DirectoryDownloads);
            FileLogger.PathLog = Path.Combine(Config.PathDownloads, "Log");
            //FileLogger.
            FileLogger.WriteLogMessage("Start", eTypeLog.Expanded);

            //Utils Util = Utils.GetInstance();
            if (LoadAPK("https://github.com/OlehR/BRB5/raw/master/Apk/Sim23/", "ua.UniCS.TM.brb5.apk", null, VerCode))
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
            get
            {
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
        public bool LoadAPK(string pPath, string pNameAPK, Action<int> pProgress, int pVersionCode)
        {
            GetDataHTTP Http = GetDataHTTP.GetInstance();
            try
            {
                pProgress?.Invoke(0);
                string FileNameVer = Path.Combine(Config.PathDownloads, "Ver.txt");
                GetFile(pPath + "Ver.txt", Config.PathDownloads);
                string Ver = File.ReadAllText(FileNameVer);

                pProgress?.Invoke(10);
                if (Ver != null && Ver.Length > 0)
                {
                    int ver = 0;
                    try
                    {
                        ver = int.Parse(Ver);
                    }
                    catch (Exception)
                    {
                    }
                    if (ver > pVersionCode)
                    {
                        string FileName = Path.Combine(Config.PathDownloads, pNameAPK);
                        GetFile(pPath + pNameAPK, Config.PathDownloads);

                        pProgress?.Invoke(60);

                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(e.Message, eTypeLog.Error);
                //e.printStackTrace();
            }

            pProgress?.Invoke(100);
            return false;
        }


        public HttpResult GetFile(string pURL, string pDir)
        {
            pDir = Config.GetPathFiles;
            try
            {
                WebClient webClient = new WebClient();

                var b=webClient.DownloadData(new Uri(pURL));
                File.WriteAllBytes(pDir, b);

                //var Res = webClient.DownloadData String(new Uri(pURL));///, pDir);

            }
            catch (Exception e)
            {

                FileLogger.WriteLogMessage(e.Message, eTypeLog.Error);
                e = e.InnerException;
            }
            return new HttpResult();
        }
    }
}