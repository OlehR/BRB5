//using BRB5.Model;
using BRB5.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using BRB5;

namespace BRB51
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Config.TypeScaner = GetTypeScaner();

            MainPage = new NavigationPage(new MainPage());//new Docs()); //new Item2(); // 
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
        public static eTypeScaner GetTypeScaner()
        {
            // TODO Xamarin.Forms.Device.RuntimePlatform is no longer supported. Use Microsoft.Maui.Devices.DeviceInfo.Platform instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
            if (Device.RuntimePlatform == Device.iOS)
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
