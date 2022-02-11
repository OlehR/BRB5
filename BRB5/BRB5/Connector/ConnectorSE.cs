using BRB5.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BRB5.Connector
{
    class ResultLogin : Result
    {
        public int? Profile { get; set; }
    }
    public class ConnectorSE : Connector
    {
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

        public override bool LoadDocsData(int pTypeDoc, string pNumberDoc, ObservableInt pProgress, bool pIsClear)
        {
            if (pTypeDoc == 11)
            {


            }
            return true;
        }

    }

    /*
     JsonConvert.SerializeObject(invoice,
  Formatting.Indented,
  new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });
      */

    class Request
    {
        public int token { get; set; }
        public string action { get; set; }
        public int? userId { get; set; }
    }

    class RequestLogin: Request
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
    }

    class DataTemplate
    {
        public int templateId { get; set; }
        public string templateName { get; set; }
        public DateTime updated { get; set; }
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
        public int templateId { get; set; }
        public int shopId { get; set; }
        public DateTime date { get; set; }
    }

    class Data
    {
        public bool success { get; set; }
        public IEnumerable<DataData> data { get; set; }
    }


}
