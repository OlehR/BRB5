using BRB5.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace BRB5.View
{
    
    public partial class RaitingDocs
	{
        private readonly TypeDoc TypeDoc;
        BRB5.Connector.Connector c;
        DB db = DB.GetDB();
        private ObservableCollection<Doc> _RD;
        public ObservableCollection<Doc> RD { get { return _RD; } set { _RD = value; OnPropertyChanged(nameof(RD)); } }

        public RaitingDocs (TypeDoc vTypeDoc)
		{
			InitializeComponent ();
            TypeDoc = vTypeDoc;
            TypeDoc.CodeDoc = 11;
            c = Connector.Connector.GetInstance();

            RD = new ObservableCollection<Doc>(c.GetRaitingDocs());
            var tempWH = db.GetWarehouse()?.ToList();
            var tempRT = db.GetRaitingTemplate()?.ToList();
            if (tempWH != null)
                foreach (Doc d in RD)
                    try 
                    { 
                        d.CodeWarehouseName = tempWH.FirstOrDefault(t =>t.CodeWarehouse == d.CodeWarehouse).Name;
                        d.RaitingTemplateName = tempRT.FirstOrDefault(t => t.Id == d.IdTemplate).Text;
                    } catch(Exception ex) { }

            this.BindingContext = this;
        }

        private async void Create(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RaitingEdit(new Doc() { DateDoc = DateTime.Today, NumberDoc = c.GetNumberDocRaiting().Info }, TypeDoc));
        }

        private async void Edit(object sender, EventArgs e)
        {
            ImageButton cc = sender as ImageButton;
            var vDoc = cc.BindingContext as Doc;
            await Navigation.PushAsync(new RaitingEdit(vDoc, TypeDoc));
        }
    }
}