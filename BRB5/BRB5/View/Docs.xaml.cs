//using BRB5.Model;
using BRB5.Model;
using BRB5.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BRB5
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Docs : ContentPage
    {
        Connector.Connector c;
        public ObservableCollection<Doc> MyDoc { get; set; }
        //public string Help { get; set; } = "ERHHHHHHH54";
        public Docs(TypeDoc TypeDoc )
        {
            DB db =  DB.GetDB();
            _ = Config.GetCurrentLocation(db.GetWarehouse());
            c = Connector.Connector.GetInstance();
            InitializeComponent();
            Routing.RegisterRoute(nameof(Item), typeof(Item));
            c.LoadDocsData(11, null, false);

            MyDoc = new ObservableCollection<Doc> ( db.GetDoc(TypeDoc).OrderByDescending(el=>el.NumberDoc));
            this.BindingContext = this;            

        }

        private async void OnButtonClicked(object sender, System.EventArgs e)
        {
            /*var p = $"{nameof(Item)}?{nameof(Item.NumberDoc)}=\"{vDoc.NumberDoc}\"&TypeDoc={vDoc.TypeDoc}";
             Shell.Current.GoToAsync(p);*/

            var s = sender as Grid;
            var vDoc = s.BindingContext as Doc;
            await Navigation.PushAsync(new Item(vDoc));
        }

        private async void Grid_Focused(object sender, FocusEventArgs e)
        {
            Grid cc = sender as Grid;
            var vDoc = cc.BindingContext as Doc;
            await Navigation.PushAsync(new Item(vDoc));
           // await Shell.Current.GoToAsync($"{nameof(Item)}?{nameof(Item.NumberDoc)}={vDoc.NumberDoc}");//&TypeDoc={vDoc.TypeDoc}
        }
    }
}