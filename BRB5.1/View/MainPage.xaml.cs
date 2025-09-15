using BarcodeScanning;
using BL;
using BL.Connector;
using BRB5;
using BRB5.Model;
using BRB6.View;
using System.Collections.ObjectModel;
using Docs = BRB6.View.Docs;

//[assembly: Xamarin.Forms.Dependency(typeof(AndroidStorageManager))]

namespace BRB6
{
    public partial class MainPage 
    {
        public ObservableCollection<TypeDoc> OCTypeDoc { get; set; }
        Connector c;
        DB db;
        BL.BL Bl;
        double _PB = 0.0;
        public double PB { get { return _PB; } set { _PB = value; OnPropertyChanged(nameof(PB)); } }
        public string Login { get; set; }
        public string Barcode { get; set; }
        public string Password { get; set; }
        public IEnumerable<LoginServer> LS { get; set; }

        public int SelectedLS { get { return LS == null || LS.Count() == 1 ? 0 : LS.ToList().FindIndex(x => x.Code == Config.LoginServer); } set { Config.LoginServer = LS.ToList()[value].Code; } }
        public bool IsVisLS { get; set; } = true;
        private bool _IsVisibleBack = false;
        public bool IsVisibleBack { get { return _IsVisibleBack; } set { _IsVisibleBack = value; OnPropertyChanged(nameof(IsVisibleBack)); } }
        public string Ver { get { return"BRB6 (" + AppInfo.VersionString + ")"; } }
        public string Company { get { return  Config.NameCompany; } }
        public bool IsSoftKeyboard { get { return Config.IsSoftKeyboard; } }

        public bool IsVisScan { get { return Config.TypeScaner == eTypeScaner.Camera; } }
        bool _IsVisBarCode = false;
        public bool IsVisBarCode { get { return _IsVisBarCode; } set { _IsVisBarCode = value; OnPropertyChanged(nameof(IsVisBarCode)); } }
        CameraView BarcodeScaner;
        public MainPage()
        {
            try
            {
                ProtoBRB.Init();
                db = DB.GetDB(ProtoBRB.GetPathDB);
                Bl = BL.BL.GetBL();
                Config.Company = db.GetConfig<eCompany>("Company");
                OCTypeDoc = new ObservableCollection<TypeDoc>();
                InitializeComponent();
                Init();
                if (Config.Company == eCompany.NotDefined) _ = Navigation.PushAsync(new Settings());
                BindingContext = this;
                Config.BarCode = BarCode;
            }
            catch (Exception ex)
            {
                _ = DisplayAlert("Помилка", "Не вдалось ініціалізувати програму. " + ex.Message + '\n' +ex.StackTrace, "OK");
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

       
        public void Dispose() { Config.BarCode -= BarCode; }
        void BarCode(string pBC)
        {
            Barcode = pBC;
            OnButtonLogin(null, null);
        }

        private void OnButtonLogin(object sender, System.EventArgs e)
        {
            _ = Task.Run(async () =>
            {
                var r = await c.LoginAsync(Login, Password, Config.LoginServer,Barcode);
                Barcode = null;
                if (r.State == 0)
                {                    
                    Dispatcher.Dispatch(() =>
                    {
                        OCTypeDoc?.Clear();
                        foreach (var i in c.GetTypeDoc(Config.Role, Config.LoginServer)) OCTypeDoc.Add(i);

#if IOS          
                        var screenHeight = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;
                        var navigationBarHeight = 70;
                        ListDocsButton.HeightRequest = screenHeight-navigationBarHeight;
#endif

                        SLLogin.IsVisible = false;
                        ListDocs.IsVisible = true;
                    });
                    Bl.OnButtonLogin(Login, Password, DeviceInfo.Platform == DevicePlatform.Android);

                    Login = db.GetConfig<string>("Login");
                    OnPropertyChanged(nameof(Login));
                    BarcodeClick(null, null);
                }
                else
                    Dispatcher.Dispatch(() =>
                    {
                        _ = DisplayAlert("Проблеми з авторизацією", r.TextError +" "+ r.Info, "OK");
                    });
            });
        }

        private async void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null) return;

            // Отримання вибраного елемента
            var vTypeDoc = e.Item as TypeDoc;

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

            var FileStatus = await Permissions.CheckStatusAsync<Permissions.StorageWrite >();
            if (FileStatus != PermissionStatus.Granted)
                FileStatus = await Permissions.RequestAsync<Permissions.StorageWrite>();

            if (FileStatus != PermissionStatus.Granted)
            {
                //await DisplayAlert("Помилка", "Потрібен дозвіл StorageWrite", "OK", FlowDirection.MatchParent);
                //return;
            }

            switch (vTypeDoc.KindDoc)
            {
                case eKindDoc.PriceCheck:
                    await Navigation.PushAsync(new PriceCheck(vTypeDoc));
                    break;
                case eKindDoc.Normal:
                case eKindDoc.Simple:
                    await Navigation.PushAsync(new Docs(vTypeDoc));
                    break;
                case eKindDoc.RaitingDoc:
                    await Navigation.PushAsync(new RaitingDoc(vTypeDoc));
                    break;
                case eKindDoc.RaitingTempate:
                    await Navigation.PushAsync(new RaitingTemplatesEdit());
                    break;
                case eKindDoc.RaitingTemplateCreate:
                    await Navigation.PushAsync(new RaitingDocsEdit(vTypeDoc));
                    break;
                case eKindDoc.PlanCheck:
                    await Navigation.PushAsync(new PlanCheckPrice());
                    break;
                case eKindDoc.ExpirationDate:
                    await Navigation.PushAsync(new ExpirationDate());
                    break;
                case eKindDoc.LotsCheck:
                case eKindDoc.Lot:
                    await Navigation.PushAsync(new LotsCheck(vTypeDoc));
                    break;
                case eKindDoc.NotDefined:
                    Dispatcher.Dispatch(() =>
                    {
                        OCTypeDoc?.Clear();
                        var r = c.GetTypeDoc(Config.Role, Config.LoginServer, vTypeDoc.Group);
                        foreach (var i in r) OCTypeDoc.Add(i);
                        IsVisibleBack = true;
                    });
                    break;
                default:
                    break;
            }

            // Скидання вибраного елемента, щоб зняти виділення
            ((ListView)sender).SelectedItem = null;
        }

        void Init()
        {
            try
            {
                _ = LocationBrb.GetCurrentLocation(db.GetWarehouse());
                Login = db.GetConfig<string>("Login");
                Bl.Init();
                c = ConnectorBase.GetInstance();
                if (c != null)
                {
                    LS = c.LoginServer();
                    if (LS == null || LS.Count() == 1)
                    {
                        IsVisLS = false;
                        Config.LoginServer = LS.First().Code;
                    }
                }
                if (Config.IsAutoLogin)
                {
                    Password = db.GetConfig<string>("Password");
                    if (!string.IsNullOrEmpty(Password))
                        OnButtonLogin(null, null);
                }
            }catch (Exception ex)
            {
                _ = DisplayAlert("Помилка Init", "Не вдалось ініціалізувати програму. " + ex.Message, "OK");
                System.Diagnostics.Debug.WriteLine(ex);
            }
            }
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (IsVisScan)
            {
                BarcodeScaner = new CameraView
                {
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    CameraEnabled = false,
                    VibrationOnDetected = false,
                    BarcodeSymbologies = BarcodeFormats.Ean13 | BarcodeFormats.Ean8 | BarcodeFormats.QRCode| BarcodeFormats.Code128,

                };
                BarcodeScaner.OnDetectionFinished += CameraView_OnDetectionFinished;
                GridZxing.Children.Add(BarcodeScaner);
            }

#if ANDROID
            if (Config.NativeBase !=null && await Config.NativeBase.CheckNewVerAsync())
            {
                var res = await DisplayAlert("Оновлення доступне", "Доступна нова версія. Бажаєте встановити?", "Yes", "No");
                MyProgress.IsVisible = true;
                if (res)  
                    _=Task.Run(async()=> Config.NativeBase.InstallAsync(Progress));
            }
            #endif
        }
        void Progress(double pProgress) => MainThread.BeginInvokeOnMainThread(() => PB = pProgress);
        protected override void OnDisappearing()  
        {  
            base.OnDisappearing();
            if (IsVisScan) BarcodeScaner.CameraEnabled = false;
        }

        private async void OnSettingsClicked(object sender, EventArgs e) { await Navigation.PushAsync(new Settings());  }

        private void OnAuthorizationClicked(object sender, EventArgs e)
        {
            ListDocs.IsVisible = false;
            SLLogin.IsVisible = true;
        }
        private void BackToMainList(object sender, EventArgs e)
        {
            Dispatcher.Dispatch(() =>
            {
                IsVisibleBack = false;
                OCTypeDoc?.Clear();
                var r = c.GetTypeDoc(Config.Role, Config.LoginServer);
                foreach (var i in r) OCTypeDoc.Add(i);
            });
        }

        private void BarcodeClick(object sender, EventArgs e)
        {
            if (IsVisScan)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    IsVisBarCode = !IsVisBarCode;
                    BarcodeScaner.CameraEnabled = IsVisBarCode;
                });
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
    }
}