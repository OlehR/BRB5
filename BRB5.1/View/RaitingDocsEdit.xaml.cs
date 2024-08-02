using BL;
using BL.Connector;
using BRB5.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ZXing;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace BRB5.View
{
    
    public partial class RaitingDocsEdit
	{
        private readonly TypeDoc TypeDoc;
        Connector c;
        DB db = DB.GetDB();
        private ObservableCollection<DocVM> _RD;
        public ObservableCollection<DocVM> RD { get { return _RD; } set { _RD = value; OnPropertyChanged(nameof(RD)); } }

        public RaitingDocsEdit (TypeDoc vTypeDoc)
		{
			InitializeComponent ();
            TypeDoc = vTypeDoc;
            TypeDoc.CodeDoc = 11;
            c = Connector.GetInstance();  
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
                    RD = new ObservableCollection<DocVM>();
                    _ = DisplayAlert("Помилка", temp.TextError, "OK");
                }
                else
                {
                    var Docs = db.GetDoc(new TypeDoc { CodeDoc = 11 });
                    RD = new ObservableCollection<DocVM>(Docs);
                }

                var tempWH = db.GetWarehouse()?.ToList();
                var tempRT = db.GetRaitingTemplate()?.ToList();
                if (tempWH != null)
                    foreach (DocVM d in RD)
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
            await Navigation.PushAsync(new RaitingDocEdit(new DocVM() { DateDoc = DateTime.Today, NumberDoc = c.GetNumberDocRaiting().Info }, TypeDoc));
        }

        private async void Edit(object sender, EventArgs e)
        {
            ImageButton cc = sender as ImageButton;
            var vDoc = cc.BindingContext as DocVM;
            await Navigation.PushAsync(new RaitingDocEdit(vDoc, TypeDoc));
        }
    }
}