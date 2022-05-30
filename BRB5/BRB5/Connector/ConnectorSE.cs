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

namespace BRB5.Connector
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
        public override IEnumerable<TypeDoc> GetTypeDoc(eRole pRole)
        {
            var Res = new List<TypeDoc>()
            {    new TypeDoc() { CodeDoc = 11, KindDoc = eKindDoc.Raiting, NameDoc = "Опитування" }, };
            return Res;
        }

        //DB db = DB.GetDB();
        public override Result Login(string pLogin, string pPassWord, eLoginServer pLoginServer)
        {
            Result Res = new Result();
            if (pLoginServer == eLoginServer.Bitrix)
            {
                string data = JsonConvert.SerializeObject(new RequestLogin() { action = "auth", login = pLogin, password = pPassWord });
                HttpResult result = Http.HTTPRequest(2, "", data, "application/json");
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
                        Res= new Result();
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
                HttpResult res = Http.HTTPRequest(pLoginServer == eLoginServer.Central ? 1 : 0, "login", "{\"login\" : \"" + pLogin + "\"}", "application/json", pLogin, pPassWord);
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

        public override Result LoadDocsData(int pTypeDoc, string pNumberDoc, ObservableInt pProgress, bool pIsClear)
        {
            var Res = new Result();
            if (pTypeDoc == 11)
            {
                string data = JsonConvert.SerializeObject(new Request() { userId = Config.CodeUser, action = "templates" });
                HttpResult result = Http.HTTPRequest(2, "", data, "application/json");//

                string data2 = JsonConvert.SerializeObject(new Request() { userId = Config.CodeUser, action = "plans" });
                HttpResult result2 = Http.HTTPRequest(2, "", data2, "application/json");//

                if (result.HttpState != eStateHTTP.HTTP_OK || result2.HttpState != eStateHTTP.HTTP_OK)
                    return new Result(result.HttpState != eStateHTTP.HTTP_OK ? result : result2);
                else
                    try
                    {
                        bool AddDoc;
                        var t = JsonConvert.DeserializeObject<Template>(result.Result);
                        var p = JsonConvert.DeserializeObject<Data>(result2.Result, new IsoDateTimeConverter { DateTimeFormat = "dd.MM.yyyy HH:mm:ss" });
                        var d = new List<Doc>();
                        var r = new List<Raiting>();
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
                                        r.Add(new Raiting() { TypeDoc = pTypeDoc, NumberDoc = DocNumber, Id = -el.sectionId, Parent = -el.parentId, Text = el.text, IsHead = true, RatingTemplate =  8 ,OrderRS= el.sectionId });
                                if (elt.questions != null)
                                    foreach (var el in elt.questions)
                                        r.Add(new Raiting() { TypeDoc = pTypeDoc, NumberDoc = DocNumber, Id = el.questionId, Parent = -el.sectionId, Text = el.text, IsHead = false, RatingTemplate = el.RatingTemplate,OrderRS=el.questionId });

                                r.Add(new Raiting() { TypeDoc = pTypeDoc, NumberDoc = DocNumber, Id = -1, Parent = 9999999, Text = "Всього", IsHead = false, RatingTemplate = 8, OrderRS = 9999999 });

                            }
                        }
                        db.ReplaceDoc(d);
                        db.ReplaceRaitingSample(r);
                        FileLogger.WriteLogMessage($"ConnectorPSU.LoadDocsData=>(pTypeDoc=>{pTypeDoc}, pNumberDoc=>{pNumberDoc},pIsClear=>{pIsClear}) Res=>(Doc=>{d.Count()},RS=>{r.Count()},{Res.State},{Res.Info},{Res.TextError})", eTypeLog.Full);

                        return Res;
                    }
                    catch (Exception e)
                    {
                        Res = new Result(-1, e.Message);
                        FileLogger.WriteLogMessage($"ConnectorPSU.LoadDocsData=>(pTypeDoc=>{pTypeDoc}, pNumberDoc=>{pNumberDoc},pIsClear=>{pIsClear}) Res=>({Res.State},{Res.Info},{Res.TextError})", eTypeLog.Error);
                        return Res;
                    }
            }
            FileLogger.WriteLogMessage($"ConnectorPSU.LoadDocsData=>(pTypeDoc=>{pTypeDoc}, pNumberDoc=>{pNumberDoc},pIsClear=>{pIsClear}) Res=>({Res.State},{Res.Info},{Res.TextError})", eTypeLog.Full);

            return Res;
        }

        /// <summary>
        /// Вивантаження документів з ТЗД (HTTP)
        /// </summary>
        /// <param name="pDoc"></param>
        /// <param name="pWares"></param>
        /// <param name="pIsClose"></param>
        /// <returns></returns>
        public override Result SendDocsData(Doc pDoc, IEnumerable<DocWares> pWares, int pIsClose)
        {
            return null;
        }
        /// <summary>
        /// Вивантаження Рейтингів
        /// </summary>
        /// <param name="pR"></param>
        /// <returns></returns>
        public override Result SendRaiting(IEnumerable<Raiting> pR, Doc pDoc)
        {
            OnSave?.Invoke("StartSave");
            var Res = new Result();
            try
            {
                var RD = new List<Raitings>();
                foreach (var el in pR)
                {
                    RD.Add(new Raitings() { questionId = el.Id, value = el.Rating, comment = el.Note });
                }

                Raiting e = pR.FirstOrDefault(d => d.Id == -1);
                if (e == null || e.Id == 0)
                    e = pR.FirstOrDefault();
                var r = new RequestSendRaiting() { userId = Config.CodeUser, action = "results", answers = RD, planId = int.Parse(e.NumberDoc), text = e.Note };
                string data = JsonConvert.SerializeObject(r);
                HttpResult result = Http.HTTPRequest(2, "", data, "application/json");//

                if (result.HttpState != eStateHTTP.HTTP_OK)
                    Res= new Result(result);
                else
                {
                    var res = JsonConvert.DeserializeObject<AnswerSendRaiting>(result.Result);
                    OnSave?.Invoke($"SendRaiting=> (res.success={res.success})");
                    if (res.success)
                    {
                        Res=SendRaitingFiles(e.NumberDoc);
                    }
                }
            }
            catch (Exception ex)
            {
                Res = new Result(ex);
            }
            FileLogger.WriteLogMessage($"ConnectorPSU.SendRaiting=>() Res=>({Res.State},{Res.Info},{Res.TextError})", eTypeLog.Error);
            OnSave?.Invoke("StartSave");
            return Res;
        }

        /// <summary>
        /// Вивантажеємо на сервер файли Рейтингів
        /// </summary>
        /// <returns></returns>
        public override Result SendRaitingFiles(string pNumberDoc)
        {
            int Sucsses = 0;
            Result LastError = null;
            var Res = new Result();
            var DirArx = Path.Combine(Config.GetPathFiles, "arx");
            if (!Directory.Exists(DirArx))
            {
                Directory.CreateDirectory(DirArx);
            }
            if (!Directory.Exists(Path.Combine( DirArx, pNumberDoc)))
            {
                Directory.CreateDirectory(Path.Combine(DirArx, pNumberDoc));
            }

            var R = new RequestSendRaitingFile() { planId = int.Parse(pNumberDoc), action = "file", userId = Config.CodeUser };

            var Files = Directory.GetFiles(Path.Combine(Config.GetPathFiles, pNumberDoc));
            int i = 0;
            foreach (var f in Files)
            {
                i++;
                if (StopSave)
                {
                    StopSave = false;
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
                    HttpResult result = Http.HTTPRequest(2, "", data, "application/json",null,null,60);

                    if (result.HttpState == eStateHTTP.HTTP_OK)
                    {
                        var res = JsonConvert.DeserializeObject<Answer>(result.Result);
                        if (res.success)
                        {
                            var FileTo = Path.Combine(DirArx, pNumberDoc, Path.GetFileName(f));
                            File.Copy(f, FileTo, true);
                            File.Delete(f);
                            //File.Move(f, FileTo);
                            Sucsses++;
                        }
                        else
                        {
                            Res = new Result(-1,"Не передався файл", f);
                        }
                        sw.Stop();
                        TimeSpan TimeSend = sw.Elapsed;
                        string text = $"ConnectorPSU.SendRaitingFiles [{i}/{Files.Length}] Send=>(File={f}, Speed=>{data.Length / (1024 * 1024 * TimeSend.TotalSeconds):n2}Mb, Size={((double)data.Length) / (1024d * 1024d):n2}Mb,Load={TimeLoad.TotalSeconds:n1},Send={TimeSend.TotalSeconds:n1}) Res=>({res})";
                        OnSave?.Invoke(text);
                        FileLogger.WriteLogMessage(text,  eTypeLog.Full );
                    }
                    else
                    {
                        FileLogger.WriteLogMessage($"ConnectorPSU.SendRaitingFiles=>(File={f}) Res=>({Res.State},{Res.Info},{Res.TextError})", eTypeLog.Expanded);
                        LastError = Res;
                    }
                }
                catch (Exception e)
                {
                    Res = new Result(e);
                    FileLogger.WriteLogMessage($"ConnectorPSU.SendRaitingFiles=>(File={f}) Res=>({Res.State},{Res.Info},{Res.TextError})", eTypeLog.Error);
                }
            }
            Res = LastError ?? Res;
            Res.TextError = $"Успішно відправлено  {Sucsses} файлів {Res.TextError}";
            return Res;
        }
    }


    /*
     JsonConvert.SerializeObject(invoice,
  Formatting.Indented,
  new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });
      */

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

    class Template: Answer
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

    class Data: Answer
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
    }
}
