using BL;
using BRB5.Model;
using System.Threading.Tasks;
using System.Timers;
using Utils;
using Timer = System.Timers.Timer;

namespace PriceChecker.View;

public partial class SplashPage : ContentPage
{
    BL.BL bl;
    DB db = DB.GetDB(Directory.GetCurrentDirectory());
    private Timer _timer;
    public SplashPage()
    {
        bl = BL.BL.GetBL();
        InitializeComponent();
        bl.Init();
        SetShopBranding();
    }
    private void SetShopBranding()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            switch (Config.CodeTM)
            {
                case eShopTM.Vopak:
                    BackgroundImage.Source = "background1vopak.png";
                    LogoImage.Source = "logo1vopak.png";
                    break;

                case eShopTM.Spar:
                    BackgroundImage.Source = "background2spar.png";
                    LogoImage.Source = "logo1spar.png";
                    break;
            }
        });

    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        App.ScanerCom.SetOnBarCode(BarCode);
    }
      
    async void BarCode(string pBarCode, string pType)
    {
        FileLogger.WriteLogMessage("SplashPage", "BarCode", $"BarCode: {pBarCode}, Type: {pType}");

        if (pBarCode.StartsWith("BRB6=>"))
        {
            var CodeWarehouse = bl.QRSettingsParse(pBarCode);
            if (CodeWarehouse != 0) {
                Config.CodeWarehouse = 0;
                try
                {
                    Config.CodeWarehouse = 0;
                    await bl.c.LoadGuidDataAsync(false); 
                    var wh = db.GetWarehouse().FirstOrDefault(el => el.CodeWarehouse == CodeWarehouse);
                    Config.CodeTM = wh?.CodeTM ?? default(eShopTM);
                }
                finally
                {
                    Config.CodeWarehouse = CodeWarehouse;
                    SetShopBranding();
                }
            }
            ShowTextWithTimeout(pBarCode);
            bl.SaveSettings();
        }
        else
        {
            var WP = bl.FoundWares(pBarCode, 0, 0, false, false, true);

            if (WP != null)
            {
                var isStaff = WP.CodeUser != 0;
                ContentPage page = isStaff ? new AdminPriceChecker(WP) : new UPriceChecker(WP);
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Navigation.PushAsync(page);
                });
            }
        }
    }
    private void ShowTextWithTimeout(string message)
    {
        // Відображаємо текст
        MainThread.BeginInvokeOnMainThread(() =>
        {
            SettingsShow.Text = message;
            SettingsShow.IsVisible = true;
        });
        // Якщо таймер уже існує — зупиняємо його
        if (_timer != null)
        {
            _timer.Stop();
            _timer.Dispose();
        }

        // Створюємо таймер на 1 хвилину (60000 мс)
        _timer = new Timer(10000);
        _timer.Elapsed += OnTimerElapsed;
        _timer.AutoReset = false; // виконається тільки 1 раз
        _timer.Start();
    }

    private void OnTimerElapsed(object sender, ElapsedEventArgs e)
    {
        // Таймер працює у фоні, тому оновлення UI виконуємо на головному потоці
        MainThread.BeginInvokeOnMainThread(() =>
        {
            SettingsShow.Text = string.Empty;
            SettingsShow.IsVisible = false;
        });

        // Зупиняємо та очищуємо таймер
        _timer.Stop();
        _timer.Dispose();
        _timer = null;
    }

}