using BL;
using BRB5.Model;
using Foundation;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Forms.PlatformConfiguration;

namespace BRB5.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
           // AppCenter.Start("c3093621-a22d-4aa8-85c5-877eb4cb439a", typeof(Analytics), typeof(Crashes));

            ZXing.Net.Mobile.Forms.iOS.Platform.Init();
            SQLitePCL.Batteries.Init();
            global::Xamarin.Forms.Forms.Init();

            //DisplayCrashReport();

            FFImageLoading.Forms.Platform.CachedImageRenderer.Init();
            DB.BaseDir = ProtoBRB.GetPathDB;
            LoadApplication(new App());

            // Отримання шляху до каталогу завантажень
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Config.PathDownloads = Path.Combine(documentsPath, "..", "Library", "Downloads");
            Config.NativeBase = new Native();

            return base.FinishedLaunching(app, options);
        }
        /*
        internal static void LogUnhandledException(Exception exception)
        {
            try
            {
                const string errorFileName = "Fatal.log";
                var libraryPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal); // iOS: Environment.SpecialFolder.Resources
                var errorFilePath = Path.Combine(libraryPath, errorFileName);
                var errorMessage = String.Format("Time: {0}\r\nError: Unhandled Exception\r\n{1}",
                DateTime.Now, exception.ToString());
                File.WriteAllText(errorFilePath, errorMessage);

            }
            catch
            {
                // just suppress any error logging exceptions
            }
        }
        [Conditional("DEBUG")]
        private static void DisplayCrashReport()
        {
            const string errorFilename = "Fatal.log";
            var libraryPath = Environment.GetFolderPath(Environment.SpecialFolder.Resources);
            var errorFilePath = Path.Combine(libraryPath, errorFilename);

            if (!File.Exists(errorFilePath))
            {
                return;
            }

            var errorText = File.ReadAllText(errorFilePath);
            var alertView = new UIAlertView("Crash Report", errorText, null, "Close", "Clear") { UserInteractionEnabled = true };
            alertView.Clicked += (sender, args) =>
            {
                if (args.ButtonIndex != 0)
                {
                    File.Delete(errorFilePath);
                }
            };
            alertView.Show();
        }*/
    }
}
