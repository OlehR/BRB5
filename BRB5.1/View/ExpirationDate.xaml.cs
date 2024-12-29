using BL;
using BL.Connector;
using BRB5.Model;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Compatibility;
using BRB5;
using Grid = Microsoft.Maui.Controls.Grid;
using BarcodeScanning;
using Newtonsoft.Json;
using BRB5.Model.DB;
using Utils;



#if ANDROID
using Android.Views;
#endif

namespace BRB6.View
{
    public partial class ExpirationDate
    {

        double _PB = 0.5;
        public double PB { get { return _PB; } set { _PB = value; OnPropertyChanged(nameof(PB)); } }
        private Connector c = ConnectorBase.GetInstance();
        //private TypeDoc TypeDoc;
        DB db = DB.GetDB();
        BL.BL Bl = BL.BL.GetBL();
        private ObservableCollection<DocVM> _MyDocsR;
        public ObservableCollection<DocVM> MyDocsR { get { return _MyDocsR; } set { _MyDocsR = value; OnPropertyChanged(nameof(MyDocsR)); } }
        bool _IsVisZKPO = false;
        public bool IsVisZKPO { get { return _IsVisZKPO; } set { _IsVisZKPO = value; OnPropertyChanged(nameof(IsVisZKPO)); OnPropertyChanged(nameof(F1Text)); } }
        string _ZKPOstr = "";
        public string ZKPOstr { get { return _ZKPOstr; } set { _ZKPOstr = value; OnPropertyChanged(nameof(ZKPOstr)); } }
        public string F1Text { get { return IsVisZKPO ? "Без ф-тра" : "ЗКПО"; } }
        bool _IsVisBarCode = false;
        public bool IsVisBarCode { get { return _IsVisBarCode; } set { _IsVisBarCode = value; OnPropertyChanged(nameof(IsVisBarCode)); } }
        public bool IsViewOut { get { return false; } }// TypeDoc.IsViewOut; } }
        public bool IsSoftKeyboard { get { return Config.IsSoftKeyboard; } }
        public bool IsVisScan { get { return Config.TypeScaner == eTypeScaner.Camera; } }
        CameraView BarcodeScaner;       
        public ExpirationDate()
        {
            NokeyBoard();
            //TypeDoc = pTypeDoc;
            Config.BarCode = BarCode;
            BindingContext = this;

            _MyDocsR = new ObservableCollection<DocVM>() 
            { 
                new DocVM() { NumberDoc = "3", Description = "Нефреш"},
                new DocVM() { NumberDoc = "2", Description = "Фреш"}               
            };
            Config.OnProgress += (pProgress) => MainThread.BeginInvokeOnMainThread(() => PB = pProgress);
            InitializeComponent();
            _ = Task.Run(async () =>
            {
                Config.OnProgress.Invoke(0.1);
                try { 
                 var res = //JsonConvert.DeserializeObject<IEnumerable<DocWaresExpirationSample>>(DataJson.ExpirationWares);
                await c.GetExpirationDateAsync(Config.CodeWarehouse);
                    Config.OnProgress.Invoke(0.8);
                    db.ReplaceDocWaresExpirationSample(res.Info);
                    Config.OnProgress.Invoke(1);
                }
                catch(Exception e)
                {
                    FileLogger.WriteLogMessage(this, "ExpirationDate", e);
                }
            });           
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
#if ANDROID
            MainActivity.Key+= OnPageKeyDown;
#endif
            }
            //c.LoadDocsDataAsync(TypeDoc.CodeDoc, null, false);

   //         if (Config.IsFilterSave && ZKPOstr.Length > 2) MyDocsR = Bl.SetColorType(db.GetDoc(TypeDoc, null, ZKPOstr));
   //         else MyDocsR = Bl.SetColorType(db.GetDoc(TypeDoc));

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
#if ANDROID
            MainActivity.Key-= OnPageKeyDown;
#endif
            }
            if (!Config.IsFilterSave)
            {
                IsVisZKPO = false;
                ZKPOEntry.Text = string.Empty;
            }
        }
        private async void OpenDoc(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null) return;
            var vDoc = e.Item as DocVM;
            await Navigation.PushAsync(new ExpiretionDateItem(vDoc.NumberDoc));
        }

        private void ZKPO(object sender, EventArgs e)
        {
           /* IsVisZKPO = !IsVisZKPO;
            if (IsVisZKPO) ZKPOEntry.Focus();
            else
            { 
                ZKPOEntry.Text = string.Empty;
                MyDocsR = Bl.SetColorType(db.GetDoc(TypeDoc));
            }*/
        }

        private void FilterDocs(object sender, EventArgs e)
        {
            /*if (ZKPOstr.Length > 2)
                MyDocsR = Bl.SetColorType(db.GetDoc(TypeDoc, null, ZKPOstr));
            if (MyDocsR.Count > 0)
            {
                MyDocsR[0].SelectedColor = true;
                ListDocs.SelectedItem = MyDocsR[0];
            }*/
        }

        private void TabBarCode(object sender, EventArgs e)
        {
            IsVisBarCode= !IsVisBarCode;
            BarcodeScaner.CameraEnabled = IsVisBarCode;
        }
        void BarCode(string pBarCode) { }
            //MyDocsR = Bl.SetColorType(db.GetDoc(TypeDoc, pBarCode, null));  
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
            //if (selectedItem != null) ;
             //   await Navigation.PushAsync(new ExpirationDateElement());            
        }
              
        private void CameraView_OnDetectionFinished(object sender, BarcodeScanning.OnDetectionFinishedEventArg e)
        {
            if (e.BarcodeResults.Length > 0)
            {
                BarcodeScaner.PauseScanning = true;
                //MyDocsR = Bl.SetColorType(db.GetDoc(TypeDoc, e.BarcodeResults[0].DisplayValue, null));
                Task.Run(async () => {
                    await Task.Delay(1000);
                    BarcodeScaner.PauseScanning = false;
                });
            }
        }

#if ANDROID
        public void OnPageKeyDown(Keycode keyCode, KeyEvent e)
        {
           switch (keyCode)
           {
            case Keycode.F1:
               ZKPO(null, EventArgs.Empty); 
               return;
            case Keycode.F2:
               Up(); 
               return;
            case Keycode.F3:
               SelectKey();
               return;
            case Keycode.F4:
               Down();
               return;
            default:
               return;
           }
         }
#endif
    }
}