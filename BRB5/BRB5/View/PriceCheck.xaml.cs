using BRB5.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing.Net.Mobile.Forms;
using Xamarin.Essentials;
//using BRB5.Connector;
namespace BRB5
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PriceCheck : ContentPage, INotifyPropertyChanged, IDisposable
{
    
        Connector.Connector c;    
        DB db = DB.GetDB();
        BL bl = BL.GetBL();

        public List<PrintBlockItems> ListPrintBlockItems { get { return db.GetPrintBlockItemsCount().ToList(); } }

        public int SelectedPrintBlockItems { get { return ListPrintBlockItems.Count>0? ListPrintBlockItems.Last().PackageNumber:-1; } }

        public bool IsVisPriceOpt { get { return WP != null && (WP.PriceOpt != 0 || WP.PriceOptOld != 0); }  }

        bool _IsVisF4 = false;
        public bool IsVisF4 { get { return _IsVisF4; } set { _IsVisF4 = value; OnPropertyChanged("IsVisF4"); } }

        bool _IsVisRepl = false;
        public bool IsVisRepl { get { return _IsVisRepl; } set { _IsVisRepl = value; OnPropertyChanged("IsVisRepl"); } }

        double _PB = 0;
        public double PB { get { return _PB; } set { _PB = value; OnPropertyChanged("PB"); } }

        WaresPrice _WP;
        public WaresPrice WP { get { return _WP; } set { _WP = value; OnPropertyChanged("WP"); OnPropertyChanged("TextColorPrice"); OnPropertyChanged("IsVisPriceOpt"); OnPropertyChanged("TextColorHttp"); } }
        //ZXingScannerView zxing;
        //ZXingDefaultOverlay overlay;

        int _PrintType = 0;//Колір чека 0-звичайний 1-жовтий, -1 не розділяти.        
        public int PrintType { get { return _PrintType; } set { _PrintType = value; OnPropertyChanged("PrintType"); OnPropertyChanged("NamePrintColorType"); OnPropertyChanged("ColorPrintColorType"); } }
        public bool IsEnabledPrintType { get { return Config.TypeUsePrinter != eTypeUsePrinter.NotDefined && Config.TypeUsePrinter != eTypeUsePrinter.StationaryWithCutAuto; }  }

        public bool IsOnline { get; set; } = true;

        /// <summary>
        /// Номер сканування цінників за день !!!TMP Треба зберігати в базі.
        /// </summary>
        int LineNumber = 0;

        int _PackageNumber = 1;

        public int AllScan { get; set; } = 0;
        public int BadScan { get; set; } = 0;
        /// <summary>
        /// Номер пакета цінників за день !!!TMP Треба зберігати в базі.
        /// </summary>
        public int PackageNumber { get { return _PackageNumber; } set { _PackageNumber = value; OnPropertyChanged("PackageNumber"); OnPropertyChanged("ListPrintBlockItems"); OnPropertyChanged("SelectedPrintBlockItems"); } } 


        //public int ColorPrintColorType() { return Color.parseColor(HttpState != eStateHTTP.HTTP_OK ? "#ffb3b3" : (PrintType == 0 ? "#ffffff" : "#3fffff00")); }
        public string NamePrintColorType { get { return PrintType == 0 ? "Звичайний" : PrintType == 1 ? "Жовтий" : "Авто"; } }//
        public string ColorPrintColorType { get { return PrintType == 0 ? "#ffffff" : PrintType == 1 ? "#ffffa8" : "#ffffff"; } }

        public string TextColorPrice { get { return (WP!=null && (WP.Price != WP.PriceOld || WP.Price == 0) && WP.PriceOpt != WP.PriceOptOld) ? "#009800" : "#ff5c5c";  } }

        public string TextColorHttp { get { return (WP != null && WP.StateHTTP==eStateHTTP.HTTP_OK) ? "#009800" : "#ff5c5c"; } }

        public bool _IsMultyLabel = false;
        public bool IsMultyLabel { get { return _IsMultyLabel; } set { _IsMultyLabel = value; OnPropertyChanged("IsMultyLabel"); OnPropertyChanged("F5Text"); } }

        public string F5Text { get { return IsMultyLabel  ? "Дублювати" : "Унікальні"; } }

        public PriceCheck()
        {
            InitializeComponent();
            c = Connector.Connector.GetInstance();
            var r = db.GetCountScanCode();

            if (Config.TypeUsePrinter == eTypeUsePrinter.StationaryWithCutAuto) PrintType = -1;


            if (r!=null)
            {
                AllScan = r.AllScan;
                BadScan = r.BadScan;
                LineNumber = r.LineNumber;
                PackageNumber = r.PackageNumber;
            }

            zxing.OnScanResult += (result) =>
                Device.BeginInvokeOnMainThread(async () =>
                // Stop analysis until we navigate away so we don't keep reading barcodes
                { zxing.IsAnalyzing = false;
                    FoundWares(result.Text, false);
                 zxing.IsAnalyzing = true;
                }
                );

            NumberOfReplenishment.Unfocused += (object sender, FocusEventArgs e) =>
            {
                decimal d;
                if (decimal.TryParse(((Entry)sender).Text, out d))
                    db.UpdateReplenishment(LineNumber, d);
            };

            Config.OnProgress += (pProgress)=>{ PB = pProgress; };
                //MainSL.Children.Add(zxing);
                
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
                this.BindingContext = this;           
        }
        

        void FoundWares(string pBarCode, bool pIsHandInput = false)
        {
            LineNumber++;
            PB = 0.2d;          
            
            if (IsOnline)
            {
                WP = c.GetPrice(c.ParsedBarCode(pBarCode, true));
                //if(WP.State>0)
            }
            else
            {
                var data = bl.GetWaresFromBarcode(0, null, pBarCode, pIsHandInput);
                WP = new WaresPrice(data);  
            }
            if (WP != null)
            {
                AllScan++;
                if (!WP.IsPriceOk)
                    BadScan++;
            }
            var duration = TimeSpan.FromMilliseconds(WP?.IsPriceOk==true?50:250);
            Vibration.Vibrate(duration);
            //if (Config.Company == eCompany.Sim23)
             //   utils.PlaySound();
            var l = new LogPrice(WP, IsOnline, PackageNumber, LineNumber);
            db.InsLogPrice(l);
           
            PB = 1;
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

        public void Dispose()
        {
            bl.SendLogPrice();
        }
                      
        private void OnClickAddPrintBlock(object sender, EventArgs e)
        {
            PackageNumber++;
            ListPrintBlockItems.Add(new PrintBlockItems() { PackageNumber = PackageNumber });
        }

        private void OnClickChangePrintColorType(object sender, EventArgs e)
        {
            if (PrintType == 0) PrintType = 1; else if (PrintType == 1) PrintType = 0;
        }

        private void OnClickPrintBlock(object sender, EventArgs e)
        {

        }
        
        private void OnF2(object sender, EventArgs e)
        {
            IsVisRepl = !IsVisRepl;
        }

        private void OnF4(object sender, EventArgs e)
        {

        }

        private void OnF5(object sender, EventArgs e)
        {
            IsVisRepl = !IsVisRepl;
        }

    }
}