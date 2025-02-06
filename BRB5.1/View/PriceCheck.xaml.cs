using BRB5.Model;
using BRB6.View;
using BL;
using BRB5;
using BarcodeScanning;
using CommunityToolkit.Maui.Core.Platform;
#if ANDROID
using Android.Views;
#endif

//using BRB5.Connector;
namespace BRB6
{
    public partial class PriceCheck : IDisposable
    {
        DB db = DB.GetDB();
        BL.BL bl = BL.BL.GetBL();

        public List<PrintBlockItems> ListPrintBlockItems { get { return db.GetPrintBlockItemsCount().ToList(); }  }

        public int SelectedPrintBlockItems { get { return ListPrintBlockItems.Count > 0 ? ListPrintBlockItems.Last().PackageNumber : -1; }  }
        public bool IsVisPriceNormal { get { return WP != null && (WP.PriceOld != WP.PriceNormal); } }
        public bool IsVisPriceOpt { get { return WP != null && (WP.PriceOpt != 0 || WP.PriceOptOld != 0); } }
        public bool IsVisPriceOptQ { get { return WP != null && WP.QuantityOpt != 0 ; } }

        public bool IsVisF4 { get { return Config.Company == eCompany.Sim23; } }
        public string F4Text { get { return IsOnline ? "OnLine" : "OffLine"; } }
        private bool _IsOnline = true;
        public bool IsOnline { get { return _IsOnline; } set { _IsOnline = value; OnPropertyChanged("F4Text"); } }

        bool _IsVisRepl = false;
        public bool IsVisRepl { get { return _IsVisRepl; } set { _IsVisRepl = value; OnPropertyChanged("IsVisRepl"); } }
        public bool IsSoftKeyboard { get { return Config.IsSoftKeyboard; } }
        double _PB = 0;
        public double PB { get { return _PB; } set { _PB = value; OnPropertyChanged(nameof(PB)); } }

        WaresPrice _WP;
        public WaresPrice WP { get { return _WP; } set { _WP = value; OnPropertyChanged("WP"); OnPropertyChanged("TextColorPrice");
                OnPropertyChanged("IsVisPriceOpt"); OnPropertyChanged(nameof(IsVisPriceNormal)); OnPropertyChanged("TextColorHttp");
                OnPropertyChanged("ColorPrintColorType"); OnPropertyChanged("IsVisPriceOptQ");
            } }
        //ZXingScannerView zxing;
        //ZXingDefaultOverlay overlay;

        int _PrintType = 0;//Колір чека 0-звичайний 1-жовтий, -1 не розділяти.        
        public int PrintType { get { return _PrintType; } set { _PrintType = value; OnPropertyChanged("PrintType"); OnPropertyChanged("ColorPrintColorType"); } }
        public bool IsEnabledPrint { get { return Config.TypeUsePrinter != eTypeUsePrinter.NotDefined; } }
        /// <summary>
        /// Номер сканування цінників за день !!!TMP Треба зберігати в базі.
        /// </summary>
        int LineNumber = 0;
        public int AllScan { get; set; } = 0;
        public int BadScan { get; set; } = 0;
        /// <summary>
        /// Номер пакета цінників за день !!!TMP Треба зберігати в базі.
        /// </summary>
        int _PackageNumber = 1;
        public int PackageNumber { get { return _PackageNumber; } set { _PackageNumber = value; OnPropertyChanged(nameof(PackageNumber)); OnPropertyChanged(nameof(ListPrintBlockItems)); OnPropertyChanged(nameof(SelectedPrintBlockItems)); } }


        //public int ColorPrintColorType() { return Color.parseColor(HttpState != eStateHTTP.HTTP_OK ? "#ffb3b3" : (PrintType == 0 ? "#ffffff" : "#3fffff00")); }

        public string ColorPrintColorType { get { return WP != null && WP.MinQuantity == 0? "#ffd8d8" : PrintType == 0 ? "#ffffff" : PrintType == 1 ? "#ffffa8" : "#ffffff"; } }
        public string TextColorPrice { get {return (WP != null && WP.Price != 0 && WP.Price == WP.PriceOld && WP.PriceOpt == WP.PriceOptOld) ? "#009800" : "#ff5c5c"; } set { OnPropertyChanged(nameof(TextColorPrice)); } }
        public string TextColorHttp { get { return (WP != null && WP.StateHTTP == eStateHTTP.HTTP_OK) ? "#009800" : "#ff5c5c"; } }

        public bool _IsMultyLabel = false;
        public bool IsMultyLabel { get { return _IsMultyLabel; } set { _IsMultyLabel = value; OnPropertyChanged(nameof(IsMultyLabel)); OnPropertyChanged(nameof(F5Text)); } }
        public string F5Text { get { return IsMultyLabel ? "Дубл." : "Унік."; } }
        public bool IsVisScan { get { return Config.TypeScaner == eTypeScaner.Camera; } }
        /// <summary>
        /// 0 - нічого , 1 - сканований цінник, 2 - сканований товар, 3 - штрихкод товату не підходить, 4 - цінник не підходить, 5 - успішно
        /// </summary>
        private eCheckWareScaned _IsWareScaned = eCheckWareScaned.Nothing;
        public eCheckWareScaned IsWareScaned { get { return _IsWareScaned; } set { _IsWareScaned = value; OnPropertyChanged(nameof(ColorDoubleScan)); OnPropertyChanged(nameof(IsWareScaned)); /*OnPropertyChanged(nameof(ButtonDoubleScan));*/ OnPropertyChanged(nameof(MessageDoubleScan)); } }
        public bool IsVisDoubleScan { get; set; }
        public bool IsVisBarcode { get { return !IsVisDoubleScan; } }
       // private string _MessageDoubleScan;
        public string MessageDoubleScan { get { return EnumMethods.GetDescription(WP?.StateDoubleScan??eCheckWareScaned.Success); } } //set {  OnPropertyChanged(nameof(MessageDoubleScan)); } }
        //public string ButtonDoubleScan { get { return IsWareScaned == eCheckWareScaned.Nothing || IsWareScaned == eCheckWareScaned.Success ? "" :  IsWareScaned == eCheckWareScaned.WareScaned || IsWareScaned == eCheckWareScaned.PriceTagNotFit ? "Відсутній ціник" : "Відсутній товар"; } }
        public string ColorDoubleScan { get { return IsWareScaned == eCheckWareScaned.Success ? "#C5FFC4" : IsWareScaned == eCheckWareScaned.Bad || IsWareScaned== eCheckWareScaned.BadPrice ? "#FFC4C4" : 
                                                     IsWareScaned == eCheckWareScaned.PriceTagScaned || IsWareScaned == eCheckWareScaned.WareScaned ? "#FEFFC4" : "#FFFFFF"; } }
        CameraView BarcodeScaner;
        public PriceCheck(TypeDoc pTypeDoc)
        {
            InitializeComponent();
            NokeyBoard();
            bl.ClearWPH();

            var r = db.GetCountScanCode();
            IsVisDoubleScan = pTypeDoc.CodeDoc == 15;

            if (Config.TypeUsePrinter == eTypeUsePrinter.StationaryWithCutAuto) PrintType = -1;

            // TODO Xamarin.Forms.Device.RuntimePlatform is no longer supported. Use Microsoft.Maui.Devices.DeviceInfo.Platform instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
            NavigationPage.SetHasNavigationBar(this, DeviceInfo.Platform == DevicePlatform.iOS);

            if (r != null)
            {
                AllScan = r.AllScan;
                BadScan = r.BadScan;
                LineNumber = r.LineNumber;
                PackageNumber = r.PackageNumber;
            }
            if (!IsVisScan)
             Config.BarCode = BarCode;
            
            this.BindingContext = this;
        }

        void Progress(double pProgress) => MainThread.BeginInvokeOnMainThread(() => PB = pProgress);
        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (!IsSoftKeyboard)
            {                
#if ANDROID
            MainActivity.Key+= OnPageKeyDown;
#endif
            }
            if (IsVisScan)
            {
                BarcodeScaner = new CameraView
                {
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    CameraEnabled = true,
                    VibrationOnDetected = false,
                    BarcodeSymbologies = BarcodeFormats.Ean13 | BarcodeFormats.Ean8 | BarcodeFormats.QRCode,

                };

                BarcodeScaner.OnDetectionFinished += CameraView_OnDetectionFinished;

                GridZxing.Children.Add(BarcodeScaner);

            }
            if (!IsVisScan) 
                BarCodeInput.Focus();
            //if (IsVisDoubleScan && WP!=null) WP.StateDoubleScan = eCheckWareScaned.Nothing;
            Config.OnProgress += Progress;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (IsVisScan) BarcodeScaner.CameraEnabled = false;

            if (!IsSoftKeyboard)
            {
#if ANDROID
            MainActivity.Key-= OnPageKeyDown;
#endif
            }
            bl.SendLogPrice();
            Config.OnProgress -= Progress;
        }
        void BarCode(string pBarCode)=>FoundWares(pBarCode, false);

        void FoundWares(string pBarCode, bool pIsHandInput = false)
        {
            if (!String.IsNullOrWhiteSpace(pBarCode))
            {
                LineNumber++;
                Config.OnProgress?.Invoke(0.2d);

                WP = bl.FoundWares(pBarCode, PackageNumber, LineNumber, pIsHandInput, IsVisDoubleScan, IsOnline);

                if (WP != null)
                {
                    AllScan++;
                    if (!WP.IsPriceOk)
                        BadScan++;
                    IsWareScaned = WP.StateDoubleScan;
                }
                if (Config.IsVibration)
                {
                    var duration = TimeSpan.FromMilliseconds(WP?.IsPriceOk == true ? 50 : 250);
                    Vibration.Vibrate(duration);
                }

                Config.OnProgress?.Invoke(0.9d);
                
                if(DeviceInfo.Platform != DevicePlatform.iOS)  BarCodeFocused(null, null);
                
            }                

        }

        public void Dispose() { Config.BarCode -= BarCode;   }
        
        private void OnClickAddPrintBlock(object sender, EventArgs e)
        {
            PackageNumber++;
            ListPrintBlockItems.Add(new PrintBlockItems() { PackageNumber = PackageNumber });
        }

        private void OnClickPrintBlock(object sender, EventArgs e)
        {
            var temp = PrintBlockItemsXaml.SelectedItem as PrintBlockItems;
            if (IsEnabledPrint) 
                _ = DisplayAlert("Друк", bl.PrintPackage(PrintType, temp.PackageNumber, IsMultyLabel), "OK");
        }

        private void OnF2(object sender, EventArgs e)
        {
            IsVisRepl = !IsVisRepl;
            if(IsVisRepl) NumberOfReplenishment.Focus();
        }

        private void OnF4(object sender, EventArgs e) {      IsOnline = !IsOnline;      }

        private void OnF5(object sender, EventArgs e)   {   IsMultyLabel = !IsMultyLabel;    }

        private async void OnClickWareInfo(object sender, EventArgs e)
        {            
            if (WP != null)
            {
                if (IsVisScan) BarcodeScaner.PauseScanning = true;
                await Navigation.PushAsync(new WareInfo(WP.ParseBarCode));
            }
        }

        private void BarCodeHandInput(object sender, EventArgs e)
        {
            var text = BarCodeInput.Text;
            FoundWares(text, true);
        }

        private void BarCodeFocused(object sender, FocusEventArgs e)
        {
            Dispatcher.Dispatch(() =>
            {
                BarCodeInput.CursorPosition = 0;
                BarCodeInput.SelectionLength = BarCodeInput.Text == null ? 0 : BarCodeInput.Text.Length;
                if (!BarCodeInput.IsFocused|| IsVisScan) 
                    BarCodeInput.Focus();
            });
        }

        private void OnClickPrintOne(object sender, EventArgs e)
        {
            if (IsEnabledPrint && WP!=null)
                _ = DisplayAlert("Друк", bl.c.PrintHTTP(new[] { WP.CodeWares }), "OK");
        }   

        private void OnUpdateReplenishment(object sender, EventArgs e)
        {
            decimal d;
            if (decimal.TryParse(NumberOfReplenishment.Text, out d))
                db.UpdateReplenishment(LineNumber, d);
        }

        private void DoubleScanReact(object sender, EventArgs e)
        {

            if(IsWareScaned==eCheckWareScaned.PriceTagScaned|| IsWareScaned == eCheckWareScaned.WareNotFit)//Відсутній товар
            {
                bl.SaveDoubleScan(102, WP, PackageNumber, LineNumber);
                WP = null;
                //IsWareScaned = eCheckWareScaned.Nothing;
                //MessageDoubleScan = "Скануйте цінник чи товар";
            }
            else if (IsWareScaned == eCheckWareScaned.WareScaned || IsWareScaned == eCheckWareScaned.PriceTagNotFit)//Відсутній ціник
            {
                bl.SaveDoubleScan(101, WP, PackageNumber, LineNumber);
                WP = null;
                //IsWareScaned = eCheckWareScaned.Nothing;
                //MessageDoubleScan = "Скануйте цінник чи товар";
            }

        }

        private void CameraView_OnDetectionFinished(object sender, BarcodeScanning.OnDetectionFinishedEventArg e)
        {
            if (e.BarcodeResults.Length > 0)
            {
                BarcodeScaner.PauseScanning = true;
                FoundWares(e.BarcodeResults[0].DisplayValue);
                Task.Run(async () => {                     
                    await Task.Delay(1000);
                    BarcodeScaner.PauseScanning = false;
                });
                
                
            }
        }

#if ANDROID
        public void OnPageKeyDown(Keycode keyCode, KeyEvent e)
        {
           switch (keyCode)
           {
            case Keycode.F1:
            OnClickPrintBlock(null, EventArgs.Empty);
               return;
            case Keycode.F2:
            OnF2(null, EventArgs.Empty);
               return;
            case Keycode.F4:
            OnF4(null, EventArgs.Empty);
               return;
            case Keycode.F5:
            OnF5(null, EventArgs.Empty);
               return;
            case Keycode.F6:
            OnClickPrintOne(null, EventArgs.Empty);
               return;

            default:
               return;
           }
         }
#endif
    }
}