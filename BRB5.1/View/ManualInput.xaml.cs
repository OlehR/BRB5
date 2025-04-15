using BL;
using BL.Connector;
using BRB5;
using BRB5.Model;
using System.Collections.ObjectModel;

namespace BRB6.View
{
    public partial class ManualInput
    {
        private readonly TypeDoc TypeDoc;
        private DocVM Doc;
        private Connector c = ConnectorBase.GetInstance();
        protected DB db = DB.GetDB();
        public bool IsSoftKeyboard { get { return Config.IsSoftKeyboard; } }
        public ObservableCollection<DocWaresEx> DocWares { get; set; } = new ObservableCollection<DocWaresEx>();
        public ManualInput (DocId pDocId, TypeDoc pTypeDoc)
        {
            NokeyBoard();
            TypeDoc = pTypeDoc;
            Doc = new DocVM(pDocId);
            var r = db.GetDocWares(Doc, 1, eTypeOrder.Scan);
            if (r != null)
                foreach (var item in r)
                    if(item.QuantityOrder!=0)
                        DocWares.Add(item);
            BindingContext = this;
            InitializeComponent();
        }       

        private void Save(object sender, EventArgs e)
        {

            if (sender is Entry entry)
            {
                var docVM = entry.BindingContext as DocVM;
            }
        }
    }
}