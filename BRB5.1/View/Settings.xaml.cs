using BL;
using BL.Connector;
using BRB5.Model;
using System.Collections.ObjectModel;
using Utils;
using BRB5;
using BarcodeScanning;

namespace BRB6.View
{
    public partial class Settings : TabbedPage
    {
        private Connector c;
        DB db = DB.GetDB();
        BL.BL Bl = BL.BL.GetBL();
        //public string Ver { get { return "Ver:"+ Assembly.GetExecutingAssembly().GetName().Version; } }
        public string Ver { get { return "Ver:" + AppInfo.VersionString; } }
        public string SN { get { return "SN:"+ Config.SN; } }
        public bool IsVisScan { get { return Config.TypeScaner == eTypeScaner.Camera; } }

        double _PB = 0.5;
        public double PB { get { return _PB; } set { _PB = value; OnPropertyChanged(nameof(PB)); } }
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

        public List<string> ListCompany { get { return Enum.GetNames(typeof(eCompany))/*.Where(el => !el.Equals(eCompany.Sim23FTP.ToString())&& !el.Equals(eCompany.VPSU.ToString()))*/.ToList(); } }
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

        public int SelectedWarehouse { get { return ListWarehouse?.FindIndex(x => x.Code == Config.CodeWarehouse)??0 ; } 
            set { Config.CodeWarehouse = ListWarehouse[value].Code; OnPropertyChanged(nameof(SelectedWarehouse)); } }

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
        //public bool IsFullScreenScan { get { return Config.IsFullScreenScan; } set { Config.IsFullScreenScan = value; } }

        public string ApiUrl1 { get { return Config.ApiUrl1; } set { Config.ApiUrl1 = value; OnPropertyChanged(nameof(ApiUrl1)); } }
        public string ApiUrl2 { get { return Config.ApiUrl2; } set { Config.ApiUrl2 = value; OnPropertyChanged(nameof(ApiUrl2)); } }
        public string ApiUrl3 { get { return Config.ApiUrl3; } set { Config.ApiUrl3 = value; OnPropertyChanged(nameof(ApiUrl3)); } }
        public string ApiUrl4 { get { return Config.ApiUrl4; } set { Config.ApiUrl4 = value; OnPropertyChanged(nameof(ApiUrl4)); } }
        public int Compress { get { return Config.Compress; } set { Config.Compress = value; OnPropertyChanged(nameof(Compress)); } }
        public ObservableCollection<Warehouse> Warehouses { get; set; }
        public ObservableCollection<Warehouse> FilteredWarehouses { get; set; } = new ObservableCollection<Warehouse>();

        bool _IsFilterWHChecked = false;
        private bool IsFilterWHChecked { get { return _IsFilterWHChecked; } set { _IsFilterWHChecked = value;
                OnPropertyChanged(nameof(IsFilterWHChecked)); OnPropertyChanged(nameof(Warehouses));  } }
        private string TextFilterWH { get; set; }

        CameraView BarcodeScaner;
        bool _IsVisBarCode = false;
        public bool IsVisBarCode { get { return _IsVisBarCode; } set { _IsVisBarCode = value; OnPropertyChanged(nameof(IsVisBarCode)); } }
        public Settings()
        {
            InitializeComponent();
            // TODO Xamarin.Forms.Device.RuntimePlatform is no longer supported. Use Microsoft.Maui.Devices.DeviceInfo.Platform instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
            NavigationPage.SetHasNavigationBar(this, DeviceInfo.Platform == DevicePlatform.iOS);
            
            c = ConnectorBase.GetInstance();

            Warehouses = new ObservableCollection<Warehouse>(ListWarehouse);
            if (Config.CodesWarehouses != null)
            {
                foreach (int i in Config.CodesWarehouses)
                {
                    var temp = Warehouses.FirstOrDefault(x => x.CodeWarehouse == i);
                    if (temp != null) temp.IsChecked = true;
                }
            }

            FilteredWarehouses = new ObservableCollection<Warehouse>(Warehouses);

            FillFilterWarehouseList();

            //LWH.ItemTapped += (object sender, ItemTappedEventArgs e) => {
            //    if (e.Item == null) return;
            //    var temp = e.Item as Warehouse;
            //    temp.IsChecked = !temp.IsChecked;
            //    ((ListView)sender).SelectedItem = null;
            //};
            FillFilterWarehouseList();

            if (Config.Company==eCompany.NotDefined) CurrentPage = Children[1];
            this.BindingContext = this;
            Config.BarCode = BarCode;
        }

        async void BarCode(string pBarCode)
        {
            if (pBarCode == null) return;
            if (pBarCode.StartsWith("BRB6=>"))
            {
                var temp = pBarCode[6..].Split(';');
                foreach (var el in temp)
                {
                    var t = el.Split('=');
                    if (t.Length == 2)
                    {
                        switch (t[0])
                        {
                            case "ApiUrl1": ApiUrl1 = t[1]; break;
                            case "ApiUrl2": ApiUrl2 = t[1]; break;
                            case "ApiUrl3": ApiUrl3 = t[1]; break;
                            case "ApiUrl4": ApiUrl4 = t[1]; break;
                            case "Compress": Compress = t[1].ToInt(); break;
                            case "Company": SelectedCompany = ListCompany.FindIndex(x => x == t[1]); break;
                            case "TypePrinter": SelectedTypePrinter = Enum.GetNames(typeof(eTypeUsePrinter)).ToList().FindIndex(x => x == t[1]); break;
                            case "TypeLog": SelectedTypeLog = ListTypeLog.FindIndex(x => x == t[1]); break;
                            case "PhotoQuality": SelectedPhotoQuality = Enum.GetNames(typeof(ePhotoQuality)).ToList().FindIndex(x => x == t[1]); break;
                            case "Warehouse": SelectedWarehouse = ListWarehouse.FindIndex(x => x.Code == t[1].ToInt()); break;
                            case "IsViewAllWH": IsViewAllWH = t[1].Equals("true"); break;
                            case "IsAutoLogin": IsAutoLogin = t[1].Equals("true"); break;
                            case "IsVibration": IsVibration = t[1].Equals("true"); break;
                            case "IsSound": IsSound = t[1].Equals("true"); break;
                            case "IsTest": IsTest = t[1].Equals("true"); break;
                            case "IsFilterSave": IsFilterSave = t[1].Equals("true"); break;
                                //case "IsFullScreenScan": IsFullScreenScan = t[1].ToBool(); break;
                        }
                    }
                }
            }
            Config.CodeWarehouse=0;
            await c.LoadGuidDataAsync(true);
            await DisplayAlert("", "Параметри вступлять в силу після перезапуску", "Перезапуск");

            OnClickSave(null, null);
            Application.Current.Quit();
        }
        public void Dispose() { Config.BarCode -= BarCode; }


        void Progress(double pProgress) => MainThread.BeginInvokeOnMainThread(() => PB = pProgress);
        protected override void OnAppearing()
        {
            base.OnAppearing();
            Config.OnProgress += Progress;

            if (IsVisScan)
            {
                BarcodeScaner = new CameraView
                {
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    CameraEnabled = false,
                    VibrationOnDetected = false,
                    BarcodeSymbologies = BarcodeFormats.Ean13 | BarcodeFormats.Ean8 | BarcodeFormats.QRCode,

                };
                BarcodeScaner.OnDetectionFinished += CameraView_OnDetectionFinished;
                GridZxing.Children.Add(BarcodeScaner);
            }
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Config.OnProgress -= Progress;

            if (IsVisScan) BarcodeScaner.CameraEnabled = false;
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
            ApiUrl4 = temp[3];
        }

        private async void OnClickIP(object sender, EventArgs e)
        {
            string currentIP = Config.NativeBase.GetIP();
            if (currentIP == null)
            {
                await DisplayAlert("Помилка", "Не вдається отримати IP-адресу.", "OK");
                return;
            }
            if (ListWarehouse.Any() && ListWarehouse.First().Name != "ddd") {
                var matchingWarehouseIndex = ListWarehouse.FindIndex(warehouse =>
                    warehouse.InternalIP.Split('.').Take(3).SequenceEqual(currentIP.Split('.').Take(3)));

                if (matchingWarehouseIndex != -1)
                {
                    SelectedWarehouse = matchingWarehouseIndex;
                    ApiUrl2 = ListWarehouse[matchingWarehouseIndex].Url;
                }
                else
                {
                    await DisplayAlert("Info", "Відповідний магазин не знайдено.", "OK");
                } 
            }
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            Bl.SaveSettings(IsAutoLogin, IsVibration, IsViewAllWH, IsSound, IsTest, IsFilterSave, /*IsFullScreenScan,*/ ApiUrl1, ApiUrl2, ApiUrl3, ApiUrl4, Compress, 
                (eCompany)SelectedCompany, (eTypeLog)SelectedTypeLog, (ePhotoQuality)SelectedPhotoQuality, (eTypeUsePrinter)SelectedTypePrinter, 
                SelectedWarehouse, SelectedWarehouse !=-1? ListWarehouse[SelectedWarehouse].Code:-2, Warehouses);
        }
        private void RefreshWarehouses(object sender, CheckedChangedEventArgs e)
        {
            var temp = sender as CheckBox;
            Bl.RefreshWarehouses(temp.AutomationId, temp.IsChecked);
        }
        private void FillFilterWarehouseList()
        {
            FilterWarehouseList.Children.Clear();
            foreach (var warehouse in Warehouses)
            {
                var stackLayout = new StackLayout { Orientation = StackOrientation.Horizontal };
                var checkBox = new CheckBox();
                checkBox.SetBinding(CheckBox.IsCheckedProperty, new Binding("IsChecked", source: warehouse));
                checkBox.AutomationId = warehouse.CodeWarehouse.ToString();
                checkBox.CheckedChanged += RefreshWarehouses;
                var label = new Label { Text = warehouse.Name, FontSize = 20, TextColor = Colors.Black };

                stackLayout.Children.Add(checkBox);
                stackLayout.Children.Add(label);

                var tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Tapped += (s, e) =>
                {
                    warehouse.IsChecked = !warehouse.IsChecked;
                    Bl.RefreshWarehouses(warehouse.CodeWarehouse.ToString(), warehouse.IsChecked);
                };
                stackLayout.GestureRecognizers.Add(tapGestureRecognizer);

                FilterWarehouseList.Children.Add(stackLayout);
            }
        }
        private void FilterWH(object sender, TextChangedEventArgs e)
        {
            string filterText = e.NewTextValue?.ToLower() ?? string.Empty;
            var filteredWarehouses = Warehouses
                .Where(wh => wh.Name.ToLower().Contains(filterText))
                .ToList();

            FilteredWarehouses.Clear();
            foreach (var warehouse in filteredWarehouses)
            {
                FilteredWarehouses.Add(warehouse);
            }

            FilterWarehouseList.Children.Clear();
            foreach (var warehouse in FilteredWarehouses)
            {
                var stackLayout = new StackLayout { Orientation = StackOrientation.Horizontal };
                var checkBox = new CheckBox();
                checkBox.SetBinding(CheckBox.IsCheckedProperty, new Binding("IsChecked", source: warehouse));
                checkBox.AutomationId = warehouse.CodeWarehouse.ToString();
                checkBox.CheckedChanged += RefreshWarehouses;
                var label = new Label { Text = warehouse.Name, FontSize = 20, TextColor = Colors.Black };

                stackLayout.Children.Add(checkBox);
                stackLayout.Children.Add(label);

                var tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Tapped += (s, e) =>
                {
                    warehouse.IsChecked = !warehouse.IsChecked;
                    Bl.RefreshWarehouses(warehouse.CodeWarehouse.ToString(), warehouse.IsChecked);
                };
                stackLayout.GestureRecognizers.Add(tapGestureRecognizer);

                FilterWarehouseList.Children.Add(stackLayout);
            }
        }

        private void CameraView_OnDetectionFinished(object sender, BarcodeScanning.OnDetectionFinishedEventArg e)
        {
            if (e.BarcodeResults.Length > 0)
            {
                BarcodeScaner.PauseScanning = true;
                BarCode(e.BarcodeResults[0].DisplayValue);
                Task.Run(async () => {
                    await Task.Delay(1000);
                    BarcodeScaner.PauseScanning = false;
                });
            }
        }
        private void CheckFilterWH(object sender, CheckedChangedEventArgs e)
        {
            bool isChecked = e.Value;
            foreach (var warehouse in FilteredWarehouses)
            {
                warehouse.IsChecked = isChecked;
                Bl.RefreshWarehouses(warehouse.CodeWarehouse.ToString(), warehouse.IsChecked);
            }
        }

        private async void QRCodeScan(object sender, EventArgs e)
        {
            if (Config.TypeScaner == eTypeScaner.Camera)
            {
                var cameraStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
                if (cameraStatus != PermissionStatus.Granted)
                    cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();

                if (cameraStatus != PermissionStatus.Granted)
                {
                    await DisplayAlert("Помилка", "Потрібен дозвіл камери", "OK", FlowDirection.MatchParent);
                    return;
                }
            }

            IsVisBarCode = !IsVisBarCode;
            BarcodeScaner.CameraEnabled = IsVisBarCode;
        }
    }
}