using BRB5.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Utils;
using System.Linq;
using System.Collections.ObjectModel;
using System.Net.Mime;
using System.Threading.Tasks;
using BRB5;
using System.Security.Cryptography;
using UtilNetwork;

namespace BL.Connector
{
    public class ConnectorPSU : ConnectorBase
    {
        ConnectorUniversal CU =  new ConnectorUniversal();
        public override IEnumerable<LoginServer> LoginServer() { return new List<LoginServer>()
            {new  LoginServer (){Code=eLoginServer.Central,Name = "ЦБ"}}; }

        public override async Task<Result> LoginAsync(string pLogin, string pPassWord, eLoginServer pLoginServer)
        {
            string data = JsonConvert.SerializeObject(new Api() { CodeData = 1, Login = pLogin, PassWord = pPassWord }); //"{\"CodeData\": \"1\"" + ", \"Login\": \"" + pLogin + "\"" + ", \"PassWord\": \"" + pPassWord + "\"}";
            HttpResult result = await Http.HTTPRequestAsync(0, "znp/", data, "application/json");//

            if (result.HttpState != eStateHTTP.HTTP_OK)
            {
                FileLogger.WriteLogMessage($"ConnectorPSU.Login=>(pLogin=>{pLogin}, pPassWord=>{pPassWord},pLoginServer=>{pLoginServer}) Res=>{result.HttpState}",eTypeLog.Error);
                return new Result(result, $"Ви не підключені до мережі {Config.Company}\n{result.Result}");
            }
            else
                try
                {
                    var r = JsonConvert.DeserializeObject<Result>(result.Result);
                    if (r.State == 0)
                        Config.Role = eRole.Admin;
                    FileLogger.WriteLogMessage($"ConnectorPSU.Login=>(pLogin=>{pLogin}, pPassWord=>{pPassWord},pLoginServer=>{pLoginServer}) Res=>({r.State},{r.Info},{r.TextError})", eTypeLog.Error);

                    return r;
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                    return new Result(e);
                }

        }

        public override ParseBarCode ParsedBarCode(string pBarCode, bool pIsHandInput) 
        {
            ParseBarCode res = new ParseBarCode();
            if (string.IsNullOrEmpty( pBarCode))
                return res;
            pBarCode = pBarCode.Trim();
            res.StartString = pBarCode;
            res.BarCode = pBarCode;
            res.IsHandInput = pIsHandInput;           

            if (pIsHandInput && pBarCode.Length <= 8 && !pBarCode.Equals(""))
            {
                try
                {
                    res.Article = Convert.ToInt32( pBarCode);
                    res.BarCode = null;
                    return res;
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                }
            }

            if (pBarCode != null)
            {
                //Utils.WriteLog("e", TAG, "ParsedBarCode=> " + pBarCode.charAt(0) + " " + pBarCode.Contains("|"));
                if (pBarCode.Contains("|") && pBarCode[0] == 'Б')
                {
                    res.CodeWares = 200000000 + Convert.ToInt32(pBarCode.Substring(1, 9));
                    res.Quantity = 1;
                    res.BarCode = null;
                }
                else
                if (pBarCode.Contains("-"))
                {
                    try
                    {
                        String[] str = pBarCode.Split('-');
                        if (str.Length == 3 )
                            res.PriceOpt = str[2].ToInt() / 100m;
                        if (str.Length >= 2) {
                            res.Price = Convert.ToInt32(str[1]) / 100m;
                            res.CodeWares = Convert.ToInt32(str[0]);
                            res.BarCode = null;
                        }                                
                    }
                    catch (Exception e)
                    {
                        FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                    }
                }
                else
                if (pBarCode.Length == 13)
                {
                    //  Log.e("XXX",number+' ' +number.Substring(0,1));
                    if (pBarCode.Substring(0, 2).Equals("22"))
                    {
                        res.Article = Convert.ToInt32(pBarCode.Substring(2, 6));
                        String Quantity = pBarCode.Substring(8, 4);
                        res.Quantity = Convert.ToDecimal(Quantity) / 1000m;
                        // Log.e("XXX",Article+" "+ Quantity );
                    }

                    if (pBarCode.Substring(0, 3).Equals("111"))
                    {
                        //isBarCode=false;
                        res.Article = Convert.ToInt32(pBarCode.Substring(3, 6));
                        String Quantity = pBarCode.Substring(9, 3);
                        res.Quantity = Convert.ToDecimal(Quantity);
                        //Log.e("XXX",Article+" "+ Quantity );
                    }

                    if (res.Article >0)
                    {
                        //res.Article = "00" + res.Article;
                        res.BarCode = null;
                    }
                }
            }
            return res;
        }

        public override WaresPrice GetPrice(ParseBarCode pBC, eTypePriceInfo pTP=eTypePriceInfo.Short)
        {
            Config.OnProgress?.Invoke(0.3d);
            WaresPrice res;
            string data = JsonConvert.SerializeObject(new ApiPrice(154, pBC) { TypePriceInfo= pTP });
            HttpResult result = Http.HTTPRequest(0, "DCT/GetPrice/", data, "application/json",null,null,2);
            Config.OnProgress?.Invoke(0.8d);
            if (result.HttpState != eStateHTTP.HTTP_OK)
                res = new WaresPrice(result);
            else
                try
                {
                    var r = JsonConvert.DeserializeObject<Result<WaresPrice>>(result.Result);
                    res = r.Info;
                    res.StateHTTP = result.HttpState;                    
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                    return new WaresPrice(-1, e.Message);
                }
            res.ParseBarCode = pBC;
            Config.OnProgress?.Invoke(0.9d);
            return res;
        }

        /// <summary>
        /// Збереження Просканованих товарів в 1С
        /// </summary>
        /// <param name="pLogPrice"></param>
        /// <returns></returns>
        public override Result SendLogPrice(IEnumerable<LogPrice> pLogPrice)
        {
            if (pLogPrice != null && pLogPrice.Count() < 1)
                return new Result(-1, "Відсутні дані на відправку");
            return CU.SendLogPrice(pLogPrice);
            /*if (pLogPrice != null && pLogPrice.Count() < 1)
                return new Result(-1, "Відсутні дані на відправку");
            var Data = pLogPrice.Select(el => el.GetPSU());
            string data = JsonConvert.SerializeObject(new ApiLogPrice(Data));
            HttpResult result = Http.HTTPRequest(0, "znp/", data, "application/json");
            return new Result(result);*/
        }

        /// <summary>
        /// Список Документів доступних по ролі
        /// </summary>
        /// <param name="pRole"></param>
        /// <returns></returns>
        public override IEnumerable<TypeDoc> GetTypeDoc(eRole pRole, eLoginServer pLS, eGroup pGroup = eGroup.NotDefined)
        {
            //var Res = new List<TypeDoc>()
            //{
            //        new TypeDoc() { Group= eGroup.Price, CodeDoc = 101, NameDoc = "Цінники" , KindDoc = eKindDoc.NotDefined},
            //        new TypeDoc() { Group= eGroup.Doc, CodeDoc = 102, NameDoc = "Документи" , KindDoc = eKindDoc.NotDefined},
            //        new TypeDoc() { Group= eGroup.FixedAssets, CodeDoc = 103, NameDoc = "Основні засоби" , KindDoc = eKindDoc.NotDefined},
            //        new TypeDoc() { Group= eGroup.Raiting, CodeDoc = 104, NameDoc = "Опитування" , KindDoc = eKindDoc.NotDefined},
            //        new TypeDoc() { Group= eGroup.Price, CodeDoc = 0, NameDoc = "Прайсчекер" , KindDoc = eKindDoc.PriceCheck},
            //        new TypeDoc() { Group= eGroup.Price, CodeDoc = 15, NameDoc = "Подвійний сканер" , KindDoc = eKindDoc.PriceCheck},
            //        new TypeDoc() {Group = eGroup.Price,  CodeDoc = 13, NameDoc = "Перевірка Акцій", KindDoc = eKindDoc.PlanCheck },
            //        new TypeDoc() { Group= eGroup.Price, CodeDoc = 14, NameDoc = "Знижки -%50%", KindDoc = eKindDoc.Normal },
            //        new TypeDoc() { Group= eGroup.Raiting, CodeDoc = 11, NameDoc = "Опитування", KindDoc = eKindDoc.RaitingDoc, DayBefore = 4 },
            //        new TypeDoc() { Group= eGroup.Raiting, CodeDoc = -1, NameDoc = "Шаблони Опитування", KindDoc = eKindDoc.RaitingTempate },
            //        new TypeDoc() { Group= eGroup.Raiting, CodeDoc = 12, NameDoc = "Керування Опитуваннями", KindDoc = eKindDoc.RaitingTemplateCreate },
            //        new TypeDoc() { Group= eGroup.Doc, CodeDoc =  1, NameDoc = "Ревізія", TypeControlQuantity = eTypeControlDoc.Ask, IsSaveOnlyScan=false, KindDoc = eKindDoc.Normal },
            //        new TypeDoc() { Group= eGroup.Doc, CodeDoc =  2, NameDoc = "Прихід", TypeControlQuantity = eTypeControlDoc.Ask, IsViewOut=true, KindDoc = eKindDoc.Normal },
            //        new TypeDoc() { Group= eGroup.Doc, CodeDoc =  3, NameDoc = "Переміщення Вих", TypeControlQuantity=eTypeControlDoc.NoControl, KindDoc = eKindDoc.Normal },
            //        new TypeDoc() { Group= eGroup.Doc, CodeDoc =  4, NameDoc = "Списання" ,  KindDoc = eKindDoc.Normal},
            //        new TypeDoc() { Group= eGroup.Doc, CodeDoc =  5, NameDoc = "Повернення" ,  KindDoc = eKindDoc.Normal},
            //        new TypeDoc() { Group= eGroup.Doc, CodeDoc =  8, NameDoc = "Переміщення Вх", TypeControlQuantity=eTypeControlDoc.Ask, IsViewOut=true, IsViewReason=true,KindDoc = eKindDoc.Normal },
            //        new TypeDoc() { Group= eGroup.FixedAssets, CodeDoc =  7, NameDoc = "Ревізія ОЗ", TypeControlQuantity=eTypeControlDoc.Ask, IsSimpleDoc=true, KindDoc = eKindDoc.Normal,CodeApi=1,IsCreateNewDoc=true },
            //};
            //foreach (var el in Res) 
            //{
            //    if (pGroup == eGroup.NotDefined) el.IsVisible = el.KindDoc == eKindDoc.NotDefined;
            //    else el.IsVisible = el.Group == pGroup && el.KindDoc != eKindDoc.NotDefined;
            //} 

            var Res = pGroup switch
            {
                eGroup.NotDefined => new List<TypeDoc>()
                 {
                     new TypeDoc() { Group= eGroup.Price, CodeDoc = 101, NameDoc = "Цінники" , KindDoc = eKindDoc.NotDefined},
                     new TypeDoc() { Group= eGroup.Doc, CodeDoc = 102, NameDoc = "Документи" , KindDoc = eKindDoc.NotDefined},
                     new TypeDoc() { Group= eGroup.FixedAssets, CodeDoc = 103, NameDoc = "Основні засоби" , KindDoc = eKindDoc.NotDefined},
                     new TypeDoc() { Group= eGroup.Raiting, CodeDoc = 104, NameDoc = "Опитування" , KindDoc = eKindDoc.NotDefined},
                 },
                eGroup.Price => new List<TypeDoc>()
                 {
                     new TypeDoc() { Group= eGroup.Price, CodeDoc = 0, NameDoc = "Прайсчекер" , KindDoc = eKindDoc.PriceCheck},
                     new TypeDoc() { Group= eGroup.Price, CodeDoc = 15, NameDoc = "Подвійний сканер" , KindDoc = eKindDoc.PriceCheck},
                     new TypeDoc() {Group = eGroup.Price,  CodeDoc = 13, NameDoc = "Перевірка Акцій", KindDoc = eKindDoc.PlanCheck },
                     new TypeDoc() { Group= eGroup.Price, CodeDoc = 14, NameDoc = "Знижки -%50%", KindDoc = eKindDoc.Normal }

                 },
                eGroup.Raiting => new List<TypeDoc>()
                 {
                     new TypeDoc() { Group= eGroup.Raiting, CodeDoc = 11, NameDoc = "Опитування", KindDoc = eKindDoc.RaitingDoc, DayBefore = 4 },
                     new TypeDoc() { Group= eGroup.Raiting, CodeDoc = -1, NameDoc = "Шаблони Опитування", KindDoc = eKindDoc.RaitingTempate },
                     new TypeDoc() { Group= eGroup.Raiting, CodeDoc = 12, NameDoc = "Керування Опитуваннями", KindDoc = eKindDoc.RaitingTemplateCreate },

                 },
                eGroup.Doc => new List<TypeDoc>()
                 {
                     new TypeDoc() { Group= eGroup.Doc, CodeDoc =  1, NameDoc = "Ревізія", TypeControlQuantity = eTypeControlDoc.Ask, IsSaveOnlyScan=false, KindDoc = eKindDoc.Normal },
                     new TypeDoc() { Group= eGroup.Doc, CodeDoc =  2, NameDoc = "Прихід", TypeControlQuantity = eTypeControlDoc.Ask, IsViewOut=true, KindDoc = eKindDoc.Normal,IsViewReason=true },
                     new TypeDoc() { Group= eGroup.Doc, CodeDoc =  3, NameDoc = "Переміщення Вих", TypeControlQuantity=eTypeControlDoc.NoControl, KindDoc = eKindDoc.Normal },
                     new TypeDoc() { Group= eGroup.Doc, CodeDoc =  4, NameDoc = "Списання" ,  KindDoc = eKindDoc.Normal},
                     new TypeDoc() { Group= eGroup.Doc, CodeDoc =  5, NameDoc = "Повернення" ,  KindDoc = eKindDoc.Normal},
                     new TypeDoc() { Group= eGroup.Doc, CodeDoc =  8, NameDoc = "Переміщення Вх", TypeControlQuantity=eTypeControlDoc.Ask, IsViewOut=true, IsViewReason=true,KindDoc = eKindDoc.Normal },

                 },
                eGroup.FixedAssets => new List<TypeDoc>()
                 {
                     new TypeDoc() { Group= eGroup.FixedAssets, CodeDoc =  7, NameDoc = "Ревізія ОЗ", TypeControlQuantity=eTypeControlDoc.Ask, IsSimpleDoc=true, KindDoc = eKindDoc.Normal,CodeApi=1,IsCreateNewDoc=true },
                 },
            };

            return Res;
        }

        public override async Task<Result> LoadGuidDataAsync(bool IsFull)
        {
            await LoadWarehouse();
            return await LoadDocsDataAsync(-1, null,  true);
        }

        /// <summary>
        /// Завантаження документів в ТЗД (HTTP)
        /// </summary>
        /// <param name="pTypeDoc">0-всі документи</param>
        /// <param name="pNumberDoc"></param>        
        /// <param name="pIsClear"></param>
        /// <returns></returns>
        public override async Task<Result> LoadDocsDataAsync(int pTypeDoc, string pNumberDoc, bool pIsClear)
        {

            if (pTypeDoc == 11)
            {
                var temp = await GetRaitingDocsAsync();
                if (temp.Info != null)
                {
                    foreach (var doc in temp.Info)
                    {
                        doc.TypeDoc = pTypeDoc;
                    }
                    db.ReplaceDoc(temp.Info);
                }
                return new Result();
            }
            else
            {
                string data = JsonConvert.SerializeObject(new ApiDoc() { CodeData = 150, TypeDoc = pTypeDoc, CodeWarehouse = Config.CodeWarehouse, Ver = 5136 });
                HttpResult result = await Http.HTTPRequestAsync(0, "znp/", data, "application/json",null,null,30d);//

                if (result.HttpState == eStateHTTP.HTTP_OK)
                {
                    string[] lines = result.Result.Split(new String[] { ";;;" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var el in lines)
                    {
                        string Sql = el.Replace("_", "").Replace(";;", "").Replace("'ARTICL'", "'ARTICLE'");
                        if (Sql.Length > 20)
                            try
                            {
                                Console.WriteLine(Sql.Substring(0, 20) + $" Length=>{Sql.Length}");
                                db.db.Execute(Sql);
                            }
                            catch (Exception e)
                            {
                                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                            }
                    }
                }
                return null;
            }
        }


        /// <summary>
        /// Вивантаження документів з ТЗД (HTTP)
        /// </summary>
        /// <param name="pDoc"></param>
        /// <param name="pWares"></param>
        /// <param name="pIsClose"></param>
        /// <returns></returns>
        public  override async Task<Result> SendDocsDataAsync(DocVM pDoc, IEnumerable<DocWares> pWares)
        {
            var r = pWares.Select(el => new decimal[] { el.OrderDoc, el.CodeWares, el.InputQuantity });
            var res = new ApiSaveDoc(153, pDoc.TypeDoc, pDoc.NumberDoc, r);
            String data = res.ToJSON();
            try
            {
                HttpResult result = await Http.HTTPRequestAsync(0, "znp/", data, "application/json", null, null);
                if (result.HttpState != eStateHTTP.HTTP_OK)
                {
                    return new Result(result);
                }
                Result Res = JsonConvert.DeserializeObject<Result>(result.Result);
                return Res;
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                return new Result(e);
            }
        }


        class WarehouseIn:Result
        {
            public IEnumerable<object[]> Warehouse  { get; set; }
        }

        public override async Task<Result<IEnumerable<Warehouse>>> LoadWarehouse()
        {
            try
            {
                string data = JsonConvert.SerializeObject(new Api() { CodeData = 210 });
                HttpResult result = Http.HTTPRequest(0, "znp/", data, "application/json", "brb", "brb");//

                if (result.HttpState == eStateHTTP.HTTP_OK)
                {
                    var r = JsonConvert.DeserializeObject<WarehouseIn>(result.Result);

                    var res = r.Warehouse.Select(el => new Warehouse() { Code = Convert.ToInt32(el[0]), Name = el[1].ToString() });
                    db.ReplaceWarehouse(res);
                    return new Result<IEnumerable<Warehouse>>(result, res);
                }
                return new Result<IEnumerable<Warehouse>>(result, null);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                return new Result<IEnumerable<Warehouse>>(e);
            }
        }

        /// <summary>
        /// Друк на стаціонарному термопринтері
        /// </summary>
        /// <param name="codeWares">Список товарів</param>
        /// <returns></returns>        
        public override string PrintHTTP(IEnumerable<long> pCodeWares) 
        {
            string Data = string.Join(",", pCodeWares);

            try
            {
                string json = new ApiPrintHTTP(Data).ToJSON(); //Config.GetApiJson(999, BuildConfig.VERSION_CODE, "\"CodeWares\":\"" + sb.toString() + "\"");
                HttpResult res = Http.HTTPRequest(0, "print/", json, "application/json", null, null);//"http://znp.vopak.local:8088/Print"
                if (res.HttpState == eStateHTTP.HTTP_OK)
                {
                    return res.Result;
                    //JSONObject jObject = new JSONObject(result.Result);
                }
                return res.HttpState.ToString();
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                return e.Message;
            }
        }

        public override async Task<Result<int>> GetIdRaitingTemplate() {
            HttpResult result = await Http.HTTPRequestAsync(0, "DCT/Raitting/GetIdRaitingTemplate", null, "application/json", "brb", "brb");//

            if (result.HttpState == eStateHTTP.HTTP_OK)
            {
                var r = JsonConvert.DeserializeObject<Result<int>>(result.Result);
                return r;
            }
            return null;
        }

        public override async Task<Result> GetNumberDocRaiting()
        {
            HttpResult result = await Http.HTTPRequestAsync(0, "DCT/Raitting/GetNumberDocRaiting", null, "application/json", "brb", "brb");//

            if (result.HttpState == eStateHTTP.HTTP_OK)
            {
                var r = JsonConvert.DeserializeObject<Result>(result.Result);
                return r;
            }
            return null;
        }

        public override async Task<Result> SaveTemplate(RaitingTemplate pRT)
        {
            HttpResult result = await Http.HTTPRequestAsync(0, "DCT/Raitting/SaveTemplate", pRT.ToJSON("yyyy-MM-ddTHH:mm:ss"), "application/json", "brb", "brb");//

            if (result.HttpState == eStateHTTP.HTTP_OK)
            {
                var r = JsonConvert.DeserializeObject<Result>(result.Result);
                return r;
            }
            return null;
        }

        public override async Task<Result> SaveDocRaiting(DocVM pDoc)
        {
            HttpResult result = await Http.HTTPRequestAsync(0, "DCT/Raitting/SaveDocRaiting", pDoc.ToJSON("yyyy-MM-ddTHH:mm:ss"), "application/json", "brb", "brb");//

            if (result.HttpState == eStateHTTP.HTTP_OK)
            {
                var r = JsonConvert.DeserializeObject<Result>(result.Result);
                return r;
            }
            return null;
        }
        public override async Task<Result<IEnumerable<RaitingTemplate>>> GetRaitingTemplateAsync()
        {
            HttpResult result = await  Http.HTTPRequestAsync(0, "DCT/Raitting/GetRaitingTemplate", null, "application/json", "brb", "brb");//

            if (result.HttpState == eStateHTTP.HTTP_OK)
            {
                var r = JsonConvert.DeserializeObject<IEnumerable<RaitingTemplate>>(result.Result);
                
                if (r != null)
                {
                    db.ReplaceRaitingTemplate(r);
                    foreach (var el in r)
                    {
                        if (el.Item.Any())
                            db.ReplaceRaitingTemplateItem(el.Item);
                    }
                }

                return new Result<IEnumerable<RaitingTemplate>>() { Info = r };
            }
            else
                return new Result<IEnumerable<RaitingTemplate>>(result,null);
           
        }
        public override async Task<Result<IEnumerable<Doc>>> GetRaitingDocsAsync()
        {
            HttpResult result = await Http.HTTPRequestAsync(0, "DCT/Raitting/GetRaitingDocs", null, "application/json", "brb", "brb");//

            if (result.HttpState == eStateHTTP.HTTP_OK)
            {
                var r = JsonConvert.DeserializeObject<IEnumerable<DocVM>>(result.Result);
                return new Result<IEnumerable<Doc>>() { Info = r };
            }
            else
                return new Result<IEnumerable<Doc>>(result, null);
        }


        public override async Task<Result<IEnumerable<DocVM>>> GetPromotion(int pCodeWarehouse) {
            HttpResult result = await Http.HTTPRequestAsync(0, "DCT/CheckPromotion/Doc", pCodeWarehouse.ToJSON(), "application/json", "brb", "brb");

            if (result.HttpState == eStateHTTP.HTTP_OK)
            {
                var r = JsonConvert.DeserializeObject<Result<IEnumerable<DocVM>>>(result.Result);
                return r;
            }
            return null;
        }
        public override async Task<Result<IEnumerable<DocWares>>> GetPromotionData(string pNumberDoc) {
            HttpResult result = await Http.HTTPRequestAsync(0, "DCT/CheckPromotion/GetPromotionData", "\"" + pNumberDoc + "\"", "application/json", "brb", "brb");

            if (result.HttpState == eStateHTTP.HTTP_OK)
            {
                var r = JsonConvert.DeserializeObject<Result<IEnumerable<DocWares>>>(result.Result);
                return r;
            }
            return null;
        }
    }

}

