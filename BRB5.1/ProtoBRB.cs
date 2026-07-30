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
                return eTypeScaner.iOS;
            if ((Config.Manufacturer.Contains("Zebra Technologies") || Config.Manufacturer.Contains("Motorola Solutions")))
                return Config.Model.StartsWith("TC56")? eTypeScaner.ZebraWithOutKeyBoard : eTypeScaner.Zebra;
            if (Config.Model.Equals("PM550") && (Config.Manufacturer.Contains("POINTMOBILE") || Config.Manufacturer.Contains("Point Mobile Co., Ltd.")))
                return eTypeScaner.PM550;
            if (Config.Model.Equals("PM351") && (Config.Manufacturer.Contains("POINTMOBILE") || Config.Manufacturer.Contains("Point Mobile Co., Ltd.")))
                return eTypeScaner.PM351;
            if (Config.Model.Equals("PM451") && (Config.Manufacturer.Contains("POINTMOBILE") || Config.Manufacturer.Contains("Point Mobile Co., Ltd.")))
                return eTypeScaner.PM351;
            if (Config.Model.Equals("PM84") && (Config.Manufacturer.Contains("POINTMOBILE") || Config.Manufacturer.Contains("Point Mobile Co., Ltd.")))
                return eTypeScaner.PM84;
            if (Config.Model.Equals("PM68") && (Config.Manufacturer.Contains("POINTMOBILE") || Config.Manufacturer.Contains("Point Mobile Co., Ltd.")))
                return eTypeScaner.PM68;
            if (Config.Model.Equals("HC61") || Config.Manufacturer.Contains("Bita"))
                return eTypeScaner.BitaHC61;
            if (Config.Model.Equals("C66") && Config.Manufacturer.Contains("CHAINWAY"))
                return eTypeScaner.ChainwayC66;
            if (Config.Model.Equals("C61") || Config.Manufacturer.Contains("CHAINWAY"))
                return eTypeScaner.ChainwayC61;
            if (Config.Model.Equals("M-K4") || Config.Manufacturer.Contains("METAPACE"))
                return eTypeScaner.MetapaceM_K4;
            if (Config.Model.Equals("NLS-MT67"))
                return eTypeScaner.NLS_MT67;
            if (Config.Model.Equals("NLS-MT93") || Config.Manufacturer.Contains("Newland"))
                return eTypeScaner.NLS_MT93;

            return eTypeScaner.Camera;
        }
        public static Color ToColor(this System.Drawing.Color color)=>Color.FromRgb(color.R, color.G, color.B);

        public static void PlayNativeBeep()
        {
#if WINDOWS
    // Для Windows працює класичний системний сигнал
    System.Console.Beep(800, 200); // 800 Гц, 200 мілісекунд
#elif ANDROID
            // Для Android використовуємо нативний ToneGenerator
            var toneGen = new Android.Media.ToneGenerator(Android.Media.Stream.Music, 100);
            // Tone.PropBip — стандартний однотонний сигнал (можна замінити на KeyConfirm або DTMF)
            toneGen.StartTone(Android.Media.Tone.PropBeep, 150);
#elif IOS || MACCATALYST
    // Для iOS відтворюємо системний звук (1052 — це звук повідомлення/Beep)
    //AudioToolbox.SystemSound.FromFile(1052).PlaySystemSound();
#endif
        }

    }
}
