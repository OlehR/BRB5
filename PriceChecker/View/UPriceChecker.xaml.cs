using BL;
using BRB5;
using BRB5.Model;
using System.Timers;
using UtilNetwork;
using Utils;
using Timer = System.Timers.Timer;

namespace PriceChecker.View;

public partial class UPriceChecker : ContentPage
{
    BL.BL bl = BL.BL.GetBL();
    private Timer _returnToSplashTimer;
    private const int TimeoutSeconds = 30;
    WaresPrice _WP;
    public Uri UriPicture { get { return new Uri(Config.ApiUrl1 + $"Wares/{WP.CodeWares:D9}.png"); } }
    public WaresPrice WP
    {
        get => _WP;
        set
        {
            _WP = value;
            OnPropertyChanged(nameof(WP));
            OnPropertyChanged(nameof(IsVisPriceOpt));
            OnPropertyChanged(nameof(UriPicture));
            OnPropertyChanged(nameof(IsCashback));
            OnPropertyChanged(nameof(IsVisPromotion));
            OnPropertyChanged(nameof(IsVisPriceMain));
        }
    }
    public bool IsVisBarcode => true;

    public string ColorBG { get; set; }
    public bool IsCashback => WP != null &&
                           !string.IsNullOrEmpty(WP.Country) &&
                           (WP.Country.ToUpper() == "НАЦІОНАЛЬНИЙ КЕШБЕК" ||
                            WP.Country.ToUpper() == "УКРАЇНА");

    public bool IsVisPromotion => WP != null && WP.ActionType > 0;
    public bool IsVisPriceOpt => WP != null && (WP.PriceOpt != 0) && !IsVisPromotion;
    public bool IsVisPriceMain =>  WP != null && (IsVisPromotion || IsVisPriceOpt);
    public UPriceChecker(WaresPrice pWP)
    {
        FileLogger.WriteLogMessage( $"UPriceChecker=>BarCode: {pWP.BarCodes}");
        WP =pWP;
        InitializeComponent();
        this.BindingContext = this;
        bl.ClearWPH();
        //Config.BarCode = BarCode;
        _returnToSplashTimer = new Timer(TimeoutSeconds * 1000);
        _returnToSplashTimer.Elapsed += OnTimeoutElapsed;
        _returnToSplashTimer.AutoReset = false;
       
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
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        App.ScanerCom?.SetOnBarCode(BarCode);
        _returnToSplashTimer.Start();
    }

    protected override void OnDisappearing()
    {
        _returnToSplashTimer.Stop();
        base.OnDisappearing();
    }

    void BarCode(string pBarCode,string pTypeBarCode=null) 
    {
        if (!string.IsNullOrWhiteSpace(pBarCode))
        {
            var tempWP = bl.FoundWares(pBarCode, 0, 0, false, false, true, eTypePriceInfo.Short);


            if (tempWP != null)
            {
                if (tempWP.CodeUser != 0) 
                {
                    string firstCode = (WP.BarCodes?.Split(',') ?? Array.Empty<string>()).FirstOrDefault() ?? "";

                    MainThread.BeginInvokeOnMainThread(async () => Shell.Current.GoToAsync("//Admin?data=" + firstCode));
                }
                else WP = tempWP;
            }

            // Reset and start the timer on every scan
            _returnToSplashTimer.Stop();
            _returnToSplashTimer.Start();
        }

    } 

    private async void OnTimeoutElapsed(object sender, ElapsedEventArgs e)
    {
        // Timer runs on a background thread, so use MainThread to navigate
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            // Navigate to the splash page
            await Navigation.PopToRootAsync(); // Or use your specific navigation logic
        });
    }
}