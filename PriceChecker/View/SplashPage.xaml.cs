﻿using BL;
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
    DB db = DB.GetDB(Directory.GetCurrentDirectory());
    private Timer _timer;
    public SplashPage()
    {
        bl = BL.BL.GetBL();
        InitializeComponent();
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
        App.ScanerCom.SetOnBarCode(BarCode);

        var r = await bl.c.LoginAsync(Config.Login, Config.Password, Config.LoginServer);
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
            bool tryClient = true;
            if (WP != null)
            {
                var isStaff = WP.CodeUser != 0;
                if (isStaff)
                {
                    Config.CodeUser = (int)WP.CodeUser;
                    Config.NameUser = WP.Name;
                    MainThread.BeginInvokeOnMainThread(async () => { await Shell.Current.GoToAsync("//Admin"); });
                }
                else
                    if (WP?.CodeWares != 0) MainThread.BeginInvokeOnMainThread(async () => { await Navigation.PushAsync(new UPriceChecker(WP)); });
            }
            if(WP == null || (WP?.CodeUser == 0 && WP?.CodeWares==0))
            {
                var clients = await bl.c.GetClient(pBarCode);
                if (clients != null && clients.Info.Any())
                {
                    MainThread.BeginInvokeOnMainThread(async () => { await Navigation.PushAsync(new CustomerInfo(clients.Info.FirstOrDefault())); });
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

    //private void TEMPHandIput(object sender, EventArgs e)
    //{
    //    BarCode(InputBC.Text,"");
    //}
}