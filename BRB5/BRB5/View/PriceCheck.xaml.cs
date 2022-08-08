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
        WaresPrice _WP;
        public WaresPrice WP { get { return _WP; } set { _WP = value; OnPropertyChanged("WP"); } }
        //ZXingScannerView zxing;
        //ZXingDefaultOverlay overlay;
        public PriceCheck()
        {
            InitializeComponent();

            c = Connector.Connector.GetInstance();
            
            zxing.OnScanResult += (result) =>
                Device.BeginInvokeOnMainThread(async () =>
                {

                    // Stop analysis until we navigate away so we don't keep reading barcodes
                    zxing.IsAnalyzing = false;
                    //zxing.IsScanning = false;
                    // Show an alert
                    WP = c.GetPrice(c.ParsedBarCode(result.Text,true));
                    
                    //await DisplayAlert("Scanned Barcode", WP.Price+" " + WP.Name, "OK");
                    
                    zxing.IsAnalyzing = true;
                    //zxing.IsScanning = true;
                    // Navigate away
                    //await Navigation.PopAsync();
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

        private void OnClickAddPrintBlock(object sender, EventArgs e)
        {

        }

        private void OnClickChangePrintColorType(object sender, EventArgs e)
        {

        }

        private void OnClickPrintBlock(object sender, EventArgs e)
        {

        }
    }
}