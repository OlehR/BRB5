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
        private int myTypeDoc;

        public ObservableCollection<Doc> MyDocsR { get; set; }

        public DocsStandart(int pTypeDoc = -1)
        {
            myTypeDoc = pTypeDoc;
            DB db = DB.GetDB();
            c = Connector.Connector.GetInstance();
            c.LoadDocsData(pTypeDoc, null, false);

            MyDocsR = new ObservableCollection<Doc>(db.GetDoc(1));
            this.BindingContext = this;

            InitializeComponent();
        }


        private async void OpenDoc(object sender, EventArgs e)
        {
            var s = sender as StackLayout;

            var vDoc = s.BindingContext as Doc;
            
            await Navigation.PushAsync(new DocStandart(vDoc, 1, eTypeOrder.Scan));
        }
    }
}