using BL;
using BL.Connector;
using BRB5.Model;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Compatibility;
using BRB5;
using Grid = Microsoft.Maui.Controls.Grid;
using BarcodeScanning;

namespace BRB6.View
{
    public partial class Docs
    {
        private Connector c = Connector.GetInstance();
        private TypeDoc TypeDoc;
        DB db = DB.GetDB();
        private ObservableCollection<DocVM> _MyDocsR;
        public ObservableCollection<DocVM> MyDocsR { get { return _MyDocsR; } set { _MyDocsR = value; OnPropertyChanged(nameof(MyDocsR)); } }
        bool _IsVisZKPO = false;
        public bool IsVisZKPO { get { return _IsVisZKPO; } set { _IsVisZKPO = value; OnPropertyChanged(nameof(IsVisZKPO)); OnPropertyChanged(nameof(F1Text)); } }
        string _ZKPOstr = "";
        public string ZKPOstr { get { return _ZKPOstr; } set { _ZKPOstr = value; OnPropertyChanged(nameof(ZKPOstr)); } }
        public string F1Text { get { return IsVisZKPO ? "Без ф-тра" : "ЗКПО"; } }
        bool _IsVisBarCode = false;
        public bool IsVisBarCode { get { return _IsVisBarCode; } set { _IsVisBarCode = value; OnPropertyChanged(nameof(IsVisBarCode)); } }
        public bool IsViewOut { get { return TypeDoc.IsViewOut; } }
        public bool IsSoftKeyboard { get { return Config.IsSoftKeyboard; } }
        public bool IsVisScan { get { return Config.TypeScaner == eTypeScaner.Camera; } }
        CameraView BarcodeScaner;

        public Docs(TypeDoc pTypeDoc )
        {
            TypeDoc = pTypeDoc;
            Config.BarCode = BarCode;
            BindingContext = this;
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (IsVisScan)
            {
                BarcodeScaner = new CameraView
                {
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    CameraEnabled = false,
                    VibrationOnDetected = false,
                    BarcodeSymbologies = BarcodeFormats.Ean13 | BarcodeFormats.Ean8 | BarcodeFormats.QRCode,

                };
                BarcodeScaner.OnDetectionFinished += CameraView_OnDetectionFinished;
                GridZxing.Children.Add(BarcodeScaner);
            }
            if (!IsSoftKeyboard)
            {
                MessagingCenter.Subscribe<KeyEventMessage>(this, "F1Pressed", message => { ZKPO(null, EventArgs.Empty); });
                MessagingCenter.Subscribe<KeyEventMessage>(this, "F2Pressed", message => { Up(); });
                MessagingCenter.Subscribe<KeyEventMessage>(this, "F4Pressed", message => { Down(); });
                MessagingCenter.Subscribe<KeyEventMessage>(this, "F3Pressed", message => { SelectKey(); });
                MessagingCenter.Subscribe<KeyEventMessage>(this, "EnterPressed", message => { EnterKey(); });
            }
            c.LoadDocsDataAsync(TypeDoc.CodeDoc, null, false);

            if (Config.IsFilterSave && ZKPOstr.Length > 2) MyDocsR = new ObservableCollection<DocVM>(db.GetDoc(TypeDoc, null, ZKPOstr));
            else MyDocsR = new ObservableCollection<DocVM>(db.GetDoc(TypeDoc));

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
            if (IsVisScan) BarcodeScaner.CameraEnabled = false;
            if (!IsSoftKeyboard)
            {
                MessagingCenter.Unsubscribe<KeyEventMessage>(this, "F1Pressed");
                MessagingCenter.Unsubscribe<KeyEventMessage>(this, "F2Pressed");
                MessagingCenter.Unsubscribe<KeyEventMessage>(this, "F3Pressed");
                MessagingCenter.Unsubscribe<KeyEventMessage>(this, "F4Pressed");
                MessagingCenter.Unsubscribe<KeyEventMessage>(this, "EnterPressed");
            }
            if (!Config.IsFilterSave)
            {
                IsVisZKPO = false;
                ZKPOEntry.Text = string.Empty;
            }
        }
        private async void OpenDoc(object sender, EventArgs e)
        {
            var s = sender as Grid;
            var vDoc = s.BindingContext as DocVM;            
            await Navigation.PushAsync(new DocItem(vDoc,TypeDoc));
        }

        private void ZKPO(object sender, EventArgs e)
        {
            IsVisZKPO = !IsVisZKPO;
            if (IsVisZKPO) ZKPOEntry.Focus();
            else
            { 
                ZKPOEntry.Text = string.Empty;
                MyDocsR = new ObservableCollection<DocVM>(db.GetDoc(TypeDoc));
            }
        }

        private void FilterDocs(object sender, EventArgs e)
        {
            if (ZKPOstr.Length > 2)
                MyDocsR = new ObservableCollection<DocVM>(db.GetDoc(TypeDoc, null, ZKPOstr));
            if (MyDocsR.Count > 0)
            {
                MyDocsR[0].SelectedColor = true;
                ListDocs.SelectedItem = MyDocsR[0];
            }
        }

        private void TabBarCode(object sender, EventArgs e)
        {
            IsVisBarCode= !IsVisBarCode;
            BarcodeScaner.CameraEnabled = IsVisBarCode;
        }
        void BarCode(string pBarCode) { MyDocsR = new ObservableCollection<DocVM>(db.GetDoc(TypeDoc, pBarCode, null));  }
        public void Dispose() {  Config.BarCode -= BarCode;  }

        private void UpDown(int key)
        {
            if (ZKPOEntry.IsFocused == false)
            {
                if (key == 2)
                {
                    if (Config.TypeScaner == eTypeScaner.PM550 || Config.TypeScaner == eTypeScaner.PM351) Up();
                    else if (Config.TypeScaner == eTypeScaner.Zebra || Config.TypeScaner == eTypeScaner.BitaHC61) Down();
                }
                else if (key == 8)
                {
                    if (Config.TypeScaner == eTypeScaner.PM550 || Config.TypeScaner == eTypeScaner.PM351) Down();
                    else if (Config.TypeScaner == eTypeScaner.Zebra || Config.TypeScaner == eTypeScaner.BitaHC61) Up();
                }
            }
        }
        private void Up()
        {
            var selectedItem = (DocVM)ListDocs.SelectedItem;
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
            var selectedItem = (DocVM)ListDocs.SelectedItem;
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
        private async void SelectKey()
        {
            var selectedItem = (DocVM)ListDocs.SelectedItem;
            if (selectedItem != null) await Navigation.PushAsync(new DocItem(selectedItem, TypeDoc));            
        }

        private void EnterKey() { if(IsVisZKPO) FilterDocs(null, null); }
      
        private void CameraView_OnDetectionFinished(object sender, BarcodeScanning.OnDetectionFinishedEventArg e)
        {
            if (e.BarcodeResults.Length > 0)
            {
                BarcodeScaner.PauseScanning = true;
                MyDocsR = new ObservableCollection<DocVM>(db.GetDoc(TypeDoc, e.BarcodeResults[0].DisplayValue, null));
                Task.Run(async () => {
                    await Task.Delay(1000);
                    BarcodeScaner.PauseScanning = false;
                });
            }
        }
    }
}