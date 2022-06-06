using System;
using System.IO;
using BRB5.Connector;
using BRB5.Model;

namespace BRB5
{
    public class Utils
    {
        static Utils utils = null;
        public static Utils GetInstance()
        {
            if (utils == null)
                utils = new Utils();
            return utils;
        }

        public bool LoadAPK(string pPath, string pNameAPK, Action<int> pProgress, int pVersionCode)
        {
            GetDataHTTP Http = GetDataHTTP.GetInstance();
            try
            {
                pProgress?.Invoke(0);
                string FileNameVer = Path.Combine(Config.PathDownloads, "Ver.txt");
                Http.GetFile(pPath + "Ver.txt", Config.PathDownloads);
                string Ver = File.ReadAllText(FileNameVer);

                pProgress?.Invoke(10);
                if (Ver != null && Ver.Length > 0)
                {
                    int ver = 0;
                    try
                    {
                        ver =int.Parse(Ver);
                    }
                    catch (Exception )
                    {
                    }
                    if (ver > pVersionCode)
                    {
                        string FileName = Path.Combine(Config.PathDownloads, pNameAPK) ;
                        Http.GetFile(pPath + pNameAPK, Config.PathDownloads);

                        pProgress?.Invoke(60);                        
                        
                        return true;
                    }
                }
            }
            catch (Exception e)
            { 
                //e.printStackTrace();
            }

            pProgress?.Invoke(100);
            return false;
        }
     
    }
}
