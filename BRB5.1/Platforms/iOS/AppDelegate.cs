using BL;
using BRB5.Model;
using Foundation;
using UIKit;
using Utils;

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
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            ProtoBRB.SetPath(Path.Combine(documentsPath, "..", "Library", "Downloads"));
            
            Config.NativeBase = new Native();
            return base.FinishedLaunching(app, options);
        }
    }
}
