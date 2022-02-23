using BRB5.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BRB5.Connector
{
    public class Api
    {
        public int CodeData { get; set; }
        public string Login { get; set; }
        public string PassWord { get; set; }
        public string SerialNumber { get; set; }
        public string NameDCT { get; set; }
        public int Ver { get; set; }
        public int CodeWarehouse { get; set; }
        public Api(int pCodeData) : this()
        {
            CodeData = pCodeData;
        }
        public Api()
        {
            CodeWarehouse = Config.CodeWarehouse;
            Login = Config.Login;
            PassWord = Config.Password;
            Ver = Config.Ver;
            SerialNumber = Config.SN;
        }
    }

    public class ApiPrice : Api
    {
        public ApiPrice() : base() { }
        public ApiPrice(int pCodeData, ParseBarCode pPBC) : base(pCodeData)
        {
            BarCode = pPBC.BarCode;
            CodeWares = pPBC.CodeWares;
            Article = pPBC.Article;
        }
        public string BarCode { get; set; }
        public int CodeWares { get; set; }
        public int Article { get; set; }
    }

    public class ApiDoc : Api
    {
        public ApiDoc() : base() { }
        public ApiDoc(int pCodeData, int pTypeDoc) : base(pCodeData)
        {
            TypeDoc = pTypeDoc;
        }
        public int TypeDoc { get; set; }

    }

    public class ConnectorPSU : Connector
    {
        public override Result Login(string pLogin, string pPassWord, eLoginServer pLoginServer)
        {
            string data = JsonConvert.SerializeObject(new Api() { CodeData = 1, Login = pLogin, PassWord = pPassWord }); //"{\"CodeData\": \"1\"" + ", \"Login\": \"" + pLogin + "\"" + ", \"PassWord\": \"" + pPassWord + "\"}";
            HttpResult result = Http.HTTPRequest(0, "", data, "application/json");//

            if (result.HttpState != eStateHTTP.HTTP_OK)
                return new Result(result, $"Ви не підключені до мережі {Config.Company}\n{result.Result}");
            else
                try
                {
                    var r = JsonConvert.DeserializeObject<Result>(result.Result);
                    if (r.State == 0)
                        Config.Role = eRole.Admin;
                    return r;
                }
                catch (Exception e)
                {
                    return new Result(-1, e.Message);
                }

        }

        public override WaresPrice GetPrice(ParseBarCode pBC)
        {
            string data = JsonConvert.SerializeObject(new ApiPrice(154, pBC));
            HttpResult result = Http.HTTPRequest(0, "", data, "application/json");

            if (result.HttpState != eStateHTTP.HTTP_OK)
                return new WaresPrice(result);
            else
                try
                {
                    var r = JsonConvert.DeserializeObject<WaresPrice>(result.Result);
                    return r;
                }
                catch (Exception e)
                {
                    return new WaresPrice(-1, e.Message);
                }
        }

        /// <summary>
        /// Список Документів доступних по ролі
        /// </summary>
        /// <param name="pRole"></param>
        /// <returns></returns>
        public override IEnumerable<TypeDoc> GetTypeDoc(eRole pRole)
        {
            var Res = new List<TypeDoc>()
            { new TypeDoc() { CodeDoc = 0, KindDoc = eKindDoc.PriceCheck, NameDoc = "Прайсчекер" },
                                         new TypeDoc() { CodeDoc = 11, KindDoc = eKindDoc.Raiting, NameDoc = "Опитування" }, };
            return Res;
        }

        public override Result LoadGuidData(bool IsFull, ObservableInt pProgress)
        {
            return LoadDocsData(-1, null, null, true);
        }

        /// <summary>
        /// Завантаження документів в ТЗД (HTTP)
        /// </summary>
        /// <param name="pTypeDoc">0-всі документи</param>
        /// <param name="pNumberDoc"></param>
        /// <param name="pProgress"></param>
        /// <param name="pIsClear"></param>
        /// <returns></returns>
        public override Result LoadDocsData(int pTypeDoc, string pNumberDoc, ObservableInt pProgress, bool pIsClear) 
        {
            string data = JsonConvert.SerializeObject(new ApiDoc() { CodeData = 150, TypeDoc = -1 });
            HttpResult result = Http.HTTPRequest(0, "", data, "application/json");//

            if (result.HttpState != eStateHTTP.HTTP_OK)
            {
                string[] lines = result.Result.Split(new String[] { ";;;" }, StringSplitOptions.None);
                foreach (var el in lines)
                    db.db.ExecuteNonQuery(el);
            }
            return null;
        }


        class WarehouseIn
        {
            public IEnumerable<Warehouse> Warehouse { get; set; }
        }

        public override IEnumerable<Warehouse> LoadWarehouse()
        {
            string data = JsonConvert.SerializeObject(new Api() { CodeData = 210 });
            HttpResult result = Http.HTTPRequest(1001, "", data, "application/json");//

            if (result.HttpState != eStateHTTP.HTTP_OK)
            {
                var r = JsonConvert.DeserializeObject<WarehouseIn>(result.Result);
                db.ReplaceWarehouse(r.Warehouse);
                return r.Warehouse;
            }
            return null;
        }

    }

}

