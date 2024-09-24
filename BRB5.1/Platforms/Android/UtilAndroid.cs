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

namespace BRB6.PlatformDependency
{
    internal class UtilAndroid
    {
        void InstallAPK()
        {
            var context = Android.App.Application.Context;
            var path = Path.Combine(Android.App.Application.Context.GetExternalFilesDir("").AbsolutePath, "BRB6.apk");
            Java.IO.File file = new Java.IO.File(path);

            using (Android.Content.Intent install = new Android.Content.Intent(Android.Content.Intent.ActionView))
            {
                Android.Net.Uri apkURI = AndroidX.Core.Content.FileProvider.GetUriForFile(context, context.ApplicationContext.PackageName + ".provider", file);
                install.SetDataAndType(apkURI, "application/vnd.android.package-archive");
                install.AddFlags(Android.Content.ActivityFlags.NewTask);
                install.AddFlags(Android.Content.ActivityFlags.GrantReadUriPermission);
                install.AddFlags(Android.Content.ActivityFlags.ClearTop);
                install.PutExtra(Android.Content.Intent.ExtraNotUnknownSource, true);

                Platform.CurrentActivity.StartActivity(install);
            }
        }
    }
}
/*
 * <uses-permission android:name="android.permission.INSTALL_PACKAGES" />
<uses-permission android:name="android.permission.REQUEST_INSTALL_PACKAGES" />
<uses-permission android:name="android.permission.REQUEST_DELETE_PACKAGES" />
<uses-permission android:name="android.permission.DELETE_PACKAGES" />

*/