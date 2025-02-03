using BRB5.Model;
using System.Collections.ObjectModel;
using BL.Connector;
using BL;
using BRB5;
using CommunityToolkit.Maui.Alerts;
using System.Globalization;
using BRB6.Template;
using CommunityToolkit.Maui.Core;

#if ANDROID
using Android.Views;
#endif

namespace BRB6.View
{
    public partial class ExpiretionDateItem
    {
        string NumberDoc;
        //private readonly TypeDoc TypeDoc;

        //private DocVM Doc;
        private Connector c = ConnectorBase.GetInstance();
        ExpirationDateElementVM _VM;
        ExpirationDateElementVM VM { get { return _VM; } set { _VM = value; OnPropertyChanged(nameof(VM)); } } 
        protected DB db = DB.GetDB();
        string _NumberOutInvoice = "";
        public string NumberOutInvoice { get { return _NumberOutInvoice; } set { _NumberOutInvoice = value; OnPropertyChanged("NumberOutInvoice"); } }
        public List<DataStr> ListDataStr
        {
            get
            {
                var list = new List<DataStr>();
                for (int i = 0; i < 10; i++)
                    list.Add(new DataStr(DateTime.Today.AddDays(-1 * i)));
                return list;
            }
        }
        public int SelectedDataStr { get; set; } = 0;
        public bool IsSoftKeyboard { get { return Config.IsSoftKeyboard; } }
        public ObservableCollection<ExpirationDateElementVM> MyDocWares { get; set; } = new ObservableCollection<ExpirationDateElementVM>();
        private ExpirationDateElementVM SelectedWare;
        public ExpiretionDateItem(string pNumberDoc)
        {
            NumberDoc = pNumberDoc;
            NokeyBoard();
            // Doc = new DocVM(pDocId);          
            InitializeComponent();
            Config.BarCode = BarCode;
            
            Task.Run(() =>
            {
                AddCustomItems();
            });
            BindingContext = this;
            var expirationDateTemplate = new ExpirationDateElementTemplate();
            expirationDateTemplate.RequestReturnToMainContent += BackToMainContent;
            AlternateContent.Content = expirationDateTemplate;
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (!IsSoftKeyboard)
            {
#if ANDROID
                MainActivity.Key += OnPageKeyDown;
#endif
            }          

        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (!IsSoftKeyboard)
            {
#if ANDROID
                MainActivity.Key -= OnPageKeyDown;
#endif
            }
        }

        public void Dispose() { Config.BarCode -= BarCode; }
        void BarCode(string pBarCode)
        {
            Task.Run(async () =>
            {
                ParseBarCode pbc = c.ParsedBarCode(pBarCode, false);
                var r = db.GetScanDataExpiration(NumberDoc, pbc);
                if (r != null)
                {
                    SelectedWare = r;
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        // Hide MainContent and show AlternateContent
                        MainContent.IsVisible = false;
                        AlternateContent.IsVisible = true;
                        (AlternateContent.Content as ExpirationDateElementTemplate).Set(r);
                    });
                }
            });

            if (AlternateContent.IsVisible)   CheckDiscount(pBarCode);
        }

        void CheckDiscount(string pBarCode)
        {
            if (SelectedWare != null)
            {
                if (SelectedWare.GetPercentColor.BarCode.Equals(pBarCode))
                {
                    var toast = Toast.Make("Штрихкод підходить", ToastDuration.Short, 20);
                    _ = toast.Show();

                    //save status!!!

                    BackToMainContent();
                }
                else  _ = DisplayAlert("", "Штрихкод не підходить", "Ok");                   
                
            }
        }
        public async void ScrollToSelectedWare()
        {
            if (SelectedWare == null)
                return;

            // Iterate through the children of WareItemsContainer
            foreach (var child in WareItemsContainer.Children)
            {
                if (child is Microsoft.Maui.Controls.View view && view.BindingContext is ExpirationDateElementVM itemModel &&
                    itemModel.CodeWares == SelectedWare.CodeWares)
                {
                    var childBounds = view.Bounds;

                    // Ensure you're calling ScrollToAsync on the correct ScrollView instance
                    await ScrollView.ScrollToAsync(0, childBounds.Y, false);
                    break;
                }
            }
        }

        private void AddCustomItems()
        {
            //int i = 0;
            foreach (var item in db.GetDataExpiration(NumberDoc))
            {
                if(item.ExpirationDate==default) continue;
                //if (i++ > 5) break;

                var wareItemTemplate = new WareItemTemplate();

                //item.ExpirationDateInput = item.ExpirationDate;
                wareItemTemplate.BindData(item);

                // Додайте подію натискання
                var tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Tapped += OpenElement;
                wareItemTemplate.GestureRecognizers.Add(tapGestureRecognizer);

                // Додайте шаблон до StackLayout
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    WareItemsContainer.Children.Add(wareItemTemplate);
                });
            }
            
        }

        private void F2Save(object sender, EventArgs e)
        {
            Task.Run(async() =>
            {
                var D = db.GetDocWaresExpiration(NumberDoc);
                var r = await c.SaveExpirationDate(new BRB5.Model.DB.DocWaresExpirationSave() { CodeWarehouse = Config.CodeWarehouse, NumberDoc= NumberDoc, Wares = D });
                var toast = Toast.Make("Збереження: "+ r.TextError, ToastDuration.Long, 14);
                MainThread.BeginInvokeOnMainThread(async () => await toast.Show());
            });
        }

        protected override bool OnBackButtonPressed()
        {
            if (MainContent.IsVisible) return base.OnBackButtonPressed();
            else BackToMainContent();
            return true;
        }

        private void DocNameFocus(object sender, FocusEventArgs e) {/* DocName.Focus();*/ }

        private void OpenElement(object sender, EventArgs e)
        {
            var s = sender as WareItemTemplate;
            var vItem = s.BindingContext as ExpirationDateElementVM;
            if (vItem == null) return;

            SelectedWare = vItem;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Hide MainContent and show AlternateContent
                MainContent.IsVisible = false;
                AlternateContent.IsVisible = true;
                (AlternateContent.Content as ExpirationDateElementTemplate).Set(vItem);
            });
        }
        private void BackToMainContent()
        {
            ScrollToSelectedWare();
            MainContent.IsVisible = true;
            AlternateContent.IsVisible = false;
            SelectedWare = null;
        }

#if ANDROID
        public void OnPageKeyDown(Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {
                case Keycode.F2:
                    F2Save(null, EventArgs.Empty);
                    return;
                default:
                    return;
            }
        }
#endif
    }

    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Drawing.Color drawingColor)
            {
                // Перетворення System.Drawing.Color в Microsoft.Maui.Graphics.Color
                return drawingColor.ToColor();
            }

            return Colors.Transparent; // Повертаємо прозорий колір за замовчуванням
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}