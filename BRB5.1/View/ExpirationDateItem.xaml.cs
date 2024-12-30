using BRB5.Model;
using System.Collections.ObjectModel;
using BL.Connector;
using BL;
using BRB5;
using CommunityToolkit.Maui.Alerts;
using System.Globalization;
using BRB6.Template;
using static Android.Icu.Text.CaseMap;



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
        bool _IsVisibleDocF6 = false;
        public bool IsVisibleDocF6 { get { return _IsVisibleDocF6; } set { _IsVisibleDocF6 = value; OnPropertyChanged("IsVisibleDocF6"); } }
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
                if (SelectedWare.GetPercentColor.BarCode.Equals(pBarCode)) _ = DisplayAlert("", "Штрихкод підходить", "Ok");
                else _ = DisplayAlert("", "Штрихкод не підходить", "Ok");
            }
        }

        private void AddCustomItems()
        {
            foreach (var item in db.GetDataExpiration(NumberDoc))
            {
                var wareItemTemplate = new WareItemTemplate();

                item.ExpirationDateInput = item.ExpirationDate;
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
            /*   Doc.NumberOutInvoice = NumberOutInvoice;
               Doc.DateOutInvoice = ListDataStr[SelectedDataStr].DateString;
               var r = c.SendDocsData(Doc, db.GetDocWares(Doc, 2, eTypeOrder.Scan));
               if (r.State != 0) _ = DisplayAlert("Помилка", r.TextError, "OK");
               else
               {
                   var toast = Toast.Make("Документ успішно збережений");
                   _ = toast.Show();
                   //_ = this.DisplayToastAsync("Документ успішно збережений");
               }*/
        }
        private async void F3Scan(object sender, EventArgs e) { /*await Navigation.PushAsync(new DocScan(Doc, TypeDoc)); */}
        private async void F4WrOff(object sender, EventArgs e) { /*await Navigation.PushAsync(new ManualInput(Doc, TypeDoc)); */ }
        private void F6Doc(object sender, EventArgs e)
        {
            IsVisibleDocF6 = !IsVisibleDocF6;
            //if (IsVisibleDocF6) DocDate.Focus();
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
            MainContent.IsVisible = true;
            AlternateContent.IsVisible = false;
            SelectedWare = null;
        }

#if ANDROID
        public void OnPageKeyDown(Keycode keyCode, KeyEvent e)
        {
            /*switch (keyCode)
            {
             case Keycode.F2:
                F2Save(null, EventArgs.Empty);
                return;
             case Keycode.F3:
                F3Scan(null, EventArgs.Empty);
                return;
             case Keycode.F4:
                F4WrOff(null, EventArgs.Empty); 
                return;
             case Keycode.F6:
                F6Doc(null, EventArgs.Empty);
                return;

             default:
                return;
            }*/
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