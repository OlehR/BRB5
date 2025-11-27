using BRB5.Model;
using Foundation;
using UIKit;


namespace BRB6
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            SQLitePCL.Batteries_V2.Init();
            SQLitePCL.raw.sqlite3_config(SQLitePCL.raw.SQLITE_CONFIG_MULTITHREAD);
            //FFImageLoading.Forms.Platform.CachedImageRenderer.Init();
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            ProtoBRB.SetPath(Path.Combine(documentsPath, "..", "Library", "Downloads"));
            
            Config.NativeBase = new Native();
            return base.FinishedLaunching(app, options);
        }
    }
}
