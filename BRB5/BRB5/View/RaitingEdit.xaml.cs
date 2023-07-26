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
	public partial class RaitingEdit : ContentPage
	{
        private Doc _RD;
        DB db = DB.GetDB();
        private readonly TypeDoc TypeDoc;

        private RaitingTemplate _SelectedTemplate;
        public RaitingTemplate SelectedTemplate { get { return _SelectedTemplate; } set { _SelectedTemplate = value; OnPropertyChanged(nameof(SelectedTemplate)); } }
        public List<RaitingTemplate> RT { get { return db.GetRaitingTemplate().ToList(); } }
        public Doc RD { get { return _RD; } set { _RD = value; OnPropertyChanged(nameof(RD)); } }
        public List<Warehouse> ListWarehouse
        {
            get
            {
                List<Warehouse> wh = null;
                try
                {
                    wh = db.GetWarehouse()?.ToList();

                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
                if (wh == null || !wh.Any())
                    wh = new List<Warehouse>() { new Warehouse() { Code = 0, Name = "ddd" } };
                return wh;

            }
        }
        private Warehouse _SelectedWarehouse;
        public Warehouse SelectedWarehouse { get { return _SelectedWarehouse; } set { _SelectedWarehouse = value; OnPropertyChanged(nameof(SelectedWarehouse)); } }
        public RaitingEdit (Doc doc, TypeDoc vTypeDoc)
		{
			InitializeComponent ();
            RD = doc;
            TypeDoc = vTypeDoc;

            this.BindingContext = this;
        }

        private async void Save(object sender, EventArgs e)
        {
            RD.IdTempate = SelectedTemplate.Id;
            RD.CodeWarehouse = SelectedWarehouse.CodeWarehouse;


            await Navigation.PushAsync(new RaitingDocs(TypeDoc));

        }
    }
}