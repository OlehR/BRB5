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
using Xamarin.CommunityToolkit.Extensions;
using System.Collections.Specialized;

namespace BRB5.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DocStandart : ContentPage
    {
        private readonly TypeDoc TypeDoc;
        
        private DocId DocId;
        private Connector.Connector c = Connector.Connector.GetInstance(); 
        protected DB db = DB.GetDB();
        public ObservableCollection<DocWaresEx> MyDocWares { get; set; } = new ObservableCollection<DocWaresEx>();
        public DocStandart(DocId pDocId,  TypeDoc pTypeDoc)
        {
            TypeDoc = pTypeDoc;
            DocId = pDocId;           
            BindingContext = this;
            InitializeComponent();
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            var r = db.GetDocWares(DocId, 1, eTypeOrder.Scan);
            if (r != null)
            {
                MyDocWares.Clear();
                foreach (var item in r)
                    MyDocWares.Add(item);
            }
        }
        private void F2(object sender, EventArgs e)
        {
            F2Save();
        }
        private async Task F2Save()
        {
            var r = c.SendDocsData(new Doc(DocId), db.GetDocWares(DocId, 2, eTypeOrder.Scan));
            if (r.State != 0) await DisplayAlert("Помилка", r.TextError, "OK");
            else await this.DisplayToastAsync("Документ успішно збережений");
        }

        private async void F3Scan(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Scan(DocId, TypeDoc));
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