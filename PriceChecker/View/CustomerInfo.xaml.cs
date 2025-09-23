using BL;
using BRB5;
using BRB5.Model;
using Model;
using System.ComponentModel;
using System.Timers;
using Timer = System.Timers.Timer;

namespace PriceChecker.View;

public partial class CustomerInfo : ContentPage, INotifyPropertyChanged
{
    private Client _Client;
    private Timer _returnToSplashTimer;
    private const int TimeoutSeconds = 30;
    BL.BL bl = BL.BL.GetBL();
    public Client Client { get { return _Client; } set { _Client = value; OnPropertyChanged(nameof(Client)); } }

    public CustomerInfo()
    {
        InitializeComponent();

        _returnToSplashTimer = new Timer(TimeoutSeconds * 1000);
        _returnToSplashTimer.Elapsed += OnTimeoutElapsed;
        _returnToSplashTimer.AutoReset = false;

        Client = new Client
        {
            NameClient = "Хтось там Наталія",
            Wallet = 0.36m,
            SumBonus = 4542.48m,
            SumMoneyBonus = 0.00m,
            MainPhone = "0951234567",
            PhoneAdd = "",
            PersentDiscount = 0,
            PercentBonus = 0,
            BarCode = "8800000833835",
            StatusCard = eStatusCard.Block, // Заблокована
            BirthDay = new DateTime(1976, 8, 30),
            IsСertificate = false
        };

        BindingContext = this;

        switch (Config.CodeTM)
        {
            case eShopTM.Vopak:
                BackgroundImage.Source = "background1vopak.png";
                LogoImage.Source = "logo1vopak.png";
                break;

            case eShopTM.Spar:
                BackgroundImage.Source = "background2spar.png";
                LogoImage.Source = "logo2spar.png";
                break;
        }
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _returnToSplashTimer.Start();
    }

    protected override void OnDisappearing()
    {
        _returnToSplashTimer.Stop();
        base.OnDisappearing();
    }
    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync(); // повертає на попередню сторінку
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


    void BarCode(string pBarCode, string pTypeBarCode = null)
    {
        if (!string.IsNullOrWhiteSpace(pBarCode))
        {
            //var tempWP = bl.FoundWares(pBarCode, 0, 0, false, false, true, eTypePriceInfo.Short);


            //if (tempWP != null)
            //{
            //    if (tempWP.CodeUser != 0)
            //    {
            //        string firstCode = (WP.BarCodes?.Split(',') ?? Array.Empty<string>()).FirstOrDefault() ?? "";

            //        MainThread.BeginInvokeOnMainThread(async () => Shell.Current.GoToAsync("//Admin?data=" + firstCode));
            //    }
            //    else WP = tempWP;
            //}

            // Reset and start the timer on every scan
            _returnToSplashTimer.Stop();
            _returnToSplashTimer.Start();
        }

    }
}