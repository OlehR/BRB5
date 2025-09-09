//using BRB5.Model;

using BL;
using BL.Connector;
using BRB5;
using BRB5.Model;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui.Controls.Compatibility;
using System.Collections.ObjectModel;
using Grid = Microsoft.Maui.Controls.Grid;

using CommunityToolkit.Maui.Alerts;

namespace BRB6
{
    public partial class RaitingDoc: ContentPage
    {
        Connector c;
        DB db = DB.GetDB();
        private readonly TypeDoc TypeDoc;
        public ObservableCollection<DocVM> MyDoc { get; set; } = new ObservableCollection<DocVM>();

        double _PB = 0.95;
        public double PB { get { return _PB; } set { _PB = value; OnPropertyChanged(nameof(PB)); } }
        //public string Help { get; set; } = "ERHHHHHHH54";
        public RaitingDoc(TypeDoc pTypeDoc)
        {
            TypeDoc = pTypeDoc;
            _ = LocationBrb.GetCurrentLocation(db.GetWarehouse());
            c = ConnectorBase.GetInstance();
            InitializeComponent();
            Routing.RegisterRoute(nameof(RaitingDocItem), typeof(RaitingDocItem));
            NavigationPage.SetHasNavigationBar(this, DeviceInfo.Platform == DevicePlatform.iOS);
            Config.OnProgress += (p) => PB = p;
            this.BindingContext = this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            GetDocs();
        }

        void GetDocs()
        {
            _ = Task.Run(async () =>
            {
                //_ = c.GetRaitingTemplateAsync();
                //var r = db.GetDoc(TypeDoc).OrderByDescending(el => el.NumberDoc);
                //ViewDoc(r);
               await c.LoadDocsDataAsync(TypeDoc.CodeDoc, null, false);
                _= c.GetRaitingTemplateAsync();
                var r = db.GetDoc(TypeDoc).OrderByDescending(el => el.NumberDoc);
                ViewDoc(r);
            });
        }

        void ViewDoc(IEnumerable<DocVM> pDoc)
        {
            Dispatcher.Dispatch(() =>
            {
                MyDoc.Clear();
                if(pDoc?.Any()==true)
                foreach (var item in pDoc)
                    MyDoc.Add(item);
            });
        }

        private async void OnButtonClicked(object sender, System.EventArgs e)
        {
            /*var p = $"{nameof(Item)}?{nameof(Item.NumberDoc)}=\"{vDoc.NumberDoc}\"&TypeDoc={vDoc.TypeDoc}";
             Shell.Current.GoToAsync(p);*/

            var s = sender as Grid;
            var vDoc = s.BindingContext as DocVM;
            await Navigation.PushAsync(new RaitingDocItem(vDoc));
        }

        private async void Grid_Focused(object sender, FocusEventArgs e)
        {
            Grid cc = sender as Grid;
            var vDoc = cc.BindingContext as DocVM;
            await Navigation.PushAsync(new RaitingDocItem(vDoc));
           // await Shell.Current.GoToAsync($"{nameof(Item)}?{nameof(Item.NumberDoc)}={vDoc.NumberDoc}");//&TypeDoc={vDoc.TypeDoc}
        }

       

        private async void SavePhoto(object sender, EventArgs e)
        {
            Microsoft.Maui.Controls.View imageButton = sender as Microsoft.Maui.Controls.View;
            Grid cc = imageButton.Parent as Grid;
            var vDoc = cc.BindingContext as DocVM;

            bool isOk = await DisplayAlert("Збереження", "Ви хочете повторно вивантажити на сервер фотографії?", "Так", "Ні");
            if (isOk == true) 
            {
                var Res= await c.SendRatingAsync(null, vDoc, true);
                var toast = Toast.Make($"{Res.TextError} =>{Res.Info}", ToastDuration.Long, 14);
                MainThread.BeginInvokeOnMainThread(async () => await toast.Show());                
            }
        }
    }
}