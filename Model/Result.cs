using System;
using System.Collections.Generic;
using System.Text;

namespace BRB5.Model
{
    public class Result
    {
        public int State { get; set; } = 0;
        public string TextError { get; set; }
        public string Info { get; set; }
        public eStateHTTP StateHTTP { get; set; }

        public Result() { }        

        public Result(int pState = 0, string pTextError = "Ok", string pInfo = "")
        {
            State = pState;
            TextError = pTextError;
            Info = pInfo;
        }

        public Result(HttpResult httpResult, string pInfo = "")
        {
            StateHTTP = httpResult.HttpState;

            if (httpResult.HttpState == eStateHTTP.HTTP_OK)
                Info = httpResult.Result;
            else
            {
                State = -1;
                TextError = httpResult.HttpState.ToString();
            }
            
            Info = pInfo;
        }
        public Result(Exception e)
        {
            State = -1;
            TextError = e.Message + "\n" + e.StackTrace;
        }        
    }

    public class Result<T>
    {
        public int State { get; set; } = 0;
        public string TextError { get; set; }
        public T Info { get; set; }
        public eStateHTTP StateHTTP { get; set; }

        public Result() { }

        public Result(Result pR)
         {
            State = pR.State;
            TextError = pR.TextError;
        }

        public Result(int pState = 0, string pTextError = "Ok")
        {
            State = pState;
            TextError = pTextError;            
        }

        public Result(HttpResult httpResult, T pInfo )
        {
            StateHTTP = httpResult.HttpState;

            if (httpResult.HttpState == eStateHTTP.HTTP_OK) ;
            //Info = httpResult.Result;
            else
            {
                State = -1;
                TextError = httpResult.HttpState.ToString();
            }

            Info = pInfo;
        }
        public Result(Exception e)
        {
            State = -1;
            TextError = e.Message + "\n" + e.StackTrace;
        }
        public Result GetResult { get { return new Result { State=State, TextError = TextError }; } }
    }

}
