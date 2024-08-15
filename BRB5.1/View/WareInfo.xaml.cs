using BL;
using BL.Connector;
using BRB5.Model;
using BRB5;

namespace BRB6.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WareInfo : ContentPage
    {
        DB db = DB.GetDB(); 
        Connector c;
        public WaresPrice WP { get; set; }
        public bool IsVisPromotion {  get; set; }  = false;
        private bool _IsNotFullScreenImg = true;
        public bool IsNotFullScreenImg { get { return _IsNotFullScreenImg; } set { _IsNotFullScreenImg = value; OnPropertyChanged(nameof(IsNotFullScreenImg));  } }
        private bool _IsFullScreenImg = false;
        public bool IsFullScreenImg { get { return _IsFullScreenImg; } set { _IsFullScreenImg = value; OnPropertyChanged(nameof(IsFullScreenImg)); } }
        private bool _IsVisIOSFull = false;
        public bool IsVisIOSFull { get { return _IsVisIOSFull; } set { _IsVisIOSFull = value; OnPropertyChanged(nameof(IsVisIOSFull)); } }
        public string ImageUri { get; set; } = "Photo.png"; 
        public bool IsEnabledPrint { get { return Config.TypeUsePrinter != eTypeUsePrinter.NotDefined; } }
        public UriImageSource Picture { get; set; }
        public Uri UriPicture { get { return new Uri(Config.ApiUrl1 + $"Wares/{WP.CodeWares:D9}.png"); } }

        public WareInfo(ParseBarCode parseBarCode)
        {
            InitializeComponent();
            c = Connector.GetInstance();
            // TODO Xamarin.Forms.Device.RuntimePlatform is no longer supported. Use Microsoft.Maui.Devices.DeviceInfo.Platform instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
            NavigationPage.SetHasNavigationBar(this, Device.RuntimePlatform == Device.iOS);            
            WP = c.GetPrice(parseBarCode, eTypePriceInfo.Full);           
            if (WP.ActionType > 0)  IsVisPromotion = true;            
            ImageUri = Config.ApiUrl1+$"Wares/{WP.CodeWares:D9}.png";            
            Picture  = new UriImageSource
            {
                Uri = new Uri(ImageUri),
                CachingEnabled = false,
                CacheValidity = new TimeSpan(7, 0, 0, 0)
            };
            // TODO Xamarin.Forms.Device.RuntimePlatform is no longer supported. Use Microsoft.Maui.Devices.DeviceInfo.Platform instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
            if (Device.RuntimePlatform == Device.iOS) 
            {
                WareImageIOS.IsVisible = true; 
                WareImage.IsVisible = false;
            } 
            this.BindingContext = this;
        }

        private void OnClickPrint(object sender, EventArgs e) {  if (IsEnabledPrint)  _ = DisplayAlert("Друк", c.PrintHTTP(new[] { WP.CodeWares }), "OK");     }
        private void OnImageTapped(object sender, EventArgs e)
        {
            IsNotFullScreenImg = !IsNotFullScreenImg;
            // TODO Xamarin.Forms.Device.RuntimePlatform is no longer supported. Use Microsoft.Maui.Devices.DeviceInfo.Platform instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
            if (Device.RuntimePlatform == Device.iOS)
                 IsVisIOSFull = !IsVisIOSFull;
            else IsFullScreenImg = !IsFullScreenImg;
        }
    }
}