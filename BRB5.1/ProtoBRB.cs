using BL;
using BRB5;
using Microsoft.Maui.Devices;
using BRB5.Model;
using Utils;
using System.Globalization;

namespace BRB6
{
    public static class ProtoBRB
    {
        public static string WorkPath { get { 
                string Res = "";
                if (DeviceInfo.Platform == DevicePlatform.Android)
                    Res = FileSystem.AppDataDirectory;
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                    Res = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                return Res;
            } }
        
        public static string GetPathDB
        {
            get
            {
                string Dir = Path.Combine(WorkPath, "db");
                if (!Directory.Exists(Dir))
                    Directory.CreateDirectory(Dir);
                return Dir;
            }
        }
        public static void Init()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            Config.Ver = int.Parse(AppInfo.VersionString.Replace(".", ""));
            Config.Manufacturer = DeviceInfo.Manufacturer;
            Config.Model = DeviceInfo.Model;           
            Config.TypeScaner = GetTypeScaner();
        }
        public static void SetPath(string pPathDownloads)
        {
            Config.PathDownloads = pPathDownloads;
            FileLogger.PathLog = Path.Combine(WorkPath, "Log");
            //2Init();
        }
        public static eTypeScaner GetTypeScaner()
        {
            // TODO Xamarin.Forms.Device.RuntimePlatform is no longer supported. Use Microsoft.Maui.Devices.DeviceInfo.Platform instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
            if (DeviceInfo.Platform == DevicePlatform.iOS)
                return eTypeScaner.Camera;
            if ((Config.Manufacturer.Contains("Zebra Technologies") || Config.Manufacturer.Contains("Motorola Solutions")))
                return eTypeScaner.Zebra;
            if (Config.Model.Equals("PM550") && (Config.Manufacturer.Contains("POINTMOBILE") || Config.Manufacturer.Contains("Point Mobile Co., Ltd.")))
                return eTypeScaner.PM550;
            if (Config.Model.Equals("PM351") && (Config.Manufacturer.Contains("POINTMOBILE") || Config.Manufacturer.Contains("Point Mobile Co., Ltd.")))
                return eTypeScaner.PM351;
            if (Config.Model.Equals("HC61") || Config.Manufacturer.Contains("Bita"))
                return eTypeScaner.BitaHC61;
            if (Config.Model.Equals("C61") || Config.Manufacturer.Contains("CHAINWAY"))
                return eTypeScaner.ChainwayC61;
            if (Config.Model.Equals("M-K4") || Config.Manufacturer.Contains("METAPACE"))
                return eTypeScaner.MetapaceM_K4;
            

            return eTypeScaner.Camera;
        }
        public static Color ToColor(this System.Drawing.Color color)=>Color.FromRgb(color.R, color.G, color.B);
        
    }
}
