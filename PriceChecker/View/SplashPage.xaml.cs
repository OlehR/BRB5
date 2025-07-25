using Utils;

namespace PriceChecker.View;

public partial class SplashPage : ContentPage
{
    public SplashPage()
    {
        InitializeComponent();
        App.ScanerCom.SetOnBarCode(BarCode);
    }
    async void BarCode(string pBarCode, string pType)
    {
        FileLogger.WriteLogMessage("SplashPage", "BarCode", $"BarCode: {pBarCode}, Type: {pType}"); 
        await Navigation.PushAsync( new UPriceChecker(pBarCode));
    }
    private async void OnBarcodeEntered(object sender, EventArgs e)
    {
        var barcode = BarcodeEntry.Text?.Trim();
        if (!string.IsNullOrEmpty(barcode))
        {

            bool isStaff = true; // Replace with actual logic to determine if the user is staff

            await Navigation.PushAsync(isStaff ? new AdminPriceChecker(barcode): new UPriceChecker(barcode));
        }
    }
}