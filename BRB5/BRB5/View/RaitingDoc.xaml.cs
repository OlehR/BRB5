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
    public partial class RaitingDoc
    {
        Connector.Connector c;
        DB db = DB.GetDB();
        private readonly TypeDoc TypeDoc;
        public ObservableCollection<Doc> MyDoc { get; set; } = new ObservableCollection<Doc>();
        //public string Help { get; set; } = "ERHHHHHHH54";
        public RaitingDoc(TypeDoc pTypeDoc )
        {
            TypeDoc= pTypeDoc;
            _ = Config.GetCurrentLocation(db.GetWarehouse());
            c = BRB5.Connector.Connector.GetInstance();
            InitializeComponent();
            Routing.RegisterRoute(nameof(RaitingDocItem), typeof(RaitingDocItem));
            c.LoadDocsData(pTypeDoc.CodeDoc, null, false);
            NavigationPage.SetHasNavigationBar(this, Device.RuntimePlatform == Device.iOS);
            MyDoc = new ObservableCollection<Doc> ( db.GetDoc(TypeDoc).OrderByDescending(el=>el.NumberDoc));
            this.BindingContext = this;            
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            var r = db.GetDoc(TypeDoc).OrderByDescending(el => el.NumberDoc);
            if (r != null)
            {
                MyDoc.Clear();
                foreach (var item in r)
                    MyDoc.Add(item);
            }
        }

        private async void OnButtonClicked(object sender, System.EventArgs e)
        {
            /*var p = $"{nameof(Item)}?{nameof(Item.NumberDoc)}=\"{vDoc.NumberDoc}\"&TypeDoc={vDoc.TypeDoc}";
             Shell.Current.GoToAsync(p);*/

            var s = sender as Grid;
            var vDoc = s.BindingContext as Doc;
            await Navigation.PushAsync(new RaitingDocItem(vDoc));
        }

        private async void Grid_Focused(object sender, FocusEventArgs e)
        {
            Grid cc = sender as Grid;
            var vDoc = cc.BindingContext as Doc;
            await Navigation.PushAsync(new RaitingDocItem(vDoc));
           // await Shell.Current.GoToAsync($"{nameof(Item)}?{nameof(Item.NumberDoc)}={vDoc.NumberDoc}");//&TypeDoc={vDoc.TypeDoc}
        }
    }
}