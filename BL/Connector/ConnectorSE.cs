using BRB5.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json.Converters;
using System.IO;
using Utils;
using System.Diagnostics;
using BRB5;
using System.Globalization;
using static System.Net.Mime.MediaTypeNames;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Threading;

namespace BL.Connector
{
    class ResultLogin : Result
    {
        public int? Profile { get; set; } = 10;
        public LoginData data { get; set; }
    }
    class LoginData
    {
        public int userId { get; set; }
        public string userName { get; set; }
    }
    public class ConnectorSE : Connector
    {
        /// <summary>
        /// Список Документів доступних по ролі
        /// </summary>
        /// <param name="pRole"></param>
        /// <returns></returns>
        public override IEnumerable<TypeDoc> GetTypeDoc(eRole pRole, eLoginServer pLS, eGroup pGroup = eGroup.NotDefined)
        {
            var Res = new List<TypeDoc>();
            if (pLS == eLoginServer.Local)
                Res.Add( new TypeDoc() { CodeDoc = 0, KindDoc = eKindDoc.PriceCheck, NameDoc = "Прайсчекер" });
            if (pLS == eLoginServer.Bitrix)
            {
                Res.Add(new TypeDoc() { CodeDoc = 11, KindDoc = eKindDoc.RaitingDoc, NameDoc = "Опитування", DayBefore = 4 });
                Res.Add(new TypeDoc() { CodeDoc = -1, KindDoc = eKindDoc.RaitingTempate, NameDoc = "Шаблони Опитування" });
                Res.Add(new TypeDoc() { CodeDoc = 12, KindDoc = eKindDoc.RaitingTemplateCreate, NameDoc = "Керування Опитуваннями" });
            }
            return Res;
        }

        public override IEnumerable<LoginServer> LoginServer()
        {
            return new List<LoginServer>()
            {new  LoginServer (){Code=eLoginServer.Local,Name = "Магазин"},
                new  LoginServer (){Code=eLoginServer.Central,Name = "ЦБ"},
             new  LoginServer (){Code=eLoginServer.Bitrix,Name = "Бітрікс"}};//
        }
        
        public override async Task<Result> LoginAsync(string pLogin, string pPassWord, eLoginServer pLoginServer)
        {
            Result Res = new Result();
            if (pLoginServer == eLoginServer.Bitrix)
            {
                string data = JsonConvert.SerializeObject(new RequestLogin() { action = "auth", login = pLogin, password = pPassWord });
                HttpResult result = await Http.HTTPRequestAsync(2, "", data, "application/json");
                if (result.HttpState != eStateHTTP.HTTP_OK)
                {
                    Res = new Result(result);
                    FileLogger.WriteLogMessage($"ConnectorPSU.Login=>(pLogin=>{pLogin}, pPassWord=>{pPassWord},pLoginServer=>{pLoginServer}) Res=>({Res.State},{Res.Info},{Res.TextError})", eTypeLog.Full);
                    return Res;
                }
                else
                {
                    try
                    {
                        var t = JsonConvert.DeserializeObject<AnswerLogin>(result.Result);
                        if (!t.success || t.data.userId <= 0)
                        {
                            Res = new Result(-1, "Не успішна авторизація. Можливо невірний логін чи пароль");
                            FileLogger.WriteLogMessage($"ConnectorPSU.Login=>(pLogin=>{pLogin}, pPassWord=>{pPassWord},pLoginServer=>{pLoginServer}) Res=>({Res.State},{Res.Info},{Res.TextError})", eTypeLog.Expanded);
                            return Res;
                        }
                        Config.CodeUser = t.data.userId;
                        Res = new Result();
                        FileLogger.WriteLogMessage($"ConnectorPSU.Login=>(pLogin=>{pLogin}, pPassWord=>{pPassWord},pLoginServer=>{pLoginServer}) Res=>({Res.State},{Res.Info},{Res.TextError})", eTypeLog.Expanded);
                        return Res;
                    }
                    catch (Exception e)
                    {
                        Res = new Result(e);
                        FileLogger.WriteLogMessage($"ConnectorPSU.Login=>(pLogin=>{pLogin}, pPassWord=>{pPassWord},pLoginServer=>{pLoginServer}) Res=>({Res.State},{Res.Info},{Res.TextError})", eTypeLog.Error);
                        return Res;
                    }
                }
            }
            else
            {
                HttpResult res = await Http.HTTPRequestAsync(pLoginServer == eLoginServer.Central ? 1 : 0, "login", "{\"login\" : \"" + pLogin + "\"}", "application/json", pLogin, pPassWord);
                if (res.HttpState == eStateHTTP.HTTP_UNAUTHORIZED || res.HttpState == eStateHTTP.HTTP_Not_Define_Error)
                {
                    //Utils.WriteLog("e", TAG, "Login >>" + res.HttpState.toString());
                    return new Result(-1, res.HttpState.ToString(), "Неправильний логін або пароль");
                }
                else if (res.HttpState != eStateHTTP.HTTP_OK)
                {
                    Res = new Result(res, "Ви не підключені до мережі " + Config.Company.ToString());
                    FileLogger.WriteLogMessage($"ConnectorPSU.Login=>(pLogin=>{pLogin}, pPassWord=>{pPassWord},pLoginServer=>{pLoginServer}) Res=>({Res.State},{Res.Info},{Res.TextError})", eTypeLog.Expanded);
                    return Res;
                }
                else
                {
                    try
                    {
                        var r = JsonConvert.DeserializeObject<ResultLogin>(res.Result);

                        if (r.State == 0)
                        {
                            Config.Role = (eRole)r.Profile;
                            Config.CodeUser = r.data.userId;
                            Config.NameUser = r.data.userName;
                            FileLogger.WriteLogMessage($"ConnectorPSU.Login=>(pLogin=>{pLogin}, pPassWord=>{pPassWord},pLoginServer=>{pLoginServer}) Res=>({Res.State},{Res.Info},{Res.TextError})", eTypeLog.Full);
                            return Res;
                        }
                        else
                        {
                            Res = new Result(r.State, r.TextError, "Неправильний логін або пароль");
                            FileLogger.WriteLogMessage($"ConnectorPSU.Login=>(pLogin=>{pLogin}, pPassWord=>{pPassWord},pLoginServer=>{pLoginServer}) Res=>({Res.State},{Res.Info},{Res.TextError})", eTypeLog.Expanded);
                            return Res;
                        }
                    }
                    catch (Exception e)
                    {
                        Res = new Result(-1, e.Message);
                        FileLogger.WriteLogMessage($"ConnectorPSU.Login=>(pLogin=>{pLogin}, pPassWord=>{pPassWord},pLoginServer=>{pLoginServer}) Res=>({Res.State},{Res.Info},{Res.TextError})", eTypeLog.Error);
                        return Res;
                    }
                }
            }
        }

        //Завантаження довідників.
        public override async Task<Result> LoadGuidDataAsync(bool IsFull) { return new Result(); }


        public override ParseBarCode ParsedBarCode(string pBarCode, bool pIsOnlyBarCode)
        {
            pBarCode = pBarCode.Trim();
            ParseBarCode Res = new ParseBarCode() { BarCode = pBarCode };
            if (pBarCode.Length > 2 && pBarCode.Substring(0, 2).Equals("29") && pBarCode.Length == 13)
            {
                Res.CodeWares = Convert.ToInt32(pBarCode.Substring(2, 8));
                Res.Price = Convert.ToDecimal(pBarCode.Substring(8, 13));
            }
            return Res;
        }

        public override WaresPrice GetPrice(ParseBarCode pBC, eTypePriceInfo pTP = eTypePriceInfo.Short)
        {
            string vCode = pBC.CodeWares > 0 ? $"code={pBC.CodeWares}" : $"BarCode = {pBC.BarCode}";
            HttpResult result = Http.HTTPRequest(0, $"PriceTagInfo?{vCode}", null, null, null, null);
            if (result.HttpState == eStateHTTP.HTTP_OK)
            {
                var res = JsonConvert.DeserializeObject<WaresPriceSE>(result.Result);
                return res.GetWaresPrice;
            }
            //LI.resHttp = res.Result;
            //LI.HttpState = res.HttpState;
            //return LI;

            return null;
        }

        public override Result SendLogPrice(IEnumerable<LogPrice> pLogPrice)
        {
            if (pLogPrice == null)
                return new Result();
            StringBuilder sb = new StringBuilder();
            var Data = pLogPrice.Where(el => el.IsGoodBarCode).Select(el => new LogPriceSE(el));
            HttpResult res = Http.HTTPRequest(0, "pricetag", Data.ToJSON(), "application/json;charset=utf-8", Config.Login, Config.Password);
            return new Result(res);
        }

        public override async Task<Result> LoadDocsDataAsync(int pTypeDoc, string pNumberDoc,  bool pIsClear)
        {
            int temp = 0;
            var Res = new Result();
            if (pTypeDoc == 11)
            {
                string data = JsonConvert.SerializeObject(new Request() { userId = Config.CodeUser, action = "templates" });
                HttpResult result = await Http.HTTPRequestAsync(2, "", data, "application/json");//

                string data2 = JsonConvert.SerializeObject(new Request() { userId = Config.CodeUser, action = "plans" });
                HttpResult result2 = await Http.HTTPRequestAsync(2, "", data2, "application/json");//

                if (result.HttpState != eStateHTTP.HTTP_OK || result2.HttpState != eStateHTTP.HTTP_OK)
                    return new Result(result.HttpState != eStateHTTP.HTTP_OK ? result : result2);
                else
                    try
                    {
                        //bool AddDoc;
                        var t = JsonConvert.DeserializeObject<Template>(result.Result);
                        var p = JsonConvert.DeserializeObject<Data>(result2.Result, new IsoDateTimeConverter { DateTimeFormat = "dd.MM.yyyy HH:mm:ss" });

                        var Doc = p.data.Select(elp => new Doc()
                                { TypeDoc = pTypeDoc, IdTemplate= elp.templateId, NumberDoc = elp.planId.ToString(), DateDoc = elp.date, CodeWarehouse = elp.shopId, Description =
                                                                        t.data?.Where(el => el.templateId == elp.templateId)?.FirstOrDefault()?.templateName }).ToList();
                        db.ReplaceDoc(Doc);

                        var RaitingTemplate = t.data.Select(el => new RaitingTemplate() { IdTemplate = el.templateId, Text = el.templateName  });
                        db.ReplaceRaitingTemplate(RaitingTemplate);
                        foreach (var item in t.data)
                        {
                           // if (item.questions != null && item.sections != null)
                            {
                                try
                                {
                                    temp = item.templateId;
                                    var tt = item.sections.Select(el =>
                                            new RaitingTemplateItem() { IdTemplate = item.templateId, Id = -el.sectionId, Parent = -el.parentId, Text = el.text, RatingTemplate = 8, OrderRS = el.sectionId }).ToList();
                                    tt.Add(new RaitingTemplateItem() { IdTemplate = item.templateId, Id = -1, Parent = 9999999, Text = "Всього", RatingTemplate = 8, OrderRS = 9999999 });

                                    db.ReplaceRaitingTemplateItem(tt);
                                    tt = item.questions.Select(el =>
                                            new RaitingTemplateItem() { IdTemplate = item.templateId, Id = el.questionId, Parent = -el.sectionId, Text = el.text, RatingTemplate = el.RatingTemplate, OrderRS = el.questionId }).ToList();
                                    db.ReplaceRaitingTemplateItem(tt);
                                }
                                catch (Exception e)
                                {
                                    var aa = temp;
                                }
                            }
                        }

              

                        /*
                        foreach (var elp in p.data)
                        {
                            var DocNumber = elp.planId.ToString();
                            AddDoc = false;

                            foreach (var elt in t.data.Where(el => el.templateId == elp.templateId))
                            {
                                if (!AddDoc)
                                {
                                    d.Add(new Doc()
                                    { TypeDoc = pTypeDoc, NumberDoc = DocNumber, DateDoc = elp.date, CodeWarehouse = elp.shopId, Description = elt.templateName });
                                }
                                if (elt.sections != null)
                                    foreach (var el in elt.sections)
                                        r.Add(new Model.RaitingDocItem() { TypeDoc = pTypeDoc, NumberDoc = DocNumber, Id = -el.sectionId, Parent = -el.parentId, Text = el.text,  RatingTemplate = 8, OrderRS = el.sectionId });
                                if (elt.questions != null)
                                    foreach (var el in elt.questions)
                                        r.Add(new Model.RaitingDocItem() { TypeDoc = pTypeDoc, NumberDoc = DocNumber, Id = el.questionId, Parent = -el.sectionId, Text = el.text,  RatingTemplate = el.RatingTemplate, OrderRS = el.questionId });

                                r.Add(new Model.RaitingDocItem() { TypeDoc = pTypeDoc, NumberDoc = DocNumber, Id = -1, Parent = 9999999, Text = "Всього",  RatingTemplate = 8, OrderRS = 9999999 });
                            }
                        }
                        */

                        FileLogger.WriteLogMessage($"ConnectorPSU.LoadDocsData=>(pTypeDoc=>{pTypeDoc}, pNumberDoc=>{pNumberDoc},pIsClear=>{pIsClear}) Res=>(Doc=>{Doc.Count()},{Res.State},{Res.Info},{Res.TextError})", eTypeLog.Full);

                        return Res;
                    }
                    catch (Exception e)
                    {
                        var aa = temp;
                        Res = new Result(-1, e.Message);
                        FileLogger.WriteLogMessage($"ConnectorPSU.LoadDocsData=>(pTypeDoc=>{pTypeDoc}, pNumberDoc=>{pNumberDoc},pIsClear=>{pIsClear}) Res=>({Res.State},{Res.Info},{Res.TextError})", eTypeLog.Error);
                        return Res;
                    }
            }
            else
            {
                var ds = Config.GetDocSetting(pTypeDoc);
                String CodeWarehouse = Config.CodeWarehouse.ToString();

                int CodeApi = 0;
                if (ds != null)
                    CodeApi = ds.CodeApi;
                else
                    if (pTypeDoc <= 0 && Config.IsLoginCO) CodeApi = 1;

                if (pTypeDoc >= 7 && pTypeDoc <= 9)
                {
                    BL bl = BL.GetBL();

                    Warehouse Wh = bl.GetWarehouse(Config.CodeWarehouse);
                    if (Wh != null)
                        CodeWarehouse = Wh.Number;
                }

                String NameApi = "documents";
                String AddPar = "";
                if (pTypeDoc >= 8 && pTypeDoc <= 9)
                {
                    NameApi = "docmoveoz";
                    AddPar = "&TypeMove=" + (pTypeDoc == 8 ? "0" : "1");
                }

                if (pTypeDoc == -1)
                    LoadGuidDataAsync((pTypeDoc == -1));

                Config.OnProgress?.Invoke(5);
                HttpResult result;
                try
                {
                    if ((pTypeDoc >= 5 && pTypeDoc <= 9) || (pTypeDoc <= 0 && Config.IsLoginCO))
                        result = Http.HTTPRequest(CodeApi, NameApi + (pTypeDoc == 5 ? "\\" + pNumberDoc : "?StoreSetting=" + CodeWarehouse) + AddPar, null, "application/json;charset=utf-8", Config.Login, Config.Password);
                    else
                        result = Http.HTTPRequest(CodeApi, "documents", null, "application/json;charset=utf-8", Config.Login, Config.Password);

                    if (result.HttpState == eStateHTTP.HTTP_OK)
                    {
                        Config.OnProgress?.Invoke(40);
                        var data = JsonConvert.DeserializeObject<InputDocs>(result.Result);


                        if (pIsClear)
                        {
                            // String sql = "DELETE FROM DOC; DELETE FROM DOC_WARES_sample; DELETE FROM DOC_WARES;";
                            db.db.Execute("DELETE FROM DOC");
                            db.db.Execute("DELETE FROM DocWaresSample");
                            db.db.Execute("DELETE FROM DocWares");
                        }
                        else
                            db.db.Execute("update doc set state=-1 where type_doc not in (5,6)" + (pTypeDoc > 0 ? $" and type_doc={pTypeDoc}" : ""));

                        foreach (Doc v in data.Doc)
                        {
                            //v.TypeDoc = ConvertTypeDoc(v.TypeDoc);
                            //v.DateDoc = v.DateDoc.Substring(0, 10);
                            v.TypeDoc += (pTypeDoc == 9 ? 1 : 0);
                        }
                        db.ReplaceDoc(data.Doc);

                        Config.OnProgress?.Invoke(60);
                        foreach (var v in data.DocWaresSample)
                            v.TypeDoc += (pTypeDoc == 9 ? 1 : 0);

                        db.ReplaceDocWaresSample(data.DocWaresSample);

                        Config.OnProgress?.Invoke(100);
                        return Res;
                    }

                }
                catch (Exception e)
                {
                    //Utils.WriteLog("e", TAG, "LoadDocsData=>", e);
                }
                return Res;
            }
            FileLogger.WriteLogMessage($"ConnectorPSU.LoadDocsData=>(pTypeDoc=>{pTypeDoc}, pNumberDoc=>{pNumberDoc},pIsClear=>{pIsClear}) Res=>({Res.State},{Res.Info},{Res.TextError})", eTypeLog.Full);
            return Res;
        }

        public override async Task<Result<IEnumerable<RaitingTemplate>>> GetRaitingTemplateAsync() { return null; }

        /// <summary>
        /// Вивантаження документів з ТЗД (HTTP)
        /// </summary>
        /// <param name="pDoc"></param>
        /// <param name="pWares"></param>
        /// <param name="pIsClose"></param>
        /// <returns></returns>
        public override Result SendDocsData(DocVM pDoc, IEnumerable<DocWares> pWares)
        {
            return null;
        }
        /// <summary>
        /// Вивантаження Рейтингів
        /// </summary>
        /// <param name="pR"></param>
        /// <returns></returns>
        public override async Task<Result> SendRaitingAsync(IEnumerable<RaitingDocItem> pR, DocVM pDoc)
        {
            OnSave?.Invoke($"Зберігаємо документ=>{pDoc.NumberDoc}");
            var Res = new Result();
            try
            {
                var RD = new List<Raitings>();
                foreach (var el in pR)
                {
                    RD.Add(new Raitings() { questionId = el.Id, value = el.Rating, comment = el.Note });
                }

                RaitingDocItem e = pR.FirstOrDefault(d => d.Id == -1);
                if (e == null || e.Id == 0)
                    e = pR.FirstOrDefault();
                var r = new RequestSendRaiting() { userId = Config.CodeUser, action = "results", answers = RD, planId = int.Parse(e.NumberDoc), text = e.Note,
                    dateStart = pDoc.DTStart,
                    dateEnd = pDoc.DTEnd
                };
                //var p = JsonConvert.DeserializeObject<Data>(result2.Result);
                string data = JsonConvert.SerializeObject(r, new IsoDateTimeConverter { DateTimeFormat = "dd.MM.yyyy HH:mm:ss" });
                //Збереження Json
                try
                {
                    var FileName = Path.Combine(FileLogger.PathLog, $"{pDoc.NumberDoc}_{DateTime.Now:yyyyMMddHHmmssfff}.json");
                    File.AppendAllText(FileName, data);
                }
                catch (Exception) { }

                var sw = Stopwatch.StartNew();

                HttpResult result =await Http.HTTPRequestAsync(2, "", data, "application/json",null,null,90);

                sw.Stop();
                TimeSpan TimeLoad = sw.Elapsed;
                OnSave?.Invoke($"Час збереження відповідей=>{TimeLoad.TotalSeconds}c");
                FileLogger.WriteLogMessage($"ConnectorPSU.SendRaiting=>(NumberDoc=>{pDoc.NumberDoc}) Res=>({result.HttpState}");

                if (result.HttpState != eStateHTTP.HTTP_OK)
                {
                    FileLogger.WriteLogMessage($"ConnectorPSU.SendRaiting=>(NumberDoc=>{pDoc.NumberDoc}) Res=>({Res.State},{Res.Info},{Res.TextError})", eTypeLog.Error);
                    Res = new Result(result);
                }
                else
                {
                    var res = JsonConvert.DeserializeObject<AnswerSendRaiting>(result.Result);
                    
                    if (res.success)
                    {                      
                        Res = await SendRaitingFilesAsync(e.NumberDoc);
                        OnSave?.Invoke($"Документ {pDoc.NumberDoc} Успішно відправлено");
                    }
                    else
                        OnSave?.Invoke($"Помилка збереження документа {pDoc.NumberDoc}");
                }
            }
            catch (Exception ex)
            {
                Res = new Result(ex);
                OnSave?.Invoke($"Помилка збереження =>{Res.TextError}");
            }
            FileLogger.WriteLogMessage($"ConnectorPSU.SendRaiting=>(NumberDoc=>{pDoc.NumberDoc}) Res=>({Res.State},{Res.Info},{Res.TextError})");
            
            return Res;
        }
        CultureInfo provider = CultureInfo.InvariantCulture;
       
        object Lock=new object();
        /// <summary>
        /// Вивантажеємо на сервер файли Рейтингів
        /// pMaxSecondSend - скільки часу відправляти, 0 - без обмежень.
        /// pSecondSkip - скільки хв не відправляти файл(для фонового відправлення
        /// </summary>
        /// <returns></returns>
        public override async Task<Result> SendRaitingFilesAsync(string pNumberDoc, int pTry = 2, int pMaxSecondSend = 0, int pSecondSkip = 0)
        {            
            FileLogger.WriteLogMessage($"SendRaitingFiles Start pNumberDoc=>{pNumberDoc} pTry=>{pTry} pMaxSecondSend=>{pMaxSecondSend} pSecondSkip=>pSecondSkip", eTypeLog.Full);

            int i = 30;
            while(IsSaving && i-->0)
            {
              if (!IsStopSave) IsStopSave = true;
              Thread.Sleep(500);
            }
            if (IsSaving)
            {
                string mes = $"Збереження файлів зупинено, Оскільки попередне збереження не завершилось.";
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name,mes);
                OnSave?.Invoke(mes);
                return new Result(-1, "StopSave", mes);
            }
            try
            {
                IsSaving = true;
                IsStopSave = false;
                var StartTime = DateTime.Now;
                
                int Sucsses = 0, Error = 0;
                Result LastError = null;
                var Res = new Result();
                var DirArx = Path.Combine(Config.PathDownloads, "arx");
                if (!Directory.Exists(DirArx))
                {
                    Directory.CreateDirectory(DirArx);
                }

                if (!Directory.Exists(Path.Combine(DirArx, pNumberDoc)))
                {
                    Directory.CreateDirectory(Path.Combine(DirArx, pNumberDoc));
                }

                var R = new RequestSendRaitingFile() { planId = int.Parse(pNumberDoc), action = "file", userId = Config.CodeUser };

                var Files = Directory.GetFiles(Path.Combine(Config.PathFiles, pNumberDoc));
                FileLogger.WriteLogMessage($"SendRaitingFiles Files=>{Files?.Length}", eTypeLog.Full);
                i = 0;
                OnSave?.Invoke($"Файлів для передачі=>{Files.Count()}");
                foreach (var f in Files)
                {                    
                    if (pMaxSecondSend > 0 && (DateTime.Now - StartTime).TotalSeconds > pMaxSecondSend) continue;
                    try
                    {
                        string s = Path.GetFileNameWithoutExtension(f).Split('_')[1] + "_" + Path.GetFileNameWithoutExtension(f).Split('_')[2];
                        if (s.Length > 18) { s = s.Substring(0, 18); }
                        R.DT = DateTime.ParseExact(s, "yyyyMMdd_HHmmssfff", provider);
                        if (pSecondSkip > 0 && (DateTime.Now - R.DT).TotalSeconds < pSecondSkip)
                        {
                            FileLogger.WriteLogMessage($"SendRaitingFiles Skip DateCreateFile File=>{f} {DateTime.Now} / {R.DT}", eTypeLog.Full);
                            OnSave?.Invoke($"Файл пропущено=>{Path.GetFileName(f)}");
                            continue;
                        }
                    }catch(Exception e)
                    {
                        FileLogger.WriteLogMessage($"SendRaitingFiles Error=> {e.Message}", eTypeLog.Error);
                        OnSave?.Invoke($"помилка при передачі {Path.GetFileName(f)} Error=>{e.Message}");
                    }
                    i++;
                    if (IsStopSave)
                    {
                        IsSaving = false;
                        return new Result(-1, "StopSave", "Збереження зупинено користувачем");
                    }
                    try
                    {
                        var sw = Stopwatch.StartNew();
                        R.file = Convert.ToBase64String(File.ReadAllBytes(f));
                        R.fileExt = Path.GetExtension(f).Substring(1);
                        R.questionId = int.Parse(Path.GetFileName(f).Split('_')[0]);                         

                        sw.Stop();
                        TimeSpan TimeLoad = sw.Elapsed;
                        sw.Start();
                        string data = JsonConvert.SerializeObject(R);
                        HttpResult result = await Http.HTTPRequestAsync(2, "", data, "application/json", null, null, 60, false);
                        R.file = null;
        
                        if (result.HttpState == eStateHTTP.HTTP_OK)
                        {
                            var res = JsonConvert.DeserializeObject<Answer>(result.Result);
                            FileLogger.WriteLogMessage($"ConnectorPSU.SendRaitingFiles  File=>{f} HTTP=>({R.ToJSON()}) HttpState=>{result.HttpState} success=>{res.success}");

                            if (res.success)
                            {
                                var FileTo = Path.Combine(DirArx, pNumberDoc, Path.GetFileName(f));
                                File.Copy(f, FileTo, true);
                                File.Delete(f);
                                Sucsses++;                               
                            }
                            else
                            {
                                Error++;
                                Res = new Result(-1, "Не передався файл", f);
                            }
                            sw.Stop();
                            TimeSpan TimeSend = sw.Elapsed;
                            string text = res.success? $"[({i}:{Error})/{Files.Length}] {Path.GetFileName(f)}=> ({data.Length / (1024 * 1024 * TimeSend.TotalSeconds):n2}Mb/c;{((double)data.Length) / (1024d * 1024d):n2}Mb;{TimeSend.TotalSeconds:n1}c))":
                               $"[({i},{Error})/{Files.Length}] Файл не передано=>{Path.GetFileName(f)}" ;
                            OnSave?.Invoke(text);
                            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, text, eTypeLog.Full);
                        }
                        else
                        {
                            Error++;
                            FileLogger.WriteLogMessage($"ConnectorPSU.SendRaitingFiles=>(File={f}) Res=>({Res.State},{Res.Info},{Res.TextError})", eTypeLog.Expanded);
                            LastError = Res;
                            OnSave?.Invoke($"[({i},{Error})/{Files.Length}] Файл не передано=>{Path.GetFileName(f)} {result.HttpState}");
                        }
                    }
                    catch (Exception e)
                    {
                        Res = new Result(e);
                        OnSave?.Invoke($"помилка при передачі {Path.GetFileName(f)} Error=>{e.Message}");
                        FileLogger.WriteLogMessage($"ConnectorPSU.SendRaitingFiles=>(File={f}) Res=>({Res.State},{Res.Info},{Res.TextError})", eTypeLog.Error);
                    }
                }
                Res = LastError ?? Res;
                Res.TextError = (Error>0?$"Не вдалось відправити {Error} файлів{Environment.NewLine}" :"") + $"Успішно відправлено {Sucsses} файлів {Res.TextError}";

                OnSave?.Invoke(Error>0? $"Не передано=>{Error} з {Files.Count()}": $"Файли успішно передані =>{Files.Count()}");
                
                IsSaving = false;
                if ( !IsStopSave && pTry > 1 && Error > 0 && (double)Error / (double)Files.Length < 0.25d)
                    return await SendRaitingFilesAsync(pNumberDoc, --pTry);                
                return Res;
            }
            finally { IsSaving = false; }
        }

        /// <summary>
        /// Завантаження Списку складів (HTTP)
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<Warehouse> LoadWarehouse()
        {
            HttpResult result;
            try
            {
                result = Http.HTTPRequest(1, "StoreSettings", "{}", "application/json","brb", "brb"); //charset=utf-8;

                if (result.HttpState == eStateHTTP.HTTP_OK)
                {
                    var res = JsonConvert.DeserializeObject<IEnumerable<InputWarehouse>>(result.Result);
                    return res.Select(el => el.GetWarehouse()).ToList();
                }
            }
            catch (Exception e)
            {
                //Utils.WriteLog("e", TAG, "LoadWarehouse=>", e);
            }
            return null;
        }
    }
    class Answer
    {
        public bool success { get; set; }
    }

    class Request
    {
        public string token { get; set; } = "jO0qyQ6PqBXr4HGqCu7T";
        public string action { get; set; }
        public int? userId { get; set; }
    }

    class RequestLogin : Request
    {
        public string login { get; set; }
        public string password { get; set; }
    }

    class DataLogin
    {
        public int userId { get; set; }
        public string userName { get; set; }
    }


    class AnswerLogin : Answer
    {
        public DataLogin data { get; set; }
    }

    class Section
    {
        public int sectionId { get; set; }
        public string text { get; set; }
        public int parentId { get; set; }
        public int Order { get; set; }
    }

    class Questions
    {
        public int questionId { get; set; }
        public int sectionId { get; set; }
        public string text { get; set; }
        public int value { get; set; }
        public int[] answers { get; set; }
        public int RatingTemplate { get { int r = 0; for (int i = 0; i < answers.Length; i++) r += 1 << (answers[i] - 1); return r; } }
        public int Order { get; set; }
    }

    class DataTemplate
    {
        public int templateId { get; set; }
        public string templateName { get; set; }
        //public DateTime updated { get; set; }
        public IEnumerable<Section> sections { get; set; }
        public IEnumerable<Questions> questions { get; set; }
    }

    class Template : Answer
    {
        public IEnumerable<DataTemplate> data { get; set; }
    }

    class DataData
    {
        public int planId { get; set; }
        public int templateId { get; set; }
        public int shopId { get; set; }
        public DateTime date { get; set; }
    }

    class Data : Answer
    {
        public IEnumerable<DataData> data { get; set; }
    }

    class Raitings
    {
        public int questionId { get; set; }
        public int value { get; set; }
        public string comment { get; set; }
    }

    class RequestSendRaiting : Request
    {
        public int planId { get; set; }
        public string text { get; set; }
        public IEnumerable<Raitings> answers { get; set; }
        public DateTime dateStart { get; set; }
        public DateTime dateEnd { get; set; }
    }

    class AnswerDataRaiting
    {
        public int questionId { get; set; }
        public int answerId { get; set; }
    }

    class AnswerSendRaiting : Answer
    {
        public IEnumerable<AnswerDataRaiting> data { get; set; }
        public IEnumerable<string> errors { get; set; }
    }

    class RequestSendRaitingFile : Request
    {
        public int planId { get; set; }
        public int questionId { get; set; }
        public string file { get; set; }
        public string fileExt { get; set; }
        public DateTime DT { get; set; }
    }
    public class LogPriceSE
    {
        //public string GetJsonSE() { return "{\"Barcode\":\"" + BarCode + "\",\"Code\":\"" + CodeWares + "\",\"Status\":" + Status + ",\"LineNumber\":" + LineNumber + ",\"NumberOfReplenishment\":" + Double.tostring(NumberOfReplenishment) + "}"; }
        public LogPriceSE(LogPrice pLP)
        {
            BarCode = pLP.BarCode;
            Code = pLP.CodeWares;
            Status = pLP.Status;
            LineNumber = pLP.LineNumber;
            NumberOfReplenishment = pLP.NumberOfReplenishment;
        }
        public string BarCode { get; set; }
        public int Code { get; set; }
        public int Status { get; set; }
        public int LineNumber { get; set; }
        public double NumberOfReplenishment { get; set; }
    }

    public class WaresPriceSE
    {
        public int Code { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string BarCodes { get; set; }
        public string Unit { get; set; }
        public string Article { get; set; }
        public int ActionType { get; set; }
        public decimal PromotionPrice { get; set; }
        public WaresPrice GetWaresPrice
        {
            get { return new WaresPrice() { CodeWares = Code, Name = Name, Price = Price, BarCodes = BarCodes, Unit = Unit, Article = Article, ActionType = ActionType, PriceOpt = PromotionPrice }; }
        }
    }
    class InputWarehouse
    {
        public int Code { get; set; }
        public String StoreCode { get; set; } //Number
        public String Name { get; set; } //Url
        public String Unit { get; set; } //Name
        public String InternalIP { get; set; }
        public String ExternalIP { get; set; }
        public String latitude { get; set; }
        public String longitude { get; set; }
        public Warehouse GetWarehouse()
        {
            return new Warehouse() { Code = Code, Number = StoreCode, Name = Unit, Url = Name, InternalIP = InternalIP, ExternalIP = ExternalIP,Location=$"{latitude},{longitude}"};
        }

    }
    class InputDocs
    {
        public Doc[] Doc { get; set; }
        public DocWaresSample[] DocWaresSample { get; set; }

    }



}
