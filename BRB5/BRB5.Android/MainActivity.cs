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
using System.Runtime.Serialization;

namespace BRB5.Droid
{
    [Activity(Label = "BRB5", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Config.PathDownloads = Path.Combine(Android.App.Application.Context.GetExternalFilesDir("").AbsolutePath, Android.OS.Environment.DirectoryDownloads);
           // Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);
            FileLogger.PathLog = Path.Combine(Config.PathDownloads, "Log");
            //FileLogger.
            FileLogger.WriteLogMessage("Start", eTypeLog.Expanded);

            //TMP!!!!
            DB db =  DB.GetDB();

            //db.SetConfig<eCompany>("Company", eCompany.Sim23);
           /* db.SetConfig<eCompany>("Company", eCompany.VPSU);
            db.SetConfig<string>("ApiUrl1", "http://api.spar.uz.ua/znp/"); //723 http://93.183.216.37:80/dev1/hs/TSD/
            db.SetConfig<string>("ApiUrl2", "http://api.spar.uz.ua/print/"); //723 "http://93.183.216.37/TK/hs/TSD/;http://37.53.84.148/TK/hs/TSD/"
            db.SetConfig<string>("ApiUrl3", "https://bitrix.sim23.ua/rest/233/ax02yr7l9hia35vj/");
            
            //db.SetConfig<string>("ApiUrl1", "http://znp.vopak.local/api/api_v1_utf8.php");
            db.SetConfig<string>("CodeWarehouse", "9");
            */
            //!!!TMP
            try
            {

                string CopyTo = Path.Combine(Config.PathDownloads, "brb.db");
                if (File.Exists(CopyTo))
                    File.Delete(CopyTo);
                File.Copy(db.PathNameDB, CopyTo, true);
                //File.Create(CopyTo);
                // File.WriteAllBytes(CopyTo, b);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(e.Message);
            }


            //Utils Util = Utils.GetInstance();
            Config.Company = db.GetConfig<eCompany>("Company");
            Config.CodeWarehouse = db.GetConfig<int>("CodeWarehouse");
            //         if ( LoadAPK($"https://github.com/OlehR/BRB5/raw/master/Apk/{Config.Company}/", "ua.UniCS.TM.brb5.apk", null, VerCode))
            //             InstallAPK(Path.Combine(Config.PathDownloads, "ua.UniCS.TM.brb5.apk"));

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
            try
            {
                Java.IO.File file = new Java.IO.File(filepath);
                if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                {
                    Android.Net.Uri URIAPK = FileProvider.GetUriForFile(Android.App.Application.Context, Android.App.Application.Context.ApplicationContext.PackageName + ".provider", file);
                    Intent intS = new Intent(Intent.ActionInstallPackage);
                    intS.SetData(URIAPK);
                    intS.SetFlags(ActivityFlags.GrantReadUriPermission);
                    intS.SetFlags(ActivityFlags.NewTask );
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
            }catch(Exception e )
            {
                FileLogger.WriteLogMessage(e.Message, eTypeLog.Error);
                e = e.InnerException;
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
    }

    
}