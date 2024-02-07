using BRB5.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;

namespace BRB5.View
{
    public partial class Docs
    {
        private Connector.Connector c=Connector.Connector.GetInstance();
        private TypeDoc TypeDoc;
        DB db = DB.GetDB();
        private ObservableCollection<Doc> _MyDocsR;
        public ObservableCollection<Doc> MyDocsR { get { return _MyDocsR; } set { _MyDocsR = value; OnPropertyChanged("MyDocsR"); } }
        bool _IsVisOPKO = false;
        public bool IsVisOPKO { get { return _IsVisOPKO; } set { _IsVisOPKO = value; OnPropertyChanged("IsVisOPKO"); } }
        string _OPKOstr = "";
        public string OPKOstr { get { return _OPKOstr; } set { _OPKOstr = value; OnPropertyChanged("OPKOstr"); } }

        bool _IsVisBarCode = false;
        public bool IsVisBarCode { get { return _IsVisBarCode; } set { _IsVisBarCode = value; OnPropertyChanged("IsVisBarCode"); } }
        public bool IsViewOut { get { return TypeDoc.IsViewOut; } }

        public bool IsVisScan { get { return Config.TypeScaner == eTypeScaner.Camera; } }

        public Docs(TypeDoc pTypeDoc )
        {
        //    MessagingCenter.Subscribe<KeyEventMessage>(this, "F1Pressed", message => { OKPO(null, EventArgs.Empty); });
        //    MessagingCenter.Subscribe<KeyEventMessage>(this, "8Pressed", message => { UpDown(8);  });
        //    MessagingCenter.Subscribe<KeyEventMessage>(this, "2Pressed", message => { UpDown(2); });
        //    MessagingCenter.Subscribe<KeyEventMessage>(this, "EnterPressed", message => { EnterPressed(); });
            TypeDoc = pTypeDoc;
            Config.BarCode = BarCode;
            BindingContext = this;
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            MessagingCenter.Subscribe<KeyEventMessage>(this, "F1Pressed", message => { OKPO(null, EventArgs.Empty); });
            MessagingCenter.Subscribe<KeyEventMessage>(this, "8Pressed", message => { UpDown(8); });
            MessagingCenter.Subscribe<KeyEventMessage>(this, "2Pressed", message => { UpDown(2); });
            MessagingCenter.Subscribe<KeyEventMessage>(this, "EnterPressed", message => { EnterKey(); });
            c.LoadDocsDataAsync(TypeDoc.CodeDoc, null, false);
            MyDocsR = new ObservableCollection<Doc>(db.GetDoc(TypeDoc));
            if (MyDocsR.Count > 0)
            {
                MyDocsR[0].SelectedColor = true;
                ListDocs.SelectedItem = MyDocsR[0];
            }       
            OnPropertyChanged(nameof(MyDocsR));
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            zxing.IsScanning = false;
            MessagingCenter.Unsubscribe<KeyEventMessage>(this, "F1Pressed");
            MessagingCenter.Unsubscribe<KeyEventMessage>(this, "8Pressed");
            MessagingCenter.Unsubscribe<KeyEventMessage>(this, "2Pressed");
            MessagingCenter.Unsubscribe<KeyEventMessage>(this, "EnterPressed");
        }
        private async void OpenDoc(object sender, EventArgs e)
        {
            var s = sender as Grid;

            var vDoc = s.BindingContext as Doc;
            
            await Navigation.PushAsync(new DocItem(vDoc,TypeDoc));
        }

        private void OKPO(object sender, EventArgs e)
        {
            IsVisOPKO = !IsVisOPKO;
        }

        private void FilterDocs(object sender, EventArgs e)
        {
            if (OPKOstr.Length > 2)
                MyDocsR = new ObservableCollection<Doc>(db.GetDoc(TypeDoc, null, OPKOstr));
        }

        private void TabBarCode(object sender, EventArgs e)
        {
            IsVisBarCode= !IsVisBarCode;
            zxing.IsScanning = IsVisBarCode;
        }
        void BarCode(string pBarCode)
        {
            MyDocsR = new ObservableCollection<Doc>(db.GetDoc(TypeDoc, pBarCode, null));
        }
        public void Dispose()
        {
            Config.BarCode -= BarCode;
        }

        private void UpDown(int key)
        {
            if (key == 2)
            {
                if (Config.TypeScaner == eTypeScaner.PM550 || Config.TypeScaner == eTypeScaner.PM351) Up();
                else if ( Config.TypeScaner == eTypeScaner.Zebra || Config.TypeScaner == eTypeScaner.BitaHC61) Down();
            }
            else if (key == 8)
            {
                if (Config.TypeScaner == eTypeScaner.PM550 || Config.TypeScaner == eTypeScaner.PM351) Down();
                else if ( Config.TypeScaner == eTypeScaner.Zebra || Config.TypeScaner == eTypeScaner.BitaHC61) Up();
            }

        }
        private void Up()
        {
            var selectedItem = (Doc)ListDocs.SelectedItem;
            if (selectedItem != null)
            {
                var selectedIndex = MyDocsR.IndexOf(selectedItem);
                if (selectedIndex > 0)
                {
                    MyDocsR[selectedIndex].SelectedColor = false;
                    MyDocsR[selectedIndex - 1].SelectedColor = true;
                    ListDocs.SelectedItem = MyDocsR[selectedIndex - 1];
                    ListDocs.ScrollTo(ListDocs.SelectedItem, ScrollToPosition.Center, false);
                }
                OnPropertyChanged(nameof(MyDocsR));
            }
        }
        private void Down()
        {
            var selectedItem = (Doc)ListDocs.SelectedItem;
            if (selectedItem != null)
            {
                var selectedIndex = MyDocsR.IndexOf(selectedItem);

                if (selectedIndex < MyDocsR.Count - 1)
                {
                    MyDocsR[selectedIndex].SelectedColor = false;
                    MyDocsR[selectedIndex + 1].SelectedColor = true;
                    ListDocs.SelectedItem = MyDocsR[selectedIndex + 1];
                    ListDocs.ScrollTo(ListDocs.SelectedItem, ScrollToPosition.Center, false);
                }
                OnPropertyChanged(nameof(MyDocsR));
            }
        }
        private async void EnterKey()
        {
            var selectedItem = (Doc)ListDocs.SelectedItem;
            if (selectedItem != null)
            {
                await Navigation.PushAsync(new DocItem(selectedItem, TypeDoc));
            }
        }
        private void FilterBarCode(ZXing.Result result)
        {
            zxing.IsAnalyzing = false;
            MyDocsR = new ObservableCollection<Doc>(db.GetDoc(TypeDoc, result.Text, null));
            zxing.IsAnalyzing = true;
        }
    }
}