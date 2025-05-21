using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.AppCompat.App;
using Android.Runtime;
using Android.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Java.Lang;
using Microsoft.Maui;
using Utils;
using static Android.Gms.Common.Apis.Api;
using UtilNetwork;
using Android.Util;

namespace BRB6.PlatformDependency
{
    internal class UtilAndroid
    {
        public static async Task InstallAPKAsync(Action<double> pProgress=null)
        {
            if (string.IsNullOrEmpty(BRB5.Model.Config.PathAPK)) return;
            try
            {                
                string path = Path.Combine(Android.App.Application.Context.GetExternalFilesDir("").AbsolutePath, Android.OS.Environment.DirectoryDownloads);
                string DestinationPath = Path.Combine(path, "ua.UniCS.TM.BRB6.apk"); //.zip
                if (File.Exists(DestinationPath))
                    File.Delete(DestinationPath);
                
                //"https://raw.githubusercontent.com/OlehR/BRB5/master/Apk/ua.UniCS.TM.BRB6.zip"
                await Http.DownloadFileWithProgressAsync(BRB5.Model.Config.PathAPK, DestinationPath, pProgress);

                string DestinationPathApk = DestinationPath; /*Path.Combine(path, "ua.UniCS.TM.BRB6.apk");
                if (File.Exists(DestinationPathApk))
                    File.Delete(DestinationPathApk);
                System.IO.Compression.ZipFile.ExtractToDirectory(DestinationPath, path);*/

                var context = Android.App.Application.Context;
                Java.IO.File file = new(DestinationPathApk);

                using Android.Content.Intent install = new(Android.Content.Intent.ActionView);
                Android.Net.Uri apkURI = AndroidX.Core.Content.FileProvider.GetUriForFile(context, context.ApplicationContext.PackageName + ".provider", file);
                install.SetDataAndType(apkURI, "application/vnd.android.package-archive");
                install.AddFlags(Android.Content.ActivityFlags.NewTask);
                install.AddFlags(Android.Content.ActivityFlags.GrantReadUriPermission);
                install.AddFlags(Android.Content.ActivityFlags.ClearTop);
                install.PutExtra(Android.Content.Intent.ExtraNotUnknownSource, true);

                Platform.CurrentActivity.StartActivity(install);
            }
            catch (System.Exception e)
            {
                FileLogger.WriteLogMessage("UtilAndroid", "InstallAPKAsync", e);
            }
        }
        


        /*public static async Task DownloadFileAsync(string pUrl, string pDestinationPath)
        {
            using HttpClient client = new();     
            using HttpResponseMessage response = await client.GetAsync(pUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            using Stream stream = await response.Content.ReadAsStreamAsync();
            using FileStream fileStream = new FileStream(pDestinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
            await stream.CopyToAsync(fileStream);
        }*/

        public static async Task<string> DownloadStringAsync(string pUrl)
        {
            using HttpClient client = new HttpClient();
            using HttpResponseMessage response = await client.GetAsync(pUrl);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

    }
}
/*
 * <uses-permission android:name="android.permission.INSTALL_PACKAGES" />
<uses-permission android:name="android.permission.REQUEST_INSTALL_PACKAGES" />
<uses-permission android:name="android.permission.REQUEST_DELETE_PACKAGES" />
<uses-permission android:name="android.permission.DELETE_PACKAGES" />

*/