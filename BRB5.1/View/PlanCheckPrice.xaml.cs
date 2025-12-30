using BL;
using BL.Connector;
using BRB5.Model;
using System.Collections.ObjectModel;

namespace BRB6.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PlanCheckPrice 
    {

        DB db = DB.GetDB();
        Connector c;

        public ObservableCollection<DocVM> PromotionList { get; set; }
        public int Selection { get; set; } = 0;
        public PlanCheckPrice()
        {
            InitializeComponent();
            c = ConnectorBase.GetInstance();

            var temp = c.GetPromotion(Config.CodeWarehouse).Result;
            if (temp.Data == null)
            {
                PromotionList = new ObservableCollection<DocVM>();
                _ = DisplayAlert("Помилка", temp.TextError, "OK");
            }
            else
            {
                PromotionList = new ObservableCollection<DocVM>(temp.Data);
                foreach (var doc in temp.Data)
                {
                    doc.TypeDoc = 13;
                }
                db.ReplaceDoc(temp.Data.Select(el=> (Doc) el.Clone() ));
            }

            this.BindingContext = this;
        }

        private void PromotionSelect(object sender, EventArgs e)
        {
            if (Selection > 0)
            {
                Button button = (Button)sender;
                Cell cc = button.Parent as Cell;
                var vDoc = cc.BindingContext as DocVM;

                _ = Navigation.PushAsync(new PlanCheckerPrice(vDoc, Selection));
            }   else _ = DisplayAlert("","Оберіть тип стелажу", "ok");
        }
    }
}