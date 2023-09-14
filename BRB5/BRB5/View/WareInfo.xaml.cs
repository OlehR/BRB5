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
            if (WP.ActionType == 0)
                Promotion = "Акція діє: з " +WP.PromotionBegin + " по "+WP.PromotionEnd;

            InitializeComponent();
            this.BindingContext = this;
        }
    }
}