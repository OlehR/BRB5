using BRB5.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.CommunityToolkit.Extensions;

namespace BRB5.View
{
    public partial class DocStandart
    {
        private readonly TypeDoc TypeDoc;
        
        private Doc Doc;
        private Connector.Connector c = Connector.Connector.GetInstance(); 
        protected DB db = DB.GetDB();
        string _NumberOutInvoice = "";
        public string NumberOutInvoice { get { return _NumberOutInvoice; } set { _NumberOutInvoice = value; OnPropertyChanged("NumberOutInvoice"); } }
        public List<DataStr> ListDataStr { 
            get { 
                var list = new List<DataStr>();
                for(int i=0; i<10; i++)
                    list.Add(new DataStr(DateTime.Today.AddDays(-1 * i)));
                
                return list;
                }
        }
        public int SelectedDataStr { get; set; } = 0;
        bool _IsVisibleDocF6 = false;
        public bool IsVisibleDocF6 { get { return _IsVisibleDocF6; } set { _IsVisibleDocF6 = value; OnPropertyChanged("IsVisibleDocF6"); } } 
        public ObservableCollection<DocWaresEx> MyDocWares { get; set; } = new ObservableCollection<DocWaresEx>();
        public DocStandart(DocId pDocId,  TypeDoc pTypeDoc)
        {
            TypeDoc = pTypeDoc;
            Doc = new Doc(pDocId);           
            BindingContext = this;
            InitializeComponent();
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            var r = db.GetDocWares(Doc, 1, eTypeOrder.Scan);
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
            Doc.NumberOutInvoice = NumberOutInvoice;
            Doc.DateOutInvoice = ListDataStr[SelectedDataStr].DateString;
            var r = c.SendDocsData(Doc, db.GetDocWares(Doc, 2, eTypeOrder.Scan));
            if (r.State != 0) await DisplayAlert("Помилка", r.TextError, "OK");
            else await this.DisplayToastAsync("Документ успішно збережений");
        }

        private async void F3Scan(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Scan(Doc, TypeDoc));
        }

        private async void F4WrOff(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ManualInput(Doc, TypeDoc));
        }

        private void F6Doc(object sender, EventArgs e)
        {
            IsVisibleDocF6 = !IsVisibleDocF6;
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