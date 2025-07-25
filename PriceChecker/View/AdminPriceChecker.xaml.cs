using BL;
using BL.Connector;
using BRB5;
using BRB5.Model;
using UtilNetwork;

namespace PriceChecker.View;

public partial class AdminPriceChecker : ContentPage
{
	//DB db = DB.GetDB();
    BL.BL bl = BL.BL.GetBL();
    Connector c;

    //public List<PrintBlockItems> ListPrintBlockItems { get { return db.GetPrintBlockItemsCount().ToList(); } }

    //public int SelectedPrintBlockItems { get { return ListPrintBlockItems.Count > 0 ? ListPrintBlockItems.Last().PackageNumber : -1; } }
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
            OnPropertyChanged(nameof(UriPicture));
            OnPropertyChanged("IsVisPriceOpt"); OnPropertyChanged(nameof(IsVisPriceNormal)); OnPropertyChanged("TextColorHttp");
            OnPropertyChanged("ColorPrintColorType"); OnPropertyChanged("IsVisPriceOptQ");
        }
    }

    int _PrintType = 0;//Колір чека 0-звичайний 1-жовтий, -1 не розділяти.        
    public int PrintType { get { return _PrintType; } set { _PrintType = value; OnPropertyChanged("PrintType"); OnPropertyChanged("ColorPrintColorType"); } }
    public bool IsEnabledPrint { get { return Config.TypeUsePrinter != eTypeUsePrinter.NotDefined; } }
    /// <summary>
    /// Номер сканування цінників за день !!!TMP Треба зберігати в базі.
    /// </summary>
    int LineNumber = 0;
    public int AllScan { get; set; } = 0;
    public int BadScan { get; set; } = 0;

    public string ColorPrintColorType { get { return WP != null && WP.MinQuantity == 0 ? "#ffd8d8" : PrintType == 0 ? "#ffffff" : PrintType == 1 ? "#ffffa8" : "#ffffff"; } }
    public string TextColorPrice { get { return (WP != null && WP.Price != 0 && WP.Price == WP.PriceOld && WP.PriceOpt == WP.PriceOptOld) ? "#009800" : "#ff5c5c"; } set { OnPropertyChanged(nameof(TextColorPrice)); } }
    public string TextColorHttp { get { return (WP != null && WP.StateHTTP == eStateHTTP.HTTP_OK) ? "#009800" : "#ff5c5c"; } }

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


    public AdminPriceChecker(string barcode, WaresPrice? pWP = null)
    {
        _WP = new();

        InitializeComponent();
        this.BindingContext = this;
        bl.ClearWPH();
        if (pWP != null)
        {
            WP = pWP;
            if (WP.RestWarehouse != null) RestWarehouseListShow(WP.RestWarehouse);
        }
        else if (!string.IsNullOrWhiteSpace(barcode))
        {
            BarCode(barcode);
        }

        if (Config.TypeUsePrinter == eTypeUsePrinter.StationaryWithCutAuto) PrintType = -1;

        NavigationPage.SetHasNavigationBar(this, DeviceInfo.Platform == DevicePlatform.iOS);
        if (!IsVisScan)
            Config.BarCode = BarCode;

    }

    void Progress(double pProgress) => MainThread.BeginInvokeOnMainThread(() => PB = pProgress);
    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (!IsVisScan)
            BarCodeInput.Focus();
        Config.OnProgress += Progress;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        bl.SendLogPrice();
        Config.OnProgress -= Progress;
    }
    void BarCode(string pBarCode) => FoundWares(pBarCode, false);

    void FoundWares(string pBarCode, bool pIsHandInput = false)
    {
        if (!String.IsNullOrWhiteSpace(pBarCode))
        {
            LineNumber++;
            Config.OnProgress?.Invoke(0.2d);

            WP = bl.FoundWares(pBarCode, 0, LineNumber, pIsHandInput, false, IsOnline);

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

            if (WP.RestWarehouse != null) RestWarehouseListShow(WP.RestWarehouse);
        }

    }

    public void Dispose() { Config.BarCode -= BarCode; }

   
    private void OnClickPrintBlock(object sender, EventArgs e)
    {
        var temp = PrintBlockItemsXaml.SelectedItem as PrintBlockItems;
        if (IsEnabledPrint)
            _ = DisplayAlert("Друк", bl.PrintPackage(PrintType, temp.PackageNumber, IsMultyLabel), "OK");
    }



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

    private void OnClickPrintOne(object sender, EventArgs e)
    {
        if (IsEnabledPrint && WP != null)
            _ = DisplayAlert("Друк", bl.c.PrintHTTP(new[] { WP.CodeWares }), "OK");
    }

    private void OnUpdateReplenishment(object sender, EventArgs e)
    {
        decimal d;
        if (decimal.TryParse(NumberOfReplenishment.Text, out d)) ;
            //db.UpdateReplenishment(LineNumber, d);
    }

    //public WareInfo(ParseBarCode parseBarCode)
    //{
    //    InitializeComponent();
    //    c = ConnectorBase.GetInstance();
    //    NavigationPage.SetHasNavigationBar(this, DeviceInfo.Platform == DevicePlatform.iOS);
    //    WP = c.GetPrice(parseBarCode, eTypePriceInfo.Full);
    //    if (WP.RestWarehouse != null) RestWarehouseListShow(WP.RestWarehouse);
    //    if (WP.Сondition != null) FillConditionList(WP.Сondition);
    //    if (WP.ActionType > 0) IsVisPromotion = true;
    //    ImageUri = Config.ApiUrl1 + $"Wares/{WP.CodeWares:D9}.png";
    //    Picture = new UriImageSource
    //    {
    //        Uri = new Uri(ImageUri),
    //        CachingEnabled = false,
    //        CacheValidity = new TimeSpan(7, 0, 0, 0)
    //    };
    //    
    //    this.BindingContext = this;
    //    CalculateAndSetScrollViewHeight();
    //}
    private void RestWarehouseListShow(IEnumerable<RestWarehouse> warehouses)
    {
        //var t = db.GetWarehouse();
        //var w = t.FirstOrDefault(x => x.CodeWarehouse == Config.CodeWarehouse);
        foreach (var warehouse in warehouses)
        {
            var grid = new Grid
            {
                ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = new GridLength(7, GridUnitType.Star) },
                        new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) },
                        new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) }
                    },
                BackgroundColor = Color.FromArgb("#adaea7")
            };

            var NameWarehouseLabel = new Label
            {
                Text = warehouse.NameWarehouse,
                TextColor = Colors.Black,
                LineBreakMode = LineBreakMode.NoWrap,
                BackgroundColor = Colors.White

            };

            var QuantityLabel = new Label
            {
                Text = warehouse.Quantity.ToString(),
                TextColor = Colors.Black,
                FontAttributes = FontAttributes.Bold,
                LineBreakMode = LineBreakMode.NoWrap,
                BackgroundColor = Colors.White
            };
            Grid.SetColumn(QuantityLabel, 1);

            var DateLabel = new Label
            {
                Text = warehouse.Date.ToString("dd.MM.yyyy"),
                TextColor = Colors.Black,
                LineBreakMode = LineBreakMode.NoWrap,
                BackgroundColor = Colors.White
            };
            Grid.SetColumn(DateLabel, 2);

            //if (w != null && w.Name == warehouse.NameWarehouse)
            //{
            //    NameWarehouseLabel.BackgroundColor = Colors.SandyBrown;
            //    QuantityLabel.BackgroundColor = Colors.SandyBrown;
            //    DateLabel.BackgroundColor = Colors.SandyBrown;
            //}

            grid.Children.Add(NameWarehouseLabel);
            grid.Children.Add(QuantityLabel);
            grid.Children.Add(DateLabel);
            RestWarehouseList.Children.Add(grid);
        }
    }
    public void FillConditionList(IEnumerable<СonditionClass> conditions)
    {
        ConditionList.Children.Clear();

        foreach (var condition in conditions)
        {
            var stackLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal
            };

            var conditionLabel = new Label
            {
                Text = condition.Сondition
            };

            var contrLabel = new Label
            {
                Text = condition.Contr
            };

            stackLayout.Children.Add(conditionLabel);
            stackLayout.Children.Add(contrLabel);

            ConditionList.Children.Add(stackLayout);
        }
    }
    

    private double GetNavigationBarHeight()
    {
        const double navigationBarHeightDp = 36;
        var density = DeviceDisplay.Current.MainDisplayInfo.Density;
        return navigationBarHeightDp * density;
    }

}
