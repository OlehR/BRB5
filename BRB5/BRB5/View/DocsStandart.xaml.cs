using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BRB5.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DocsStandart : ContentPage
    {
        private Connector.Connector c;
        public string col { get; set; } = "#ffffff";
        public ObservableCollection<Doc> MyDocsR { get; set; }

        public ICommand F3Scan => new Command(OnF3Scan);
        public DocsStandart(int pTypeDoc = -1)
        {
            DB db = DB.GetDB();
            c = Connector.Connector.GetInstance();
            InitializeComponent();
            //Routing.RegisterRoute(nameof(Item), typeof(Item));
            c.LoadDocsData(pTypeDoc, null, false);

            MyDocsR = new ObservableCollection<Doc>(db.GetDoc(1));
            this.BindingContext = this;
        }

        private async void OnF3Scan()
        {
            await Navigation.PushAsync(new Scan());
        }
    }
}