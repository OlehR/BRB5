using System;
using System.Collections.Generic;
using System.Text;

namespace BRB5.Model
{
    public class Result
    {
        public int State { get; set; }
        public string TextError { get; set; }
        public string Info { get; set; }

        public Result() { }

        public Result(int pState = 0, string pTextError = "Ok", string pInfo = "")
        {
            State = pState;
            TextError = pTextError;
            Info = pInfo;
        }

        public Result(HttpResult httpResult, string pInfo="")
        {
            if (httpResult.HttpState == eStateHTTP.HTTP_OK)
                Info = httpResult.Result;            
            else
            {
                State = -1;
                TextError = httpResult.HttpState.ToString();
            }
            Info = pInfo;
        }

    }

}
