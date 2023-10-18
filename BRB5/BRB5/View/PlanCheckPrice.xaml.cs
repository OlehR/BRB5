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
        Connector.Connector c;

        public ObservableCollection<Doc> PromotionList { get; set; }
        public int Selection { get; set; } = 0;
        public PlanCheckPrice()
        {
            InitializeComponent();
            c = Connector.Connector.GetInstance();

            PromotionList = new ObservableCollection<Doc>
            {
                new Doc() { TypeDoc = 13, NumberDoc = "1234589", Description = "action" },
                new Doc() { TypeDoc = 13, NumberDoc = "1234589", Description = "action1" },
                new Doc() { TypeDoc = 13, NumberDoc = "1234589", Description = "action2" },
                new Doc() { TypeDoc = 13, NumberDoc = "1234589", Description = "action3" },
                new Doc() { TypeDoc = 13, NumberDoc = "1234589", Description = "action4" },
                new Doc() { TypeDoc = 13, NumberDoc = "1234589", Description = "action5" },
                new Doc() { TypeDoc = 13, NumberDoc = "1234589", Description = "action6" }
            };


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