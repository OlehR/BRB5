using BRB5.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls.Xaml;
using ZXing.Net.Mobile.Forms;
using ZXing;
using ZXing.Mobile;
using BRB5.View;
using System.Net.NetworkInformation;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

//using BRB5.Connector;
namespace BRB5
{
    public partial class PriceCheck : IDisposable
    {

        Connector.Connector c;
        DB db = DB.GetDB();
        BL bl = BL.GetBL();

        public List<PrintBlockItems> ListPrintBlockItems { get { return db.GetPrintBlockItemsCount().ToList(); }  }

        public int SelectedPrintBlockItems { get { return ListPrintBlockItems.Count > 0 ? ListPrintBlockItems.Last().PackageNumber : -1; }  }
        public bool IsVisPriceNormal { get { return WP != null && (WP.PriceOld != WP.PriceNormal); } }
        public bool IsVisPriceOpt { get { return WP != null && (WP.PriceOpt != 0 || WP.PriceOptOld != 0); } }

        bool _IsVisF4 = false;
        public bool IsVisF4 { get { return _IsVisF4; } set { _IsVisF4 = value; OnPropertyChanged("IsVisF4"); } }

        bool _IsVisRepl = false;
        public bool IsVisRepl { get { return _IsVisRepl; } set { _IsVisRepl = value; OnPropertyChanged("IsVisRepl"); } }

        double _PB = 0;
        public double PB { get { return _PB; } set { _PB = value; OnPropertyChanged("PB"); } }

        WaresPrice _WP;
        public WaresPrice WP { get { return _WP; } set { _WP = value; OnPropertyChanged("WP"); OnPropertyChanged("TextColorPrice");
                OnPropertyChanged("IsVisPriceOpt"); OnPropertyChanged(nameof(IsVisPriceNormal)); OnPropertyChanged("TextColorHttp"); } }
        ZXingScannerView zxing;
        //ZXingDefaultOverlay overlay;

        int _PrintType = 0;//Колір чека 0-звичайний 1-жовтий, -1 не розділяти.        
        public int PrintType { get { return _PrintType; } set { _PrintType = value; OnPropertyChanged("PrintType"); OnPropertyChanged("ColorPrintColorType"); } }
        public bool IsEnabledPrint { get { return Config.TypeUsePrinter != eTypeUsePrinter.NotDefined; } }
        public bool IsOnline { get; set; } = true;

        /// <summary>
        /// Номер сканування цінників за день !!!TMP Треба зберігати в базі.
        /// </summary>
        int LineNumber = 0;

        int _PackageNumber = 1;

        public int AllScan { get; set; } = 0;
        public int BadScan { get; set; } = 0;
        /// <summary>
        /// Номер пакета цінників за день !!!TMP Треба зберігати в базі.
        /// </summary>
        public int PackageNumber { get { return _PackageNumber; } set { _PackageNumber = value; OnPropertyChanged("PackageNumber"); OnPropertyChanged("ListPrintBlockItems"); OnPropertyChanged("SelectedPrintBlockItems"); } }


        //public int ColorPrintColorType() { return Color.parseColor(HttpState != eStateHTTP.HTTP_OK ? "#ffb3b3" : (PrintType == 0 ? "#ffffff" : "#3fffff00")); }

        public string ColorPrintColorType { get { return PrintType == 0 ? "#ffffff" : PrintType == 1 ? "#ffffa8" : "#ffffff"; } }

        public string TextColorPrice { get {return (WP != null && WP.Price != 0 && WP.Price == WP.PriceOld && WP.PriceOpt == WP.PriceOptOld) ? "#009800" : "#ff5c5c"; } set { OnPropertyChanged(nameof(TextColorPrice)); } }

        public string TextColorHttp { get { return (WP != null && WP.StateHTTP == eStateHTTP.HTTP_OK) ? "#009800" : "#ff5c5c"; } }

        public bool _IsMultyLabel = false;
        public bool IsMultyLabel { get { return _IsMultyLabel; } set { _IsMultyLabel = value; OnPropertyChanged("IsMultyLabel"); OnPropertyChanged("F5Text"); } }

        public string F5Text { get { return IsMultyLabel ? "Дублювати" : "Унікальні"; } }

        public bool IsVisScan { get { return Config.TypeScaner == eTypeScaner.Camera; } }


        public PriceCheck()
        {
            InitializeComponent();
            MessagingCenter.Subscribe<KeyEventMessage>(this, "F1Pressed", message => { OnClickPrintBlock(null, EventArgs.Empty); });
            MessagingCenter.Subscribe<KeyEventMessage>(this, "F2Pressed", message => { OnF2(null, EventArgs.Empty); });
            MessagingCenter.Subscribe<KeyEventMessage>(this, "F5Pressed", message => { OnF5(null, EventArgs.Empty); });
            MessagingCenter.Subscribe<KeyEventMessage>(this, "F6Pressed", message => { OnClickPrintOne(null, EventArgs.Empty); });


            c = Connector.Connector.GetInstance();
            var r = db.GetCountScanCode();

            if (Config.TypeUsePrinter == eTypeUsePrinter.StationaryWithCutAuto) PrintType = -1;
            // TODO Xamarin.Forms.Device.RuntimePlatform is no longer supported. Use Microsoft.Maui.Devices.DeviceInfo.Platform instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
            NavigationPage.SetHasNavigationBar(this, Device.RuntimePlatform == Device.iOS|| Config.TypeScaner==eTypeScaner.BitaHC61 || Config.TypeScaner == eTypeScaner.Zebra || Config.TypeScaner == eTypeScaner.PM550 || Config.TypeScaner == eTypeScaner.PM351);

            if (r != null)
            {
                AllScan = r.AllScan;
                BadScan = r.BadScan;
                LineNumber = r.LineNumber;
                PackageNumber = r.PackageNumber;
            }
            if (!IsVisScan)
             Config.BarCode = BarCode;

            NumberOfReplenishment.Unfocused += (object sender, FocusEventArgs e) =>
            {
                decimal d;
                if (decimal.TryParse(((Entry)sender).Text, out d))
                    db.UpdateReplenishment(LineNumber, d);
            };

            Config.OnProgress += (pProgress) => { PB = pProgress; };
            this.BindingContext = this;
        }

        void BarCode(string pBarCode)
        {
            FoundWares(pBarCode, false);
        }

        void FoundWares(string pBarCode, bool pIsHandInput = false)
        {
            LineNumber++;
            PB = 0.2d;

            if (IsOnline)
            {
                WP = c.GetPrice(c.ParsedBarCode(pBarCode, pIsHandInput));
                //if(WP.State>0)
            }
            else
            {
                var data = bl.GetWaresFromBarcode(0, null, pBarCode, pIsHandInput);
                WP = new WaresPrice(data);
            }
            if (WP != null)
            {
                AllScan++;
                if (!WP.IsPriceOk)
                    BadScan++;
            }
            var duration = TimeSpan.FromMilliseconds(WP?.IsPriceOk == true ? 50 : 250);
            Vibration.Vibrate(duration);
            //if (Config.Company == eCompany.Sim23)
            //   utils.PlaySound();
            var l = new LogPrice(WP, IsOnline, PackageNumber, LineNumber);
            db.InsLogPrice(l);

            PB = 1;
            BarCodeInput.Focus();
            BarCodeFocused(null, null);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (IsVisScan)
            {
                zxing = SetZxing(GridZxing, zxing);
                zxing.IsScanning = true;
                zxing.IsAnalyzing = true;
            }
            BarCodeInput.Focus();
        }

        /// <summary>
        /// Костиль через баг https://github.com/Redth/ZXing.Net.Mobile/issues/710
        /// </summary>
        ZXingScannerView SetZxing(Grid pV, ZXingScannerView pZxing)
        {
            if (pZxing != null)
            {
                // TODO Xamarin.Forms.Device.RuntimePlatform is no longer supported. Use Microsoft.Maui.Devices.DeviceInfo.Platform instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
                if (Device.RuntimePlatform == Device.iOS)
                    return pZxing;
                pV.Children.Remove(pZxing);
            }
            pZxing = new ZXingScannerView();
            pV.Children.Add(pZxing);

            pZxing.Options = new MobileBarcodeScanningOptions
            {
                PossibleFormats = new List<BarcodeFormat>
                    {
                        BarcodeFormat.All_1D,
                        BarcodeFormat.QR_CODE,
                    },
                UseNativeScanning = true,
            };
            pZxing.OnScanResult += (result) =>
                Device.BeginInvokeOnMainThread(async () =>
                // Stop analysis until we navigate away so we don't keep reading barcodes
                {
                    pZxing.IsAnalyzing = false;
                    FoundWares(result.Text, false);
                    pZxing.IsAnalyzing = true;
                });            
            return pZxing;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (IsVisScan) zxing.IsScanning = false;

            MessagingCenter.Unsubscribe<KeyEventMessage>(this, "F1Pressed");
            MessagingCenter.Unsubscribe<KeyEventMessage>(this, "F2Pressed");
            MessagingCenter.Unsubscribe<KeyEventMessage>(this, "F5Pressed");
            MessagingCenter.Unsubscribe<KeyEventMessage>(this, "F6Pressed");
        }

        public void Dispose()
        {
            bl.SendLogPrice();
            Config.BarCode -= BarCode;
        }

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
        }

        private void OnF4(object sender, EventArgs e)
        {

        }

        private void OnF5(object sender, EventArgs e)
        {            
            IsMultyLabel = !IsMultyLabel;
        }

        private async void OnClickWareInfo(object sender, EventArgs e)
        {            
            if (WP != null)
            {
                if (IsVisScan) zxing.IsAnalyzing = false;
                await Navigation.PushAsync(new WareInfo(WP.ParseBarCode));
            }
        }

        private void BarCodeHandInput(object sender, EventArgs e)
        {
            var text = ((Entry)sender).Text;
            FoundWares(text, true);
        }

        private void BarCodeFocused(object sender, FocusEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                BarCodeInput.CursorPosition = 0;
                BarCodeInput.SelectionLength = BarCodeInput.Text == null ? 0 : BarCodeInput.Text.Length;
            });
        }

        private void OnClickPrintOne(object sender, EventArgs e)
        {
            if (IsEnabledPrint && WP!=null)
                _ = DisplayAlert("Друк", c.PrintHTTP(new[] { WP.CodeWares }), "OK");
        }
    }
}