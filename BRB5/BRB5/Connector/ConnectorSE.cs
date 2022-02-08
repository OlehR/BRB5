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
    class ConnectorSE: Connector
    {
        public override Result Login( String pLogin,  String pPassWord,  bool pIsLoginCO =false)
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
                        Config.Role = (eRole) r.Profile;
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
}
