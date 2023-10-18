using BRB5.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing.Mobile;
using ZXing;
using ZXing.Net.Mobile.Forms;

namespace BRB5.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PlanCheckerPrice : ContentPage
    {
        ZXingScannerView zxing;
        public bool IsVisScan { get { return Config.TypeScaner == eTypeScaner.Camera; } }


        public PlanCheckerPrice(DocId pDocId, int Selection)
        {

            NavigationPage.SetHasNavigationBar(this, Device.RuntimePlatform == Device.iOS);
            InitializeComponent();
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
        }
        protected override void OnDisappearing()
        {
            if (IsVisScan) zxing.IsScanning = false;
            base.OnDisappearing();
        }

        ZXingScannerView SetZxing(Grid pV, ZXingScannerView pZxing)
        {
            if (pZxing != null)
            {
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
                    //
                    //
                    pZxing.IsAnalyzing = true;
                });
            return pZxing;
        }
    }
}