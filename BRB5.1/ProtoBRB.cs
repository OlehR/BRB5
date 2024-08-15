using BL;
using BRB5;
using Microsoft.Maui.Devices;
using BRB5.Model;
using Utils;
using System.Globalization;

namespace BRB6
{
    public class ProtoBRB
    {
        public static string GetPathDB
        {
            get
            {
                string Dir = Path.GetTempPath();
                // TODO Xamarin.Forms.Device.RuntimePlatform is no longer supported. Use Microsoft.Maui.Devices.DeviceInfo.Platform instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
                if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    Dir = Path.Combine(FileSystem.AppDataDirectory, "db");
                    if (!Directory.Exists(Dir))
                        Directory.CreateDirectory(Dir);
                }
                else
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                    Dir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

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
            Config.NativeBase = new Native();
            Config.TypeScaner = GetTypeScaner();
        }
        public static void SetPath(string pPathDownloads)
        {
            Config.PathDownloads = pPathDownloads;
            FileLogger.PathLog = Path.Combine(Config.PathDownloads, "Log");
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
            return eTypeScaner.Camera;
        }
    }
}
