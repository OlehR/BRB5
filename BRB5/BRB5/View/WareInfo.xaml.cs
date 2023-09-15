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
        public WareInfo(ParseBarCode parseBarCode)
        {
            c = Connector.Connector.GetInstance();
            
            //
            WP = c.GetPrice(parseBarCode);
            WP.LastArrivalDate= DateTime.Now;
            WP.LastArrivalQuantity= 20.9M;
            WP.StateWare = 1;
            WP.TermsForIlliquidWare = eTermsForIlliquidWare.FullRefund;
            var t = new DefectBalance { Date = WP.LastArrivalDate , Quantity=100 ,  WH=new Warehouse { Name="aisbcild"} };
            WP.BalanceDefects = new List<DefectBalance> { t,  t  };
            //
            if (WP.ActionType == 1) 
            {
                Promotion = "Акція діє: з " + WP.PromotionBegin.ToString("dd.MM") + " по " + WP.PromotionEnd.ToString("dd.MM");
                IsVisPromotion = true;
            }

            ImageUri = "http://api.spar.uz.ua/Wares/" + WP.CodeWares.ToString("D9") + ".png";

            InitializeComponent();
            this.BindingContext = this;
        }

        private void OnClickPrint(object sender, EventArgs e)
        {

        }

        private void OnClickMenu(object sender, EventArgs e)
        {

        }

        private void BarCode(object sender, EventArgs e)
        {

        }
    }
}