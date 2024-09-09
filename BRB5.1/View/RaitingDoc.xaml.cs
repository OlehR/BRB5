//using BRB5.Model;
using BL;
using BL.Connector;
using BRB5.Model;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Compatibility;
using Grid = Microsoft.Maui.Controls.Grid;
using BRB5;

namespace BRB6
{
    public partial class RaitingDoc: ContentPage
    {
        Connector c;
        DB db = DB.GetDB();
        private readonly TypeDoc TypeDoc;
        public ObservableCollection<DocVM> MyDoc { get; set; } = new ObservableCollection<DocVM>();
        //public string Help { get; set; } = "ERHHHHHHH54";
        public RaitingDoc(TypeDoc pTypeDoc)
        {
            TypeDoc = pTypeDoc;
            _ = LocationBrb.GetCurrentLocation(db.GetWarehouse());
            c = Connector.GetInstance();
            InitializeComponent();
            Routing.RegisterRoute(nameof(RaitingDocItem), typeof(RaitingDocItem));
            // TODO Xamarin.Forms.Device.RuntimePlatform is no longer supported. Use Microsoft.Maui.Devices.DeviceInfo.Platform instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
            NavigationPage.SetHasNavigationBar(this, Device.RuntimePlatform == Device.iOS);

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
                var r = db.GetDoc(TypeDoc).OrderByDescending(el => el.NumberDoc);
                ViewDoc(r);
                await c.LoadDocsDataAsync(TypeDoc.CodeDoc, null, false);
                _ = c.GetRaitingTemplateAsync();
                r = db.GetDoc(TypeDoc).OrderByDescending(el => el.NumberDoc);
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
    }
}