using BRB5;
using BRB5.Connector;
using BRB5.Model;
//using BRB5.Model;
using System;
using System.Collections.Generic;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Hello World!");

                DB db = new DB();
                var D = new List<Doc>() { new Doc() { TypeDoc = 11, NumberDoc = "1", DateDoc= DateTime.Now.Date.ToString("yyyy-MM-dd"), NameUser = "Рутковський J", Description = "ТЗ 1001" } };
                db.ReplaceDoc(D);
                var R = new List<Raiting>() {
                     new Raiting() { TypeDoc = 11, NumberDoc = "1", Id = 1, IsHead = true,  RatingTemplate=1+2+4+8 ,Text = "Чистота" } ,
                     new Raiting() { TypeDoc = 11, NumberDoc = "1", Id = 2, IsHead = false, RatingTemplate=1+2+4+8 ,Text = "Чистотf Кавомашини" } ,
            };


                
                db.ReplaceRaitingSample(R);
      
                db.SetConfig<eCompany>("Company", eCompany.SparPSU);

                var xxxx= db.GetConfig<eCompany>("Company");

                db.SetConfig<string>("ApiUrl1", "http://api.spar.uz.ua/znp/");//"http://93.183.216.37:80/dev1/hs/TSD/"); //
                db.SetConfig<string>("ApiUrl2", "http://api.spar.uz.ua/print/"); //"http://93.183.216.37/TK/hs/TSD/;http://37.53.84.148/TK/hs/TSD/");//
                
                Config.Ver = 5000;
                Config.CodeWarehouse = 9;
                Config.SN = "xxxx";
                Config.Login = "nov";
                Config.Password = "123";

                var C = Connector.GetInstance();
                var xx = C.Login(Config.Login, Config.Password);
                    
                    C.GetPrice(new BRB5.Model.ParseBarCode() { BarCode = "4000521000301" });

            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
