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
using System.Threading.Tasks;
using System.Threading;
using System.Drawing;
using BRB5.Model.DB;

namespace BL.Connector
{
    public class ConnectorUniversal : ConnectorBase
    {
        public ConnectorUniversal()
        {
            PercentColor = new PercentColor[4] { 
                new PercentColor(10, Color.FromArgb(0xC6F8BD), Color.LightGreen, "72301609"), 
                new PercentColor(25, Color.FromArgb(0xF7FAB2), Color.Yellow, "72301616"), 
                new PercentColor(50, Color.FromArgb(0xFFD7A6), Color.Orange ,  "72301623"), 
                new PercentColor(75,Color.FromArgb(0xFDABAB) , Color.Pink, "72301630") };
        }

        IEnumerable<TypeDoc> TypeDoc;
        /// <summary>
        /// Список Документів доступних по ролі
        /// </summary>
        /// <param name="pRole"></param>
        /// <returns></returns>
        public override IEnumerable<TypeDoc> GetTypeDoc(eRole pRole, eLoginServer pLS, eGroup pGroup = eGroup.NotDefined)
        {            
            return TypeDoc;
        }

        public override IEnumerable<LoginServer> LoginServer() => new List<LoginServer>() { new LoginServer() { Code = eLoginServer.Central, Name = "ЦБ" } };

        public override async Task<Result> LoginAsync(string pLogin, string pPassWord, eLoginServer pLoginServer)
        {
            Result Res = new Result();            
            if (pLoginServer == eLoginServer.Central)
            {
                User Data = new User() { Login = pLogin, PassWord = pPassWord };
                HttpResult result = await Http.HTTPRequestAsync(0, "DCT/Login", Data.ToJson(), "application/json", null);
                if (result.HttpState == eStateHTTP.HTTP_OK)
                {
                    Result<User> res = JsonConvert.DeserializeObject<Result<User>>(result.Result);
                    Config.Role = res.Info?.Role ?? 0;
                    Config.CodeUser = res.Info?.CodeUser ?? 0;
                    Config.NameUser = res.Info?.NameUser;
                    TypeDoc = res.Info?.TypeDoc;
                    FileLogger.WriteLogMessage($"ConnectorPSU.Login=>(pLogin=>{pLogin}, pPassWord=>{pPassWord},pLoginServer=>{pLoginServer}) Res=>({Res.State},{Res.Info},{Res.TextError})", eTypeLog.Full);
                    return res.GetResult;
                }
                else
                    return new Result(result);                
            }
            return new Result(-1,"Невідомий сервер");
        }

        public override async Task<Result> LoadGuidDataAsync(bool pIsFull)
        {
            try
            {
                Config.OnProgress?.Invoke(0.03);
                AppContext.SetSwitch("System.Reflection.NullabilityInfoContext.IsSupported", true);
                string Data = Config.CodeWarehouse.ToString();
                HttpResult result = await Http.HTTPRequestAsync(0, "DCT/GetGuid", Data, "application/json", null);
                Config.OnProgress?.Invoke(0.4);
                if (result.HttpState == eStateHTTP.HTTP_OK)
                {
                    var res = JsonConvert.DeserializeObject<Result<BRB5.Model.Guid>>(result.Result);
                    Config.OnProgress?.Invoke(0.60);
                    SaveGuide(res.Info, pIsFull);                    
                }                
                //await GetDaysLeft();
                Config.OnProgress?.Invoke(1);               
                return new Result(result);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                return new Result(e);
            }
        }
        
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
            /*string vCode = pBC.CodeWares > 0 ? $"code={pBC.CodeWares}" : $"BarCode = {pBC.BarCode}";
            HttpResult result = Http.HTTPRequest(0, $"PriceTagInfo?{vCode}", null, null, null, null);
            if (result.HttpState == eStateHTTP.HTTP_OK)
            {
                var res = JsonConvert.DeserializeObject<WaresPriceSE>(result.Result);
                return res.GetWaresPrice;
            }*/
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

        public override async Task<Result> LoadDocsDataAsync(int pTypeDoc, string pNumberDoc, bool pIsClear)
        {
            int temp = 0;
            var Res = new Result();
            /*if (pTypeDoc == 11)
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
                        {
                            TypeDoc = pTypeDoc,
                            IdTemplate = elp.templateId,
                            NumberDoc = elp.planId.ToString(),
                            DateDoc = elp.date,
                            CodeWarehouse = elp.shopId,
                            Description = t.data?.Where(el => el.templateId == elp.templateId)?.FirstOrDefault()?.templateName
                        }).ToList();
                        db.ReplaceDoc(Doc);

                        var RaitingTemplate = t.data.Select(el => new RaitingTemplate() { IdTemplate = el.templateId, Text = el.templateName });
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

                        FileLogger.WriteLogMessage($"ConnectorPSU.LoadDocsData=>(pTypeDoc=>{pTypeDoc}, pNumberDoc=>{pNumberDoc},pIsClear=>{pIsClear}) Res=>(Doc=>{Doc.Count()},{Res.State},{Res.Info},{Res.TextError})", eTypeLog.Full);

                        return Res;
                    }
                    catch (Exception e)
                    {
                        Res = new Result(-1, e.Message);
                        FileLogger.WriteLogMessage($"ConnectorPSU.LoadDocsData=>(pTypeDoc=>{pTypeDoc}, pNumberDoc=>{pNumberDoc},pIsClear=>{pIsClear}) Res=>({Res.State},{Res.Info},{Res.TextError})", eTypeLog.Error);
                        return Res;
                    }
            }
            else
            {
                var ds = Config.GetDocSetting(pTypeDoc);
                string CodeWarehouse = Config.CodeWarehouse.ToString();

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

                string NameApi = "documents";
                string AddPar = "";
                if (pTypeDoc >= 8 && pTypeDoc <= 9)
                {
                    NameApi = "docmoveoz";
                    AddPar = "&TypeMove=" + (pTypeDoc == 8 ? "0" : "1");
                }

                if (pTypeDoc == -1)
                    LoadGuidDataAsync((pTypeDoc == -1));

                Config.OnProgress?.Invoke(0.05);
                HttpResult result;
                try
                {
                    if ((pTypeDoc >= 5 && pTypeDoc <= 9) || (pTypeDoc <= 0 && Config.IsLoginCO))
                        result = Http.HTTPRequest(CodeApi, NameApi + (pTypeDoc == 5 ? "\\" + pNumberDoc : "?StoreSetting=" + CodeWarehouse) + AddPar, null, "application/json;charset=utf-8", Config.Login, Config.Password);
                    else
                        result = Http.HTTPRequest(CodeApi, "documents", null, "application/json;charset=utf-8", Config.Login, Config.Password);

                    if (result.HttpState == eStateHTTP.HTTP_OK)
                    {
                        Config.OnProgress?.Invoke(0.40);
                        var data = JsonConvert.DeserializeObject<InputDocs>(result.Result);


                        if (pIsClear)
                        {
                            // string sql = "DELETE FROM DOC; DELETE FROM DOC_WARES_sample; DELETE FROM DOC_WARES;";
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

                        Config.OnProgress?.Invoke(0.60);
                        foreach (var v in data.DocWaresSample)
                            v.TypeDoc += (pTypeDoc == 9 ? 1 : 0);

                        db.ReplaceDocWaresSample(data.DocWaresSample);

                        Config.OnProgress?.Invoke(0.100);
                        FileLogger.WriteLogMessage($"ConnectorPSU.LoadDocsData=>(pTypeDoc=>{pTypeDoc}, pNumberDoc=>{pNumberDoc},pIsClear=>{pIsClear}) Res=>({Res.State},{Res.Info},{Res.TextError})", eTypeLog.Full);

                        return Res;
                    }
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                }*/
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
            /*
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
                var r = new RequestSendRaiting()
                {
                    userId = Config.CodeUser,
                    action = "results",
                    answers = RD,
                    planId = int.Parse(e.NumberDoc),
                    text = e.Note,
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

                HttpResult result = await Http.HTTPRequestAsync(2, "", data, "application/json", null, null, 90);

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
            */
            return Res;
        }
        CultureInfo provider = CultureInfo.InvariantCulture;
        
        /// <summary>
        /// Вивантажеємо на сервер файли Рейтингів
        /// pMaxSecondSend - скільки часу відправляти, 0 - без обмежень.
        /// pSecondSkip - скільки хв не відправляти файл(для фонового відправлення
        /// </summary>
        /// <returns></returns>
        public override async Task<Result> SendRaitingFilesAsync(string pNumberDoc, int pTry = 2, int pMaxSecondSend = 0, int pSecondSkip = 0)
        {
            FileLogger.WriteLogMessage($"SendRaitingFiles Start pNumberDoc=>{pNumberDoc} pTry=>{pTry} pMaxSecondSend=>{pMaxSecondSend} pSecondSkip=>pSecondSkip", eTypeLog.Full);
            /*
            int i = 30;
            while (IsSaving && i-- > 0)
            {
                if (!IsStopSave) IsStopSave = true;
                Thread.Sleep(500);
            }
            if (IsSaving)
            {
                string mes = $"Збереження файлів зупинено, Оскільки попередне збереження не завершилось.";
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, mes);
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
                    }
                    catch (Exception e)
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
                            string text = res.success ? $"[({i}:{Error})/{Files.Length}] {Path.GetFileName(f)}=> ({data.Length / (1024 * 1024 * TimeSend.TotalSeconds):n2}Mb/c;{((double)data.Length) / (1024d * 1024d):n2}Mb;{TimeSend.TotalSeconds:n1}c))" :
                               $"[({i},{Error})/{Files.Length}] Файл не передано=>{Path.GetFileName(f)}";
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
                Res.TextError = (Error > 0 ? $"Не вдалось відправити {Error} файлів{Environment.NewLine}" : "") + $"Успішно відправлено {Sucsses} файлів {Res.TextError}";

                OnSave?.Invoke(Error > 0 ? $"Не передано=>{Error} з {Files.Count()}" : $"Файли успішно передані =>{Files.Count()}");

                IsSaving = false;
                if (!IsStopSave && pTry > 1 && Error > 0 && (double)Error / (double)Files.Length < 0.25d)
                    return await SendRaitingFilesAsync(pNumberDoc, --pTry);
                return Res;
            }
            finally { IsSaving = false; }
            */
            return null;
        }

        /// <summary>
        /// Завантаження Списку складів (HTTP)
        /// </summary>
        /// <returns></returns>
        public override async Task<Result<IEnumerable<Warehouse>>> LoadWarehouse()
        {
            /*HttpResult result;
            try
            {
                result = await Http.HTTPRequestAsync(1, "StoreSettings", "{}", "application/json", "brb", "brb"); //charset=utf-8;

                if (result.HttpState == eStateHTTP.HTTP_OK)
                {
                    var res = JsonConvert.DeserializeObject<IEnumerable<InputWarehouse>>(result.Result);
                    var R = res.Select(el => el.GetWarehouse()).ToList();
                    db.ReplaceWarehouse(R);
                    return new Result<IEnumerable<Warehouse>>(result,R);
                }
                else 
                    return new Result<IEnumerable<Warehouse>>(result,null);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                return new Result<IEnumerable<Warehouse>>(e);
            }  */
            return null;
        }
        public override async Task<Result<IEnumerable<DocWaresExpirationSample>>> GetExpirationDateAsync(int pCodeWarehouse)
        {
            try
            {
                //HttpResult result = Http.HTTPRequest(0, "GetExpirationDate", pCodeWarehouse.ToString(), null, null, null);
                HttpResult result = await Http.HTTPRequestAsync(0, "DCT/GetExpirationDate", pCodeWarehouse.ToString(), "application/json", null);

                if (result.HttpState == eStateHTTP.HTTP_OK)
                {
                    Config.OnProgress?.Invoke(0.5);
                    var res = JsonConvert.DeserializeObject<Result<IEnumerable<DocWaresExpirationSample>>>(result.Result);

                    /* result = await Http.HTTPRequestAsync(3, "DCT/GetExpirationWares", null, null, null);
                     if (result.HttpState == eStateHTTP.HTTP_OK)
                     {
                         Config.OnProgress?.Invoke(0.95);
                         var ExpirationWares = JsonConvert.DeserializeObject<IEnumerable<ExpirationWares>>(result.Result);
                         db.ReplaceExpirationWares(ExpirationWares);
                     }*/
                    return res;
                }
                else
                    return new Result<IEnumerable<DocWaresExpirationSample>>(result, null);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                return new Result<IEnumerable<DocWaresExpirationSample>>(e);
            }                
        }

        public override async Task<Result> SaveExpirationDate(DocWaresExpirationSave pED)
        {         
            try
            {
                AppContext.SetSwitch("System.Reflection.NullabilityInfoContext.IsSupported", true);
                string Data = pED.ToJSON("yyyy-MM-dd");
                HttpResult result = await Http.HTTPRequestAsync(0, "DCT/SaveExpirationDate", Data, "application/json", null);
                if (result.HttpState == eStateHTTP.HTTP_OK)
                {
                    var res = JsonConvert.DeserializeObject<Result>(result.Result);
                    return res;
                }
                return new Result(result);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                return new Result(e);
            }
        }
    }

}


