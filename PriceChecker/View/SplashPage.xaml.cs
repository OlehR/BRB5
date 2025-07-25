using BL;
using Utils;

namespace PriceChecker.View;

public partial class SplashPage : ContentPage
{
    BL.BL bl = BL.BL.GetBL();
    public SplashPage()
    {
        InitializeComponent();
        App.ScanerCom.SetOnBarCode(BarCode);
    }
     void BarCode(string pBarCode, string pType)
    {
        FileLogger.WriteLogMessage("SplashPage", "BarCode", $"BarCode: {pBarCode}, Type: {pType}");

        var WP = bl.FoundWares(pBarCode, 0, 0, false, false, true);

        if (WP != null)
        {
            var isStaff = WP.CodeUser!=0;
            ContentPage page = isStaff ? new AdminPriceChecker(WP) : new UPriceChecker(WP);
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PushAsync(page);
            });
        }

    }
  
}