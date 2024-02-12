using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
//using Xamarin.Essentials;
using AndroidX.AppCompat.App;
using BRB5.Connector;
using BRB5.Model;
using BRB5.View;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using Utils;
using Xamarin.Essentials;
using Xamarin.Forms;
using Result = Android.App.Result;

namespace BRB5.Droid
{
    [Activity(Label = "BRB5", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        MyBroadcastReceiver BR;
       
        //public static string SerialNumber = "None";
        protected override void OnCreate(Bundle savedInstanceState)
        {
            
            AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;
            base.OnCreate(savedInstanceState);
            BR = new MyBroadcastReceiver();
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            DB db = DB.GetDB(ProtoBRB.GetPathDB);
            // Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);
            Config.PathDownloads = Path.Combine(Android.App.Application.Context.GetExternalFilesDir("").AbsolutePath, Android.OS.Environment.DirectoryDownloads);
            Config.SN = GetDeviceId();
            Config.Ver = int.Parse(AppInfo.VersionString.Replace(".", ""));
            Config.Manufacturer = DeviceInfo.Manufacturer;
            Config.Model = DeviceInfo.Model;
            Config.Company = db.GetConfig<eCompany>("Company");
           

            FileLogger.PathLog = Path.Combine(Config.PathDownloads, "Log");            
            FileLogger.WriteLogMessage("Start", eTypeLog.Expanded);
            ///!!!!=TMP копіювання бази
            try
            {
                string path1 = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);

                var FileDestination = Path.Combine(path1, "brb5.db");
                if( File.Exists(FileDestination)) File.Delete(FileDestination); 

                File.Copy(db.PathNameDB, FileDestination, true);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(e.Message);
            }
                        
            if ( LoadAPK($"https://github.com/OlehR/BRB5/raw/master/Apk/{Config.Company}/", "ua.UniCS.TM.brb5.apk", null, VerCode))
                         InstallAPK(Path.Combine(Config.PathDownloads, "ua.UniCS.TM.brb5.apk"));

            NativeMedia.Platform.Init(this, savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            ZXing.Net.Mobile.Forms.Android.Platform.Init();
            ZXing.Mobile.MobileBarcodeScanner.Initialize(Application);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            global::ZXing.Net.Mobile.Forms.Android.Platform.Init();
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init(enableFastRenderer: true );
            Xamarin.KeyboardHelper.Platform.Droid.Effects.Init(this);

            LoadApplication(new App());
        }

        protected override void OnResume()
        {
            base.OnResume();
            if(Config.TypeScaner!=eTypeScaner.Camera && Config.TypeScaner != eTypeScaner.NotDefine )
            RegisterReceiver(BR, new IntentFilter(MyBroadcastReceiver.IntentEvent));
            // Code omitted for clarity
        }

        protected override void OnPause()
        {
            if (Config.TypeScaner != eTypeScaner.Camera && Config.TypeScaner != eTypeScaner.NotDefine)
                UnregisterReceiver(BR);
            // Code omitted for clarity
            base.OnPause();
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent)
        {
            if (NativeMedia.Platform.CheckCanProcessResult(requestCode, resultCode, intent))
                NativeMedia.Platform.OnActivityResult(requestCode, resultCode, intent);

            base.OnActivityResult(requestCode, resultCode, intent);
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        void InstallAPK(string filepath)
        {
            /*try
            {
                Java.IO.File file = new Java.IO.File(filepath);
                if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                {
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
                        InstallPackageAndroidQAndAbove(Application.Context, filepath);
                    else
                    {
                        Android.Net.Uri URIAPK = AndroidX.Core.Content.FileProvider.GetUriForFile(Android.App.Application.Context, Android.App.Application.Context.ApplicationContext.PackageName + ".provider", file);
                        Intent intS = new Intent(Intent.ActionInstallPackage);
                        intS.SetData(URIAPK);
                        intS.SetFlags(ActivityFlags.GrantReadUriPermission);
                        intS.SetFlags(ActivityFlags.NewTask);
                        Application.Context.StartActivity(intS);
                    }
                }
                else
                {
                    Android.Net.Uri URIAPK = Android.Net.Uri.FromFile(file);
                    Intent intS = new Intent(Intent.ActionView);
                    intS.SetDataAndType(URIAPK, "application/vnd.android.package-archive");
                    intS.SetFlags(ActivityFlags.NewTask);
                    Application.Context.StartActivity(intS);
                }
            }catch(Exception e )
            {
                FileLogger.WriteLogMessage(e.Message, eTypeLog.Error);
                e = e.InnerException;
            }*/
        }
        
        const string PACKAGE_INSTALLED_ACTION =
            "com.example.android.apis.content.SESSION_API_PACKAGE_INSTALLED";

        public static void InstallPackageAndroidQAndAbove(Android.Content.Context context, string filePath)
        {

            var packageInstaller = context.PackageManager.PackageInstaller;
            var sessionParams = new PackageInstaller.SessionParams(PackageInstallMode.FullInstall);
            int sessionId = packageInstaller.CreateSession(sessionParams);
            var session = packageInstaller.OpenSession(sessionId);

            addApkToInstallSession(filePath, session);

            // Create an install status receiver.
            Intent intent = new Intent(context, context.Class);
            intent.SetAction(Intent.ActionInstallPackage);
            PendingIntent pendingIntent = PendingIntent.GetActivity(context, 0, intent, 0);
            IntentSender statusReceiver = pendingIntent.IntentSender;

            // Commit the session (this will start the installation workflow).
            session.Commit(statusReceiver);

        }

        private static void addApkToInstallSession(string filePath, PackageInstaller.Session session)
        {
            var packageInSession = session.OpenWrite("package", 0, -1);
            var input = new FileStream(filePath, FileMode.Open, FileAccess.Read);//context.ContentResolver.OpenInputStream(apkUri);
            try
            {
                if (input != null)
                {
                    input.CopyTo(packageInSession);
                }
                else
                {
                    throw new Exception("Inputstream is null");
                }
            }
            finally
            {
                packageInSession.Close();
                input.Close();
            }
            /*using (var input = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var packageInSession = session.OpenWrite("package", 0, -1))
                {
                    input.CopyTo(packageInSession);
                    packageInSession.Close();
                }
                input.Close();
            }*/
            //That this is necessary could be a Xamarin bug.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }


        // Note: this Activity must run in singleTop launchMode for it to be able to receive the //intent
        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);

            Bundle extras = intent.Extras;

            if (PACKAGE_INSTALLED_ACTION.Equals(intent.Action))
            {
                int status = extras.GetInt(PackageInstaller.ExtraStatus);
                String message = extras.GetString(PackageInstaller.ExtraStatusMessage);

                switch (status)
                {
                    case (int)PackageInstallStatus.PendingUserAction:
                        // This test app isn't privileged, so the user has to confirm the install.
                        Intent confirmIntent = (Intent)extras.Get(Intent.ExtraIntent);
                        StartActivity(confirmIntent);
                        break;
                    case (int)PackageInstallStatus.Success:
                        Toast.MakeText(this, "Install succeeded!", ToastLength.Long).Show();
                        break;
                    case (int)PackageInstallStatus.Failure:
                    case (int)PackageInstallStatus.FailureAborted:
                    case (int)PackageInstallStatus.FailureBlocked:
                    case (int)PackageInstallStatus.FailureConflict:
                    case (int)PackageInstallStatus.FailureIncompatible:
                    case (int)PackageInstallStatus.FailureInvalid:
                    case (int)PackageInstallStatus.FailureStorage:
                        Toast.MakeText(this, "Install failed! " + status + ", " + message,
                                ToastLength.Long).Show();
                        break;
                    default:
                        Toast.MakeText(this, "Unrecognized status received from installer: " + status,
                               ToastLength.Long).Show();
                        break;
                }
            }

            
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
        
        public bool LoadAPK(string pPath, string pNameAPK, Action<int,string> pProgress, int pVersionCode)
        {
            GetDataHTTP Http = GetDataHTTP.GetInstance();
            try
            {
                pProgress?.Invoke(0,"Start");
               // string FileNameVer = Path.Combine(Config.PathDownloads, "Ver.txt");
                string Ver = GetHttpString(pPath + "Ver.txt");

                pProgress?.Invoke(10,"");
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
                    if ( ver > pVersionCode)
                    {
                        pProgress?.Invoke(15,$"Завантажуємо нову версію={ver} Текуча={pVersionCode}");
                        string FileName = Path.Combine(Config.PathDownloads, pNameAPK);
                        GetHttpFile(pPath + pNameAPK, Path.Combine(Config.PathDownloads, pNameAPK));

                        pProgress?.Invoke(60,"Завантаження завершено") ;
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                pProgress?.Invoke(65, $"LoadAPK Помилка=> {e.Message}");
                FileLogger.WriteLogMessage(e.Message, eTypeLog.Error);
                //e.printStackTrace();
            }

            pProgress?.Invoke(100,"");
            return false;
        }

        public HttpResult GetHttpFile(string pURL, string pFile)
        {
            //pDir = Config.GetPathFiles+"";
            try
            {
                if(File.Exists(pFile))
                    File.Delete(pFile);

                WebClient webClient = new WebClient();

                webClient.DownloadFile(pURL, pFile);
               // var b = webClient.DownloadData(new Uri(pURL));
                //File.WriteAllBytes(pFile, b);    
            }
            catch (Exception e)
            {

                FileLogger.WriteLogMessage(e.Message, eTypeLog.Error);
                e = e.InnerException;
            }
            return new HttpResult();
        }

        public string GetHttpString(string pURL)
        {
            try
            {
                WebClient webClient = new WebClient();
                return webClient.DownloadString(new Uri(pURL));
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(e.Message, eTypeLog.Error);
                e = e.InnerException;
                return null;
            }

        }

        public override bool OnKeyUp([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {
                case Keycode.Back:
                    MessagingCenter.Send(new KeyEventMessage { Key = "BackPressed" }, "BackPressed");
                    break;
                case Keycode.F1:
                    MessagingCenter.Send(new KeyEventMessage { Key = "F1Pressed" }, "F1Pressed");
                    break;
                case Keycode.F2:
                    MessagingCenter.Send(new KeyEventMessage { Key = "F2Pressed" }, "F2Pressed");
                    break;
                case Keycode.F3:
                    MessagingCenter.Send(new KeyEventMessage { Key = "F3Pressed" }, "F3Pressed");
                    break;
                case Keycode.F4:
                    MessagingCenter.Send(new KeyEventMessage { Key = "F4Pressed" }, "F4Pressed");
                    break;
                case Keycode.F5:
                    MessagingCenter.Send(new KeyEventMessage { Key = "F5Pressed" }, "F5Pressed");
                    break;
                case Keycode.F6:
                    MessagingCenter.Send(new KeyEventMessage { Key = "F6Pressed" }, "F6Pressed");
                    break;
                case Keycode.F7:
                    MessagingCenter.Send(new KeyEventMessage { Key = "F7Pressed" }, "F7Pressed");
                    break;
                case Keycode.F8:
                    MessagingCenter.Send(new KeyEventMessage { Key = "F8Pressed" }, "F8Pressed");
                    break;
                case Keycode.Num2:
                    MessagingCenter.Send(new KeyEventMessage { Key = "2Pressed" }, "2Pressed");
                    break;
                case Keycode.Num8:
                    MessagingCenter.Send(new KeyEventMessage { Key = "8Pressed" }, "8Pressed");
                    break;
                case Keycode.Enter:
                    MessagingCenter.Send(new KeyEventMessage { Key = "EnterPressed" }, "EnterPressed");
                    break;
                default:
                    break;
            }
            return base.OnKeyUp(keyCode, e);
        }
    }

    
}