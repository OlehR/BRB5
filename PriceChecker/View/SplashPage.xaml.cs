namespace PriceChecker.View;

public partial class SplashPage : ContentPage
{
    public SplashPage()
    {
        InitializeComponent();
    }

    private async void OnBarcodeEntered(object sender, EventArgs e)
    {
        var barcode = BarcodeEntry.Text?.Trim();
        if (!string.IsNullOrEmpty(barcode))
        {
            var priceCheckerPage = new UPriceChecker(barcode);
            await Navigation.PushAsync(priceCheckerPage);
        }
    }
}