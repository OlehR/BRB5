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

        double _PB = 0.95;
        public double PB { get { return _PB; } set { _PB = value; OnPropertyChanged(nameof(PB)); } }
        private Connector c = ConnectorBase.GetInstance();
        //private TypeDoc TypeDoc;
        DB db = DB.GetDB();
        BL.BL Bl = BL.BL.GetBL();
        private ObservableCollection<DocExpiration> _MyDocsR;
        public ObservableCollection<DocExpiration> MyDocsR { get { return _MyDocsR; } set { _MyDocsR = value; OnPropertyChanged(nameof(MyDocsR)); } }
        
        public bool IsViewOut { get { return false; } }// TypeDoc.IsViewOut; } }
        public bool IsSoftKeyboard { get { return Config.IsSoftKeyboard; } }
        public ExpirationDate()
        {
            Config.OnProgress += Progress;
            NokeyBoard();
            //TypeDoc = pTypeDoc;
            BindingContext = this;

            _MyDocsR = new ObservableCollection<DocExpiration>(db.GetDocExpiration());
            
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
        void Progress(double pProgress) => MainThread.BeginInvokeOnMainThread(() => PB = pProgress);
        protected override void OnAppearing()
        {
            base.OnAppearing();
           
            if (!IsSoftKeyboard)
            {
#if ANDROID
            MainActivity.Key+= OnPageKeyDown;
#endif
            }    
            OnPropertyChanged(nameof(MyDocsR));
            Config.OnProgress += Progress;
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (!IsSoftKeyboard)
            {
#if ANDROID
            MainActivity.Key-= OnPageKeyDown;
#endif
            }
            Config.OnProgress -= Progress;
        }
        private async void OpenDoc(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null) return;
            var vDoc = e.Item as DocExpiration;
            await Navigation.PushAsync(new ExpiretionDateItem(vDoc.NumberDoc));
        }             
       
        private void F2Save(object sender, TappedEventArgs e)
        {

        }

#if ANDROID
        public void OnPageKeyDown(Keycode keyCode, KeyEvent e)
        {
           switch (keyCode)
           {
            case Keycode.F2:
               F2Save(null, null); 
               return;
            default:
               return;
           }
         }

#endif
    }
}