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
            /*var D = new List<Doc>() { 
                new Doc() { TypeDoc =11,NumberDoc="1", DateDoc = DateTime.Now.Date, NameUser ="Рутковський О", ExtInfo="1001", Description="ТЗ 1001"},
                new Doc() { TypeDoc =11,NumberDoc="SE00002", DateDoc = DateTime.Now.Date, NameUser ="Пупкін О", ExtInfo="1104" ,Description="ТЗ 1104"},
            };
            db.ReplaceDoc(D);
            var R = new List<Raiting>() {  
                     new Raiting() { TypeDoc = 11, NumberDoc = "1", Id = 1,  Parent=0,  IsHead = true,  RatingTemplate=1+2+4+8 ,Text = "Чистота" } ,
                     new Raiting() { TypeDoc = 11, NumberDoc = "1", Id = 2,  Parent=1,  IsHead = false, RatingTemplate=1+2+4+8 ,Text = "Чистотf Кавомашини" } ,
                     new Raiting() { TypeDoc = 11, NumberDoc = "1", Id = 3,  Parent=1,  IsHead = false, RatingTemplate=1+2+4+8 ,Text = "Чистота всього іншого" } ,
                     new Raiting() { TypeDoc = 11, NumberDoc = "1", Id = 10, Parent=0,  IsHead = true, RatingTemplate=1+2+4+8 ,Text = "Планограми" } ,
                     new Raiting() { TypeDoc = 11, NumberDoc = "1", Id = 11, Parent=10, IsHead = false, RatingTemplate=1+2+4+8 ,Text = "Планограма Хімії"  } ,
                     new Raiting() { TypeDoc = 11, NumberDoc = "1", Id = 12, Parent=10, IsHead = false, RatingTemplate=1+2+4+8 ,Text = "Планограма Супутка" } ,
                     new Raiting() { TypeDoc = 11, NumberDoc = "1", Id = 13, Parent=10, IsHead = false, RatingTemplate=1+2+4+8 ,Text = "Планограма Молочка" } ,
                     new Raiting() { TypeDoc = 11, NumberDoc = "1", Id = 14, Parent=10, IsHead = false, RatingTemplate=1+2+4+8 ,Text = "Планограма Сигарет" } ,
                     new Raiting() { TypeDoc = 11, NumberDoc = "1", Id = 15, Parent=10, IsHead = false, RatingTemplate=1+2+4+8 ,Text = "Планограма Сири" } ,
                     new Raiting() { TypeDoc = 11, NumberDoc = "1", Id = 15, Parent=10, IsHead = false, RatingTemplate=1+2+4+8 ,Text = "Планограма Алкоголь" } ,
                     new Raiting() { TypeDoc = 11, NumberDoc = "1", Id = 15, Parent=10, IsHead = false, RatingTemplate=1+2+4+8 ,Text = "Планограма Овочі" } ,
                     new Raiting() { TypeDoc = 11, NumberDoc = "1", Id = 15, Parent=10, IsHead = false, RatingTemplate=1+2+4+8 ,Text = "Планограма Фрукти" } ,
                     new Raiting() { TypeDoc = 11, NumberDoc = "1", Id = 15, Parent=10, IsHead = false, RatingTemplate=1+2+4+8 ,Text = "Планограма Бакалія" } ,
            };            
            db.ReplaceRaitingSample(R);
            */

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
