using System;
using System.Collections.Generic;
using System.IO;
using BRB5.Connector;
using BRB5.Model;
using System.Linq;
using Utils;

namespace BRB5
{
    public class Utils
    {
        static Utils utils = null;
        public static Utils GetUtils()
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
                        ver = int.Parse(Ver);
                    }
                    catch (Exception)
                    {
                    }
                    if (ver > pVersionCode)
                    {
                        string FileName = Path.Combine(Config.PathDownloads, pNameAPK);
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


        public (long,long) DelDir(string pDir, IEnumerable<string> pNotDelDir)
        {
            FileLogger.WriteLogMessage($"DelDir Start =>{pDir}");
            if (pNotDelDir == null || !pNotDelDir.Any())
                return(0,0);            
            long SizeDel = 0,SizeUse=0;
            var dirs = Directory.GetDirectories(Config.PathFiles, "*", SearchOption.TopDirectoryOnly);
            foreach (var el in dirs)
            {
                var dir = new DirectoryInfo(el);
                var LD= pNotDelDir.Where(e => el.EndsWith(e));
                if (LD.Count() > 0)
                {
                    SizeUse += DirSize(dir);
                    FileLogger.WriteLogMessage($"DelDir Skip =>{dir.FullName}");
                }
                else
                {
                    SizeDel += DirSize(dir);
                    FileLogger.WriteLogMessage($"DelDir=>{dir.FullName}");
                    dir.Delete(true);
                }
            }
            FileLogger.WriteLogMessage($"DelDir End SizeDel=>{SizeDel}, SizeUse=>{SizeUse}");
            return (SizeDel , SizeUse);

        }

        public long DirSize(DirectoryInfo d)
        {
            long size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += DirSize(di);
            }
            return size;
        }

        public  double GetFreeSpace(string pPath)
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            var r=allDrives.Where(el => pPath.StartsWith(el.Name)).OrderByDescending(el => el.Name.Length);
            if (r != null && r.Count() > 0)
                return r.FirstOrDefault().AvailableFreeSpace;
          return 20d*1024d*1024d;
        }
    }
}
