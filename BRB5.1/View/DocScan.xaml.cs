using BL;
using BL.Connector;
using BRB5.Model;
using System.Collections.ObjectModel;
using BRB5;
using BarcodeScanning;
#if ANDROID
using Android.Views;
#endif

namespace BRB6.View
{
    public partial class DocScan
    {
        private ObservableCollection<DocWaresEx> _ListWares;
        public ObservableCollection<DocWaresEx> ListWares { get { return _ListWares; } set { _ListWares = value; OnPropertyChanged("ListWares"); } }

        DocWaresEx _ScanData;
        public DocWaresEx ScanData { get { return _ScanData; } set { _ScanData = value; OnPropertyChanged("ScanData"); } }
        protected DB db = DB.GetDB();
        private Connector c;
        BL.BL Bl = BL.BL.GetBL();
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

        CameraView BarcodeScaner;
        //ZXingScannerView zxing;
        public DocScan(DocId pDocId, TypeDoc pTypeDoc = null)
        {
            InitializeComponent();
            NokeyBoard();
            DocId = pDocId;
            TypeDoc = pTypeDoc != null ? pTypeDoc : Config.GetDocSetting(pDocId.TypeDoc);
            c = ConnectorBase.GetInstance();
            var tempListWares = db.GetDocWares(pDocId, 2, eTypeOrder.Scan);
            foreach (var t in tempListWares) { t.Ord = -1; }
            ListWares = tempListWares == null ? new ObservableCollection<DocWaresEx>() : new ObservableCollection<DocWaresEx>(tempListWares);
            OrderDoc = ListWares.Count > 0 ? ListWares.First().OrderDoc : 0;
            if (ListWares.Count > 0)  ListViewWares.SelectedItem = ListWares[0];
            // TODO Xamarin.Forms.Device.RuntimePlatform is no longer supported. Use Microsoft.Maui.Devices.DeviceInfo.Platform instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
            NavigationPage.SetHasNavigationBar(this, Device.RuntimePlatform == Device.iOS || Config.TypeScaner == eTypeScaner.BitaHC61 || Config.TypeScaner == eTypeScaner.ChainwayC61 || Config.TypeScaner == eTypeScaner.Zebra || Config.TypeScaner == eTypeScaner.PM550 || Config.TypeScaner == eTypeScaner.PM351);
            this.BindingContext = this;
        }
        void BarCode(string pBarCode)
        {
            ScanData = db.GetScanData(DocId, c.ParsedBarCode(pBarCode, true/*?*/));
            FindWareByBarCodeAsync(pBarCode);
            if (ScanData != null)
            {
                ScanData.BarCode = pBarCode;
                if (ScanData.QuantityBarCode > 0) ScanData.InputQuantity = ScanData.QuantityBarCode;
                else inputQ.Text = "";

                inputQ.Focus();
                AddWare();
            }
        }
        public void Dispose() { Config.BarCode -= BarCode; }
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
                        ListViewWares.SelectedItem = ListWares[0];
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
        
        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (IsVisScan)
            {
                BarcodeScaner = new CameraView
                {
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    CameraEnabled = true,
                    VibrationOnDetected = false,
                    BarcodeSymbologies = BarcodeFormats.Ean13 | BarcodeFormats.Ean8 | BarcodeFormats.QRCode,

                };

                BarcodeScaner.OnDetectionFinished += CameraView_OnDetectionFinished;

                GridZxing.Children.Add(BarcodeScaner);
            }
            else Config.BarCode = BarCode;
            if (!IsSoftKeyboard)
            {
#if ANDROID
            MainActivity.Key+= OnPageKeyDown;
#endif
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (!IsSoftKeyboard)
            {
#if ANDROID
            MainActivity.Key-= OnPageKeyDown;
#endif
            }

            if (IsVisScan) BarcodeScaner.CameraEnabled = false;
        }

        public async void FindWareByBarCodeAsync(string BarCode)
        {
            if (ScanData == null)
            {
                await DisplayAlert("Товар не знайдено", "Даний штрихкод => " + BarCode + " відсутній в базі", "Ok");
            }
            else
            {
                if (!ScanData.IsRecord)
                {
                    if (TypeDoc.TypeControlQuantity == eTypeControlDoc.Ask)
                    {
                        bool addItem = await DisplayAlert("Додати відсутній товар?", ScanData.NameWares, "Ok", "Cancel");
                        TempBarcode = BarCode;
                        if (addItem)
                        {
                            ScanData.IsRecord = true;
                            ScanData.QuantityMax = decimal.MaxValue;
                            FindWareByBarCodeAsync(TempBarcode);
                        }
                        else ScanData = null;
                        return;
                    }
                    if (TypeDoc.TypeControlQuantity == eTypeControlDoc.Control)
                    {
                        await DisplayAlert("Товар відсутній в документі", ScanData.NameWares, "Ok");
                        ScanData = null;
                        return;
                    }
                }

                ScanData.BeforeQuantity = Bl.CountBeforeQuantity(ScanData.CodeWares, ListWares);

                if (TypeDoc.IsSimpleDoc)
                {
                    if (ScanData.BeforeQuantity > 0)
                    {
                        await DisplayAlert("Вже добавлено в документ!", ScanData.NameWares, "Ok");
                        return;
                    }
                }
            }
        }

        private void Reset(object sender, EventArgs e) { Bl.Reset(ScanData, ListWares); }

        private void CalcQuantity(object sender, TextChangedEventArgs e)
        {
            if (ScanData != null && ScanData.QuantityBarCode == 0)
                ScanData.QuantityBarCode = ScanData.InputQuantity * ScanData.Coefficient;
        }       

        private void Up(object sender, EventArgs e)
        {
            var selectedItem = (DocWaresEx)ListViewWares.SelectedItem;
            if (selectedItem != null)
            {
                var selectedIndex = ListWares.IndexOf(selectedItem);
                if (selectedIndex > 0)
                {
                    ListViewWares.SelectedItem = ListWares[selectedIndex - 1];
                    ListViewWares.ScrollTo(ListViewWares.SelectedItem, ScrollToPosition.Center, false);
                }
                OnPropertyChanged(nameof(ListWares));
            }
        }

        private void Down(object sender, EventArgs e)
        {
            var selectedItem = (DocWaresEx)ListViewWares.SelectedItem;
            if (selectedItem != null)
            {
                var selectedIndex = ListWares.IndexOf(selectedItem);

                if (selectedIndex < ListWares.Count - 1)
                {
                    ListViewWares.SelectedItem = ListWares[selectedIndex + 1];
                    ListViewWares.ScrollTo(ListViewWares.SelectedItem, ScrollToPosition.Center, false);
                }
                OnPropertyChanged(nameof(ListWares));
            }
        }

        private void CameraView_OnDetectionFinished(object sender, OnDetectionFinishedEventArg e)
        {
            if (e.BarcodeResults.Length > 0)
            {
                BarcodeScaner.PauseScanning = true;
                BarCode(e.BarcodeResults[0].DisplayValue);
                Task.Run(async () => {
                    await Task.Delay(1000);
                    BarcodeScaner.PauseScanning = false;
                });


            }
        }

        private void CompletedInputQ(object sender, EventArgs e) {  AddWare(); }
#if ANDROID
        public void OnPageKeyDown(Keycode keyCode, KeyEvent e)
        { 
           switch (keyCode)
           {
            case Keycode.F1:
               Reset(null, EventArgs.Empty);
               return;
            case Keycode.F2:
               Up(null, EventArgs.Empty);
               return;
            case Keycode.F3:
               Down(null, EventArgs.Empty);
               return;
            case Keycode.F8:

               return;
            default:
               return;
           }
         }
#endif
    }
}