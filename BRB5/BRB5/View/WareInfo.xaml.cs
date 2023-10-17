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
        Connector.Connector c;
        public WaresPrice WP { get; set; }
        private string _Promotion;
        public string Promotion { get { return _Promotion; } set { _Promotion = value; OnPropertyChanged(nameof(Promotion)); } }
        public bool IsVisPromotion {  get; set; }  = false;
        public string ImageUri { get; set; } = "Photo.png"; 
        public bool IsEnabledPrint { get { return Config.TypeUsePrinter != eTypeUsePrinter.NotDefined; } }
        public WareInfo(ParseBarCode parseBarCode)
        {
            InitializeComponent();
            c = Connector.Connector.GetInstance();
            NavigationPage.SetHasNavigationBar(this, Device.RuntimePlatform == Device.iOS);
            
            WP = c.GetPrice(parseBarCode, eTypePriceInfo.Full);
           
            if (WP.ActionType >0) 
            {
                Promotion = $"Акція діє: з {WP.PromotionBegin:dd.MM}  по {WP.PromotionEnd:dd.MM}";
                IsVisPromotion = true;
            }

            ImageUri = Config.ApiUrl1+$"Wares/{WP.CodeWares:D9}.png";
            WareImage.Source = new UriImageSource
            {
                Uri = new Uri(ImageUri),
                CachingEnabled = true,
                CacheValidity = new TimeSpan(7, 0, 0, 0)
            };

            this.BindingContext = this;
        }

        private void OnClickPrint(object sender, EventArgs e)
        {
            if (IsEnabledPrint) 
                _ = DisplayAlert("Друк", c.PrintHTTP(new[] { WP.CodeWares }), "OK");            
        }

        private void OnClickMenu(object sender, EventArgs e)
        {

        }
    }
}