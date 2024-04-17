using BRB5.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;
using Xamarin.Essentials;
using BRB5.View;
using BRB5.ViewModel;
using BL;
using Utils;

//using BRB5.Connector;
namespace BRB5
{
    public partial class PriceCheck : IDisposable
    {
        DB db = DB.GetDB();
        BL.BL bl = BL.BL.GetBL();

        public List<PrintBlockItems> ListPrintBlockItems { get { return db.GetPrintBlockItemsCount().ToList(); }  }

        public int SelectedPrintBlockItems { get { return ListPrintBlockItems.Count > 0 ? ListPrintBlockItems.Last().PackageNumber : -1; }  }
        public bool IsVisPriceNormal { get { return WP != null && (WP.PriceOld != WP.PriceNormal); } }
        public bool IsVisPriceOpt { get { return WP != null && (WP.PriceOpt != 0 || WP.PriceOptOld != 0); } }

        public bool IsVisF4 { get { return Config.Company == eCompany.Sim23; } }
        public string F4Text { get { return IsOnline ? "OnLine" : "OffLine"; } }
        private bool _IsOnline = true;
        public bool IsOnline { get { return _IsOnline; } set { _IsOnline = value; OnPropertyChanged("F4Text"); } }

        bool _IsVisRepl = false;
        public bool IsVisRepl { get { return _IsVisRepl; } set { _IsVisRepl = value; OnPropertyChanged("IsVisRepl"); } }
        public bool IsSoftKeyboard { get { return Config.IsSoftKeyboard; } }
        double _PB = 0;
        public double PB { get { return _PB; } set { _PB = value; OnPropertyChanged("PB"); } }

        WaresPrice _WP;
        public WaresPrice WP { get { return _WP; } set { _WP = value; OnPropertyChanged("WP"); OnPropertyChanged("TextColorPrice");
                OnPropertyChanged("IsVisPriceOpt"); OnPropertyChanged(nameof(IsVisPriceNormal)); OnPropertyChanged("TextColorHttp"); } }
        ZXingScannerView zxing;
        //ZXingDefaultOverlay overlay;

        int _PrintType = 0;//Колір чека 0-звичайний 1-жовтий, -1 не розділяти.        
        public int PrintType { get { return _PrintType; } set { _PrintType = value; OnPropertyChanged("PrintType"); OnPropertyChanged("ColorPrintColorType"); } }
        public bool IsEnabledPrint { get { return Config.TypeUsePrinter != eTypeUsePrinter.NotDefined; } }
        /// <summary>
        /// Номер сканування цінників за день !!!TMP Треба зберігати в базі.
        /// </summary>
        int LineNumber = 0;
        public int AllScan { get; set; } = 0;
        public int BadScan { get; set; } = 0;
        /// <summary>
        /// Номер пакета цінників за день !!!TMP Треба зберігати в базі.
        /// </summary>
        int _PackageNumber = 1;
        public int PackageNumber { get { return _PackageNumber; } set { _PackageNumber = value; OnPropertyChanged(nameof(PackageNumber)); OnPropertyChanged(nameof(ListPrintBlockItems)); OnPropertyChanged(nameof(SelectedPrintBlockItems)); } }


        //public int ColorPrintColorType() { return Color.parseColor(HttpState != eStateHTTP.HTTP_OK ? "#ffb3b3" : (PrintType == 0 ? "#ffffff" : "#3fffff00")); }

        public string ColorPrintColorType { get { return PrintType == 0 ? "#ffffff" : PrintType == 1 ? "#ffffa8" : "#ffffff"; } }
        public string TextColorPrice { get {return (WP != null && WP.Price != 0 && WP.Price == WP.PriceOld && WP.PriceOpt == WP.PriceOptOld) ? "#009800" : "#ff5c5c"; } set { OnPropertyChanged(nameof(TextColorPrice)); } }
        public string TextColorHttp { get { return (WP != null && WP.StateHTTP == eStateHTTP.HTTP_OK) ? "#009800" : "#ff5c5c"; } }

        public bool _IsMultyLabel = false;
        public bool IsMultyLabel { get { return _IsMultyLabel; } set { _IsMultyLabel = value; OnPropertyChanged(nameof(IsMultyLabel)); OnPropertyChanged(nameof(F5Text)); } }
        public string F5Text { get { return IsMultyLabel ? "Дубл." : "Унік."; } }
        public bool IsVisScan { get { return Config.TypeScaner == eTypeScaner.Camera; } }
        private string CurrentEntry = "BarcodeEntry";
        /// <summary>
        /// 0 - нічого , 1 - сканований цінник, 2 - сканований товар, 3 - штрихкод товату не підходить, 4 - цінник не підходить, 5 - успішно
        /// </summary>
        private eCheckWareScaned _IsWareScaned = eCheckWareScaned.Nothing;
        public eCheckWareScaned IsWareScaned { get { return _IsWareScaned; } set { _IsWareScaned = value; OnPropertyChanged(nameof(ColorDoubleScan)); OnPropertyChanged(nameof(IsWareScaned)); OnPropertyChanged(nameof(ButtonDoubleScan)); } }
        public bool IsVisDoubleScan { get; set; }
        public bool IsVisBarcode { get { return !IsVisDoubleScan; } }
        private string _MessageDoubleScan;
        public string MessageDoubleScan {  get { return _MessageDoubleScan; } set { _MessageDoubleScan = value; OnPropertyChanged(nameof(MessageDoubleScan)); } }
        public string ButtonDoubleScan { get { return IsWareScaned == eCheckWareScaned.Nothing || IsWareScaned == eCheckWareScaned.Success ? "" :  IsWareScaned == eCheckWareScaned.WareScaned || IsWareScaned == eCheckWareScaned.PriceTagNotFit ? "Відсутній ціник" : "Відсутній товар"; } }
        public string ColorDoubleScan { get { return IsWareScaned == eCheckWareScaned.Success ? "#C5FFC4" : IsWareScaned == eCheckWareScaned.WareNotFit || IsWareScaned== eCheckWareScaned.PriceTagNotFit ? "#FFC4C4" : 
                                                     IsWareScaned == eCheckWareScaned.PriceTagScaned || IsWareScaned == eCheckWareScaned.WareScaned ? "#FEFFC4" : "#FFFFFF"; } }
        
        public PriceCheck(TypeDoc pTypeDoc)
        {
            InitializeComponent();            
            var r = db.GetCountScanCode();
            IsVisDoubleScan = pTypeDoc.CodeDoc == 15;

            if (Config.TypeUsePrinter == eTypeUsePrinter.StationaryWithCutAuto) PrintType = -1;

            NavigationPage.SetHasNavigationBar(this, Device.RuntimePlatform == Device.iOS);

            if (r != null)
            {
                AllScan = r.AllScan;
                BadScan = r.BadScan;
                LineNumber = r.LineNumber;
                PackageNumber = r.PackageNumber;
            }
            if (!IsVisScan)
             Config.BarCode = BarCode;

            Config.OnProgress += (pProgress) => Device.BeginInvokeOnMainThread(() => PB = pProgress);
            this.BindingContext = this;
        }

        void BarCode(string pBarCode)=>FoundWares(pBarCode, false);

        void FoundWares(string pBarCode, bool pIsHandInput = false)
        {
            LineNumber++;
            Config.OnProgress?.Invoke(0.2d);
            
            (WP, MessageDoubleScan) = bl.FoundWares(pBarCode, PackageNumber, LineNumber, pIsHandInput, IsVisDoubleScan, IsOnline);

            if (WP != null)
            {
                AllScan++;
                if (!WP.IsPriceOk)
                    BadScan++;
                IsWareScaned = WP.StateDoubleScan;
            }
            var duration = TimeSpan.FromMilliseconds(WP?.IsPriceOk == true ? 50 : 250);
            Vibration.Vibrate(duration);

            Config.OnProgress?.Invoke(0.9d);
            if (!IsVisScan)
            {
                BarCodeInput.Focus();
                BarCodeFocused(null, null);
            }

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (!IsSoftKeyboard)
            {
                MessagingCenter.Subscribe<KeyEventMessage>(this, "F1Pressed", message => { OnClickPrintBlock(null, EventArgs.Empty); });
                MessagingCenter.Subscribe<KeyEventMessage>(this, "F2Pressed", message => { OnF2(null, EventArgs.Empty); });
                MessagingCenter.Subscribe<KeyEventMessage>(this, "F4Pressed", message => { OnF4(null, EventArgs.Empty); });
                MessagingCenter.Subscribe<KeyEventMessage>(this, "F5Pressed", message => { OnF5(null, EventArgs.Empty); });
                MessagingCenter.Subscribe<KeyEventMessage>(this, "F6Pressed", message => { OnClickPrintOne(null, EventArgs.Empty); });
                MessagingCenter.Subscribe<KeyEventMessage>(this, "BackPressed", message => { KeyBack(); });
                MessagingCenter.Subscribe<KeyEventMessage>(this, "EnterPressed", message => { KeyEnter(); });
            }
            if (IsVisScan)
            {
                zxing = ZxingBRB5.SetZxing(GridZxing, zxing, (BarCode) => FoundWares(BarCode));
                zxing.IsScanning = true;
                zxing.IsAnalyzing = true;
            }
            if (!IsVisScan) BarCodeInput.Focus();
            if (IsVisDoubleScan) MessageDoubleScan = "Скануйте цінник чи товар";
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (IsVisScan) zxing.IsScanning = false;

            if (!IsSoftKeyboard)
            {
                MessagingCenter.Unsubscribe<KeyEventMessage>(this, "F1Pressed");
                MessagingCenter.Unsubscribe<KeyEventMessage>(this, "F2Pressed");
                MessagingCenter.Unsubscribe<KeyEventMessage>(this, "F4Pressed");
                MessagingCenter.Unsubscribe<KeyEventMessage>(this, "F5Pressed");
                MessagingCenter.Unsubscribe<KeyEventMessage>(this, "F6Pressed");
                MessagingCenter.Unsubscribe<KeyEventMessage>(this, "BackPressed");
                MessagingCenter.Unsubscribe<KeyEventMessage>(this, "EnterPressed");
            }
            bl.SendLogPrice();
        }

        public void Dispose()
        {           
            Config.BarCode -= BarCode;
        }
        
        private void OnClickAddPrintBlock(object sender, EventArgs e)
        {
            PackageNumber++;
            ListPrintBlockItems.Add(new PrintBlockItems() { PackageNumber = PackageNumber });
        }

        private void OnClickPrintBlock(object sender, EventArgs e)
        {
            var temp = PrintBlockItemsXaml.SelectedItem as PrintBlockItems;
            if (IsEnabledPrint) 
                _ = DisplayAlert("Друк", bl.PrintPackage(PrintType, temp.PackageNumber, IsMultyLabel), "OK");
        }

        private void OnF2(object sender, EventArgs e)
        {
            IsVisRepl = !IsVisRepl;
            if(IsVisRepl) NumberOfReplenishment.Focus();
        }

        private void OnF4(object sender, EventArgs e)
        {
            IsOnline = !IsOnline;
        }

        private void OnF5(object sender, EventArgs e)
        {            
            IsMultyLabel = !IsMultyLabel;
        }

        private async void OnClickWareInfo(object sender, EventArgs e)
        {            
            if (WP != null)
            {
                if (IsVisScan) zxing.IsAnalyzing = false;
                await Navigation.PushAsync(new WareInfo(WP.ParseBarCode));
            }
        }

        private void BarCodeHandInput(object sender, EventArgs e)
        {
            var text = BarCodeInput.Text;
            FoundWares(text, true);
        }

        private void BarCodeFocused(object sender, FocusEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                BarCodeInput.CursorPosition = 0;
                BarCodeInput.SelectionLength = BarCodeInput.Text == null ? 0 : BarCodeInput.Text.Length;
            });
        }

        private void OnClickPrintOne(object sender, EventArgs e)
        {
            if (IsEnabledPrint && WP!=null)
                _ = DisplayAlert("Друк", bl.c.PrintHTTP(new[] { WP.CodeWares }), "OK");
        }
    
        private async void KeyBack()
        {
            await Navigation.PopAsync();    
        }

        private void EntryTextChanged(object sender, TextChangedEventArgs e)
        {
            CurrentEntry = (sender as Entry).AutomationId;
        }
        private void KeyEnter()
        {
            if (IsVisDoubleScan)
                //TEMP!!
                return;
            switch (CurrentEntry)
            {
                case "BarcodeEntry":
                    BarCodeHandInput(null,null);
                    break;
                case "ReplenishmentEntry":
                    OnUpdateReplenishment(null, null);
                    break;
            }
        }

        private void OnUpdateReplenishment(object sender, EventArgs e)
        {
            decimal d;
            if (decimal.TryParse(NumberOfReplenishment.Text, out d))
                db.UpdateReplenishment(LineNumber, d);
        }

        private void DoubleScanReact(object sender, EventArgs e)
        {

            if(IsWareScaned==eCheckWareScaned.PriceTagScaned|| IsWareScaned == eCheckWareScaned.WareNotFit)//Відсутній товар
            {
                bl.SaveDoubleScan(102, WP, PackageNumber, LineNumber);
                WP = null;
                IsWareScaned = eCheckWareScaned.Nothing;
                MessageDoubleScan = "Скануйте цінник чи товар";
            }
            else if (IsWareScaned == eCheckWareScaned.WareScaned || IsWareScaned == eCheckWareScaned.PriceTagNotFit)//Відсутній ціник
            {
                bl.SaveDoubleScan(101, WP, PackageNumber, LineNumber);
                WP = null;
                IsWareScaned = eCheckWareScaned.Nothing;
                MessageDoubleScan = "Скануйте цінник чи товар";
            }

        }
    }
}