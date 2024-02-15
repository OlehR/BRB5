using BRB5.Model;
using BRB5.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace BRB5.View
{
    public partial class DocScan
    {
        private ObservableCollection<DocWaresEx> _ListWares;
        public ObservableCollection<DocWaresEx> ListWares { get { return _ListWares; } set { _ListWares = value; OnPropertyChanged("ListWares"); } }

        DocWaresEx _ScanData;
        public DocWaresEx ScanData { get { return _ScanData; } set { _ScanData = value; OnPropertyChanged("ScanData"); } }
        protected DB db = DB.GetDB();
        private Connector.Connector c;
        public TypeDoc TypeDoc { get; set; }
        public int OrderDoc { get; set; }
        public bool IsSoftKeyboard { get { return Config.IsSoftKeyboard; } }
        public bool IsVisScan { get { return Config.TypeScaner == eTypeScaner.Camera; } }
        private DocId DocId;
        private bool _IsVisQ = false;
        public bool IsVisQ { get { return _IsVisQ; } set { _IsVisQ = value; OnPropertyChanged(nameof(IsVisQ)); } }
        private bool _IsVisQOk = false;
        public bool IsVisQOk { get { return _IsVisQOk; } set { _IsVisQOk = value; OnPropertyChanged(nameof(IsVisQOk)); } }
        private string _DisplayQuestion;
        public string DisplayQuestion { get { return _DisplayQuestion; } set { _DisplayQuestion = value; OnPropertyChanged(nameof(DisplayQuestion)); } }
        private string TempBarcode;
        ZXingScannerView zxing;
        public DocScan(DocId pDocId, TypeDoc pTypeDoc = null)
        {
            InitializeComponent();
            DocId = pDocId;
            TypeDoc = pTypeDoc!=null? pTypeDoc:Config.GetDocSetting(pDocId.TypeDoc);
            c = Connector.Connector.GetInstance();
            var tempListWares = db.GetDocWares(pDocId, 2, eTypeOrder.Scan);
            foreach (var t in tempListWares) { t.Ord = -1; }
            ListWares = tempListWares == null ? new ObservableCollection<DocWaresEx>(): new ObservableCollection<DocWaresEx>(tempListWares);
            OrderDoc = ListWares.Count > 0 ? ListWares.First().OrderDoc : 0;
            NavigationPage.SetHasNavigationBar(this, Device.RuntimePlatform == Device.iOS || Config.TypeScaner == eTypeScaner.BitaHC61 || Config.TypeScaner == eTypeScaner.Zebra || Config.TypeScaner == eTypeScaner.PM550 || Config.TypeScaner == eTypeScaner.PM351);
            this.BindingContext = this;
        }
        void BarCode(string pBarCode)
        {
            ScanData = db.GetScanData(DocId, c.ParsedBarCode(pBarCode, true/*?*/));
            FindWareByBarCodeAsync(pBarCode);

            if (ScanData != null)
            {
                ScanData.BarCode = pBarCode;

                if (ScanData.QuantityBarCode > 0)
                    ScanData.InputQuantity = ScanData.QuantityBarCode;
                else
                    inputQ.Text = "";

                inputQ.Focus();
                AddWare();
            }
        }
        public void Dispose()
        {
            Config.BarCode -= BarCode;
        }
        private void AddWare()
        {
            if (ScanData != null)
            {
                if (ScanData.InputQuantity > 0)
                {
                    ScanData.Quantity = ScanData.InputQuantity;
                    ScanData.OrderDoc = ++OrderDoc;
                    ScanData.Ord = -1;
                    if (db.ReplaceDocWares(ScanData))
                    {
                        ListWares.Insert(0, ScanData);
                        foreach (var ware in ListWares)
                        {
                            if (ware.CodeWares == ScanData.CodeWares) ware.Ord = -1;
                        }
                        ScanData = null;
                    }
                    inputQ.Unfocus();
                }
            }
        }
        private void UnfocusedInputQ(object sender, FocusEventArgs e)
        {
            if (ScanData != null)
            {
                if (!inputQ.IsFocused && ScanData.InputQuantity == 0)
                    inputQ.Focus();
                else
                    AddWare();
            }
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
            if (IsVisScan)
            {
                zxing = ZxingBRB5.SetZxing(GridZxing, zxing, (Barcode) => BarCode(Barcode));
                zxing.IsScanning = true;
                zxing.IsAnalyzing = true;
            }
            else Config.BarCode = BarCode;
            if (!IsSoftKeyboard)
            {
                MessagingCenter.Subscribe<KeyEventMessage>(this, "F1Pressed", message => { Reset(null, EventArgs.Empty); });
                MessagingCenter.Subscribe<KeyEventMessage>(this, "F2Pressed", message => { });
                MessagingCenter.Subscribe<KeyEventMessage>(this, "F3Pressed", message => { });
                MessagingCenter.Subscribe<KeyEventMessage>(this, "F8Pressed", message => { });
                MessagingCenter.Subscribe<KeyEventMessage>(this, "BackPressed", message => { KeyBack(); });
                MessagingCenter.Subscribe<KeyEventMessage>(this, "EnterPressed", message => { AddWare(); });
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (IsVisScan) zxing.IsScanning = false;

            if (!IsSoftKeyboard)
            {
                MessagingCenter.Unsubscribe<KeyEventMessage>(this, "F1Pressed");
                MessagingCenter.Unsubscribe<KeyEventMessage>(this, "F2Pressed");
                MessagingCenter.Unsubscribe<KeyEventMessage>(this, "F3Pressed");
                MessagingCenter.Unsubscribe<KeyEventMessage>(this, "F8Pressed");
                MessagingCenter.Unsubscribe<KeyEventMessage>(this, "BackPressed");
                MessagingCenter.Unsubscribe<KeyEventMessage>(this, "EnterPressed");
            }
        }

        public void FindWareByBarCodeAsync(string BarCode)
        {
            if(ScanData == null)
            {
                DisplayQuestion = "Товар не знайдено \n"+ "Даний штрихкод=> " + BarCode + " відсутній в базі";
                IsVisQ = true;
            }
            else
            {
                if (!ScanData.IsRecord)
                {
                    if (TypeDoc.TypeControlQuantity == eTypeControlDoc.Ask)
                    {
                        DisplayQuestion = "Добавити відсутній товар? \n" + ScanData.NameWares;
                        IsVisQ = true;
                        IsVisQOk = true;
                        TempBarcode = BarCode;
                        return;
                    }
                    if (TypeDoc.TypeControlQuantity == eTypeControlDoc.Control)
                    {
                        DisplayQuestion = "Товар відсутній в документі \n" + ScanData.NameWares;
                        IsVisQ = true;
                        ScanData = null;
                        return;
                    }
                }

                ScanData.BeforeQuantity = CountBeforeQuantity(ScanData.CodeWares);

                if (TypeDoc.IsSimpleDoc)
                {
                    if (ScanData.BeforeQuantity > 0)
                    {
                        DisplayQuestion = "Вже добавлено в документ! \n" + ScanData.NameWares;
                        IsVisQ = true;
                        return;
                    }
                }
                if (IsVisScan)  zxing.IsAnalyzing = true;
            }

            return;
        }

        private void Reset(object sender, EventArgs e)
        {
            if (ScanData != null && ListWares.Count() > 0)
            {
                foreach (var ware in ListWares)
                {
                    if (ware.CodeWares == ScanData.CodeWares&& ware.InputQuantity!=0)
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

        private void OkClicked(object sender, EventArgs e)      
        {
            IsVisQ = false;
            IsVisQOk = false;
            if (TypeDoc.TypeControlQuantity == eTypeControlDoc.Ask)
            {
                ScanData.IsRecord = true;
                ScanData.QuantityMax = decimal.MaxValue;
                FindWareByBarCodeAsync(TempBarcode);
            }

            if (IsVisScan) zxing.IsAnalyzing = true;
        }

        private void CancelClicked(object sender, EventArgs e)
        {
            IsVisQ = false;
            IsVisQOk = false;
            if (TypeDoc.TypeControlQuantity == eTypeControlDoc.Ask) ScanData = null;

            if (IsVisScan) zxing.IsAnalyzing = true;
        }
        private async void KeyBack()
        {
            await Navigation.PopAsync();
        }

    }
}