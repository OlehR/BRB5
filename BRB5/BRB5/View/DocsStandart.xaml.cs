using BRB5.Connector;
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
    public partial class DocsStandart : ContentPage
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


        public DocsStandart(TypeDoc pTypeDoc )
        {
            TypeDoc = pTypeDoc;
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
            private async void OpenDoc(object sender, EventArgs e)
        {
            var s = sender as Grid;

            var vDoc = s.BindingContext as Doc;
            
            await Navigation.PushAsync(new DocStandart(vDoc,TypeDoc));
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

        private void BarCode(object sender, EventArgs e)
        {
            IsVisBarCode= !IsVisBarCode;
            zxing.IsScanning = IsVisBarCode;
        }

        private void FilterBarCode(ZXing.Result result)
        {
            zxing.IsAnalyzing = false;
            MyDocsR = new ObservableCollection<Doc>(db.GetDoc(TypeDoc, result.Text, null));
            zxing.IsAnalyzing = true;
        }
    }
}