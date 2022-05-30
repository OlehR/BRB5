using BRB5;
using BRB5.Connector;
using BRB5.Model;
using Newtonsoft.Json;
//using BRB5.Model;
using System;
using System.Collections.Generic;
using System.IO;
using Utils;

namespace Test
{
    class Program
    {
        static public int[] answers { get; set; } = { 1, 2, 4 };
        static public int RatingTemplate { get 
            { int r = 0; 
                for (int i = 0; i < answers.Length; i++) 
                    r += 1 << (answers[i]-1); 
                
                return r; } 
        }

        static void Main(string[] args)
        {
            var a= RatingTemplate;
            return;
          /*
            var ftp = new FTP();
            string ss = $"{DateTime.Now:yyyy.MM.dd}";
            ftp.CreateDir($"Data/{ss}");
            ftp.DownLoad("Template/User.json", "d:/Temp");*/
            try
            {
                Console.WriteLine("Hello World!");

                DB db = new DB();
                db.SetConfig<eCompany>("Company", eCompany.Sim23FTP);
                //Config.GetPathFiles = @"d:\Temp";
                var c = Connector.GetInstance();
                c.Login("test", "1", eLoginServer.Offline);
                /*  //  var D = new List<Doc>() { new Doc() { TypeDoc = 11, NumberDoc = "1", DateDoc= DateTime.Now.Date, NameUser = "Рутковський J", Description = "ТЗ 1001" } };
                   // db.ReplaceDoc(D);
                    var R = new List<Raiting>() {
                         new Raiting() { TypeDoc = 11, NumberDoc = "1", Id = 1,  Parent=0,  IsHead = true,  RatingTemplate=1+2+4+8 ,Text = "Чистота" , Order=1} ,
                         new Raiting() { TypeDoc = 11, NumberDoc = "1", Id = 2,  Parent=1,  IsHead = false, RatingTemplate=1+2+4+8 ,Text = "Чистотf Кавомашини", Order=2 } ,
                         new Raiting() { TypeDoc = 11, NumberDoc = "1", Id = 3,  Parent=1,  IsHead = false, RatingTemplate=1+2+4+8 ,Text = "Чистота всього іншого" , Order=3} ,
                         new Raiting() { TypeDoc = 11, NumberDoc = "1", Id = 10, Parent=0,  IsHead = true, RatingTemplate=1+2+4+8 ,Text = "Планограми", Order=4 } ,
                         new Raiting() { TypeDoc = 11, NumberDoc = "1", Id = 11, Parent=10, IsHead = false, RatingTemplate=1+2+4+8 ,Text = "Планограма Хімії", Order=5   } ,
                         new Raiting() { TypeDoc = 11, NumberDoc = "1", Id = 12, Parent=10, IsHead = false, RatingTemplate=1+2+4+8 ,Text = "Планограма Супутка" , Order=7 } ,
                         new Raiting() { TypeDoc = 11, NumberDoc = "1", Id = 13, Parent=10, IsHead = false, RatingTemplate=1+2+4+8 ,Text = "Планограма Молочка", Order=8  } ,
                         new Raiting() { TypeDoc = 11, NumberDoc = "1", Id = 14, Parent=10, IsHead = false, RatingTemplate=1+2+4+8 ,Text = "Планограма Сигарет" , Order=9 } ,
                         new Raiting() { TypeDoc = 11, NumberDoc = "1", Id = 15, Parent=10, IsHead = false, RatingTemplate=1+2+4+8 ,Text = "Планограма Сири", Order=10  } ,
                         new Raiting() { TypeDoc = 11, NumberDoc = "1", Id = 15, Parent=10, IsHead = false, RatingTemplate=1+2+4+8 ,Text = "Планограма Алкоголь", Order=6  } ,
                         new Raiting() { TypeDoc = 11, NumberDoc = "1", Id = 15, Parent=10, IsHead = false, RatingTemplate=1+2+4+8 ,Text = "Планограма Овочі" , Order=20 } ,
                         new Raiting() { TypeDoc = 11, NumberDoc = "1", Id = 15, Parent=10, IsHead = false, RatingTemplate=1+2+4+8 ,Text = "Планограма Фрукти", Order=21 } ,
                         new Raiting() { TypeDoc = 11, NumberDoc = "1", Id = 15, Parent=10, IsHead = false, RatingTemplate=1+2+4+8 ,Text = "Планограма Бакалія" , Order=22 } ,
                };


                    var xx = R.ToJSON();                
                    db.ReplaceRaitingSample(R);*/

              
                var xxxx= db.GetConfig<eCompany>("Company");

                db.SetConfig<string>("ApiUrl1", "http://api.spar.uz.ua/znp/");//"http://93.183.216.37:80/dev1/hs/TSD/"); //
                db.SetConfig<string>("ApiUrl2", "http://api.spar.uz.ua/print/"); //"http://93.183.216.37/TK/hs/TSD/;http://37.53.84.148/TK/hs/TSD/");//
                db.SetConfig<string>("ApiUrl3", "https://bitrix.sim23.ua/rest/233/ax02yr7l9hia35vj/");


                Config.Ver = 5000;
                Config.CodeWarehouse = 9;
                Config.SN = "xxxx";
                Config.Login = "nov";
                Config.Password = "123";

                var C = Connector.GetInstance();
                C.LoadDocsData(11, null, null, false);

            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static  Result Login(string pLogin, string pPassWord, eLoginServer pLoginServer)
        {
            Result Res;

            var Ftp = new FTP();
            DB db = new DB();

            string ss = $"{DateTime.Now:yyyy.MM.dd}";
            //Ftp.CreateDir($"Data/{ss}");
            try
            {

                if (Ftp.DownLoad("Template/User.json", Config.GetPathFiles))
                {
                    var strU = File.ReadAllText(Path.Combine(Config.GetPathFiles, "User.json"));
                    var U = JsonConvert.DeserializeObject<List<User>>(strU);
                    db.ReplaceUser(U);

                    var strWh = File.ReadAllText(Path.Combine(Config.GetPathFiles, "Warehouse.json"));
                    var Wh = JsonConvert.DeserializeObject<List<Warehouse>>(strWh);
                    db.ReplaceWarehouse(Wh);
                }
            }
            catch (Exception e)
            {
                Res = new Result(-1, e.Message);
                FileLogger.WriteLogMessage($"ConnectorSE_FTP.Login=>() Res=>({Res.State},{Res.Info},{Res.TextError})", eTypeLog.Error);
                return Res;
            }

            var res = db.GetUserLogin(new User() { Login = pLogin.Trim(), PassWord = pPassWord.Trim() });
            if (res != null && res.CodeUser > 0)
            {
                return new Result(0);
            }
            return new Result(-1, "Невірний логін чи пароль");
        }
    }
}
