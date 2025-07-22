using BRB5.Model;
using BL; 
using BRB5;
using UtilNetwork;

namespace PriceChecker.View;

public partial class UPriceChecker : ContentPage
{
	

    DB db = DB.GetDB();
    BL.BL bl = BL.BL.GetBL();

    public List<PrintBlockItems> ListPrintBlockItems { get { return db.GetPrintBlockItemsCount().ToList(); } }

    public int SelectedPrintBlockItems { get { return ListPrintBlockItems.Count > 0 ? ListPrintBlockItems.Last().PackageNumber : -1; } }
    public bool IsVisPriceNormal { get { return WP != null && (WP.PriceOld != WP.PriceNormal); } }
    public bool IsVisPriceOpt { get { return WP != null && (WP.PriceOpt != 0 || WP.PriceOptOld != 0); } }
    public bool IsVisPriceOptQ { get { return WP != null && WP.QuantityOpt != 0; } }

    public bool IsVisF4 { get { return Config.Company == eCompany.Sim23; } }
    public string F4Text { get { return IsOnline ? "OnLine" : "OffLine"; } }
    private bool _IsOnline = true;
    public bool IsOnline { get { return _IsOnline; } set { _IsOnline = value; OnPropertyChanged("F4Text"); } }

    bool _IsVisRepl = false;
    public bool IsVisRepl { get { return _IsVisRepl; } set { _IsVisRepl = value; OnPropertyChanged("IsVisRepl"); } }
    public bool IsSoftKeyboard { get { return Config.IsSoftKeyboard; } }
    double _PB = 0;
    public double PB { get { return _PB; } set { _PB = value; OnPropertyChanged(nameof(PB)); } }

    WaresPrice _WP;
    public WaresPrice WP
    {
        get { return _WP; }
        set
        {
            _WP = value; OnPropertyChanged("WP"); OnPropertyChanged("TextColorPrice");
            OnPropertyChanged("IsVisPriceOpt"); OnPropertyChanged(nameof(IsVisPriceNormal)); OnPropertyChanged("TextColorHttp");
            OnPropertyChanged("ColorPrintColorType"); OnPropertyChanged("IsVisPriceOptQ");
        }
    }
    //ZXingScannerView zxing;
    //ZXingDefaultOverlay overlay;

    int _PrintType = 0;//????? ???? 0-????????? 1-??????, -1 ?? ?????????.        
    public int PrintType { get { return _PrintType; } set { _PrintType = value; OnPropertyChanged("PrintType"); OnPropertyChanged("ColorPrintColorType"); } }
    public bool IsEnabledPrint { get { return Config.TypeUsePrinter != eTypeUsePrinter.NotDefined; } }
    /// <summary>
    /// ????? ?????????? ???????? ?? ???? !!!TMP ????? ????????? ? ????.
    /// </summary>
    int LineNumber = 0;
    public int AllScan { get; set; } = 0;
    public int BadScan { get; set; } = 0;
    /// <summary>
    /// ????? ?????? ???????? ?? ???? !!!TMP ????? ????????? ? ????.
    /// </summary>
    int _PackageNumber = 1;
    public int PackageNumber { get { return _PackageNumber; } set { _PackageNumber = value; OnPropertyChanged(nameof(PackageNumber)); OnPropertyChanged(nameof(ListPrintBlockItems)); OnPropertyChanged(nameof(SelectedPrintBlockItems)); } }


    //public int ColorPrintColorType() { return Color.parseColor(HttpState != eStateHTTP.HTTP_OK ? "#ffb3b3" : (PrintType == 0 ? "#ffffff" : "#3fffff00")); }

    public string ColorPrintColorType { get { return WP != null && WP.MinQuantity == 0 ? "#ffd8d8" : PrintType == 0 ? "#ffffff" : PrintType == 1 ? "#ffffa8" : "#ffffff"; } }
    public string TextColorPrice { get { return (WP != null && WP.Price != 0 && WP.Price == WP.PriceOld && WP.PriceOpt == WP.PriceOptOld) ? "#009800" : "#ff5c5c"; } set { OnPropertyChanged(nameof(TextColorPrice)); } }
    public string TextColorHttp { get { return (WP != null && WP.StateHTTP == eStateHTTP.HTTP_OK) ? "#009800" : "#ff5c5c"; } }

    public bool _IsMultyLabel = false;
    public bool IsMultyLabel { get { return _IsMultyLabel; } set { _IsMultyLabel = value; OnPropertyChanged(nameof(IsMultyLabel)); OnPropertyChanged(nameof(F5Text)); } }
    public string F5Text { get { return IsMultyLabel ? "????." : "????."; } }
    public bool IsVisScan { get { return Config.TypeScaner == eTypeScaner.Camera; } }
    /// <summary>
    /// 0 - ?????? , 1 - ?????????? ??????, 2 - ?????????? ?????, 3 - ???????? ?????? ?? ?????????, 4 - ?????? ?? ?????????, 5 - ???????
    /// </summary>
    private eCheckWareScaned _IsWareScaned = eCheckWareScaned.Nothing;
    public eCheckWareScaned IsWareScaned { get { return _IsWareScaned; } set { _IsWareScaned = value; OnPropertyChanged(nameof(ColorDoubleScan)); OnPropertyChanged(nameof(IsWareScaned)); /*OnPropertyChanged(nameof(ButtonDoubleScan));*/ OnPropertyChanged(nameof(MessageDoubleScan)); } }
    public bool IsVisDoubleScan { get; set; }
    public bool IsVisBarcode { get { return !IsVisDoubleScan; } }
    // private string _MessageDoubleScan;
    public string MessageDoubleScan { get { return EnumMethods.GetDescription(WP?.StateDoubleScan ?? eCheckWareScaned.Success); } } //set {  OnPropertyChanged(nameof(MessageDoubleScan)); } }
                                                                                                                                    //public string ButtonDoubleScan { get { return IsWareScaned == eCheckWareScaned.Nothing || IsWareScaned == eCheckWareScaned.Success ? "" :  IsWareScaned == eCheckWareScaned.WareScaned || IsWareScaned == eCheckWareScaned.PriceTagNotFit ? "????????? ?????" : "????????? ?????"; } }
    public string ColorDoubleScan
    {
        get
        {
            return IsWareScaned == eCheckWareScaned.Success ? "#C5FFC4" : IsWareScaned == eCheckWareScaned.Bad || IsWareScaned == eCheckWareScaned.BadPrice ? "#FFC4C4" :
                                                 IsWareScaned == eCheckWareScaned.PriceTagScaned || IsWareScaned == eCheckWareScaned.WareScaned ? "#FEFFC4" : "#FFFFFF";
        }
    }
    public UPriceChecker()
    {
        InitializeComponent();

        this.BindingContext = this;
        bl.ClearWPH();

        if (Config.TypeUsePrinter == eTypeUsePrinter.StationaryWithCutAuto) PrintType = -1;

        if (!IsVisScan)
            Config.BarCode = BarCode;

    }

    void Progress(double pProgress) => MainThread.BeginInvokeOnMainThread(() => PB = pProgress);
    protected override void OnAppearing()
    {
        base.OnAppearing();
        Config.OnProgress += Progress;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Config.OnProgress -= Progress;
    }
    void BarCode(string pBarCode) => FoundWares(pBarCode, false);

    void FoundWares(string pBarCode, bool pIsHandInput = false)
    {
        if (!String.IsNullOrWhiteSpace(pBarCode))
        {
            LineNumber++;
            Config.OnProgress?.Invoke(0.2d);

            WP = bl.FoundWares(pBarCode, PackageNumber, LineNumber, pIsHandInput, IsVisDoubleScan, IsOnline);

            if (WP != null)
            {
                AllScan++;
                if (!WP.IsPriceOk)
                    BadScan++;
                IsWareScaned = WP.StateDoubleScan;
            }
            if (Config.IsVibration)
            {
                var duration = TimeSpan.FromMilliseconds(WP?.IsPriceOk == true ? 50 : 250);
                Vibration.Vibrate(duration);
            }

            Config.OnProgress?.Invoke(0.9d);

            if (DeviceInfo.Platform != DevicePlatform.iOS) BarCodeFocused(null, null);

        }

    }

    public void Dispose() { Config.BarCode -= BarCode; }

 

    private void BarCodeHandInput(object sender, EventArgs e)
    {
        var text = BarCodeInput.Text;
        FoundWares(text, true);
    }

    private void BarCodeFocused(object sender, FocusEventArgs e)
    {
        Dispatcher.Dispatch(() =>
        {
            BarCodeInput.CursorPosition = 0;
            BarCodeInput.SelectionLength = BarCodeInput.Text == null ? 0 : BarCodeInput.Text.Length;
            if (!BarCodeInput.IsFocused || IsVisScan)
                BarCodeInput.Focus();
        });
    } 
}