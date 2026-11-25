using BL;
using BL.Connector;
using BRB5;
using BRB5.Model;
using System.Collections.ObjectModel;

namespace BRB6.View
{
    public partial class ScanBarCode
    {
        private readonly TypeDoc TypeDoc;
        private Connector c = ConnectorBase.GetInstance();
        DB db => DB.Db;
        public int OrderDoc { get; set; }
        public bool IsSoftKeyboard { get { return Config.IsSoftKeyboard; } }
        public ObservableCollection<DocVM> Doc { get; set; } = [];
        public ScanBarCode(TypeDoc pTypeDoc)
        {
            NokeyBoard();
            TypeDoc = pTypeDoc;
            
            var r = db.GetDoc(TypeDoc);            
            BindingContext = this;
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            Config.BarCode = BarCode;
        }

        void BarCode(string pBarCode)
        {
            DocVM D = new() { TypeDoc = TypeDoc.CodeDoc, NumberDoc = pBarCode, BarCode = pBarCode, DateDoc = DateTime.Now };
            db.ReplaceDoc([D]);
            Task.Run(async () => 
            { 
                var r = await c.SendDocsDataAsync(D,null);
                if (r.Success)
                {
                    D.State = 1;
                    db.ReplaceDoc([D]);
                }
                D.IsNeedSave = !r.Success;
                Doc.Add(D);
            });

        }


        private async Task OnSaveAsync(object sender, TappedEventArgs e)
        {        
            foreach(var el in Doc)
            {
                if (!el.IsSavedState)
                {
                    var r = await c.SendDocsDataAsync(el, null);
                    if (r.Success)
                    {
                        el.State = 1;
                        db.ReplaceDoc([el]);
                    }
                    el.IsNeedSave = !r.Success;
                }

            }
                // Your click logic here
                DisplayAlert("Tapped", $"You clicked", "OK");            
        }
        public void Dispose() { Config.BarCode -= BarCode; }
        
    }
}