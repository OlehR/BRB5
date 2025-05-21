using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace BRB5.Model
{
    public class Config
    {
        public static Action<string> BarCode;
        public static Action<double> OnProgress;
        public static string Manufacturer = "";
        public static string Model = "";     
        public static int CodeWarehouse;
        public static string NameCompany="?";
        public static string PathAPK = "https://dct.spar.uz.ua/apk/ua.UniCS.TM.BRB6.apk"; //"https://media.githubusercontent.com/media/OlehR/BRB5/refs/heads/master/Apk/ua.UniCS.TM.BRB6.apk";
        public static List<int> CodesWarehouses { get; set; }
        //static eCompany _Company = eCompany.NotDefined;
        public static eTypeScaner TypeScaner;
        public static eCompany Company { get; set; } = eCompany.NotDefined;
        
        public static string PathDownloads = null;

        public static string ApiUrl1 { get; set; }

        public static string ApiUrl2 { get; set; }

        public static string ApiUrl3 { get; set; }
        public static string ApiUrl4 { get; set; }

        public static string SN;
        public static DateTime DateLastLoadGuid { get; set; }
        public static int Ver { get; set; }
        public static bool IsViewAllWH { get; set; } = false;
        public static bool IsAutoLogin { get; set; } = false;
        public static bool IsVibration { get; set; } = false;
        public static bool IsSound { get; set; } = false;
        public static bool IsTest { get; set; } = true;
        public static bool IsFilterSave { get; set; } = false;
        //public static bool IsFullScreenScan { get; set; } = false;

        public static bool IsLoginCO = true;
        public static eLoginServer LoginServer = eLoginServer.Bitrix;
        public static string Login { get; set; } = "LOX";
        public static string Password { get; set; } = "321";
        public static eRole Role = eRole.NotDefined;
        public static int CodeUser { get; set; } = 233;
        public static string NameUser { get; set; }
        public static eTypeUsePrinter TypeUsePrinter { get; set; } = eTypeUsePrinter.NotDefined;
        
        public static int GetCodeUnitWeight { get { return Company == eCompany.Sim23 /*|| Company == eCompany.Sim23FTP*/ ? 166 : 7; } }

        public static int GetCodeUnitPiece { get { return Company == eCompany.Sim23 /*|| Company == eCompany.Sim23FTP */? 796 : 19; } }

        public static string PathFiles { get { string res=@"D:\temp";
                try {
                    res = Path.Combine(Config.PathDownloads,"BRBFiles"); 
                    if(!Directory.Exists(res))
                        Directory.CreateDirectory(res);
                } catch (Exception e)
                { 
                } return res; } }

        public static IEnumerable<TypeDoc> TypeDoc;
        public static TypeDoc GetDocSetting(int pTypeDoc) => TypeDoc.FirstOrDefault(x => x.CodeDoc == pTypeDoc);

        //static object Lock= new object();
        public static bool IsSoftKeyboard { get { return TypeScaner == eTypeScaner.Camera; } }

        public static NativeBase NativeBase;
        public static ePhotoQuality PhotoQuality { get;set; }

        public static int Compress { get; set; } = 90;

    }
}
