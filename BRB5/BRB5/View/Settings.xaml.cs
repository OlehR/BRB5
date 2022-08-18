using BRB5.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utils;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BRB5.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Settings : TabbedPage, INotifyPropertyChanged
    {

        DB db = DB.GetDB();
        public string Ver { get { return "Ver:"+ Assembly.GetExecutingAssembly().GetName().Version; } }

        public string SN { get { return "SN:"; } }

        //public List TypeUsePrinter { get { return Enum.GetValues(typeof(eTypeUsePrinter)).Cast<eTypeUsePrinter>(); } }
        //private eTypeUsePrinter _TypeUsePrinter = eTypeUsePrinter.NotDefined;
        //public eTypeUsePrinter TypeUsePrinter { get { return _TypeUsePrinter; } set { _TypeUsePrinter = value; OnPropertyChanged("TypeUsePrinter"); } }

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

        public List<Warehouse> ListWarehouse { get { return db.GetWarehouse().ToList(); } }        

        public int SelectedWarehouse { get { return ListWarehouse.FindIndex(x => x.Code == Config.CodeWarehouse) ; } set { Config.CodeWarehouse = ListWarehouse[value].Code; } }

        public int SelectedCompany { get { return ListCompany.FindIndex(x => x == Enum.GetName(typeof(eCompany),Config.Company)); } set { Config.Company = (eCompany)value; } }
        public int SelectedTypePrinter { get { return Enum.GetNames(typeof(eTypeUsePrinter)).ToList().FindIndex(x => x == Enum.GetName(typeof(eTypeUsePrinter), Config.TypeUsePrinter)); } set { Config.TypeUsePrinter = (eTypeUsePrinter)value; } }
        public int SelectedTypeLog { get { return ListTypeLog.FindIndex(x => x == Enum.GetName(typeof(eTypeLog), FileLogger.TypeLog)); } set { FileLogger.TypeLog = (eTypeLog)value; } }

        public bool IsAutoLogin { get { return Config.IsAutoLogin; } set { Config.IsAutoLogin = value; } }
        public bool IsVibration { get { return Config.IsVibration; } set { Config.IsVibration = value; } }
        public bool IsSound { get { return Config.IsSound; } set { Config.IsSound = value; } }
        public bool IsTest { get { return Config.IsTest; } set { Config.IsTest = value; } }

        public string ApiUrl1 { get { return Config.ApiUrl1; } set { Config.ApiUrl1 = value; } }
        public string ApiUrl2 { get { return Config.ApiUrl2; } set { Config.ApiUrl2 = value; } }
        public string ApiUrl3 { get { return Config.ApiUrl3; } set { Config.ApiUrl3 = value; } }

        public Settings()
        {
            InitializeComponent();

            

            this.BindingContext = this;
        }

        private void OnClickLoad(object sender, EventArgs e)
        {

        }

        private void OnClickLoadDoc(object sender, EventArgs e)
        {

        }

        private void OnCopyDB(object sender, EventArgs e)
        {

        }

        private void OnRestoreDB(object sender, EventArgs e)
        {

        }

        private void OnClickGen(object sender, EventArgs e)
        {

        }

        private void OnClickIP(object sender, EventArgs e)
        {

        }

        private void OnClickSave(object sender, EventArgs e)
        {

            db.SetConfig<bool>("IsAutoLogin", IsAutoLogin);
            db.SetConfig<bool>("IsVibration", IsVibration);
            db.SetConfig<bool>("IsSound", Config.IsSound);
            db.SetConfig<bool>("IsTest", IsTest);

            db.SetConfig<string>("ApiUrl1", ApiUrl1);
            db.SetConfig<string>("ApiUrl2", ApiUrl2);
            db.SetConfig<string>("ApiUrl3", ApiUrl3);

            db.SetConfig<eCompany>("Company", (eCompany)SelectedCompany);
            db.SetConfig<eTypeLog>("TypeLog", (eTypeLog)SelectedTypeLog);
            db.SetConfig<eTypeUsePrinter>("TypeUsePrinter", (eTypeUsePrinter)SelectedTypePrinter);

            db.SetConfig<int>("CodeWarehouse", ListWarehouse[SelectedWarehouse].Code);
        }
    }
}