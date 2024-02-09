using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using Utils;

namespace BRB5
{
    public class FTP
    {
        private string Host;
        private string User;
        private string PassWord;
       

        public FTP(string pHost = "ftp://93.183.216.37:2121", string pUser = "cs_audit", string pPassWord = "Fdj#486&Hbx")
        {
            Host = pHost;
            User = pUser;
            PassWord = pPassWord;
        }

        public bool FtpDirectoryExists(string pDir)
        {
            try
            {
                var request = (FtpWebRequest)WebRequest.Create($"{Host}/{pDir}");
                request.Credentials = new NetworkCredential(User, PassWord);
                request.Method = WebRequestMethods.Ftp.GetDateTimestamp;

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    return false;
                else
                    return true;
            }
            return true;
        }

        public bool CreateDir(string pDir)
        {
            try
            {
                WebRequest request = WebRequest.Create($"{Host}/{pDir}");
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                request.Credentials = new NetworkCredential(User, PassWord);
                using (var resp = (FtpWebResponse)request.GetResponse())
                {
                    Console.WriteLine(resp.StatusCode);
                }
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage($"FTP.UpLoad=>{e.Message}/nStackTrace=> {e.StackTrace}", eTypeLog.Error);
                return false;
            }
            return true;
        }

        public bool UpLoad(string pPathFile , string pDir,string pFileName=null)
        {
            try
            {
                if(string.IsNullOrEmpty(pFileName))
                 pFileName = new FileInfo(pPathFile).Name;
                string uploadUrl = $"{Host}/{pDir}{pFileName}";
                FtpWebRequest req = (FtpWebRequest)FtpWebRequest.Create(uploadUrl);
                req.Proxy = null;
                req.Method = WebRequestMethods.Ftp.UploadFile;
                req.Credentials = new NetworkCredential(User, PassWord);
                req.UseBinary = true;
                req.UsePassive = true;
                byte[] data = File.ReadAllBytes(pPathFile);
                req.ContentLength = data.Length;
                Stream stream = req.GetRequestStream();
                stream.Write(data, 0, data.Length);
                stream.Close();
                FtpWebResponse res = (FtpWebResponse)req.GetResponse();
                //return res.StatusDescription;
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage($"FTP.UpLoad=>{e.Message}/nStackTrace=> {e.StackTrace}", eTypeLog.Error);
                return false;
            }
            //  fs.Flush();
            return true;
        }

        public bool UpLoad(byte[] pData, string pPathFileTo)
        {
            try
            {
                //string PureFileName = new FileInfo(pPathFile).Name;
                //string uploadUrl = $"{Host}/{pDir}{PureFileName}";
                FtpWebRequest req = (FtpWebRequest)FtpWebRequest.Create(pPathFileTo);
                req.Proxy = null;
                req.Method = WebRequestMethods.Ftp.UploadFile;
                req.Credentials = new NetworkCredential(User, PassWord);
                req.UseBinary = true;
                req.UsePassive = true;
                //byte[] data = File.ReadAllBytes(pPathFile);
                req.ContentLength = pData.Length;
                Stream stream = req.GetRequestStream();
                stream.Write(pData, 0, pData.Length);
                stream.Close();
                FtpWebResponse res = (FtpWebResponse)req.GetResponse();
                //return res.StatusDescription;
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage($"FTP.UpLoad=>{e.Message}/nStackTrace=> {e.StackTrace}", eTypeLog.Error);
                return false;
            }
            //  fs.Flush();
            return true;
        }

        public bool DownLoad(string pPathFileFrom, string pDirTo)
        {
            string PureFileName = new FileInfo(pPathFileFrom).Name;
            
            try
            {
                //Create FTP Request.
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"{Host}/{pPathFileFrom}");
                request.Method = WebRequestMethods.Ftp.DownloadFile;

                //Enter FTP Server credentials.
                request.Credentials = new NetworkCredential(User, PassWord);
                request.UsePassive = true;
                request.UseBinary = true;
                request.EnableSsl = false;

                //Fetch the Response and read it into a MemoryStream object.
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    string file = Path.Combine(pDirTo, PureFileName);
                    if(File.Exists(file))
                        File.Delete(file);
                    using (Stream fileStream = new FileStream(Path.Combine(pDirTo, PureFileName), FileMode.Create)) 
                    {
                        responseStream.CopyTo(fileStream);
                    }
                }
            }
            catch (WebException e)
            {
                FileLogger.WriteLogMessage($"FTP.DownLoad=>{e.Message}/nStackTrace=> {e.StackTrace}", eTypeLog.Error);
                //throw new Exception((ex.Response as FtpWebResponse).StatusDescription);
                return false;
            }

            
            return true;
        }
    }


}
