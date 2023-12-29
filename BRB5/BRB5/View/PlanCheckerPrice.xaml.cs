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
using static SQLite.SQLite3;

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
                    item.OrderDoc = i++; }
                               
                db.ReplaceDocWaresSample(temp.Info.Select(el=> new DocWaresSample(el)));

                WaresList = new ObservableCollection<DocWaresEx>(db.GetDocWares(Doc, 1, eTypeOrder.Name));
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
        private void WareFocus(string pBarCode)
        {
            var parseBarCode = c.ParsedBarCode(pBarCode, true);
            var temp = db.GetScanData(Doc, parseBarCode);

            var tempSelected = WaresList.FirstOrDefault(item => item.CodeWares == temp.CodeWares);
            Device.BeginInvokeOnMainThread(async () =>
            {
                if (tempSelected != null)
                {
                    ListWares.ScrollTo(tempSelected, ScrollToPosition.Start, false);
                    var index = WaresList.IndexOf(tempSelected);
                    var list = ListWares.TemplatedItems.ToList();
                    var Scaned = (((list[index] as ViewCell).View as Grid).Children.ElementAt(2) as Frame).Content as Entry;
                    await Task.Delay(10);
                    Scaned.Focus();
                }
                else _ = DisplayAlert("", "Товар відсутній", "ok");
            });
            
        }

        private void Save(object sender, EventArgs e)
        {
            var res = c.SendDocsData(Doc, WaresList);
            _ = DisplayAlert("Збереження", res.TextError, "ok");            
        }

        private void SaveItem(object sender, EventArgs e)
        {
            var temp = sender as Entry;
            var codeWares = temp.AutomationId;

            var tempSelected = WaresList.FirstOrDefault(item => item.CodeWares.ToString() == codeWares);
            tempSelected.Quantity = tempSelected.InputQuantity;

            db.ReplaceDocWares(tempSelected);
        }
    }
}