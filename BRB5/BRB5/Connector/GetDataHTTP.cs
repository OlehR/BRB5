using BRB5.Model;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BRB5.Connector
{
    public class GetDataHTTP
    {
        protected static GetDataHTTP Instance = null;
        static string[][] Url;
        static int[] DefaultApi;
        
        protected static string TAG = "BRB5/GetDataHTTP";
    
        public GetDataHTTP()
        {
            var db = DB.GetDB();
            var ApiUrl1 = db.GetConfig<string>("ApiUrl1");
            var ApiUrl2 = db.GetConfig<string>("ApiUrl2");
            var ApiUrl3 = db.GetConfig<string>("ApiUrl3");
            Init(new string[] { ApiUrl1, ApiUrl2, ApiUrl3 });
        }
        
        public void Init(string[] pUrl)
        {
            DefaultApi = new int[pUrl.Length];
            Url = new string[3][];
            for (int i = 0; i < pUrl.Length; i++)
            {
                DefaultApi[i] = 0;
                if (pUrl[i] != null)
                {
                    string[] Urls = pUrl[i].Split(';');
                    Url[i] = Urls;
                }
            }
            Instance = this;
        }

        public static GetDataHTTP GetInstance()
        {
            if (Instance == null)
            {
               Instance =  new GetDataHTTP();
            }
            return Instance;
        }

        public async Task<HttpResult> HTTPRequestAsync(String pURL, String pData, String pContentType, String pLogin, String pPassWord)
        {
            try
            {
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

                HttpClient client;
                if (string.IsNullOrEmpty(pLogin))
                    client = new HttpClient();
                else
                {
                    var credentials = new NetworkCredential(pLogin, pPassWord);
                    handler.Credentials = credentials;
                    client = new HttpClient(handler);
                }

                client.Timeout = TimeSpan.FromSeconds(5);
                
                //client.BaseAddress = new Uri(pURL);
                //var byteArray = Encoding.ASCII.GetBytes("admin:Xa38dF79");
                //client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));


                StringContent content = new StringContent(pData, Encoding.UTF8, pContentType);
                HttpResponseMessage response = await client.PostAsync(pURL, content).ConfigureAwait(continueOnCapturedContext: false); 
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    return new HttpResult() { HttpState = eStateHTTP.HTTP_OK, Result = result };
                }
                else
                {
                    return new HttpResult() { HttpState = (eStateHTTP)(int) response.StatusCode, Result = response.StatusCode.ToString() };
                }               
            }
            catch(Exception e)
            {
                return new HttpResult() { HttpState = eStateHTTP.Exeption, Result = e.Message };
            }
        }

        public HttpResult HTTPRequest(int pUrlApi, String pApi, String pData, String pContentType, String pLogin=null, String pPassWord=null)
        { //!!!!TMP
            if (pUrlApi == 2)
            {
                pLogin = "TSD";
                pPassWord = "321123Sd";
            }
            if (pLogin != null && pLogin.Equals("Admin"))
            {
                pLogin = (Config.Company == eCompany.Sim23 ? "brb" : "c");
                pPassWord = (Config.Company == eCompany.Sim23 ? "brb" : "c");
            }
            HttpResult res = new HttpResult();
            if (Url != null && Url.Length >= pUrlApi && Url[pUrlApi] != null)
            {
                res = HTTPRequestAsync(Url[pUrlApi][DefaultApi[pUrlApi]] + pApi, pData, pContentType, pLogin, pPassWord).Result;
                if (res.HttpState != eStateHTTP.HTTP_OK && res.HttpState != eStateHTTP.HTTP_UNAUTHORIZED)
                {
                    for (int i = 0; i < Url[pUrlApi].Length; i++)
                    {
                        if (i != DefaultApi[pUrlApi] && !string.IsNullOrEmpty( Url[pUrlApi][i]))
                        {
                            res = HTTPRequestAsync(Url[pUrlApi][i] + pApi, pData, pContentType, pLogin, pPassWord).Result;
                            if (res.HttpState == eStateHTTP.HTTP_OK)
                                DefaultApi[pUrlApi] = i;
                        }
                    }
                }
            }
            return res;
        }
    }
}
