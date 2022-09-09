using BRB5.Model;
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
    public partial class DocStandart : ContentPage
    {
        private int myTypeDoc;
        public ObservableCollection<DocWaresEx> MyDocWares { get; set; }
        public DocStandart(DocId pDocId, int pTypeResult, eTypeOrder pTypeOrder, int pTypeDoc)
        {
            DB db = DB.GetDB();
            myTypeDoc = pTypeDoc;
            var r = db.GetDocWares(pDocId, pTypeResult, pTypeOrder);
            if(r != null)
                MyDocWares = new ObservableCollection<DocWaresEx>(r);
            this.BindingContext = this;
            InitializeComponent();
        }

        private void F2Save(object sender, EventArgs e)
        {

        }

        private async void F3Scan(object sender, EventArgs e)
        {

            await Navigation.PushAsync(new Scan(myTypeDoc));
        }

        private void F4WrOff(object sender, EventArgs e)
        {

        }

        private void F6Doc(object sender, EventArgs e)
        {

        }
    }
    public class AlternateColorDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate EvenTemplate { get; set; }
        public DataTemplate UnevenTemplate { get; set; }

        private int indexer = 0;
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            // TODO: Maybe some more error handling here
            return indexer++ % 2 == 0 ? EvenTemplate : UnevenTemplate;
        }
    }
}