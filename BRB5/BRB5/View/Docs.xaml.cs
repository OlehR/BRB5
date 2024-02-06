using BRB5.Model;
using System;
using System.Collections.ObjectModel;
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
            MessagingCenter.Subscribe<KeyEventMessage>(this, "F1Pressed", message => { OKPO(null, EventArgs.Empty); });
            MessagingCenter.Subscribe<KeyEventMessage>(this, "8Pressed", message => {  });
            MessagingCenter.Subscribe<KeyEventMessage>(this, "2Pressed", message => {  });
            MessagingCenter.Subscribe<KeyEventMessage>(this, "EnterPressed", message => {  });
            TypeDoc = pTypeDoc;
            Config.BarCode = BarCode;
            BindingContext = this;
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            c.LoadDocsData(TypeDoc.CodeDoc, null, false);
            MyDocsR = new ObservableCollection<Doc>(db.GetDoc(TypeDoc));
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

        private void FilterBarCode(ZXing.Result result)
        {
            zxing.IsAnalyzing = false;
            MyDocsR = new ObservableCollection<Doc>(db.GetDoc(TypeDoc, result.Text, null));
            zxing.IsAnalyzing = true;
        }
    }
}