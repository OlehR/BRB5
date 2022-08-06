using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing.Net.Mobile.Forms;

namespace BRB5
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PriceCheck : ContentPage
    {
        //ZXingScannerView zxing;
        //ZXingDefaultOverlay overlay;
        public PriceCheck()
        {
            InitializeComponent();
            
            zxing.OnScanResult += (result) =>
                Device.BeginInvokeOnMainThread(async () =>
                {

                    // Stop analysis until we navigate away so we don't keep reading barcodes
                    zxing.IsAnalyzing = false;
                    //zxing.IsScanning = false;
                    // Show an alert
                    await DisplayAlert("Scanned Barcode", result.Text, "OK");

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