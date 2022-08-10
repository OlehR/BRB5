using BRB5.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing.Net.Mobile.Forms;
//using BRB5.Connector;
namespace BRB5
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PriceCheck : ContentPage, INotifyPropertyChanged
    {
        Connector.Connector c;        

        bool _IsVisPriceOpt = false;
        public bool IsVisPriceOpt { get { return _IsVisPriceOpt; } set { _IsVisPriceOpt = value; OnPropertyChanged("IsVisPriceOpt"); } }

        bool _IsVisF4 = false;
        public bool IsVisF4 { get { return _IsVisF4; } set { _IsVisF4 = value; OnPropertyChanged("IsVisF4"); } }

        bool _IsVisRepl = false;
        public bool IsVisRepl { get { return _IsVisRepl; } set { _IsVisRepl = value; OnPropertyChanged("IsVisRepl"); } }

        double _PB = 0;
        public double PB { get { return _PB; } set { _PB = value; OnPropertyChanged("PB"); } }

        WaresPrice _WP;
        public WaresPrice WP { get { return _WP; } set { _WP = value; OnPropertyChanged("WP"); } }
        //ZXingScannerView zxing;
        //ZXingDefaultOverlay overlay;

        int _PrintType = 0;//Колір чека 0-звичайний 1-жовтий, -1 не розділяти.        
        public int PrintType { get { return _PrintType; } set { _PrintType = value; OnPropertyChanged("PrintType"); OnPropertyChanged("NamePrintColorType"); OnPropertyChanged("ColorPrintColorType"); } }


        //public int ColorPrintColorType() { return Color.parseColor(HttpState != eStateHTTP.HTTP_OK ? "#ffb3b3" : (PrintType == 0 ? "#ffffff" : "#3fffff00")); }
        public string NamePrintColorType { get { return PrintType == 0 ? "Звичайний" : PrintType == 1 ? "Жовтий" : "Авто"; } }
        public string ColorPrintColorType { get { return PrintType == 0 ? "#ffffff" : PrintType == 1 ? "#ffffa8" : "#ffffff"; } }


        string _TextColorPrice = "#90EE90";
        public string TextColorPrice { get { return _TextColorPrice; } set { _TextColorPrice = value; OnPropertyChanged("TextColorPrice"); } }

        string _TextColorPriceOpt = "#90EE90";
        public string TextColorPriceOpt { get { return _TextColorPriceOpt; } set { _TextColorPriceOpt = value; OnPropertyChanged("TextColorPriceOpt"); } }


        public PriceCheck()
        {
            InitializeComponent();

            c = Connector.Connector.GetInstance();


            zxing.OnScanResult += (result) =>
                Device.BeginInvokeOnMainThread(async () =>
                {
                    PB = 0.2;
                    IsVisPriceOpt = false;
                    TextColorPrice = "#90EE90";
                    TextColorPriceOpt = "#90EE90";
                    // Stop analysis until we navigate away so we don't keep reading barcodes
                    zxing.IsAnalyzing = false;
                    //zxing.IsScanning = false;
                    // Show an alert
                    WP = c.GetPrice(c.ParsedBarCode(result.Text,true));
                    if (WP.PriceOpt != 0 || WP.PriceOptOld != 0) {
                        IsVisPriceOpt = true;
                        if (WP.PriceOpt != WP.PriceOptOld) TextColorPriceOpt = "#ff5c5c";
                    }
                    //await DisplayAlert("Scanned Barcode", WP.Price+" " + WP.Name, "OK");

                    if (WP.Price != WP.PriceOld) TextColorPrice = "#ff5c5c";

                    zxing.IsAnalyzing = true;
                    //zxing.IsScanning = true;
                    // Navigate away
                    //await Navigation.PopAsync();
                    PB = 1;
                });

            
            //MainSL.Children.Add(zxing);

            /*overlay = new ZXingDefaultOverlay
            {
                TopText = "Hold your phone up to the barcode",
                BottomText = "Scanning will happen automatically",
                ShowFlashButton = zxing.HasTorch,
                AutomationId = "zxingDefaultOverlay",
            };
            overlay.FlashButtonClicked += (sender, e) =>
            {
                zxing.IsTorchOn = !zxing.IsTorchOn;
            };*/


            /* ZXingScannerPage scanPage = new ZXingScannerPage();
             ZXingScannerPage.ScanResultDelegate scanResultDelegate = (Result) =>
             {
                 scanPage.IsScanning = false;
                 Device.BeginInvokeOnMainThread(() =>
                 {
                     Navigation.PopAsync();

                 });
             };
             scanPage.OnScanResult += scanResultDelegate;
            await Navigation.PushAsync(scanPage);*/
            this.BindingContext = this;
           
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();

            zxing.IsScanning = true;
        }

        protected override void OnDisappearing()
        {
            zxing.IsScanning = false;

            base.OnDisappearing();
        }

        private void OnClickChangePrintType(object sender, EventArgs e)
        {
            
        }

        int count = 1;
        private void OnClickAddPrintBlock(object sender, EventArgs e)
        {
            count++;
            ((Button)sender).Text = $"{count}";
        }

        private void OnClickChangePrintColorType(object sender, EventArgs e)
        {
            if (PrintType == 0) PrintType = 1; else if (PrintType == 1) PrintType = 0;
        }

        private void OnClickPrintBlock(object sender, EventArgs e)
        {

        }
    }
}