using BRB5.Model;
using BL.Connector;
using BL;
using CommunityToolkit.Maui.Alerts;
using System.Globalization;
using BRB6.Template;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using BRB5;


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
        IEnumerable<ExpirationDateElementVM> All;
        protected DB db = DB.GetDB();
        string _NumberOutInvoice = "";
        public string NumberOutInvoice { get { return _NumberOutInvoice; } set { _NumberOutInvoice = value; OnPropertyChanged(nameof(NumberOutInvoice)); } }
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
        //public ObservableCollection<ExpirationDateElementVM> MyDocWares { get; set; } = new ObservableCollection<ExpirationDateElementVM>();
        private ExpirationDateElementVM SelectedWare;
        private decimal _currentRest;
        public decimal CurrentRest
        {
            get => _currentRest;
            set { _currentRest = value; OnPropertyChanged(nameof(CurrentRest)); }
        }
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
                    r = WareItemsContainer.Children
                        .OfType<Microsoft.Maui.Controls.View>()
                        .Select(view => view.BindingContext as ExpirationDateElementVM)
                        .FirstOrDefault(ware => ware != null && ware.CodeWares == r.CodeWares && r.DocId.Equals(ware.DocId) ) ?? r;

                    SelectedWare = r;
                    ScrollToSelectedWare();

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        HandleSelectedWare(r);
                    });
                }
            });

            if (AlternateContent.IsVisible) CheckDiscount(pBarCode);
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
                    itemModel.CodeWares == SelectedWare.CodeWares && itemModel?.DocId.Equals(SelectedWare.DocId)==true  )
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
            All = db.GetDataExpiration(NumberDoc);
            foreach (var item in All)
            {
                if(string.IsNullOrEmpty( item.DocId))
                    item.DocId = "zz" + DateTime.Now.ToString("yyyyMMddHHmmssffff");
                //if(item.ExpirationDate==default) continue;
                //if (i++ > 5) break;
                var wareItemTemplate = CreateWareItemTemplate(item);                
                // Додайте шаблон до StackLayout
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    WareItemsContainer.Children.Add(wareItemTemplate);
                });
            }            
        }

        private void F2Save(object sender, EventArgs e)
        {
            if (MainContent.IsVisible)
            {
                Task.Run(async () =>
                {
                    var D = db.GetDocWaresExpiration(NumberDoc);
                    var r = await c.SaveExpirationDate(new BRB5.Model.DB.DocWaresExpirationSave() { CodeWarehouse = Config.CodeWarehouse, NumberDoc = NumberDoc, Wares = D });
                    var toast = Toast.Make("Збереження: " + r.TextError, ToastDuration.Long, 14);
                    MainThread.BeginInvokeOnMainThread(async () => await toast.Show());
                });
            }
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
                HandleSelectedWare(vItem);
            });
        }
        private void BackToMainContent(ExpirationDateElementVM pEDE = null)
        {
            if (pEDE != null)
            {
                var existingItem = WareItemsContainer.Children
                    .OfType<Microsoft.Maui.Controls.View>()
                    .Select(view => view.BindingContext as ExpirationDateElementVM)
                    .FirstOrDefault(ware => ware != null && ware.CodeWares == pEDE.CodeWares && pEDE?.DocId.Equals(ware?.DocId)==true);

                if (existingItem == null)
                {
                    Insert(pEDE);
                    //WareItemsContainer.Children.Add(CreateWareItemTemplate(pEDE));
                }
            }
            MainContent.IsVisible = true; 
            //TopSave.IsVisible = true;
            AlternateContent.IsVisible = false;
            TotalStackLayout.IsVisible = false;
            pEDE = null;
        }

        WareItemTemplate CreateWareItemTemplate(ExpirationDateElementVM pE)
        {
            var wareItemTemplate = new WareItemTemplate();
            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += OpenElement;
            wareItemTemplate.GestureRecognizers.Add(tapGestureRecognizer);
            wareItemTemplate.BindData(pE);            
            return wareItemTemplate;
        }
        /// <summary>
        /// вставляє елемент після першого найденого аналогічного, або вкінці якщо не знайдено
        /// </summary>
        /// <param name="pEDE"></param>
        void Insert(ExpirationDateElementVM pEDE)
        {
            int ind = -1;
            foreach (var el in WareItemsContainer.Children.OfType<Microsoft.Maui.Controls.View>())
            {
                var d = el.BindingContext as ExpirationDateElementVM;
                if (d.CodeWares == pEDE.CodeWares)
                {
                    ind = WareItemsContainer.Children.IndexOf(el);
                    break;
                }
            }
            var WIT = CreateWareItemTemplate(pEDE);
            if (ind >= 0)
                WareItemsContainer.Children.Insert(ind+1, WIT);
            else
                WareItemsContainer.Children.Add(WIT);
        }
        
        private void HandleSelectedWare(ExpirationDateElementVM selectedWare)
        {
            Task.Run(() => {
                try
                {
                    BRB5.Model.Connector c = ConnectorBase.GetInstance();
                    var Pr = c.GetPrice(new() { CodeWares = selectedWare.CodeWares }, eTypePriceInfo.Normal);

                    // Оновлюємо властивість у головному потоці для UI
                    MainThread.BeginInvokeOnMainThread(() => {
                        CurrentRest = Pr?.Data?.Rest ?? 0;
                    });
                }
                catch(Exception e) { /* Обробка помилок */ }
            });

            // Hide MainContent and show AlternateContent
            MainContent.IsVisible = false;
            //TopSave.IsVisible = false;
            AlternateContent.IsVisible = true;
            TotalStackLayout.IsVisible = true; 
            (AlternateContent.Content as ExpirationDateElementTemplate).Set(selectedWare, NumberDoc, 
                All?.Where(el => el.CodeWares == selectedWare.CodeWares && el.DocId != selectedWare.DocId));
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