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
        public int OrderDoc { get; set; }
        public bool IsSoftKeyboard { get { return Config.IsSoftKeyboard; } }
        public ObservableCollection<DocWaresEx> DocWares { get; set; } = new ObservableCollection<DocWaresEx>();
        public ManualInput (DocVM pDoc, TypeDoc pTypeDoc)
        {
            NokeyBoard();
            TypeDoc = pTypeDoc;
            Doc = pDoc;
            var r = db.GetDocWares(Doc, 1, eTypeOrder.Scan);
            if (r != null)
            {
                IEnumerable<DocWaresEx> filtered;

                if (Doc.CodeReason == 1)
                {
                    filtered = r.Where(x => x.QuantityOrder != 0 && x.CodeReason == -1);
                }
                else
                {
                    filtered = r.Where(x => x.QuantityOrder != 0);
                }

                foreach (var item in filtered)
                {
                    item.QuantityOld = item.InputQuantity;
                    DocWares.Add(item);
                }

                OrderDoc = r.Max(el=> el.OrderDoc);
            }
            BindingContext = this;
            InitializeComponent();
        }       

        private void Save(object sender, EventArgs e)
        {
            if (sender is Entry entry)
            {
                var w = entry.BindingContext as DocWaresEx;
                if (w.QuantityOld != w.InputQuantity)
                {
                    w.Quantity = w.InputQuantity;
                    w.QuantityOld = w.InputQuantity;
                    w.OrderDoc = ++OrderDoc;
                    db.ReplaceDocWares(w, true);
                }
            }
        }
    }
}