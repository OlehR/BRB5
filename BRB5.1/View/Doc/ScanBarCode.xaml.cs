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
            Doc = new(db.GetDoc(TypeDoc).Where(el => el.State == 0));            
            BindingContext = this;
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            Config.BarCode = BarCode;
        }

        void BarCode(string pBarCode)
        {
            if (Doc.Where(el => el.BarCode.Equals(pBarCode)).Any())
                return;
            DocVM D = new() { TypeDoc = TypeDoc.CodeDoc, NumberDoc = pBarCode, BarCode = pBarCode, DateDoc = DateTime.Now, CodeWarehouse= Config.CodeWarehouse};
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


        private async void OnSaveAsync(object sender, TappedEventArgs e)
        {
            int i = 0;
            foreach(var el in Doc)
            {
                if (el.State==0)
                {
                    var r = await c.SendDocsDataAsync(el, null);
                    if (r.Success)
                    {
                        el.State = 1;
                        db.ReplaceDoc([el]);
                        i++;
                    }
                    el.IsNeedSave = !r.Success;
                }

            }
                DisplayAlert("Збереженя", $"Зберегло {i}{Doc.Select(el=>el.State==0).Count()}","OK");            
        }
        public void Dispose() { Config.BarCode -= BarCode; }

        
    }
}