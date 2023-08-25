using BRB5.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BRB5.View
{
	public partial class RaitingEdit
	{
        DB db = DB.GetDB();
        BRB5.Connector.Connector c;
        private readonly TypeDoc TypeDoc;

        private RaitingTemplate _SelectedTemplate;
        public RaitingTemplate SelectedTemplate { get { return _SelectedTemplate; } set { _SelectedTemplate = value; OnPropertyChanged(nameof(SelectedTemplate)); } }
        public List<RaitingTemplate> RT { get { return db.GetRaitingTemplate().ToList(); } }
        private Doc _RD;
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
            c = Connector.Connector.GetInstance();
            RD = doc;
            TypeDoc = vTypeDoc;

            this.BindingContext = this;
        }

        private async void Save(object sender, EventArgs e)
        {
            RD.IdTemplate = SelectedTemplate.Id;
            RD.CodeWarehouse = SelectedWarehouse.CodeWarehouse;

            _ = DisplayAlert("збереження", c.SaveDocRaiting(RD).TextError, "OK");

            await Navigation.PushAsync(new RaitingDocs(TypeDoc));

        }
    }
}