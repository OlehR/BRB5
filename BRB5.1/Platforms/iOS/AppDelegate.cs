using BL;
using BRB5.Model;
using Foundation;
using System.Net.NetworkInformation;
using UIKit;

namespace BRB6
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            SQLitePCL.Batteries.Init();

            //FFImageLoading.Forms.Platform.CachedImageRenderer.Init();
            DB.BaseDir = ProtoBRB.GetPathDB;
            //LoadApplication(new App());

            // Отримання шляху до каталогу завантажень
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Config.PathDownloads = Path.Combine(documentsPath, "..", "Library", "Downloads");
            Config.NativeBase = new Native();

            return base.FinishedLaunching(app, options);
        }
    }
}
