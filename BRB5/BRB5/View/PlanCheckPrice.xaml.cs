using BL;
using BL.Connector;
using BRB5.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BRB5.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PlanCheckPrice : ContentPage
    {

        DB db = DB.GetDB();
        Connector c;

        public ObservableCollection<Doc> PromotionList { get; set; }
        public int Selection { get; set; } = 0;
        public PlanCheckPrice()
        {
            InitializeComponent();
            c = Connector.GetInstance();

            var temp = c.GetPromotion(Config.CodeWarehouse);
            if (temp.Info == null)
            {
                PromotionList = new ObservableCollection<Doc>();
                _ = DisplayAlert("Помилка", temp.TextError, "OK");
            }
            else
            {
                PromotionList = new ObservableCollection<Doc>(temp.Info);
                foreach (var doc in temp.Info)
                {
                    doc.TypeDoc = 13;
                }
                db.ReplaceDoc(temp.Info);
            }

            this.BindingContext = this;
        }

        private void PromotionSelect(object sender, EventArgs e)
        {
            if (Selection > 0)
            {
                Button button = (Button)sender;
                Cell cc = button.Parent as Cell;
                var vDoc = cc.BindingContext as Doc;

                _ = Navigation.PushAsync(new PlanCheckerPrice(vDoc, Selection));
            }   else _ = DisplayAlert("","Оберіть тип стелажу", "ok");
        }
    }
}