using BRB5.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils;
using Xamarin.Essentials;

namespace BRB5.View
{
    public partial class Settings
    {
        private Connector.Connector c;
        DB db = DB.GetDB();
        //public string Ver { get { return "Ver:"+ Assembly.GetExecutingAssembly().GetName().Version; } }
        public string Ver { get { return "Ver:" + AppInfo.VersionString; } }
        public string SN { get { return "SN:"+ Config.SN; } }

        public List<string> ListTypeUsePrinter
        {
            get
            {
                List<string> list = new List<string>();
                foreach (eTypeUsePrinter type in Enum.GetValues(typeof(eTypeUsePrinter)))
                {
                    list.Add(EnumMethods.GetDescription(type));
                }
                return list;
            }
        }

        public List<string> ListCompany { get { return Enum.GetNames(typeof(eCompany)).ToList(); } }
        public List<string> ListTypeLog { get { return Enum.GetNames(typeof(eTypeLog)).ToList(); } }

        public List<Warehouse> ListWarehouse
        {
            get
            {
                List<Warehouse> wh = null;
                try
                {
                    wh = db.GetWarehouse()?.ToList();

                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
                if (wh == null || !wh.Any())
                    wh = new List<Warehouse>() { new Warehouse() { Code = 0, Name = "ddd" } };
                return wh;

            }
        }        

        public int SelectedWarehouse { get { return ListWarehouse?.FindIndex(x => x.Code == Config.CodeWarehouse)??0 ; } set { Config.CodeWarehouse = ListWarehouse[value].Code; } }

        public int SelectedCompany { get { return ListCompany.FindIndex(x => x == Enum.GetName(typeof(eCompany),Config.Company)); } set { Config.Company = (eCompany)value; OnPropertyChanged("IsVisApi3"); } }
        public int SelectedTypePrinter { get { return Enum.GetNames(typeof(eTypeUsePrinter)).ToList().FindIndex(x => x == Enum.GetName(typeof(eTypeUsePrinter), Config.TypeUsePrinter)); } set { Config.TypeUsePrinter = (eTypeUsePrinter)value; } }
        public int SelectedTypeLog { get { return ListTypeLog.FindIndex(x => x == Enum.GetName(typeof(eTypeLog), FileLogger.TypeLog)); } set { FileLogger.TypeLog = (eTypeLog)value; } }

        public bool IsVisApi3 { get { return Config.Company == eCompany.Sim23; } } 
        public bool IsAutoLogin { get { return Config.IsAutoLogin; } set { Config.IsAutoLogin = value; } }
        public bool IsVibration { get { return Config.IsVibration; } set { Config.IsVibration = value; } }
        public bool IsSound { get { return Config.IsSound; } set { Config.IsSound = value; } }
        public bool IsTest { get { return Config.IsTest; } set { Config.IsTest = value; } }

        public string ApiUrl1 { get { return Config.ApiUrl1; } set { Config.ApiUrl1 = value; OnPropertyChanged("ApiUrl1"); } }
        public string ApiUrl2 { get { return Config.ApiUrl2; } set { Config.ApiUrl2 = value; OnPropertyChanged("ApiUrl2"); } }
        public string ApiUrl3 { get { return Config.ApiUrl3; } set { Config.ApiUrl3 = value; OnPropertyChanged("ApiUrl3"); } }

        public Settings()
        {
            InitializeComponent();


            c = Connector.Connector.GetInstance();

            this.BindingContext = this;
        }

        private void OnClickLoad(object sender, EventArgs e)
        {
            c.LoadGuidData(true);
            Config.DateLastLoadGuid = DateTime.Now;
            db.SetConfig<DateTime>("DateLastLoadGuid", Config.DateLastLoadGuid);
        }

        private void OnClickLoadDoc(object sender, EventArgs e)
        {

            c.LoadDocsData(0, null, false);
        }

        private void OnCopyDB(object sender, EventArgs e)
        {

        }

        private void OnRestoreDB(object sender, EventArgs e)
        {

        }

        private void OnClickGen(object sender, EventArgs e)
        {
            switch (Config.Company)
            {
                case eCompany.NotDefined:
                    ApiUrl1 = "";
                    ApiUrl2 = "";
                    ApiUrl3 = "";
                    break;
                case eCompany.Sim23:
                    
                    ApiUrl1 = "http://93.183.216.37:80/dev1/hs/TSD/";//
                    ApiUrl2 = "http://37.53.84.148/TK/hs/TSD/";// "http://93.183.216.37/TK/hs/TSD/;http://37.53.84.148/TK/hs/TSD/";
                    ApiUrl3 = "https://bitrix.sim23.ua/rest/233/ax02yr7l9hia35vj/";
                    break;
                case eCompany.Sim23FTP:
                    ApiUrl1 = "";
                    ApiUrl2 = "";
                    ApiUrl3 = "";
                    break;
                case eCompany.VPSU:
                case eCompany.SparPSU:
                    ApiUrl1 = "http://api.spar.uz.ua/znp/";
                    ApiUrl2 = "http://api.spar.uz.ua/print/";
                    ApiUrl3 = "";
                    break;
            }            
        }

        private void OnClickIP(object sender, EventArgs e)
        {

        }

        private void OnClickSave(object sender, EventArgs e)
        {

            db.SetConfig<bool>("IsAutoLogin", IsAutoLogin);
            db.SetConfig<bool>("IsVibration", IsVibration);
            db.SetConfig<bool>("IsSound", IsSound);
            db.SetConfig<bool>("IsTest", IsTest);

            db.SetConfig<string>("ApiUrl1", ApiUrl1 ?? "");
            db.SetConfig<string>("ApiUrl2", ApiUrl2 ?? "");
            db.SetConfig<string>("ApiUrl3", ApiUrl3 ?? "");

            db.SetConfig<eCompany>("Company", (eCompany)SelectedCompany);
            db.SetConfig<eTypeLog>("TypeLog", (eTypeLog)SelectedTypeLog);
            db.SetConfig<eTypeUsePrinter>("TypeUsePrinter", (eTypeUsePrinter)SelectedTypePrinter);
            if(SelectedWarehouse>-1) db.SetConfig<int>("CodeWarehouse", ListWarehouse[SelectedWarehouse].Code);
        }
    }
}