//using BRB5.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BRB5
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            //TMP!!!!
            DB db = new DB();
            db.SetConfig<eCompany>("Company", eCompany.SparPSU);
            db.SetConfig<string>("ApiUrl1", "http://api.spar.uz.ua/znp/");
            db.SetConfig<string>("ApiUrl2", "http://api.spar.uz.ua/print/");
            
            var D = new List<Doc>() { 
                new Doc() { TypeDoc =11,NumberDoc="1", DateDoc = DateTime.Now.Date.ToString("yyyy-MM-dd"), NameUser ="Рутковський О", ExtInfo="1001", Description="ТЗ 1001"},
                new Doc() { TypeDoc =11,NumberDoc="SE00002", DateDoc = DateTime.Now.Date.ToString("yyyy-MM-dd"), NameUser ="Пупкін О", ExtInfo="1104" ,Description="ТЗ 1104"},
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
    }
}
