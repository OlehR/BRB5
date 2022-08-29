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
        public ObservableCollection<DocWaresEx> MyDocWares { get; set; }
        public ICommand F2Save => new Command(OnF2Save);
        public ICommand F3Scan => new Command(OnF3Scan);
        public ICommand F4WrOff => new Command(OnF4WrOff);
        public ICommand F6Doc => new Command(OnF6Doc);
        public DocStandart(DocId pDocId, int pTypeResult, eTypeOrder pTypeOrder)
        {
            DB db = DB.GetDB();
            InitializeComponent();

            var r = db.GetDocWares(pDocId, pTypeResult, pTypeOrder);
            if(r != null)
                MyDocWares = new ObservableCollection<DocWaresEx>(r);
            this.BindingContext = this;
        }
        private async void OnF2Save()
        {

        }
        private async void OnF3Scan()
        {
            await Navigation.PushAsync(new Scan());
        }
        private async void OnF4WrOff()
        {

        }
        private async void OnF6Doc()
        {

        }
    }
}