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
    public partial class DocsStandart : ContentPage
    {
        Connector.Connector Con;
        public string col { get; set; } = "#ffffff";
        public ObservableCollection<Doc> MyDocsR { get; set; }
        public DocsStandart(int pTypeDoc = -1)
        {
            DB db = DB.GetDB();
            Con = Connector.Connector.GetInstance();
            InitializeComponent();
            //Routing.RegisterRoute(nameof(Item), typeof(Item));
            Con.LoadDocsData(pTypeDoc, null, false);

            MyDocsR = new ObservableCollection<Doc>(db.GetDoc(1));
            this.BindingContext = this;
        }
    }
}