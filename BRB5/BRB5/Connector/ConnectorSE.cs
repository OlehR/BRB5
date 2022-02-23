using BRB5.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json.Converters;
using System.IO;

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
            if (pLoginServer == eLoginServer.Bitrix)
            {
                string data = JsonConvert.SerializeObject(new RequestLogin() { action = "auth", login = pLogin, password = pPassWord });
                HttpResult result = Http.HTTPRequest(2, "", data, "application/json");
                if (result.HttpState != eStateHTTP.HTTP_OK)
                    return new Result(result);
                else
                {
                    try
                    {
                        var t = JsonConvert.DeserializeObject<AnswerLogin>(result.Result);
                        if (!t.success || t.data.userId <= 0)
                            return new Result(-1, "Не успішна авторизація. Можливо невірний логін чи пароль");
                        Config.CodeUser = t.data.userId;
                        return new Result();
                    }
                    catch (Exception e) { return new Result(e); }
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
                    return new Result(res, "Ви не підключені до мережі " + Config.Company.ToString());
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
                            return new Result();
                        }
                        else
                            return new Result(r.State, r.TextError, "Неправильний логін або пароль");
                    }
                    catch (Exception e)
                    {
                        //Utils.WriteLog("e", TAG, "Login=>", e);
                        return new Result(-1, e.Message);
                    }
                }
            }
        }

        public override Result LoadDocsData(int pTypeDoc, string pNumberDoc, ObservableInt pProgress, bool pIsClear)
        {
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
                        var t = JsonConvert.DeserializeObject<Template>(result.Result);
                        var p = JsonConvert.DeserializeObject<Data>(result2.Result, new IsoDateTimeConverter { DateTimeFormat = "dd.MM.yyyy HH:mm:ss" });
                        var d = new List<Doc>();
                        var r = new List<Raiting>();
                        foreach (var elp in p.data)
                        {
                            var DocNumber = elp.planId.ToString();
                            d.Add(new Doc()
                            { TypeDoc = pTypeDoc, NumberDoc = DocNumber, DateDoc = elp.date, ExtInfo = elp.shopId.ToString() });


                            foreach (var elt in t.data.Where(el => el.templateId == elp.templateId))
                            {
                                if (elt.sections != null)
                                    foreach (var el in elt.sections)
                                        r.Add(new Raiting() { TypeDoc = pTypeDoc, NumberDoc = DocNumber, Id = -el.sectionId, Parent = -el.parentId, Text = el.text, IsHead = true, RatingTemplate = 1 + 2 + 4 + 8 });
                                if (elt.questions != null)
                                    foreach (var el in elt.questions)
                                        r.Add(new Raiting() { TypeDoc = pTypeDoc, NumberDoc = DocNumber, Id = el.questionId, Parent = -el.sectionId, Text = el.text, IsHead = false, RatingTemplate = el.RatingTemplate });

                                r.Add(new Raiting() { TypeDoc = pTypeDoc, NumberDoc = DocNumber, Id = -1, Parent = 9999999, Text = "Всього", IsHead = false, RatingTemplate = 0 });

                            }
                        }
                        db.ReplaceDoc(d);
                        db.ReplaceRaitingSample(r);
                    }
                    catch (Exception e)
                    {
                        return new Result(-1, e.Message);
                    }
            }
            return null;
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
        public override Result SendRaiting(IEnumerable<Raiting> pR)
        {
            try
            {
                var RD = new List<Raitings>();
            foreach (var el in pR)
            {
                RD.Add(new Raitings() { questionId = el.Id, value = el.Rating, comment = el.Note });
            }

            Raiting e = pR.FirstOrDefault(d => d.Id == -1);
            if (e == null || e.Id==0)
                e = pR.FirstOrDefault();
            var r = new RequestSendRaiting() { userId = Config.CodeUser, action = "results", answers = RD, planId = int.Parse(e.NumberDoc), text = e.Note };
            string data = JsonConvert.SerializeObject(r);
            HttpResult result = Http.HTTPRequest(2, "", data, "application/json");//

            if (result.HttpState != eStateHTTP.HTTP_OK)
                return new Result(result);
            
                var res = JsonConvert.DeserializeObject<AnswerSendRaiting>(result.Result);
                if (res.success)
                {
                    SendRaitingFiles(e.NumberDoc);
                }
                return null;
            }
            catch (Exception ex)
            {
                return new Result(ex);
            }
        }

        /// <summary>
        /// Вивантажеємо на сервер файли Рейтингів
        /// </summary>
        /// <returns></returns>
        public override Result SendRaitingFiles(string NumberDoc)
        {
            var R = new RequestSendRaitingFile() { planId = int.Parse(NumberDoc), action = "file",userId=Config.CodeUser};
            foreach (var f in Directory.GetFiles(Path.Combine(Config.GetPathFiles, NumberDoc)))
            {
                R.file = Convert.ToBase64String(File.ReadAllBytes(Path.Combine(Config.GetPathFiles, f)));
                R.fileExt = Path.GetExtension(f).Substring(1);
                R.questionId = int.Parse(Path.GetFileName(f).Split('_')[0]);

                string data = JsonConvert.SerializeObject(R);
                HttpResult result = Http.HTTPRequest(2, "", data, "application/json");

                if (result.HttpState == eStateHTTP.HTTP_OK)
                {
                    var res = JsonConvert.DeserializeObject<Answer>(result.Result);
                }
                
            }
            return new Result();
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
    }

    class Questions
    {
        public int questionId { get; set; }
        public int sectionId { get; set; }
        public string text { get; set; }
        public int value { get; set; }
        public int[] answers { get; set; }
        public int RatingTemplate { get { int r = 0; for (int i = 0; i < answers.Length; i++) r += (1 >> answers[i]); return r; } }
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
