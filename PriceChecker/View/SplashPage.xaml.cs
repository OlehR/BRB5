using BL;
using BRB5;
using BRB5.Model;
using Model;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Timers;
using Utils;
using Timer = System.Timers.Timer;

namespace PriceChecker.View;

public partial class SplashPage : ContentPage
{
    BL.BL bl;
    DB db;
    private Timer _timer;
    public SplashPage()
    {
        InitializeComponent();
        this.BindingContext = this;

        string Path = "";
        if (DeviceInfo.Platform == DevicePlatform.Android)
            Path = FileSystem.AppDataDirectory;
        if (DeviceInfo.Platform == DevicePlatform.WinUI)
            Path = Directory.GetCurrentDirectory();

        db = DB.GetDB(Path);
        bl = BL.BL.GetBL();

        InputBC.Completed += (s, e) =>
            {
            if (!string.IsNullOrEmpty(InputBC.Text))
            {
                string text = InputBC.Text;
                InputBC.Text = string.Empty; // Очищуємо відразу для наступного сканування
                Task.Run(async () => BarCode(text, "Scanner"));
            }
        };
        InputBC.Unfocused += (s, e) => { if (this.IsLoaded) InputBC.Focus(); };

        OnScreenKeyboard.OkPressed += OnScreenKeyboard_OkPressed;
        bl.Init();
        SetShopBranding();
        Config.TypeDoc = new[]{
            new TypeDoc { Group = eGroup.Doc, CodeDoc = 51, NameDoc = "Установка цін", KindDoc = eKindDoc.Normal },
            new TypeDoc { Group = eGroup.Doc, CodeDoc = 52, NameDoc = "Друк пакетів", KindDoc = eKindDoc.Normal }
        };
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
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        App.ScanerCom?.SetOnBarCode(BarCode);

        var r = await bl.c.LoginAsync(Config.Login, Config.Password, Config.LoginServer);
        //_ = Task.Delay(200).ContinueWith(t => {
        //    InputBC.Focus();
        //    InputBC.IsEnabled = false;
        //    Task.Delay(200);
        //    InputBC.IsEnabled = true;
        //});

    }

    async void BarCode(string pBarCode, string pType)
    {
        FileLogger.WriteLogMessage("SplashPage", "BarCode", $"BarCode: {pBarCode}, Type: {pType}");

        if (pBarCode.StartsWith("BRB6=>"))
        {
            var CodeWarehouse = bl.QRSettingsParse(pBarCode);
            if (CodeWarehouse != 0)
            {
                try
                {
                    Config.CodeWarehouse = 0;
                    await bl.c.LoadGuidDataAsync(false);
                    var wh = db.GetWarehouse().FirstOrDefault(el => el.CodeWarehouse == CodeWarehouse);
                    Config.CodeTM = wh?.CodeTM ?? default;
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
            bool tryClient = true;
            if (WP != null)
            {
                var isStaff = WP.CodeUser != 0;
                if (isStaff)
                {
                    Config.CodeUser = (int)WP.CodeUser;
                    Config.NameUser = WP.Name;
                    _ = Task.Run(async () => { bl.OnButtonLogin(Config.Login, Config.Password, false); });
                    MainThread.BeginInvokeOnMainThread(async () => { await Shell.Current.GoToAsync("//Admin"); });
                }
                else
                    if (WP?.CodeWares != 0) MainThread.BeginInvokeOnMainThread(async () => { await Navigation.PushAsync(new UPriceChecker(WP)); });
            }
            if (WP == null || (WP?.CodeUser == 0 && WP?.CodeWares == 0))
            {
                var clients = await bl.c.GetClient(pBarCode);
                if (clients != null && clients.Data.Any())
                {
                    MainThread.BeginInvokeOnMainThread(async () => { await Navigation.PushAsync(new CustomerInfo(clients.Data.FirstOrDefault())); });
                }
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


    private void OnKeyboardTapped(object sender, TappedEventArgs e)
    {
        OnScreenKeyboard.IsVisible = !OnScreenKeyboard.IsVisible;
    }

    private void OnScreenKeyboard_OkPressed(object sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            InputBC.IsEnabled = false;
            string text = InputBC.Text;
            if (!string.IsNullOrEmpty(text)) Task.Run(async () => BarCode(text, ""));
            InputBC.Text = "";
            InputBC.IsEnabled = true;
        });

        // сховати клаву і зняти фокус з Entry
        OnScreenKeyboard.IsVisible = false;
    }
}