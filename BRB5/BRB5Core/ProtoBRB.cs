using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Storage;

namespace BRB5
{
    public class ProtoBRB
    {
        public static string GetPathDB
        {
            get
            {
                string Dir = Path.GetTempPath();
                // TODO Xamarin.Forms.Device.RuntimePlatform is no longer supported. Use Microsoft.Maui.Devices.DeviceInfo.Platform instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
                if (Device.RuntimePlatform == Device.Android)
                {
                    Dir = Path.Combine(FileSystem.AppDataDirectory, "db");
                    if (!Directory.Exists(Dir))
                        Directory.CreateDirectory(Dir);
                }
                else
                if (Device.RuntimePlatform == Device.iOS)
                    Dir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

                return Dir;
            }
        }
    }
}
