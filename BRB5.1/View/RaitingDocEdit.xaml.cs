using BL;
using BL.Connector;
using BRB5;
using BRB5.Model;

namespace BRB51.View
{
    public partial class RaitingDocEdit 
    {
        DB db = DB.GetDB();
        Connector c;
        private readonly TypeDoc TypeDoc;
        public List<RaitingTemplate> RT { get { return db.GetRaitingTemplate().Where(t => t.Text != null).ToList(); } }

        public int SelectedTemplate { get; set; }
        
        private DocVM _RD;
        public DocVM RD { get { return _RD; } set { _RD = value; OnPropertyChanged(nameof(RD)); } }
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
        public int SelectedWarehouse { get; set; }
        public bool IsSoftKeyboard { get { return Config.IsSoftKeyboard; } }
        public RaitingDocEdit (DocVM doc, TypeDoc vTypeDoc)
		{
			InitializeComponent ();
            c = Connector.GetInstance();
            RD = doc;
            TypeDoc = vTypeDoc;
            SelectedTemplate = RT.FindIndex(t => t.IdTemplate == RD.IdTemplate);
            SelectedWarehouse = ListWarehouse.FindIndex(t => t.CodeWarehouse == RD.CodeWarehouse);
            this.BindingContext = this;
        }

        private async void Save(object sender, EventArgs e)
        {
            if (SelectedWarehouse >= 0 && SelectedTemplate >= 0 && !string.IsNullOrEmpty(RD.Description))
            {
                RD.IdTemplate = RT.ElementAt(SelectedTemplate).IdTemplate;
                RD.CodeWarehouse = ListWarehouse.ElementAt(SelectedWarehouse).CodeWarehouse;

                _ = DisplayAlert("збереження", c.SaveDocRaiting(RD).TextError, "OK");

                await Navigation.PopAsync();
            } else _ = DisplayAlert("збереження", "заповніть всі дані", "OK");
        }
    }
}