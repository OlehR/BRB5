using BRB5.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private ObservableCollection<DocWaresEx> _ListWares;
        public ObservableCollection<DocWaresEx> ListWares { get { return _ListWares; } set { _ListWares = value; OnPropertyChanged("ListWares"); } }

        DocWaresEx _ScanData;
        public DocWaresEx ScanData { get { return _ScanData; } set { _ScanData = value; OnPropertyChanged("ScanData"); } }
        protected DB db = DB.GetDB();
        private Connector.Connector c;
        public TypeDoc TypeDoc { get; set; }
        public int OrderDoc { get; set; }

        public Scan(int pTypeDoc, DocId pDocId)
        {
            InitializeComponent();
            TypeDoc = Config.GetDocSetting(pTypeDoc);
            c = Connector.Connector.GetInstance();
            var tempListWares = db.GetDocWares(pDocId, 2, eTypeOrder.Scan);
            foreach (var t in tempListWares) { t.Ord = -1; }
            ListWares = tempListWares == null ? new ObservableCollection<DocWaresEx>(): new ObservableCollection<DocWaresEx>(tempListWares);
            OrderDoc = ListWares.Count > 0 ? ListWares.First().OrderDoc : 0;

            zxing.OnScanResult += (result) =>
                Device.BeginInvokeOnMainThread(async () =>
                // Stop analysis until we navigate away so we don't keep reading barcodes
                {
                    zxing.IsAnalyzing = false;

                    ScanData = db.GetScanData(pDocId, c.ParsedBarCode(result.Text, true/*?*/));
                    _ = FindWareByBarCodeAsync(result.Text);

                    //QuantityBarCode = ScanData.QuantityBarCode;

                    ScanData.BeforeQuantity = CountBeforeQuantity(ScanData.CodeWares);

                    if (ScanData != null)
                    { 
                        inputQ.Text = "";
                        AddWare();
                    }
                    zxing.IsAnalyzing = true;
                });

            this.BindingContext = this;
        }

        private void AddWare()
        {
            inputQ.Unfocused += (object sender, FocusEventArgs e) =>
            {
                if (ScanData != null) {
                    if (!e.IsFocused && ScanData.InputQuantity == 0)
                        ((Entry)sender).Focus();
                    else
                    {
                        ScanData.Quantity = ScanData.InputQuantity;
                        ScanData.OrderDoc = ++OrderDoc;
                        ScanData.Ord = -1;
                        if (db.ReplaceDocWares(ScanData))
                        {
                            ListWares.Insert(0, ScanData);
                            foreach (var ware in ListWares){
                                if (ware.CodeWares == ScanData.CodeWares)ware.Ord = -1;}

                            ScanData = null;
                        }
                    }
                }
            };

        }

        public decimal CountBeforeQuantity(int pCodeWares)
        {
            decimal res= 0;
            if(ListWares.Count() > 0) 
            { 
                foreach(var ware in ListWares)
                {
                    ware.Ord = -1;
                    if (ware.CodeWares == pCodeWares)
                    {
                        res += ware.InputQuantity;
                        ware.Ord = 0;
                    }
                }
            }
            return res;
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

        private void Reset(object sender, EventArgs e)
        {
            if (ScanData != null && ListWares.Count() > 0)
            {
                foreach (var ware in ListWares)
                {
                    if (ware.CodeWares == ScanData.CodeWares)
                    {
                        ware.QuantityOld = ware.InputQuantity;
                        ware.InputQuantity = 0;
                        ware.Quantity = 0; 
                        db.ReplaceDocWares(ware);
                    }
                }

            }
        }

        private void CalcQuantity(object sender, TextChangedEventArgs e)
        {
            if (ScanData != null && ScanData.QuantityBarCode == 0)
                ScanData.QuantityBarCode = ScanData.InputQuantity * ScanData.Coefficient;
        }
    }
}