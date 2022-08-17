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

        public int CodeWarehouse { get { return ListWarehouse.FindIndex(x => x.Code == Config.CodeWarehouse) ; } }

        public int SelectedCompany { get { return ListCompany.FindIndex(x => x == Enum.GetName(typeof(eCompany),Config.Company)); } }
        public int SelectedTypePrinter { get { return Enum.GetNames(typeof(eTypeUsePrinter)).ToList().FindIndex(x => x == Enum.GetName(typeof(eTypeUsePrinter), Config.TypeUsePrinter)); } }
        public int SelectedTypeLog { get { return ListTypeLog.FindIndex(x => x == Enum.GetName(typeof(eTypeLog), FileLogger.TypeLog)); } }

        public bool IsAutoLogin { get { return Config.IsAutoLogin; } }
        public bool IsVibration { get { return Config.IsVibration; } }
        public bool IsSound { get { return Config.IsSound; } }
        public bool IsTest { get { return Config.IsTest; } }

        public string ApiUrl1 { get { return Config.ApiUrl1; } }
        public string ApiUrl2 { get { return Config.ApiUrl2; } }
        public string ApiUrl3 { get { return Config.ApiUrl3; } }

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
    }
}