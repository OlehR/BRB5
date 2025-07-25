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
            OnPropertyChanged(nameof(TextColorPrice));
            OnPropertyChanged(nameof(IsVisPriceOpt));
            OnPropertyChanged(nameof(UriPicture));
        }
    }

    public bool IsVisPriceOpt => WP != null && (WP.PriceOpt != 0);

    public bool IsVisBarcode => true;

    public string TextColorPrice =>
        (WP != null && WP.Price != 0 )
            ? "#009800"
            : "#ff5c5c";

    public UPriceChecker(WaresPrice pWP)
    {
        FileLogger.WriteLogMessage( $"UPriceChecker=>BarCode: {pWP.BarCodes}");
        WP =pWP;
        InitializeComponent();
        this.BindingContext = this;
        bl.ClearWPH();
        App.ScanerCom.SetOnBarCode(BarCode);
        //Config.BarCode = BarCode;
        _returnToSplashTimer = new Timer(TimeoutSeconds * 1000);
        _returnToSplashTimer.Elapsed += OnTimeoutElapsed;
        _returnToSplashTimer.AutoReset = false;
    }
    void BarCode(string pBarCode,string pTypeBarCode=null) 
    {
        if (!string.IsNullOrWhiteSpace(pBarCode))
        {
            var tempWP = bl.FoundWares(pBarCode, 0, 0, false, false, true);


            if (tempWP != null)
            {
                if (tempWP.CodeUser != 0)  MainThread.BeginInvokeOnMainThread(async () =>await Navigation.PushAsync(new AdminPriceChecker(tempWP, WP)));
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