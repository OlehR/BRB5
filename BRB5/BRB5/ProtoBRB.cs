using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace BRB5
{
    public class ProtoBRB
    {
        public static string GetPathDB
        {
            get
            {
                string Dir = Path.GetTempPath();
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
