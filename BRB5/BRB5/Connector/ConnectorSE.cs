using BRB5.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json.Converters;

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
        public override Result Login(String pLogin, String pPassWord, bool pIsLoginCO = false)
        {
            HttpResult res = Http.HTTPRequest(pIsLoginCO ? 1 : 0, "login", "{\"login\" : \"" + pLogin + "\"}", "application/json", pLogin, pPassWord);
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
                                if (elt.section != null)
                                    foreach (var el in elt.section)
                                        r.Add(new Raiting() { TypeDoc = pTypeDoc, NumberDoc = DocNumber, Id = el.sectionId, Parent = el.parentId, Text = el.text, IsHead = true, RatingTemplate = 1 + 2 + 4 + 8 });
                                if (elt.questions != null)
                                    foreach (var el in elt.questions)
                                        r.Add(new Raiting() { TypeDoc = pTypeDoc, NumberDoc = DocNumber, Id = el.questionId, Parent = el.sectionId, Text = el.text, IsHead = false, RatingTemplate = el.RatingTemplate });
                                /*
      Id INTEGER  NOT NULL,
      Parent INTEGER  NOT NULL,
      IsHead INTEGER  NOT NULL DEFAULT(0),
      Text TEXT,
      RatingTemplate INTEGER         NOT NULL DEFAULT (0)*/
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
            var RD = new List<Raitings>();
            foreach (var el in pR)
            {
                RD.Add(new Raitings() { questionId = el.Id, value = el.Rating, comment = el.Text });
            }

            var e = pR.First(d => d.Id == -1);
            if (e == null)
                e = pR.First();
            var r = new RequestSendRaiting() { userId = Config.CodeUser, action = "results",  answers = RD,planId=int.Parse(e.NumberDoc), text=e.Note};
            string data2 = JsonConvert.SerializeObject(new Request() { userId = Config.CodeUser, action = "plans" });
            HttpResult result = Http.HTTPRequest(2, "", data2, "application/json");//

            if (result.HttpState != eStateHTTP.HTTP_OK )
                return new Result( result);
            try
            {

            }
            catch(Exception ex)
            {

            }
                    return null;
        }

    }





    /*
     JsonConvert.SerializeObject(invoice,
  Formatting.Indented,
  new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });
      */

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
        public IEnumerable<Section> section { get; set; }
        public IEnumerable<Questions> questions { get; set; }
    }

    class Template
    {
        public bool success { get; set; }
        public IEnumerable<DataTemplate> data { get; set; }
    }

    class DataData
    {
        public int planId { get; set; }
        public int templateId { get; set; }
        public int shopId { get; set; }
        public DateTime date { get; set; }
    }

    class Data
    {
        public bool success { get; set; }
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


}
