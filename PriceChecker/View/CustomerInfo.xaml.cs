using BRB5.Model;
using Model;
using System.ComponentModel;

namespace PriceChecker.View;

public partial class CustomerInfo : ContentPage, INotifyPropertyChanged
{
    private Client _Client;
    public Client Client { get { return _Client; } set { _Client = value; OnPropertyChanged(nameof(Client)); } }

    public CustomerInfo()
    {
        InitializeComponent();
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

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync(); // повертає на попередню сторінку
    }
}