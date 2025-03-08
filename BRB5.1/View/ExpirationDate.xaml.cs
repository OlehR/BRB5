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
using Microsoft.Maui.Controls;


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
            InitializeComponent();
            Config.OnProgress += Progress;
            NokeyBoard();
            //TypeDoc = pTypeDoc;
            BindingContext = this;
            var docs = db.GetDocExpiration();
            _MyDocsR = new ObservableCollection<DocExpiration>(docs.Concat(docs).Concat(docs));

            PopulateDocs();
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
        private async void OpenDoc(DocExpiration doc)
        {
            await Navigation.PushAsync(new ExpiretionDateItem(doc.NumberDoc));
        }

        private void PopulateDocs()
        {
            if (MyDocsR != null)
            {
                foreach (var doc in MyDocsR)
                {
                    var stackLayout = new Microsoft.Maui.Controls.StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        Padding = new Thickness(5),
                        BackgroundColor = Color.FromArgb("#E0E0E0")
                    };

                    var descriptionLabel = new Label
                    {
                        Text = doc.Description,
                        TextColor = Application.Current.RequestedTheme == AppTheme.Light ? Colors.Black : Colors.White,
                        FontSize = 20,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        VerticalOptions = LayoutOptions.CenterAndExpand
                    };
                    stackLayout.Children.Add(descriptionLabel);

                    var countStack = new HorizontalStackLayout
                    {
                        Spacing = 1,
                        VerticalOptions = LayoutOptions.CenterAndExpand,
                        HorizontalOptions = LayoutOptions.End
                    };
                    countStack.Children.Add(new Label { Text = doc.CountInput.ToString() });
                    countStack.Children.Add(new Label { Text = "/" });
                    countStack.Children.Add(new Label { Text = doc.Count.ToString() });
                    stackLayout.Children.Add(countStack);

                    var tapGestureRecognizer = new TapGestureRecognizer();
                    tapGestureRecognizer.Tapped += (s, e) => OpenDoc(doc);
                    stackLayout.GestureRecognizers.Add(tapGestureRecognizer);


                    DocsStackLayout.Children.Add(stackLayout);
                }
            }
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