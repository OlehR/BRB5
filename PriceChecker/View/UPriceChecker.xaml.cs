using BL;
using BRB5;
using BRB5.Model;
using UtilNetwork;

namespace PriceChecker.View;

public partial class UPriceChecker : ContentPage
{
    DB db = DB.GetDB();
    BL.BL bl = BL.BL.GetBL();

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
        }
    }

    public bool IsVisPriceOpt => WP != null && (WP.PriceOpt != 0);

    public bool IsVisBarcode => true;

    public string TextColorPrice =>
        (WP != null && WP.Price != 0 )
            ? "#009800"
            : "#ff5c5c";

    public UPriceChecker()
    {
        _WP = new();
        InitializeComponent();
        this.BindingContext = this;
        bl.ClearWPH();
        Config.BarCode = BarCode;
    }
    private void BarCodeHandInput(object sender, EventArgs e)
    {
        var text = BarCodeInput.Text;
        FoundWares(text, true);
    }
    void BarCode(string pBarCode) => FoundWares(pBarCode, false);

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
        }
    }
}