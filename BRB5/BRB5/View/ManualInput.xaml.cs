using BRB5.Model;
using System.Collections.ObjectModel;

namespace BRB5.View
{
    public partial class ManualInput
    {
        private readonly TypeDoc TypeDoc;
        private Doc Doc;
        private Connector.Connector c = Connector.Connector.GetInstance();
        protected DB db = DB.GetDB();
        public ObservableCollection<DocWaresEx> DocWares { get; set; } = new ObservableCollection<DocWaresEx>();
        public ManualInput (DocId pDocId, TypeDoc pTypeDoc)
        {
            TypeDoc = pTypeDoc;
            Doc = new Doc(pDocId);
            var r = db.GetDocWares(Doc, 1, eTypeOrder.Scan);
            if (r != null)
                foreach (var item in r)
                    if(item.QuantityOrder!=0)
                        DocWares.Add(item);
            BindingContext = this;
            InitializeComponent();
        }
    }
}