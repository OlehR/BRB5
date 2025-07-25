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
    DB db = DB.GetDB();
    BL.BL bl = BL.BL.GetBL();
    private Timer _returnToSplashTimer;
    private const int TimeoutSeconds = 5;

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

    public UPriceChecker(string? barcode = null)
    {
        FileLogger.WriteLogMessage( $"UPriceChecker=>BarCode: {barcode}");
        _WP = new();
        InitializeComponent();
        this.BindingContext = this;
        bl.ClearWPH();
        App.ScanerCom.SetOnBarCode(BarCode);
        //Config.BarCode = BarCode;
        _returnToSplashTimer = new Timer(TimeoutSeconds * 1000);
        _returnToSplashTimer.Elapsed += OnTimeoutElapsed;
        _returnToSplashTimer.AutoReset = false;
        if (!string.IsNullOrWhiteSpace(barcode))
        {
            BarCode(barcode);
        }
    }
    private void BarCodeHandInput(object sender, EventArgs e)
    {
        var text = BarCodeInput.Text;
        FoundWares(text, true);
    }
    void BarCode(string pBarCode,string pTypeBarCode=null) => FoundWares(pBarCode, false);

    void FoundWares(string pBarCode, bool pIsHandInput = false)
    {
        if (!string.IsNullOrWhiteSpace(pBarCode))
        {
            WP = bl.FoundWares(pBarCode, 0, 0, pIsHandInput, false, true);

            if (Config.IsVibration)
            {
                var duration = TimeSpan.FromMilliseconds(WP?.IsPriceOk == true ? 50 : 250);
                Vibration.Vibrate(duration);
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