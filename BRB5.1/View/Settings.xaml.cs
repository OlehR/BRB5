using BL;
using BL.Connector;
using BRB5.Model;
using System.Collections.ObjectModel;
using Utils;
using BRB5;
using BarcodeScanning;
using CommunityToolkit.Maui.Alerts;


namespace BRB6.View
{
    public partial class Settings : TabbedPage
    {
        private Connector c;
        DB db = DB.GetDB();
        BL.BL Bl = BL.BL.GetBL();
        //public string Ver { get { return "Ver:"+ Assembly.GetExecutingAssembly().GetName().Version; } }
        public string Ver => "Ver:" + AppInfo.VersionString;
        public string SN => "SN:" + Config.SN;
        public bool IsVisScan => Config.TypeScaner == eTypeScaner.Camera;

        double _PB = 0.0;
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
        public List<string> ListCompany => Enum.GetNames(typeof(eCompany))/*.Where(el => !el.Equals(eCompany.Sim23FTP.ToString())&& !el.Equals(eCompany.VPSU.ToString()))*/.ToList(); 
        public List<string> ListTypeLog => Enum.GetNames(typeof(eTypeLog)).ToList(); 
        public List<string> ListReplenishment => Enum.GetNames(typeof(eTypeLog)).ToList(); 

        List<Warehouse> wh = null;
        public List<Warehouse> ListWarehouse
        {
            get
            {
                if (wh == null)
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

        public bool IsSoftKeyboard => Config.IsSoftKeyboard; 
        public bool IsVisApi3 => Config.Company == eCompany.Sim23;
        public bool IsViewAllWH => Config.IsViewAllWH;
        public bool IsAutoLogin => Config.IsAutoLogin;
        public bool IsVibration => Config.IsVibration;
        public bool IsSound => Config.IsSound;
        public bool IsTest => Config.IsTest;
        public bool IsFilterSave => Config.IsFilterSave;     
        //public bool IsFullScreenScan { get { return Config.IsFullScreenScan; } set { Config.IsFullScreenScan = value; } }

        public string ApiUrl1 { get { return Config.ApiUrl1; } set { Config.ApiUrl1 = value; OnPropertyChanged(nameof(ApiUrl1)); } }
        public string ApiUrl2 => Config.ApiUrl2; 
        public string ApiUrl3 => Config.ApiUrl3;
        public string ApiUrl4 => Config.ApiUrl4; 
        public int Compress => Config.Compress;
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
            int CodeWarehouse = 0;
            QRCodeScan(null,null);
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
                            case "ApiUrl1": Config.ApiUrl1 = t[1]; break;
                            case "ApiUrl2": Config.ApiUrl2 = t[1]; break;
                            case "ApiUrl3": Config.ApiUrl3 = t[1]; break;
                            case "ApiUrl4": Config.ApiUrl4 = t[1]; break;
                            case "Compress": Config.Compress = t[1].ToInt(); break;
                            case "Company": Config.Company = (eCompany) Enum.Parse(typeof(eCompany), t[1]);break;
                            //SelectedCompany =ListCompany.FindIndex(x => x == t[1]); break;
                            case "TypePrinter": Config.TypeUsePrinter = (eTypeUsePrinter)Enum.Parse(typeof(eTypeUsePrinter), t[1]); break;
                            case "TypeLog": FileLogger.TypeLog = (eTypeLog)Enum.Parse(typeof(eTypeLog), t[1]); break;
                            case "PhotoQuality": Config.PhotoQuality = (ePhotoQuality)Enum.Parse(typeof(ePhotoQuality), t[1]); break;
                            case "Warehouse": CodeWarehouse=t[1].ToInt(); break;
                            case "IsViewAllWH": Config.IsViewAllWH = t[1].Equals("true"); break;
                            case "IsAutoLogin": Config.IsAutoLogin = t[1].Equals("true"); break;
                            case "IsVibration": Config.IsVibration = t[1].Equals("true"); break;
                            case "IsSound": Config.IsSound = t[1].Equals("true"); break;
                            case "IsTest": Config.IsTest = t[1].Equals("true"); break;
                            case "IsFilterSave": Config.IsFilterSave = t[1].Equals("true"); break;
                            //case "IsFullScreenScan": IsFullScreenScan = t[1].ToBool(); break;
                            case "NameCompany":
                                Config.NameCompany = t[1];
                                db.SetConfig<string>("NameCompany", Config.NameCompany ?? "?");
                                break;
                        }
                    }
                }
                
                Config.CodeWarehouse = CodeWarehouse;
                OnClickSave(null, null);
                Connector.CleanConnector();
                c = ConnectorBase.GetInstance();
                GetDataHTTP.Init();                
                var R= await c.LoadGuidDataAsync(true);

                if (CodeWarehouse == 0)
                {
                    wh = null;
                    OnClickIP(null, null);
                    OnClickSave(null, null);
                }
                else { }
                    //var toast = Toast.Make("Не вдається отримати IP-адресу.");
                    // = toast.Show();
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await DisplayAlert("", $"Параметри вступлять в силу після перезапуску R=>{R?.State}", DeviceInfo.Platform == DevicePlatform.Android ? "Перезапуск" : "Ok");
                        if (DeviceInfo.Platform == DevicePlatform.Android)
                            Application.Current.Quit();
                    });
            }
            else
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("", "Даний QR без налаштувань.", "OK");
                });           
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
        private async void OnClickLoad(object sender, EventArgs e)
        {
            var r = await c.LoadGuidDataAsync(true);
            Config.DateLastLoadGuid = DateTime.Now;
            db.SetConfig<DateTime>("DateLastLoadGuid", Config.DateLastLoadGuid);
            if (r.State == 0)
            {
                var toast = Toast.Make($"Завантаження даних завершено. {r.Info}");
                _ = toast.Show();
            }
            else
            {
                var toast = Toast.Make($"Помилка завантаження даних. Код помилки: {r.State} {r.TextError}");
                _ = toast.Show();
            }
        }

        private void OnClickLoadDoc(object sender, EventArgs e) { c.LoadDocsDataAsync(0, null, false); }

        private void OnCopyDB(object sender, EventArgs e) {
            try
            {              
                var FileDestination = Path.Combine(Config.PathDownloads, "brb6.db");
                if (File.Exists(FileDestination)) File.Delete(FileDestination);
                byte[] buffer = File.ReadAllBytes(DB.PathNameDB);
                File.WriteAllBytes(FileDestination, buffer);
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage(this, "OnCopyDB", ex);
            }
        }

        private void OnRestoreDB(object sender, EventArgs e) {   }

        private void OnClickGen(object sender, EventArgs e)
        {
            Bl.GenApiUrl();
            OnPropertyChanged(nameof(ApiUrl1));
            OnPropertyChanged(nameof(ApiUrl2));
            OnPropertyChanged(nameof(ApiUrl3));
        }

        private async void OnClickIP(object sender, EventArgs e)
        {
            string currentIP = Config.NativeBase.GetIP();
            if (currentIP == null)
            {
                if (sender == null)
                {
                    var toast = Toast.Make("Не вдається отримати IP-адресу.");
                    _ = toast.Show();
                }
                else

                    await DisplayAlert("Помилка", "Не вдається отримати IP-адресу.", "OK");
                return;
            }
            if (ListWarehouse.Any() && ListWarehouse.First().Name != "ddd") {
                var matchingWarehouseIndex = ListWarehouse.FindIndex(warehouse =>
                    warehouse.InternalIP.Split('.').Take(3).SequenceEqual(currentIP.Split('.').Take(3)));

                if (matchingWarehouseIndex != -1)
                {
                    SelectedWarehouse = matchingWarehouseIndex;
                    Config.ApiUrl2 = ListWarehouse[matchingWarehouseIndex].Url;
                    var toast = Toast.Make($"{ListWarehouse[matchingWarehouseIndex].Name} IP=>{currentIP}");
                    _ = toast.Show();
                }
                else
                {
                    if (sender == null)
                    {
                        var toast = Toast.Make($"Відповідний магазин не знайдено. IP=>{currentIP}");
                        _ = toast.Show();
                    }
                    else
                        await DisplayAlert("Info", $"Відповідний магазин не знайдено. IP=>{currentIP}", "OK");
                } 
            }
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            Config.CodesWarehouses = Warehouses.Where(x => x.IsChecked).Select(x => x.CodeWarehouse).ToList();
            Bl.SaveSettings();
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
        private void OnInfo(object sender, EventArgs e)
        {

        }
    }
}