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
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui.Graphics.Text;




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

            _ = Task.Run((Func<Task>)(async () =>
            {
                Config.OnProgress.Invoke(0.1);
                try { 
                 var res = await c.GetExpirationDateAsync(Config.CodeWarehouse);
                    if (res != null && res.Success)
                    {
                        Config.OnProgress.Invoke(0.8);
                        db.ReplaceDocWaresExpirationSample((IEnumerable<DocWaresExpirationSample>)res.Data);
                        Config.OnProgress.Invoke(0.9);
                        _MyDocsR = new ObservableCollection<DocExpiration>(db.GetDocExpiration());
                        Config.OnProgress.Invoke(1);
                        PopulateDocs();
                    }
                }
                catch(Exception e)
                {
                    Config.OnProgress.Invoke(0);
                    FileLogger.WriteLogMessage(this, "ExpirationDate", e);
                }
            }));           
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
            _MyDocsR = new ObservableCollection<DocExpiration>(db.GetDocExpiration());
            PopulateDocs();
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
            MainThread.BeginInvokeOnMainThread(() =>
            {
                DocsStackLayout.Children.Clear();
                if (MyDocsR == null) return;

                foreach (var doc in MyDocsR)
                {
                    double percent = doc.Count == 0 ? 0 : (double)doc.CountInput / doc.Count;

                    Color textColor;
                    if (percent <= 0.33)
                        textColor = Colors.Red;
                    else if (percent <= 0.66)
                        textColor = Colors.Orange;
                    else if (percent < 1.0)
                        textColor = Colors.Yellow;
                    else
                        textColor = Colors.Green;

                    var grid = new Grid
                    {
                        HeightRequest = 40,
                        Margin = new Thickness(0, 2),
                        Padding = 2,
                        BackgroundColor = Colors.Transparent
                    };

                    // Контейнер для динамічної ширини прогресбару
                    var progressContainer = new Grid
                    {
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        VerticalOptions = LayoutOptions.Fill,
                    };

                    var progressBar = new BoxView
                    {
                        Color = Color.FromArgb("#B9F6CA"),
                        HorizontalOptions = LayoutOptions.Start,
                        VerticalOptions = LayoutOptions.Fill
                    };

                    // Додати прогресбар у контейнер
                    progressContainer.Children.Add(progressBar);
                    grid.Children.Add(progressContainer);

                    var contentLayout = new Microsoft.Maui.Controls.StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        Spacing = 5
                    };

                    var descriptionLabel = new Label
                    {
                        Text = doc.Description,
                        TextColor = Application.Current.RequestedTheme == AppTheme.Light ? Colors.Black : Colors.White,
                        FontSize = 20,
                        HorizontalOptions = LayoutOptions.StartAndExpand,
                        VerticalOptions = LayoutOptions.Center
                    };

                    var countInputLabel = new Label
                    {
                        Text = doc.CountInput.ToString(),
                        FontSize = 20,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = textColor
                    };

                    var slashLabel = new Label
                    {
                        Text = "/",
                        FontSize = 20,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = textColor
                    };

                    var countLabel = new Label
                    {
                        Text = doc.Count.ToString(),
                        FontSize = 20,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = textColor
                    };

                    contentLayout.Children.Add(descriptionLabel);
                    contentLayout.Children.Add(countInputLabel);
                    contentLayout.Children.Add(slashLabel);
                    contentLayout.Children.Add(countLabel);

                    grid.Children.Add(contentLayout);

                    var borderFrame = new Frame
                    {
                        BorderColor = Colors.Gray,
                        CornerRadius = 4,
                        Padding = 0,
                        Margin = 0,
                        HasShadow = false,
                        Content = grid
                    };

                    // Gesture
                    var tapGestureRecognizer = new TapGestureRecognizer();
                    tapGestureRecognizer.Tapped += (s, e) => OpenDoc(doc);
                    borderFrame.GestureRecognizers.Add(tapGestureRecognizer);

                    // Додати елемент
                    DocsStackLayout.Children.Add(borderFrame);

                    // Динамічно встановити ширину прогресбару після рендеру
                    progressContainer.SizeChanged += (s, e) =>
                    {
                        progressBar.WidthRequest = percent * progressContainer.Width;
                    };
                }
            });
        }


        private void F2Save(object sender, TappedEventArgs e)
        {
            //if (MainContent.IsVisible)
            //{
            Task.Run(async () =>
            {
                var D = db.GetDocWaresExpiration("");
                var r = await c.SaveExpirationDate(new BRB5.Model.DB.DocWaresExpirationSave() { CodeWarehouse = Config.CodeWarehouse, NumberDoc = "", Wares = D });
                var toast = Toast.Make("Збереження: " + r.TextError, ToastDuration.Long, 14);
                MainThread.BeginInvokeOnMainThread(async () => await toast.Show());

            });
            //}

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