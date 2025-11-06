using BRB5;
using BRB5.Model;
using BRB5.Model.DB;
using Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UtilNetwork;
using Utils;



namespace BL.Connector
{
    public class ConnectorUniversal : ConnectorBase
    {
        ConnectorBase СonnectorLocal = null;
        public ConnectorUniversal()
        {
            PercentColor = [
                new PercentColor(0, Color.White, Color.White, ""), //Товар з хорошим строком
                new PercentColor(10, Color.FromArgb(0x84FF57), Color.FromArgb(0x84FF57), "72301609"),  //10%
                new PercentColor(25, Color.FromArgb(0xFFF157), Color.FromArgb(0xFFF157), "72301616"),  //25%
                new PercentColor(50, Color.FromArgb(0xFEB044), Color.FromArgb(0xFEB044), "72301623"), //50%
                new PercentColor(75, Color.FromArgb(0xE874FF), Color.FromArgb(0xE874FF), "72301630") , //75%
                new PercentColor(100, Color.Gray, Color.Gray, "") //Протермінований товар
            ];
        }
        bool IsLocalPrice => Config.TypeDoc?.Where(el => el.KindDoc == eKindDoc.PriceCheck).FirstOrDefault()?.CodeApi == 1;
        // IEnumerable<TypeDoc> TypeDoc=null;
        bool isGroup = false;
        /// <summary>
        /// Список Документів доступних по ролі
        /// </summary>
        /// <param name="pRole"></param>
        /// <returns></returns>
        public override IEnumerable<TypeDoc> GetTypeDoc(eRole pRole, eLoginServer pLS, eGroup pGroup = eGroup.NotDefined)
        {
            if(!isGroup) return Config.TypeDoc;
            if (pGroup == eGroup.NotDefined) return Config.TypeDoc.Where(el => el.KindDoc == eKindDoc.NotDefined);
            return Config.TypeDoc.Where(el => el.Group == pGroup);
        }

        public override IEnumerable<LoginServer> LoginServer() => 
            new List<LoginServer>() { 
                new() { Code = eLoginServer.Central, Name = "ЦБ" }//, 
                //new() { Code = eLoginServer.Bitrix, Name = "Бітрікс" } 
            };

        public override async Task<Result> LoginAsync(string pLogin, string pPassWord, eLoginServer pLoginServer, string pBarCode=null)
        {
            try
            {
                if ((string.IsNullOrEmpty(pLogin) || string.IsNullOrEmpty(pPassWord)) && string.IsNullOrEmpty(pBarCode)) 
                    return new Result(-1, "Порожній пароль або логін");
                Result Res = new Result();
                if (pLoginServer == eLoginServer.Central || pLoginServer == eLoginServer.Bitrix)
                {
                    string Data;
                    Data = new User() { Login = pLogin, PassWord = pPassWord, LoginServer = pLoginServer, BarCode = pBarCode }.ToJson();

                    HttpResult result = await GetDataHTTP.HTTPRequestAsync(0, "DCT/Login", Data, "application/json", null);
                    if (result.HttpState == eStateHTTP.HTTP_OK)
                    {
                        Result<BRB5.Model.AnswerLogin> res = JsonConvert.DeserializeObject<Result<BRB5.Model.AnswerLogin>>(result.Result);
                        if (!res.Success) return res;
                        Config.Role = res.Info?.Role ?? 0;
                        Config.CodeUser = res.Info?.CodeUser ?? 0;
                        Config.NameUser = res.Info?.NameUser;
                        Config.TypeDoc = res.Info?.TypeDoc;
                        Config.UserGuid = res.Info?.UserGuid ?? System.Guid.Empty;
                        CustomerBarCode = res.Info?.CustomerBarCode;

                        if (!string.IsNullOrEmpty(res.Info?.PathAPK))
                            Config.PathAPK = res.Info?.PathAPK;
                        isGroup = Config.TypeDoc.Any(el => el.KindDoc == eKindDoc.NotDefined);

                        Config.LocalCompany = res.Info?.LocalConnect ?? eCompany.NotDefined;
                        if (Config.LocalCompany == eCompany.Sim23)
                        {
                            СonnectorLocal = new ConnectorSE();
                            CustomerBarCode = CustomerBarCode.Append(new CustomerBarCode() { TypeBarCode = eTypeBarCode.ManualInput, LenghtCode = 7, TypeCode=eTypeCode.Code });///!!!!TMP!!!
                        }

                        //IsLocalPrice = Config.TypeDoc?.Where(el => el.KindDoc == eKindDoc.PriceCheck).FirstOrDefault()?.CodeApi == 1;
                        FileLogger.WriteLogMessage($"ConnectorPSU.Login=>(pLogin=>{pLogin}, pPassWord=>{pPassWord},pLoginServer=>{pLoginServer}) Res=>({Res.State},{Res.Info},{Res.TextError})", eTypeLog.Full);
                        return res.GetResult;
                    }
                    else
                        return new Result(result);
                }
                return new Result(-1, "Невідомий сервер");
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                return new Result(e);
            }
        }

        public override async Task<Result> LoadGuidDataAsync(bool pIsFull)
        {
            string Info = null;
            try
            {
                Config.OnProgress?.Invoke(0.03);
                AppContext.SetSwitch("System.Reflection.NullabilityInfoContext.IsSupported", true);
                string Data = Config.CodeWarehouse.ToString();
                HttpResult result = await GetDataHTTP.HTTPRequestAsync(0, "DCT/GetGuid", Data, "application/json", null,null,30);
                Config.OnProgress?.Invoke(0.4);
                if (result.HttpState == eStateHTTP.HTTP_OK)
                {
                    var res = JsonConvert.DeserializeObject<Result<BRB5.Model.Guid>>(result.Result);
                    Config.OnProgress?.Invoke(0.60);
                    if(res.Info!=null)
                        SaveGuide(res.Info, pIsFull);
                    Config.OnProgress?.Invoke(0.60);
                    var r=await GetRaitingTemplateAsync();
                    Info=$"Товарів=>{res?.Info?.Wares?.Count()}\nСкладів=>{res?.Info?.Warehouse?.Count()} \nШаблонів рейтингу =>{r?.Info?.Count()}";
                }                
                //await GetDaysLeft();
                Config.OnProgress?.Invoke(1);               
                return new Result(result) { Info=Info};
            }
            catch (Exception e)
            {                 
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                return new Result(e);
            }
        }

        /*public override ParseBarCode ParsedBarCode(string pBarCode, bool pIsOnlyBarCode)
        {
            base.ParsedBarCode(pBarCode, pIsOnlyBarCode);
            if (СonnectorLocal!=null)
               return СonnectorLocal.ParsedBarCode(pBarCode, pIsOnlyBarCode);
            pBarCode = pBarCode.Trim();
            ParseBarCode Res = new() { BarCode = pBarCode };
                        
            if (pBarCode.Length > 2 && pBarCode[..2].Equals("29") && pBarCode.Length == 13)
            {
                Res.CodeWares = Convert.ToInt32(pBarCode[2..8]);
                Res.Price = Convert.ToDecimal(pBarCode[8..13]);
            }
            return Res;
        }*/

        public override Result<WaresPrice> GetPrice(ParseBarCode pBC, eTypePriceInfo pTP = eTypePriceInfo.Short)
        {
            Config.OnProgress?.Invoke(0.2d);           
            Result<WaresPrice> Res = null;
            if (IsLocalPrice)
            {
                if (СonnectorLocal != null)
                   Res= СonnectorLocal.GetPrice(pBC, pTP);
            }
            else
            {
                try
                {
                    ApiPrice Data = new ApiPrice() { CodeWares = pBC.CodeWares, Article = pBC.Article, BarCode = pBC.BarCode, CodeWarehouse = Config.CodeWarehouse,TypePriceInfo= pTP };
                    HttpResult result = GetDataHTTP.HTTPRequest(0, "DCT/GetPrice", Data.ToJson(), "application/json", null);
                    if (result.HttpState == eStateHTTP.HTTP_OK)
                    {
                        Res = JsonConvert.DeserializeObject<Result<WaresPrice>>(result.Result);
                        if (Res.Info != null)                        
                            Res.Info.ParseBarCode = pBC;
                    }
                    else Res= new(result);
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                    Res= new(e); // Res = new WaresPrice(-1, e.Message);
                }
            }
            Config.OnProgress?.Invoke(0.9d);
            return Res;
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
            if (IsLocalPrice)
            {
                if (СonnectorLocal != null)
                    return СonnectorLocal.SendLogPrice(pLogPrice);
            }
            try
            {
                string Data = new LogPriceSave()
                {
                    CodeWarehouse = Config.CodeWarehouse,
                    CodeUser = Config.CodeUser,
                    SerialNumber = Config.SN,
                    LogPrice = pLogPrice
                }.ToJSON("yyyy-MM-ddTHH:mm:ss.ffff");
                HttpResult result = GetDataHTTP.HTTPRequest(0, @"DCT\SaveLogPrice", Data, "application/json", Config.Login, Config.Password, 120);
                if (result.HttpState == eStateHTTP.HTTP_OK)
                {
                    return JsonConvert.DeserializeObject<Result>(result.Result);
                }
                return new Result(result);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                return new Result(e);
            }
            /*
            if (pLogPrice == null)
                return new Result();
            StringBuilder sb = new StringBuilder();
            var Data = pLogPrice.Where(el => el.IsGoodBarCode).Select(el => new LogPriceSE(el));
            HttpResult res = Http.HTTPRequest(0, "pricetag", Data.ToJSON(), "application/json;charset=utf-8", Config.Login, Config.Password);
            return new Result(res);*/
        }

        public override async Task<Result> LoadDocsDataAsync(int pTypeDoc, string pNumberDoc, bool pIsClear)
        {
            TypeDoc TD = null;
            try
            {
                TD = Config.GetDocSetting(pTypeDoc);
            }
            catch { }
            try
            {
                if (TD?.KindDoc == eKindDoc.RaitingDoc)
                {
                    HttpResult result = await GetDataHTTP.HTTPRequestAsync(0, "DCT/Rating/GetRatingDoc", $"{Config.CodeUser}", "application/json", null);
                    if (result.HttpState == eStateHTTP.HTTP_OK)
                    {
                        var res = JsonConvert.DeserializeObject<Result<IEnumerable<Doc>>>(result.Result);
                        if (res.State == 0)                        
                            db.ReplaceDoc(res.Info);                        
                    }
                    else
                        return new Result(result); 
                    return new Result();
                }
                else
                {
                    if(TD?.CodeApi==1)
                    {
                        if (СonnectorLocal != null)
                            return await СonnectorLocal.LoadDocsDataAsync(pTypeDoc, pNumberDoc, pIsClear);
                        return new Result(-1, "Локальний конектор не визначено");
                    }

                    AppContext.SetSwitch("System.Reflection.NullabilityInfoContext.IsSupported", true);
                    GetDocs Data = new() { CodeWarehouse = Config.CodeWarehouse, TypeDoc = pTypeDoc, NumberDoc = pNumberDoc,CodeUser=Config.CodeUser };
                    HttpResult result = await GetDataHTTP.HTTPRequestAsync(0, "DCT/LoadDoc", Data.ToJson(), "application/json", null);
                    if (result.HttpState == eStateHTTP.HTTP_OK)
                    {
                        var res = JsonConvert.DeserializeObject<Result<Docs>>(result.Result);
                        if (res.State == 0)
                        {
                            db.ReplaceDoc(res.Info.Doc,TD?.IsOnlyHttp==true ?pTypeDoc : 0 );
                            db.ReplaceDocWaresSample(res.Info.Wares);
                        }
                        return new Result();
                    }
                    return new Result(result);
                }
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                return new Result(e);
            }
        }

        
        /// <summary>
        /// Вивантаження документів з ТЗД (HTTP)
        /// </summary>
        /// <param name="pDoc"></param>
        /// <param name="pWares"></param>
        /// <param name="pIsClose"></param>
        /// <returns></returns>
        public override async Task<Result> SendDocsDataAsync(DocVM pDoc, IEnumerable<DocWares> pWares)
        {
            try
            {
                var TD = Config.GetDocSetting(pDoc.TypeDoc);
                if (TD?.CodeApi == 1 || (Config.IsTest && Config.LocalCompany==eCompany.Sim23 && (pDoc.TypeDoc==5 || pDoc.TypeDoc == 14 || pDoc.TypeDoc == 15) )) //Тимчасовий хак.
                {
                    if (СonnectorLocal != null)
                    {
                        var Res= await СonnectorLocal.SendDocsDataAsync(pDoc, pWares);
                        //if(!(Config.IsTest &&  Config.LocalCompany == eCompany.Sim23 && (pDoc.TypeDoc == 5 || pDoc.TypeDoc == 14)))
                        //{
                            return Res;
                        //}
                    }
                    else
                        return new Result(-1, "Локальний конектор не визначено");
                }

                string Data = new SaveDoc() { NameDCT=Config.SN, Doc = pDoc, Wares = pWares }.ToJson();
                HttpResult result = await GetDataHTTP.HTTPRequestAsync(0, "DCT/SaveDoc", Data, "application/json", null);
                if (result.HttpState == eStateHTTP.HTTP_OK)
                {
                    return JsonConvert.DeserializeObject<Result>(result.Result);
                }
                return new Result(result);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                return new Result(e);
            }
        }
        /// <summary>
        /// Вивантаження Рейтингів
        /// </summary>
        /// <param name="pR"></param>
        /// <returns></returns>
        public override async Task<Result> SendRatingAsync(IEnumerable<RaitingDocItem> pR, DocVM pDoc, bool pIsArchive = false)
        {
            try
            {
                if (pIsArchive)
                {
                    var Res = await SendRatingFilesArchiveAsync(pDoc.NumberDoc);                    
                    if (Res.State == 0)
                        await SendRatingFilesAsync(pDoc.NumberDoc);
                    return Res;
                }
                
                var sw = Stopwatch.StartNew();
                // return await SendRatingFilesAsync(pDoc.NumberDoc);
                OnSave?.Invoke($"Старт збереження відповідей");
                string Data = new RaitingDocForSave() 
                { NumberDoc=pDoc.NumberDoc, CodeUser = Config.CodeUser, DTEnd = pDoc.DTEnd, DTStart = pDoc.DTStart, Item = pR.Select(el=>new RaitingDocItemSave(el) ) 
                }.ToJSON("yyyy-MM-ddTHH:mm:ss.ffff");
                HttpResult result = GetDataHTTP.HTTPRequest(0, @"DCT\Rating\SaveRating", Data, "application/json", Config.Login, Config.Password,180);
                sw.Stop();
                TimeSpan TimeSend = sw.Elapsed;
                string mes= $"Збереження відповідей=>{result.HttpState} за {TimeSend.TotalSeconds:n1}c";
                FileLogger.WriteLogMessage(mes);
                OnSave?.Invoke(mes);


                if (result.HttpState == eStateHTTP.HTTP_OK)
                {
                    var R = JsonConvert.DeserializeObject<Result>(result.Result);
                    if (R.State == 0)
                    {
                        return await SendRatingFilesAsync(pDoc.NumberDoc);
                    }
                    return R;
                }

                return new Result(result);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                return new Result(e);
            }
        }

        CultureInfo provider = CultureInfo.InvariantCulture;

        string DirArx => Path.Combine(Config.PathDownloads, "arx");
        public  async Task<Result> SendRatingFilesArchiveAsync(string pNumberDoc)
        {
            string path = Path.Combine(DirArx, pNumberDoc);            
            if(!Directory.Exists(path))
                return new Result(0, "Ok", "Фото в архіві ще немає!");
            var Files = Directory.GetFiles(path);
            FileLogger.WriteLogMessage($"SendRatingFilesArchiveAsync NumberDoc=>{pNumberDoc} Files=>{Files?.Length}", eTypeLog.Full);
            int Ok = 0, Error = 0, i = 0;
            //OnSave?.Invoke($"Файлів для передачі=>{Files.Count()}");
            if (Files.Length > 0)
                foreach (var f in Files)
                {
                    try
                    {
                        FileLogger.WriteLogMessage($"SendRatingFilesArchiveAsync start Files=>{f}", eTypeLog.Full);
                        string RR = await Http.UploadFileAsync(GetDataHTTP.Url[0][0] + "DCT/Rating/UploadFile", f);
                        FileLogger.WriteLogMessage($"SendRatingFilesArchiveAsync end Files=>{f} RR", eTypeLog.Full);
                        if (!string.IsNullOrEmpty(RR))
                            Ok++;
                        else
                            Error++;
                        Config.OnProgress?.Invoke((double)++i / (double)Files.Length);
                    }
                    catch(Exception e)
                    {
                        FileLogger.WriteLogMessage($"SendRatingFilesArchiveAsync Error=>",e);
                        Error++;
                    }
                }  
            return new Result()
            {
                State = Error,
                Info = $"Передано файлів=>{Ok} з {Files.Count()}",
                TextError = Error > 0 ? $"Не передано файлів=>{Error}" : "Ok"
            };
        }

        /// <summary>
        /// Вивантажеємо на сервер файли Рейтингів
        /// pMaxSecondSend - скільки часу відправляти, 0 - без обмежень.
        /// pSecondSkip - скільки хв не відправляти файл(для фонового відправлення
        /// </summary>
        /// <returns></returns>
        public override async Task<Result> SendRatingFilesAsync(string pNumberDoc, int pTry = 2, int pMaxSecondSend = 0, int pSecondSkip = 0)
        {
            try
            {
                FileLogger.WriteLogMessage($"SendRaitingFiles Start pNumberDoc=>{pNumberDoc} pTry=>{pTry} pMaxSecondSend=>{pMaxSecondSend} pSecondSkip=>{pSecondSkip}", eTypeLog.Full);

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
                    //var DirArx = Path.Combine(Config.PathDownloads, "arx");
                    if (!Directory.Exists(DirArx))
                        Directory.CreateDirectory(DirArx);

                    if (!Directory.Exists(Path.Combine(DirArx, pNumberDoc)))
                        Directory.CreateDirectory(Path.Combine(DirArx, pNumberDoc));

                    var Files = Directory.GetFiles(Path.Combine(Config.PathFiles, pNumberDoc));
                    FileLogger.WriteLogMessage($"SendRaitingFiles Files=>{Files?.Length}", eTypeLog.Full);
                    i = 0;
                    OnSave?.Invoke($"Файлів для передачі=>{Files.Count()}");
                    if (Files.Length > 0)
                        foreach (var f in Files)
                        {
                            var FI = new FileInfo(f);
                            if (pMaxSecondSend > 0 && (DateTime.Now - StartTime).TotalSeconds > pMaxSecondSend) continue;
                            try
                            {
                                string s = Path.GetFileNameWithoutExtension(f).Split('_')[2] + "_" + Path.GetFileNameWithoutExtension(f).Split('_')[3];
                                if (s.Length > 18) { s = s.Substring(0, 18); }
                                var DT = DateTime.ParseExact(s, "yyyyMMdd_HHmmssfff", provider);
                                if (pSecondSkip > 0 && (DateTime.Now - DT).TotalSeconds < pSecondSkip)
                                {
                                    FileLogger.WriteLogMessage($"SendRaitingFiles Skip DateCreateFile File=>{f} {DateTime.Now} / {DT}", eTypeLog.Full);
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
                                string RR = await Http.UploadFileAsync(GetDataHTTP.Url[0][0] + "DCT/Rating/UploadFile", f);

                                if (!string.IsNullOrEmpty(RR))
                                {
                                    var res = JsonConvert.DeserializeObject<Result>(RR);
                                    FileLogger.WriteLogMessage($"ConnectorPSU.SendRaitingFiles  File=>{f} success=>{res.State}");
                                    sw.Stop();
                                    TimeSpan TimeSend = sw.Elapsed;
                                    string text = res.Success ? $"[({i}:{Error})/{Files.Length}] {Path.GetFileName(f)}=> ({FI.Length / (1024 * 1024 * TimeSend.TotalSeconds):n2}Mb/c;{((double)FI.Length) / (1024d * 1024d):n2}Mb;{TimeSend.TotalSeconds:n1}c))" :
                                       $"[({i},{Error})/{Files.Length}] Файл не передано=>{Path.GetFileName(f)}";

                                    if (res.State == 0)
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

                                    OnSave?.Invoke(text);
                                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, text, eTypeLog.Full);
                                }
                                else
                                {
                                    Error++;
                                    FileLogger.WriteLogMessage($"ConnectorPSU.SendRaitingFiles=>(File={f}) Res=>({Res.State},{Res.Info},{Res.TextError})", eTypeLog.Expanded);
                                    LastError = Res;
                                    OnSave?.Invoke($"[({i},{Error})/{Files.Length}] Файл не передано=>{Path.GetFileName(f)}");
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
                        return await SendRatingFilesAsync(pNumberDoc, --pTry);
                    return Res;
                }
                finally { IsSaving = false; }
            }catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                return new Result(e);
            }
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
                result = await GetDataHTTP.HTTPRequestAsync(1, "StoreSettings", "{}", "application/json", "brb", "brb"); //charset=utf-8;

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
                HttpResult result = await GetDataHTTP.HTTPRequestAsync(0, "DCT/GetExpirationDate", pCodeWarehouse.ToString(), "application/json", null);

                if (result.HttpState == eStateHTTP.HTTP_OK)
                {
                    Config.OnProgress?.Invoke(0.5);
                    var res = JsonConvert.DeserializeObject<Result<IEnumerable<DocWaresExpirationSample>>>(result.Result);

                    /* result = await GetDataHTTP.HTTPRequestAsync(3, "DCT/GetExpirationWares", null, null, null);
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
                HttpResult result = await GetDataHTTP.HTTPRequestAsync(0, "DCT/SaveExpirationDate", Data, "application/json", null);
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
        public override async Task<Result<string>> GetNameWarehouseFromDoc(DocId pD)
        {
            try
            {
                HttpResult result = await GetDataHTTP.HTTPRequestAsync(0, "DCT/GetNameWarehouseFromDoc", pD.ToJson(), "application/json", null);
                if (result.HttpState == eStateHTTP.HTTP_OK)
                {
                    var res = JsonConvert.DeserializeObject<Result<string>>(result.Result);
                    if (res.State == 0) return res;
                }
                return new Result<string>(result);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                return new Result<string>(e);
            }
        }

        public override async Task<Result> GetInfo() {
            try
            {
                HttpResult result = await GetDataHTTP.HTTPRequestAsync(0, "GetInfo", null, "application/json", null);
                if (result.HttpState == eStateHTTP.HTTP_OK)
                {
                    var res = result.Result;
                    return new Result(){Info= res};
                }
                return new Result<string>(result);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                return new Result<string>(e);
            }
        }

        public override async Task<Result> UploadFile(string pFile, string pFileName = null)
        {
            string RR = await Http.UploadFileAsync(GetDataHTTP.Url[0][0] + "UploadFile", pFile, pFileName);
            var res = JsonConvert.DeserializeObject<Result>(RR);
            return res;
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
                HttpResult res = GetDataHTTP.HTTPRequest(0, "print/", json, "application/json", null, null);//"http://znp.vopak.local:8088/Print"
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
        #region Raiting
        public override async Task<Result<int>> GetIdRaitingTemplate()
        {
            HttpResult result = await GetDataHTTP.HTTPRequestAsync(0, "DCT/Raitting/GetIdRaitingTemplate", null, "application/json", "brb", "brb");//

            if (result.HttpState == eStateHTTP.HTTP_OK)
            {
                var r = JsonConvert.DeserializeObject<Result<int>>(result.Result);
                return r;
            }
            return null;
        }

        public override async Task<Result> GetNumberDocRaiting()
        {
            HttpResult result = await GetDataHTTP.HTTPRequestAsync(0, "DCT/Raitting/GetNumberDocRaiting", null, "application/json", "brb", "brb");//

            if (result.HttpState == eStateHTTP.HTTP_OK)
            {
                var r = JsonConvert.DeserializeObject<Result>(result.Result);
                return r;
            }
            return null;
        }

        public override async Task<Result> SaveTemplate(RaitingTemplate pRT)
        {
            HttpResult result = await GetDataHTTP.HTTPRequestAsync(0, "DCT/Raitting/SaveTemplate", pRT.ToJSON("yyyy-MM-ddTHH:mm:ss"), "application/json", "brb", "brb");//

            if (result.HttpState == eStateHTTP.HTTP_OK)
            {
                var r = JsonConvert.DeserializeObject<Result>(result.Result);
                return r;
            }
            return null;
        }

        public override async Task<Result> SaveDocRaiting(DocVM pDoc)
        {
            HttpResult result = await GetDataHTTP.HTTPRequestAsync(0, "DCT/Raitting/SaveDocRaiting", pDoc.ToJSON("yyyy-MM-ddTHH:mm:ss"), "application/json", "brb", "brb");//

            if (result.HttpState == eStateHTTP.HTTP_OK)
            {
                var r = JsonConvert.DeserializeObject<Result>(result.Result);
                return r;
            }
            return null;
        }

        public override async Task<Result<IEnumerable<RaitingTemplate>>> GetRaitingTemplateAsync()
        {
            try
            {
                var result = await GetDataHTTP.HTTPRequestAsync(0, "DCT/Rating/GetRatingTemplate", $"{Config.CodeUser}", "application/json", null, null, 120);
                if (result.HttpState == eStateHTTP.HTTP_OK)
                {
                    var res = JsonConvert.DeserializeObject<Result<IEnumerable<RaitingTemplate>>>(result.Result);
                    if (res.State == 0)
                    {
                        db.ReplaceRaitingTemplate(res.Info);
                        foreach (var el in res.Info)
                            if (el.Item.Any())
                                db.ReplaceRaitingTemplateItem(el.Item);
                        return res;
                    }
                    return new();
                }
                return new(result);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                return new(e);
            }
        }
 
        public override async Task<Result<IEnumerable<Doc>>> GetRaitingDocsAsync()
        {
            HttpResult result = await GetDataHTTP.HTTPRequestAsync(0, "DCT/Raitting/GetRaitingDocs", null, "application/json", "brb", "brb");//

            if (result.HttpState == eStateHTTP.HTTP_OK)
            {
                var r = JsonConvert.DeserializeObject<IEnumerable<DocVM>>(result.Result);
                return new Result<IEnumerable<Doc>>() { Info = r };
            }
            else
                return new Result<IEnumerable<Doc>>(result, null);
        }

        public override async Task<Result<IEnumerable<DocVM>>> GetPromotion(int pCodeWarehouse)
        {
            HttpResult result = await GetDataHTTP.HTTPRequestAsync(0, "DCT/CheckPromotion/Doc", pCodeWarehouse.ToJSON(), "application/json", "brb", "brb");

            if (result.HttpState == eStateHTTP.HTTP_OK)
            {
                var r = JsonConvert.DeserializeObject<Result<IEnumerable<DocVM>>>(result.Result);
                return r;
            }
            return null;
        }

        public override async Task<Result<IEnumerable<DocWares>>> GetPromotionData(string pNumberDoc)
        {
            HttpResult result = await GetDataHTTP.HTTPRequestAsync(0, "DCT/CheckPromotion/GetPromotionData", "\"" + pNumberDoc + "\"", "application/json", "brb", "brb");

            if (result.HttpState == eStateHTTP.HTTP_OK)
            {
                var r = JsonConvert.DeserializeObject<Result<IEnumerable<DocWares>>>(result.Result);
                return r;
            }
            return null;
        }

        public override async Task<Result<IEnumerable<Client>>> GetClient(string pBarCode)
        {
            var dd = new { BarCode = pBarCode };
            HttpResult result = await GetDataHTTP.HTTPRequestAsync(0, "DCT/GetClient", dd.ToJSON(), "application/json", "brb", "brb");

            if (result.HttpState == eStateHTTP.HTTP_OK)
            {
                var r = JsonConvert.DeserializeObject<Result<IEnumerable<Client>>>(result.Result);
                return r;
            }
            return null;
        }
        #endregion
    }

}


