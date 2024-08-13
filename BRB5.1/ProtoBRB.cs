using Microsoft.Maui.Devices;

namespace BRB51
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
    }
}
