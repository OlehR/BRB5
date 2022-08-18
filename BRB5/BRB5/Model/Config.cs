using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xamarin.Essentials;

namespace BRB5.Model
{
    public class Config
    {
        public static Action<double> OnProgress;
        //static DB db = DB.GetDB();
        public static int CodeWarehouse;
        static eCompany _Company = eCompany.NotDefined;
        public static eCompany Company
        {
            get {
                if (_Company == eCompany.NotDefined)
                {
                    DB db = DB.GetDB();
                    _Company = db.GetConfig<eCompany>("Company");
                }
                return  _Company; }
            set { _Company = value; }
        }
        public static string PathDownloads = null;

        static string _ApiUrl1 = "";
        public static string ApiUrl1
        {
            get
            {
                if (Company == eCompany.NotDefined)
                {
                    _ApiUrl1 = "";
                }
                if (Company == eCompany.Sim23)
                {
                    _ApiUrl1 = "http://93.183.216.37:80/dev1/hs/TSD/";
                }
                if (Company == eCompany.Sim23FTP)
                {
                    _ApiUrl1 = "";
                }
                if (Company == eCompany.VPSU|| Company == eCompany.SparPSU)
                {
                    _ApiUrl1 = "http://api.spar.uz.ua/znp/";
                }
                return _ApiUrl1;
            }
            set { _ApiUrl1 = value; }
        }

        static string _ApiUrl2 = "";
        public static string ApiUrl2
        {
            get
            {
                if (Company == eCompany.NotDefined)
                {
                    _ApiUrl2 = "";
                }
                if (Company == eCompany.Sim23)
                {
                    _ApiUrl2 = "http://93.183.216.37/TK/hs/TSD/;http://37.53.84.148/TK/hs/TSD/";
                }
                if (Company == eCompany.Sim23FTP)
                {
                    _ApiUrl2 = "";
                }
                if (Company == eCompany.VPSU || Company == eCompany.SparPSU)
                {
                    _ApiUrl2 = "http://api.spar.uz.ua/print/";
                }
                return _ApiUrl2;
            }
            set { _ApiUrl2 = value; }
        }

        static string _ApiUrl3 = "";
        public static string ApiUrl3
        {
            get
            {
                if (Company == eCompany.NotDefined)
                {
                    _ApiUrl3 = "";
                }
                if (Company == eCompany.Sim23)
                {
                    _ApiUrl3 = "https://bitrix.sim23.ua/rest/233/ax02yr7l9hia35vj/";
                }
                if (Company == eCompany.Sim23FTP)
                {
                    _ApiUrl3 = "";
                }
                if (Company == eCompany.VPSU || Company == eCompany.SparPSU)
                {
                    _ApiUrl3 = "";
                }
                return _ApiUrl3;
            }
            set { _ApiUrl3 = value; }
        }

        public static string SN;
        public static int Ver;
        public static bool IsAutoLogin { get; set; } = false;
        public static bool IsVibration { get; set; } = false;
        public static bool IsSound { get; set; } = false;
        public static bool IsTest { get; set; } = true;
        public static bool IsLoginCO = true;
        public static eLoginServer LoginServer = eLoginServer.Bitrix;
        public static string Login { get; set; } = "LOX";
        public static string Password { get; set; } = "321";
        public static eRole Role = eRole.NotDefined;
        public static int CodeUser { get; set; } = 233;
        public static string NameUser { get; set; }
        public static eTypeUsePrinter TypeUsePrinter { get; set; } = eTypeUsePrinter.NotDefined;
        
        public static int GetCodeUnitWeight { get { return Company == eCompany.Sim23 || Company == eCompany.Sim23FTP ? 166 : 7; } }

        public static int GetCodeUnitPiece { get { return Company == eCompany.Sim23 || Company == eCompany.Sim23FTP ? 796 : 19; } }

        public static string PathFiles { get { string res=@"D:\temp"; try { res = Path.Combine(FileSystem.AppDataDirectory,"BRBFiles"); } catch (Exception e) { } return res; } }

        public static DocSetting GetDocSetting(int pTypeDoc) { return null; }
        
        
        // public static string GenRaitingFileName(Raiting r);
    }
}
