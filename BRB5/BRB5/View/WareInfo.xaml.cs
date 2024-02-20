using BL;
using BL.Connector;
using BRB5.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BRB5.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WareInfo : ContentPage
    {
        DB db = DB.GetDB(); 
        Connector c;
        public WaresPrice WP { get; set; }
        public bool IsVisPromotion {  get; set; }  = false;
        public bool IsNotFullScreenImg { get; set; } = true;
        public bool IsFullScreenImg { get; set; } = false;
        public string ImageUri { get; set; } = "Photo.png"; 
        public bool IsEnabledPrint { get { return Config.TypeUsePrinter != eTypeUsePrinter.NotDefined; } }
        public UriImageSource Picture { get; set; }
        public Uri UriPicture { get { return new Uri(Config.ApiUrl1 + $"Wares/{WP.CodeWares:D9}.png"); } }

        public WareInfo(ParseBarCode parseBarCode)
        {
            InitializeComponent();
            c = Connector.GetInstance();
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
           
            //WareImage.Source= new Uri(;
            WareImageFull.Source=Picture;
            this.BindingContext = this;
        }

        private void OnClickPrint(object sender, EventArgs e)
        {
            if (IsEnabledPrint) 
                _ = DisplayAlert("Друк", c.PrintHTTP(new[] { WP.CodeWares }), "OK");            
        }

        private void OnImageTapped(object sender, EventArgs e)
        {
            WareImageFull.IsVisible = !WareImageFull.IsVisible;
            WareInfoMain.IsVisible = !WareInfoMain.IsVisible;
        }
    }
}