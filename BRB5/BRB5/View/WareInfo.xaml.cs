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
        public WareInfo(ParseBarCode parseBarCode)
        {
            c = Connector.Connector.GetInstance();
            
            //

            WP = c.GetPrice(parseBarCode);
            WP.LastArrivalDate= DateTime.Now;
            WP.LastArrivalQuantity= 20.9M;
            WP.StateWare = 1;
            WP.TermsForIlliquidWare = eTermsForIlliquidWare.FullRefund;

            InitializeComponent();
            this.BindingContext = this;
        }
    }
}