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
        Connector.Connector c;
        protected DB db = DB.GetDB();
        ZXingScannerView zxing;
        private Doc Doc;
        public bool IsVisScan { get { return Config.TypeScaner == eTypeScaner.Camera; } }
        public ObservableCollection<DocWaresEx> WaresList { get; set; }

        public PlanCheckerPrice(Doc pDoc, int Selection)
        {
            Doc = pDoc;
            c = Connector.Connector.GetInstance();
            NavigationPage.SetHasNavigationBar(this, Device.RuntimePlatform == Device.iOS);
            InitializeComponent();

            GetData();           

            if (!IsVisScan)
                Config.BarCode = BarCode;
            this.BindingContext = this;
        }

        private void GetData()
        {
            var temp = c.GetPromotionData(Doc.NumberDoc);
            if (temp.Info == null)
            {
                WaresList = new ObservableCollection<DocWaresEx>();
                _ = DisplayAlert("Помилка", temp.TextError, "OK");
            }
            else
            {
                int i=0;
                foreach (var item in temp.Info) {
                    item.TypeDoc = 13;
                    item.OrderDoc = i++;
                    db.ReplaceDocWares(item); }
                WaresList = new ObservableCollection<DocWaresEx>(db.GetDocWares(Doc, 2, eTypeOrder.Name));
            }            
        }

        void BarCode(string pBarCode)
        {
            WareFocus(pBarCode);
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

        public void Dispose()
        {
            Config.BarCode -= BarCode;
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
                    WareFocus(result.Text);
                    pZxing.IsAnalyzing = true;
                });
            return pZxing;
        }
        void WareFocus(string pBarCode)
        {

        }
    
    }
}