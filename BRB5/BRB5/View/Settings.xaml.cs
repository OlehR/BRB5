using BL;
using BL.Connector;
using BRB5.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Utils;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace BRB5.View
{
    public partial class Settings
    {
        private Connector c;
        DB db = DB.GetDB();
        BL.BL Bl = BL.BL.GetBL();
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
        public List<string> ListPhotoQuality
        {
            get
            {
                List<string> list = new List<string>();
                foreach (ePhotoQuality type in Enum.GetValues(typeof(ePhotoQuality)))
                {
                    list.Add(EnumMethods.GetDescription(type));
                }
                return list;
            }
        }

        public List<string> ListCompany { get { return Enum.GetNames(typeof(eCompany)).Where(el => !el.Equals(eCompany.Sim23FTP.ToString())&& !el.Equals(eCompany.VPSU.ToString())).ToList(); } }
        public List<string> ListTypeLog { get { return Enum.GetNames(typeof(eTypeLog)).ToList(); } }
        public List<string> ListReplenishment { get { return Enum.GetNames(typeof(eTypeLog)).ToList(); } }

        public List<Warehouse> ListWarehouse
        {
            get
            {
                List<Warehouse> wh = null;
                try
                {
                    wh = db.GetWarehouse()?.OrderBy(q => q.Name).ToList();

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

        public int SelectedCompany { get { return ListCompany.FindIndex(x => x == Enum.GetName(typeof(eCompany),Config.Company)); } set { Config.Company = (eCompany)value; OnPropertyChanged(nameof(IsVisApi3)); } }
        public int SelectedTypePrinter { get { return Enum.GetNames(typeof(eTypeUsePrinter)).ToList().FindIndex(x => x == Enum.GetName(typeof(eTypeUsePrinter), Config.TypeUsePrinter)); } set { Config.TypeUsePrinter = (eTypeUsePrinter)value; } }
        public int SelectedTypeLog { get { return ListTypeLog.FindIndex(x => x == Enum.GetName(typeof(eTypeLog), FileLogger.TypeLog)); } set { FileLogger.TypeLog = (eTypeLog)value; } }
        public int SelectedPhotoQuality { get { return Enum.GetNames(typeof(ePhotoQuality)).ToList().FindIndex(x => x == Enum.GetName(typeof(ePhotoQuality), Config.PhotoQuality)); } set { Config.PhotoQuality = (ePhotoQuality)value; } }

        public bool IsSoftKeyboard { get { return Config.IsSoftKeyboard; } }
        public bool IsVisApi3 { get { return Config.Company == eCompany.Sim23; } }
        public bool IsViewAllWH { get { return Config.IsViewAllWH; } set { Config.IsViewAllWH = value; } }
        public bool IsAutoLogin { get { return Config.IsAutoLogin; } set { Config.IsAutoLogin = value; } }
        public bool IsVibration { get { return Config.IsVibration; } set { Config.IsVibration = value; } }
        public bool IsSound { get { return Config.IsSound; } set { Config.IsSound = value; } }
        public bool IsTest { get { return Config.IsTest; } set { Config.IsTest = value; } }
        public bool IsFilterSave { get { return Config.IsFilterSave; } set { Config.IsFilterSave = value; } }     
        public bool IsFullScreenScan { get { return Config.IsFullScreenScan; } set { Config.IsFullScreenScan = value; } }

        public string ApiUrl1 { get { return Config.ApiUrl1; } set { Config.ApiUrl1 = value; OnPropertyChanged(nameof(ApiUrl1)); } }
        public string ApiUrl2 { get { return Config.ApiUrl2; } set { Config.ApiUrl2 = value; OnPropertyChanged(nameof(ApiUrl2)); } }
        public string ApiUrl3 { get { return Config.ApiUrl3; } set { Config.ApiUrl3 = value; OnPropertyChanged(nameof(ApiUrl3)); } }
        public int Compress { get { return Config.Compress; } set { Config.Compress = value; OnPropertyChanged(nameof(Compress)); } }
        public ObservableCollection<Warehouse> Warehouses { get; set; }

        public Settings()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, Device.RuntimePlatform == Device.iOS);

            c = Connector.GetInstance();

            Warehouses = new ObservableCollection<Warehouse>(ListWarehouse);
            if (Config.CodesWarehouses != null) {
                foreach (int i in Config.CodesWarehouses) {
                    var temp = Warehouses.FirstOrDefault(x => x.CodeWarehouse == i);
                    if (temp != null) temp.IsChecked = true;
                } }
            LWH.ItemTapped += (object sender, ItemTappedEventArgs e) => {
                if (e.Item == null) return;
                var temp = e.Item as Warehouse;
                temp.IsChecked = !temp.IsChecked;
                ((ListView)sender).SelectedItem = null;
            };
            if(Config.Company==eCompany.NotDefined) CurrentPage = Children[1];
            this.BindingContext = this;
        }

        private void OnClickLoad(object sender, EventArgs e)
        {
            c.LoadGuidDataAsync(true);
            Config.DateLastLoadGuid = DateTime.Now;
            db.SetConfig<DateTime>("DateLastLoadGuid", Config.DateLastLoadGuid);
        }

        private void OnClickLoadDoc(object sender, EventArgs e) { c.LoadDocsDataAsync(0, null, false); }

        private void OnCopyDB(object sender, EventArgs e) {  }

        private void OnRestoreDB(object sender, EventArgs e) {   }

        private void OnClickGen(object sender, EventArgs e)
        {
            var temp = Bl.GenApiUrl();
            ApiUrl1 = temp[0];
            ApiUrl2 = temp[1];
            ApiUrl3 = temp[2];
        }

        private void OnClickIP(object sender, EventArgs e)  {   }

        private void OnClickSave(object sender, EventArgs e)
        {
            Bl.SaveSettings(IsAutoLogin, IsVibration, IsViewAllWH, IsSound, IsTest, IsFilterSave, IsFullScreenScan, ApiUrl1, ApiUrl2, ApiUrl3, Compress, 
                (eCompany)SelectedCompany, (eTypeLog)SelectedTypeLog, (ePhotoQuality)SelectedPhotoQuality, (eTypeUsePrinter)SelectedTypePrinter, 
                SelectedWarehouse, ListWarehouse[SelectedWarehouse].Code, Warehouses);
        }
        private void RefreshWarehouses(object sender, CheckedChangedEventArgs e)
        {
            var temp = sender as CheckBox;
            Bl.RefreshWarehouses(temp.AutomationId, temp.IsChecked);
        }
    }
}