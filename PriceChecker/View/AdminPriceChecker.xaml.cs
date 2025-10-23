using BL;
using BL.Connector;
using BRB5;
using BRB5.Model;
using BRB5.Model.DB;
using System.Timers;
using UtilNetwork;
using Timer = System.Timers.Timer;

namespace PriceChecker.View;

[QueryProperty(nameof(dBarCode), "data")]
public partial class AdminPriceChecker : ContentPage
{
    DB db = DB.GetDB();
    BL.BL bl = BL.BL.GetBL(); 

    private Timer _returnTimer;
    private const int TimeoutSeconds = 60;
    public List<PrintBlockItems> ListPrintBlockItems { get { return db.GetPrintBlockItemsCount().ToList(); } }
    public int SelectedPrintBlockItems { get { return ListPrintBlockItems.Count > 0 ? ListPrintBlockItems.Last().PackageNumber : -1; } }
    /// <summary>
    /// Номер пакета цінників за день !!!TMP Треба зберігати в базі.
    /// </summary>
    int _PackageNumber = 1;
    public int PackageNumber { get { return _PackageNumber; } set { _PackageNumber = value; OnPropertyChanged(nameof(PackageNumber)); OnPropertyChanged(nameof(ListPrintBlockItems)); OnPropertyChanged(nameof(SelectedPrintBlockItems)); } }

    public bool IsVisPriceNormal { get { return WP != null && (WP.PriceOld != WP.PriceNormal); } }
    public bool IsVisPriceOpt { get { return WP != null && (WP.PriceOpt != 0 || WP.PriceOptOld != 0); } }
   
    private bool _IsOnline = true;
    public bool IsOnline { get { return _IsOnline; } set { _IsOnline = value; OnPropertyChanged("F4Text"); } }

    bool _IsVisRepl = false;
    public bool IsVisRepl { get { return _IsVisRepl; } set { _IsVisRepl = value; OnPropertyChanged("IsVisRepl"); } }
    public bool IsSoftKeyboard { get { return Config.IsSoftKeyboard; } }
    double _PB = 0;
    public double PB { get { return _PB; } set { _PB = value; OnPropertyChanged(nameof(PB)); } }
    public bool IsVisPromotion => WP != null && WP.ActionType > 0;
    WaresPrice User { get; set; }
    WaresPrice _WP;    
    public WaresPrice WP
    {
        get { return _WP; }
        set
        {
            _WP = value; OnPropertyChanged("WP"); OnPropertyChanged(nameof(TextColorPrice));
            OnPropertyChanged(nameof(UriPicture));
            OnPropertyChanged(nameof(IsVisPriceOpt)); OnPropertyChanged(nameof(IsVisPriceNormal)); OnPropertyChanged("TextColorHttp");
            OnPropertyChanged(nameof(ColorPrintColorType));
            OnPropertyChanged(nameof(IsVisPromotion));
        }
    }

    int _PrintType = -1;//Колір чека 0-звичайний 1-жовтий, -1 не розділяти.        
    public int PrintType { get { return _PrintType; } set { _PrintType = value; OnPropertyChanged("PrintType"); OnPropertyChanged("ColorPrintColorType"); } }
    public bool IsEnabledPrint { get { return Config.CodeWarehouse != 0; } }
    /// <summary>
    /// Номер сканування цінників за день !!!TMP Треба зберігати в базі.
    /// </summary>
    int LineNumber = 0;
    public int AllScan { get; set; } = 0;
    public int BadScan { get; set; } = 0;

    public string ColorPrintColorType { get { return WP != null && WP.MinQuantity == 0 ? "#ffd8d8" : PrintType == 0 ? "#ffffff" : PrintType == 1 ? "#ffffa8" : "#ffffff"; } }
    public string TextColorPrice { get { return (WP != null && WP.Price != 0 && WP.Price == WP.PriceOld && WP.PriceOpt == WP.PriceOptOld) ? "#009800" : "#ff5c5c"; } set { OnPropertyChanged(nameof(TextColorPrice)); } }
    public string TextColorHttp { get { return (WP != null && bl.LastResult.StateHTTP == eStateHTTP.HTTP_OK) ? "#009800" : "#ff5c5c"; } }

    public bool _IsMultyLabel = false;
    public bool IsMultyLabel { get { return _IsMultyLabel; } set { _IsMultyLabel = value; OnPropertyChanged(nameof(IsMultyLabel)); OnPropertyChanged(nameof(F5Text)); } }
    public string F5Text { get { return IsMultyLabel ? "Дубл." : "Унік."; } }
    public bool IsVisScan { get { return Config.TypeScaner == eTypeScaner.Camera; } }
    /// <summary>
    /// 0 - нічого , 1 - сканований цінник, 2 - сканований товар, 3 - штрихкод товату не підходить, 4 - цінник не підходить, 5 - успішно
    /// </summary>
    private eCheckWareScaned _IsWareScaned = eCheckWareScaned.Nothing;
    public eCheckWareScaned IsWareScaned { get { return _IsWareScaned; } set { _IsWareScaned = value; OnPropertyChanged(nameof(ColorDoubleScan)); OnPropertyChanged(nameof(IsWareScaned)); OnPropertyChanged(nameof(MessageDoubleScan)); } }

    public string MessageDoubleScan { get { return EnumMethods.GetDescription(WP?.StateDoubleScan ?? eCheckWareScaned.Success); } } 
    public string ColorDoubleScan
    {
        get
        {
            return IsWareScaned == eCheckWareScaned.Success ? "#C5FFC4" : IsWareScaned == eCheckWareScaned.Bad || IsWareScaned == eCheckWareScaned.BadPrice ? "#FFC4C4" :
                                                 IsWareScaned == eCheckWareScaned.PriceTagScaned || IsWareScaned == eCheckWareScaned.WareScaned ? "#FEFFC4" : "#FFFFFF";
        }
    }

    public string ImageUri { get; set; } = "photo.png";
    public UriImageSource Picture { get; set; }
    public Uri UriPicture { get { return new Uri(Config.ApiUrl1 + $"Wares/{WP.CodeWares:D9}.png"); } }
    private int InfoHeight;
    public string ColorBG { get; set; }
    public string dBarCode { get; set; }

    public AdminPriceChecker()
    {
        _WP = new();
        InitializeComponent();
        this.BindingContext = this;
        OnScreenKeyboard.OkPressed += OnScreenKeyboard_OkPressed;
        OnScreenKeyboard.UserInteracted += (s, e) => ResetTimer();
        bl.ClearWPH();      
             
        switch (Config.CodeTM)
        {
            case eShopTM.Vopak:
                BackgroundImage.Source = "background1vopak.png";
                LogoImage.Source = "logo1vopak.png";
                ColorBG = "#ffde2f";
                OnPropertyChanged(nameof(ColorBG));
                break;

            case eShopTM.Spar:
                BackgroundImage.Source = "background2spar.png";
                LogoImage.Source = "logo2spar.png";
                ColorBG = "#e31e24";
                OnPropertyChanged(nameof(ColorBG));
                break;

        }

        if (Config.TypeUsePrinter == eTypeUsePrinter.StationaryWithCutAuto) PrintType = -1;

        NavigationPage.SetHasNavigationBar(this, DeviceInfo.Platform == DevicePlatform.iOS);


        // Ініціалізація таймера
        _returnTimer = new Timer(TimeoutSeconds * 1000);
        _returnTimer.Elapsed += OnTimeoutElapsed;
        _returnTimer.AutoReset = false;

        // Запуск при відкритті сторінки
        ResetTimer();

        // Ловимо будь-який тап на Content
        if (Content is Microsoft.Maui.Controls.View view)
        {
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (_, __) => ResetTimer();
            view.GestureRecognizers.Add(tapGesture);
        }

        var r = db.GetCountScanCode();
        if (r != null)
        {
            AllScan = r.AllScan;
            BadScan = r.BadScan;
            LineNumber = r.LineNumber;
            PackageNumber = r.PackageNumber;
        }
    }
    private void OnScreenKeyboard_OkPressed(object sender, EventArgs e)
    {
        var text = BarCodeInput.Text;
        if (!string.IsNullOrEmpty(text))
        {
            FoundWares(text, true);
        }

        // сховати клаву і зняти фокус з Entry
        OnScreenKeyboard.IsVisible = false;
        BarCodeInput.Unfocus();
        ResetTimer();
    }
    private void BarCodeInput_Focused(object sender, FocusEventArgs e) =>  OnScreenKeyboard.IsVisible = true;
    private void OnSwipedRight(object sender, SwipedEventArgs e)
    {
        if (e.Direction == SwipeDirection.Right)
        {
            Shell.Current.FlyoutIsPresented = true;
        }
    }

    void Progress(double pProgress) => MainThread.BeginInvokeOnMainThread(() => PB = pProgress);
    protected override void OnAppearing()
    {
        base.OnAppearing();
        Config.OnProgress += Progress;

        App.ScanerCom?.SetOnBarCode(BarCode);
        if (!string.IsNullOrEmpty(dBarCode))
        {
            FoundWares(dBarCode);
        }

    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Config.OnProgress -= Progress;
    }
    void BarCode(string pBarCode, string pTypeBarCode = null) => FoundWares(pBarCode, false);

    void FoundWares(string pBarCode, bool pIsHandInput = false)
    {
        if (!String.IsNullOrWhiteSpace(pBarCode))
        {
            LineNumber++;
            Config.OnProgress?.Invoke(0.2d);

            MainThread.BeginInvokeOnMainThread(() => {
                OnScreenKeyboard.IsVisible = false;
                BarCodeInput.Unfocus(); 
                ConditionList.Children.Clear(); 
            });

            WP = bl.FoundWares(pBarCode, PackageNumber, LineNumber, pIsHandInput, false, IsOnline, eTypePriceInfo.Full);

            if (WP != null)
            {
                AllScan++;
                if (!WP.IsPriceOk)
                    BadScan++;
                IsWareScaned = WP.StateDoubleScan;
                if (WP.Сondition != null) FillConditionList(WP.Сondition);
            }

            Config.OnProgress?.Invoke(0.9d);
        }

        ResetTimer();
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
        ResetTimer();
    }

    private void OnClickPrintOne(object sender, EventArgs e)
    {
        if (IsEnabledPrint && WP != null)
            _ = DisplayAlert("Друк", bl.c.PrintHTTP(new[] { WP.CodeWares }), "OK");

        ResetTimer();
    }
    private void OnClickPrintBlock(object sender, EventArgs e)
    {
        var temp = PrintBlockItemsXaml.SelectedItem as PrintBlockItems;
        if (IsEnabledPrint)
            _ = DisplayAlert("Друк", bl.PrintPackage(PrintType, temp.PackageNumber, IsMultyLabel), "OK");
    }
    private void OnClickAddPrintBlock(object sender, EventArgs e)
    {
        PackageNumber++;
        ListPrintBlockItems.Add(new PrintBlockItems() { PackageNumber = PackageNumber });
    }

    public void FillConditionList(IEnumerable<СonditionClass> conditions)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ConditionList.Children.Clear();
        });

        foreach (var condition in conditions)
        {
            var stackLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal
            };

            var conditionLabel = new Label
            {
                Text = condition.Сondition,
                FontSize = 20,
                TextColor = Colors.DarkBlue
            };

            var contrLabel = new Label
            {
                Text = condition.Contr,
                FontSize = 20,
                TextColor = Colors.DarkBlue
            };

            stackLayout.Children.Add(conditionLabel);
            stackLayout.Children.Add(contrLabel);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                ConditionList.Children.Add(stackLayout);
            });
        }
    }

    private async void OnBackClick(object sender, EventArgs e)
    {
        await MainThread.InvokeOnMainThreadAsync(async () => { await Shell.Current.GoToAsync("//Splash"); });
    }

    private void ResetTimer()
    {
        _returnTimer.Stop();
        _returnTimer.Start();
    }

    private async void OnTimeoutElapsed(object sender, ElapsedEventArgs e)
    {
        await MainThread.InvokeOnMainThreadAsync(async () => {  await Shell.Current.GoToAsync("//Splash"); });
    }

    // Скидання таймера при взаємодії з елементами
    private void AnyControlInteracted(object sender, EventArgs e)
    {
        ResetTimer();
    }
}
