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
using System.Collections.ObjectModel;

namespace BRB5.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PlanCheckerPrice : ContentPage
    {
        ZXingScannerView zxing;
        public bool IsVisScan { get { return Config.TypeScaner == eTypeScaner.Camera; } }
        public ObservableCollection<WaresPrice> WaresList { get; set; }

        public PlanCheckerPrice(DocId pDocId, int Selection)
        {

            NavigationPage.SetHasNavigationBar(this, Device.RuntimePlatform == Device.iOS);
            InitializeComponent();

            //
            WaresList = new ObservableCollection<WaresPrice>
            {
                new WaresPrice(){ Name="Coca Cola", CodeWares=119132},
                new WaresPrice(){ Name="Fanta", CodeWares=119131},
                new WaresPrice(){ Name="Артек Вафлі", CodeWares=69514},
                new WaresPrice(){ Name="Печево Марія", CodeWares=66992},
                new WaresPrice(){ Name="Молочний шоколад Рошен", CodeWares=157715},
                new WaresPrice(){ Name="Печево до кави", CodeWares=149965},
                new WaresPrice(){ Name="1Coca Cola", CodeWares=1119132},
                new WaresPrice(){ Name="1Fanta", CodeWares=1119131},
                new WaresPrice(){ Name="1Артек Вафлі", CodeWares=169514},
                new WaresPrice(){ Name="1Печево Марія", CodeWares=166992},
                new WaresPrice(){ Name="1Молочний шоколад Рошен", CodeWares=1157715},
                new WaresPrice(){ Name="1Печево до кави", CodeWares=1149965}
            };
            //

            this.BindingContext = this;
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