using BRB5.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using ZXing;

namespace BRB5.View
{
    
    public partial class RaitingDocsEdit
	{
        private readonly TypeDoc TypeDoc;
        BRB5.Connector.Connector c;
        DB db = DB.GetDB();
        private ObservableCollection<Doc> _RD;
        public ObservableCollection<Doc> RD { get { return _RD; } set { _RD = value; OnPropertyChanged(nameof(RD)); } }

        public RaitingDocsEdit (TypeDoc vTypeDoc)
		{
			InitializeComponent ();
            TypeDoc = vTypeDoc;
            TypeDoc.CodeDoc = 11;
            c = Connector.Connector.GetInstance();  
            this.BindingContext = this;
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();

            Task.Run(async () =>
            {
                var temp = await c.GetRaitingDocsAsync();
                if (temp.Info == null)
                {
                    RD = new ObservableCollection<Doc>();
                    _ = DisplayAlert("Помилка", temp.TextError, "OK");
                }
                else RD = new ObservableCollection<Doc>(temp.Info);

                var tempWH = db.GetWarehouse()?.ToList();
                var tempRT = db.GetRaitingTemplate()?.ToList();
                if (tempWH != null)
                    foreach (Doc d in RD)
                        try
                        {
                            d.CodeWarehouseName = tempWH.FirstOrDefault(t => t.CodeWarehouse == d.CodeWarehouse).Name;
                            d.RaitingTemplateName = tempRT.FirstOrDefault(t => t.IdTemplate == d.IdTemplate).Text;
                        }
                        catch (Exception ex) { }
            });

        }

        private async void Create(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RaitingDocEdit(new Doc() { DateDoc = DateTime.Today, NumberDoc = c.GetNumberDocRaiting().Info }, TypeDoc));
        }

        private async void Edit(object sender, EventArgs e)
        {
            ImageButton cc = sender as ImageButton;
            var vDoc = cc.BindingContext as Doc;
            await Navigation.PushAsync(new RaitingDocEdit(vDoc, TypeDoc));
        }
    }
}