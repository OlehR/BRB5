using BRB5.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing.QrCode.Internal;

namespace BRB5.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Scan : ContentPage
    {
        public List<DocWaresEx> ListWares { get; set; }
        
        DocWaresEx _ScanData;
        public DocWaresEx ScanData { get { return _ScanData; } set { _ScanData = value; OnPropertyChanged("ScanData"); } }
        protected DB db = DB.GetDB();
        private Connector.Connector c;
        public TypeDoc TypeDoc { get; set; }
        public Scan(int pTypeDoc, DocId pDocId)
        {
            InitializeComponent();
            TypeDoc = Config.GetDocSetting(pTypeDoc);
            c = Connector.Connector.GetInstance();

            zxing.OnScanResult += (result) =>
                Device.BeginInvokeOnMainThread(async () =>
                // Stop analysis until we navigate away so we don't keep reading barcodes
                {
                    zxing.IsAnalyzing = false;

                    ScanData = db.GetScanData(pDocId, c.ParsedBarCode(result.Text, true/*?*/));
                    _ = FindWareByBarCodeAsync(result.Text);

                    zxing.IsAnalyzing = true;
                });

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
        public async Task FindWareByBarCodeAsync(string BarCode)
        {
            if(ScanData == null)
            {
                _ = DisplayAlert("Товар не знайдено", "Даний штрихкод=> " + BarCode + " відсутній в базі", "OK");
            }
            else
            {
                if (!ScanData.IsRecord)
                {
                    if (TypeDoc.TypeControlQuantity == eTypeControlDoc.Ask)
                    {
                        if (await DisplayAlert("Добавити відсутній товар?", ScanData.NameWares, "OK", "Cancel"))
                        {
                            ScanData.IsRecord = true;
                            ScanData.QuantityMax = decimal.MaxValue;
                            _ = FindWareByBarCodeAsync(BarCode);
                        }
                        return;
                    }
                    if (TypeDoc.TypeControlQuantity == eTypeControlDoc.Control)
                    {
                        _ = DisplayAlert("Товар відсутній в документі", ScanData.NameWares, "OK");
                        //? ScanData = null;
                        return;
                    }
                }

                /*
                   WaresItem.BeforeQuantity = CountBeforeQuantity(ListWares, WaresItem.CodeWares);
                */
                if (TypeDoc.IsSimpleDoc)
                {
                    if (ScanData.BeforeQuantity > 0)
                    {
                        _ = DisplayAlert("Вже добавлено в документ!", ScanData.NameWares, "OK");
                        return;
                    }
                    //ScanData.QuantityBarCode = 1;
                    //!!!TMP
                }              

            }

            if (ScanData.QuantityBarCode > 0) ;
               // inputCount.setText(Double.toString(WaresItem.QuantityBarCode));
               //!!!
            //SetAlert(WaresItem.CodeWares);
            //!!!
            return;
        }
    }
}