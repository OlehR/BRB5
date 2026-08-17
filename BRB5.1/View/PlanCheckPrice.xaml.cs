using BL;
using BL.Connector;
using BRB5.Model;
using System.Collections.ObjectModel;
using UtilNetwork;

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
            PromotionList = new ObservableCollection<DocVM>();
            //Result<IEnumerable<DocVM>> temp = new();
            Task.Run(async () => { 
            var temp = await c.GetPromotion(Config.CodeWarehouse);
            if (temp?.Data == null)
            {
                PromotionList = new ObservableCollection<DocVM>();
                _ = DisplayAlert("Помилка", temp?.TextError, "OK");
            }
            else
            {
                    foreach (var doc in temp.Data)
                    {
                        doc.TypeDoc = 13;
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            PromotionList.Add(doc);    
                        });
                    }
                    db.ReplaceDoc(temp.Data.Select(el=> (Doc) el.Clone() ));
            }
            });
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