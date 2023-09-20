using BRB5.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Utils;
using System.Linq;
using System.Collections.ObjectModel;

namespace BRB5.Connector
{
    public class ConnectorPSU : Connector
    {
        public override IEnumerable<LoginServer> LoginServer() { return new List<LoginServer>()
            {new  LoginServer (){Code=eLoginServer.Central,Name = "ЦБ"}}; }
        public override Result Login(string pLogin, string pPassWord, eLoginServer pLoginServer)
        {
            string data = JsonConvert.SerializeObject(new Api() { CodeData = 1, Login = pLogin, PassWord = pPassWord }); //"{\"CodeData\": \"1\"" + ", \"Login\": \"" + pLogin + "\"" + ", \"PassWord\": \"" + pPassWord + "\"}";
            HttpResult result = Http.HTTPRequest(0, "znp/", data, "application/json");//

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
                    var r = new Result(-1, e.Message);
                    FileLogger.WriteLogMessage($"ConnectorPSU.Login=>(pLogin=>{pLogin}, pPassWord=>{pPassWord},pLoginServer=>{pLoginServer}) Res=>({r.State},{r.Info},{r.TextError})", eTypeLog.Error);
                    return r;
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
                    //Utils.WriteLog("e", TAG, "ParsedBarCode=> " + pBarCode, e);
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
                        if (str.Length == 3)
                            res.PriceOpt = Convert.ToInt32(str[2]) / 100m;
                        if (str.Length >= 2) {
                            res.Price = Convert.ToInt32(str[1]) / 100m;
                            res.CodeWares = Convert.ToInt32(str[0]);
                            res.BarCode = null;
                        }                                
                    }
                    catch (Exception e)
                    {
                        //Utils.WriteLog("e", TAG, "PriceBarCode", e);
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
            HttpResult result = Http.HTTPRequest(0, "DCT/GetPrice/", data, "application/json");
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
            var Data = pLogPrice.Select(el => el.GetPSU());
            string data = JsonConvert.SerializeObject(new ApiLogPrice(Data));
            HttpResult result = Http.HTTPRequest(0, "znp/", data, "application/json");
            return new Result(result);
        }

        /// <summary>
        /// Список Документів доступних по ролі
        /// </summary>
        /// <param name="pRole"></param>
        /// <returns></returns>
        public override IEnumerable<TypeDoc> GetTypeDoc(eRole pRole, eLoginServer pLS)
        {
            var Res = new List<TypeDoc>(){ 
                                        new TypeDoc() { CodeDoc = 0, NameDoc = "Прайсчекер" , KindDoc = eKindDoc.PriceCheck},                                        
                                        new TypeDoc() { CodeDoc = 1, NameDoc = "Ревізія", TypeControlQuantity = eTypeControlDoc.Ask, IsSaveOnlyScan=false, KindDoc = eKindDoc.Normal },
                                        new TypeDoc() { CodeDoc = 2, NameDoc = "Прихід", TypeControlQuantity = eTypeControlDoc.Ask, IsViewOut=true, KindDoc = eKindDoc.Normal },
                                        new TypeDoc() { CodeDoc = 3, NameDoc = "Переміщення Вих", TypeControlQuantity=eTypeControlDoc.NoControl, KindDoc = eKindDoc.Normal },
                                        new TypeDoc() { CodeDoc = 4, NameDoc = "Списання" ,  KindDoc = eKindDoc.Normal},
                                        new TypeDoc() { CodeDoc = 5, NameDoc = "Повернення" ,  KindDoc = eKindDoc.Normal},
                                        new TypeDoc() { CodeDoc = 7, NameDoc = "Ревізія ОЗ", TypeControlQuantity=eTypeControlDoc.Ask, IsSimpleDoc=true, KindDoc = eKindDoc.Normal,CodeApi=1,IsCreateNewDoc=true },
                                        new TypeDoc() { CodeDoc = 8, NameDoc = "Переміщення Вх", TypeControlQuantity=eTypeControlDoc.Ask, IsViewOut=true, IsViewReason=true,KindDoc = eKindDoc.Normal },
                                        new TypeDoc() { CodeDoc = 11, KindDoc = eKindDoc.RaitingDoc, NameDoc = "Опитування", DayBefore = 4 },
                                        new TypeDoc() { CodeDoc = -1, KindDoc = eKindDoc.RaitingTempate, NameDoc = "Шаблони Опитування" },
                                        new TypeDoc() { CodeDoc = 12, KindDoc = eKindDoc.RaitingTemplateCreate, NameDoc = "Керування Опитуваннями" }
        };
            return Res;
        }

        public override Result LoadGuidData(bool IsFull)
        {
            return LoadDocsData(-1, null,  true);
        }

        /// <summary>
        /// Завантаження документів в ТЗД (HTTP)
        /// </summary>
        /// <param name="pTypeDoc">0-всі документи</param>
        /// <param name="pNumberDoc"></param>
        /// <param name="pProgress"></param>
        /// <param name="pIsClear"></param>
        /// <returns></returns>
        public override Result LoadDocsData(int pTypeDoc, string pNumberDoc, bool pIsClear) 
        {

            if (pTypeDoc == 11)
            {
                var temp = GetRaitingDocs();
                if (temp.Info != null)
                {
                    foreach (var doc in temp.Info)
                    {
                        doc.TypeDoc = pTypeDoc;
                    }
                    db.ReplaceDoc(temp.Info);

                    var RT=GetRaitingTemplate();

                }
                return new Result();
            }
            else
            {
                string data = JsonConvert.SerializeObject(new ApiDoc() { CodeData = 150, TypeDoc = pTypeDoc,CodeWarehouse=Config.CodeWarehouse,Ver=5136 });
                HttpResult result = Http.HTTPRequest(0, "znp/", data, "application/json");//

                if (result.HttpState == eStateHTTP.HTTP_OK)
                {
                    string[] lines = result.Result.Split(new String[] { ";;;" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var el in lines)
                    {
                        string Sql = el.Replace("_", "").Replace(";;", "");
                        if(Sql.Length>20)
                        try
                        {
                                Console.WriteLine(Sql.Substring(0, 20) + $" Length=>{Sql.Length}") ;
                            db.db.Execute(Sql);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(Sql +Environment.NewLine+ e.Message);
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
        public override Result SendDocsData(Doc pDoc, IEnumerable<DocWares> pWares)
        {
            var r = pWares.Select(el => new Object[] { el.OrderDoc, el.CodeWares, el.InputQuantity });
            var res = new ApiSaveDoc(153, pDoc.TypeDoc, pDoc.NumberDoc, r);
            String data = res.ToJSON();
            try
            {
                HttpResult result = Http.HTTPRequest(0, "znp/", data, "application/json", null, null);
                if (result.HttpState != eStateHTTP.HTTP_OK)
                {
                    return new Result(result);
                }
                Result Res = JsonConvert.DeserializeObject<Result>(result.Result);
                return Res;
            }
            catch (Exception e)
            {
                //Utils.WriteLog("e",TAG, "SyncDocsData=>" +data,e);
                return new Result(-1, e.Message + data);
            }
        }


        class WarehouseIn:Result
        {
            public IEnumerable<object[]> Warehouse  { get; set; }
        }

        public override IEnumerable<Warehouse> LoadWarehouse()
        {
            string data = JsonConvert.SerializeObject(new Api() { CodeData = 210 });
            HttpResult result = Http.HTTPRequest(0, "znp/", data, "application/json","brb","brb");//

            if (result.HttpState == eStateHTTP.HTTP_OK)
            {
                var r = JsonConvert.DeserializeObject<WarehouseIn>(result.Result);               
                   
                var res = r.Warehouse.Select(el => new Warehouse() { Code = Convert.ToInt32(el[0]), Name = el[1].ToString() });
                db.ReplaceWarehouse(res);
                return res;
            }
            return null;
        }

        /// <summary>
        /// Друк на стаціонарному термопринтері
        /// </summary>
        /// <param name="codeWares">Список товарів</param>
        /// <returns></returns>        
        public override string PrintHTTP(IEnumerable<int> pCodeWares) 
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
                //Utils.WriteLog("e", TAG, "printHTTP  >>", e);
                return e.Message;
            }
        }

        public override Result<int> GetIdRaitingTemplate() {
            HttpResult result = Http.HTTPRequest(0, "DCT/Raitting/GetIdRaitingTemplate", null, "application/json", "brb", "brb");//

            if (result.HttpState == eStateHTTP.HTTP_OK)
            {
                var r = JsonConvert.DeserializeObject<Result<int>>(result.Result);
                return r;
            }
            return null;
        }

        public override Result GetNumberDocRaiting()
        {
            HttpResult result = Http.HTTPRequest(0, "DCT/Raitting/GetNumberDocRaiting", null, "application/json", "brb", "brb");//

            if (result.HttpState == eStateHTTP.HTTP_OK)
            {
                var r = JsonConvert.DeserializeObject<Result>(result.Result);
                return r;
            }
            return null;
        }

        public override Result SaveTemplate(RaitingTemplate pRT)
        {
            HttpResult result = Http.HTTPRequest(0, "DCT/Raitting/SaveTemplate", pRT.ToJSON("yyyy-MM-ddTHH:mm:ss"), "application/json", "brb", "brb");//

            if (result.HttpState == eStateHTTP.HTTP_OK)
            {
                var r = JsonConvert.DeserializeObject<Result>(result.Result);
                return r;
            }
            return null;
        }

        public override Result SaveDocRaiting(Doc pDoc)
        {
            HttpResult result = Http.HTTPRequest(0, "DCT/Raitting/SaveDocRaiting", pDoc.ToJSON("yyyy-MM-ddTHH:mm:ss"), "application/json", "brb", "brb");//

            if (result.HttpState == eStateHTTP.HTTP_OK)
            {
                var r = JsonConvert.DeserializeObject<Result>(result.Result);
                return r;
            }
            return null;
        }
        public override Result<IEnumerable<RaitingTemplate>> GetRaitingTemplate()
        {
            HttpResult result = Http.HTTPRequest(0, "DCT/Raitting/GetRaitingTemplate", null, "application/json", "brb", "brb");//

            if (result.HttpState == eStateHTTP.HTTP_OK)
            {
                var r = JsonConvert.DeserializeObject<IEnumerable<RaitingTemplate>>(result.Result);
                return new Result<IEnumerable<RaitingTemplate>>() { Info = r };
            }
            else
                return new Result<IEnumerable<RaitingTemplate>>(result,null);
           
        }
        public override Result<IEnumerable<Doc>> GetRaitingDocs()
        {
            HttpResult result = Http.HTTPRequest(0, "DCT/Raitting/GetRaitingDocs", null, "application/json", "brb", "brb");//

            if (result.HttpState == eStateHTTP.HTTP_OK)
            {
                var r = JsonConvert.DeserializeObject<IEnumerable<Doc>>(result.Result);
                return new Result<IEnumerable<Doc>>() { Info = r };
            }
            else
                return new Result<IEnumerable<Doc>>(result, null);
        }


    }

}

