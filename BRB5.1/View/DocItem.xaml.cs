using BRB5.Model;
using System.Collections.ObjectModel;
using BL.Connector;
using BL;
using BRB5;
using CommunityToolkit.Maui.Alerts;
#if ANDROID
using Android.Views;
#endif

namespace BRB6.View
{
    public partial class DocItem 
    {
        private readonly TypeDoc TypeDoc;        
        private DocVM Doc;
        private Connector c = ConnectorBase.GetInstance(); 
        protected DB db = DB.GetDB();
        string _NumberOutInvoice = "";
        public string NumberOutInvoice { get { return _NumberOutInvoice; } set { _NumberOutInvoice = value; OnPropertyChanged("NumberOutInvoice"); } }
        public List<DataStr> ListDataStr { 
            get { 
                var list = new List<DataStr>();
                for(int i=0; i<10; i++)
                    list.Add(new DataStr(DateTime.Today.AddDays(-1 * i)));
                
                return list;
                }
        }
        public int SelectedDataStr { get; set; } = 0;
        public bool IsSoftKeyboard { get {  return Config.IsSoftKeyboard; } }
        bool _IsVisibleDocF6 = false;
        public bool IsVisibleDocF6 { get { return _IsVisibleDocF6; } set { _IsVisibleDocF6 = value; OnPropertyChanged("IsVisibleDocF6"); } } 
        public ObservableCollection<DocWaresEx> MyDocWares { get; set; } = new ObservableCollection<DocWaresEx>();
        public bool IsVisF5Act => TypeDoc.KindDoc == eKindDoc.Lot|| TypeDoc.IsViewAct;
        public bool IsVisF2 => TypeDoc.KindDoc != eKindDoc.Lot;
        public bool IsUseArticle => Config.IsUseArticle;
        public bool IsViewReason { get { return TypeDoc.IsViewReason; } }
        public bool IsViewNoReason { get { return !TypeDoc.IsViewReason; } }

        //// Колекція варіантів для Picker
        //public ObservableCollection<BRB5.Model.DB.Reason> Reasons { get; set; }

        //// Поточний вибраний елемент
        //private BRB5.Model.DB.Reason _selectedReason;
        //public BRB5.Model.DB.Reason SelectedReason
        //{
        //    get => _selectedReason;
        //    set
        //    {
        //        if (_selectedReason != value)
        //        {
        //            _selectedReason = value;
        //            OnPropertyChanged();
        //            if (value != null)
        //                Doc.CodeReason = value.CodeReason;
        //        }
        //    }
        //}
        public DocItem(DocVM pDocId,  TypeDoc pTypeDoc)
        {
            InitializeComponent();
            NokeyBoard();
            TypeDoc = pTypeDoc;
            Doc = pDocId;

            //// Отримуємо список причин із бази
            //var reasonsFromDb = db.GetReason(pTypeDoc.KindDoc);

            //// Конвертуємо у ObservableCollection (щоб Picker оновлювався автоматично)
            //Reasons = new ObservableCollection<BRB5.Model.DB.Reason>(reasonsFromDb);

            //// Якщо у Doc вже є вибраний CodeReason, ставимо його як SelectedItem
            //if (Doc.CodeReason != 0)
            //{
            //    SelectedReason = Reasons.FirstOrDefault(r => r.CodeReason == Doc.CodeReason);
            //}
            //else
            //{
            //    MainThread.BeginInvokeOnMainThread(() =>
            //    {
            //        ReasonPicker.Focus();
            //    });
            //}

            BindingContext = this;
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
            var r = db.GetDocWares(Doc, 1, eTypeOrder.Scan);
            if (r != null)
            {
                MyDocWares.Clear();
                int index = 0;
                foreach (var item in r)
                {
                    item.Even = (index % 2 == 0);
                    MyDocWares.Add(item);
                    index++;
                }
                PopulateDocWaresStackLayout();
            }
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
        }
        private void PopulateDocWaresStackLayout()
        {
            if (MyDocWares == null || !MyDocWares.Any())
                return;

            DocWaresStackLayout.Children.Clear(); // Clear existing children
            DocWaresStackLayout.Spacing = 0; // Remove vertical spacing between elements

            List<DocWaresEx> docWares = new ();

            if (Doc.CodeReason == 1)
            {
                docWares = MyDocWares.Where(x => x.CodeReason==-1).ToList();
            }
            else
            {
                docWares = MyDocWares.ToList();
            }
            
            foreach (var docWare in docWares)
            {
                // Create the main container StackLayout
                var mainStackLayout = new StackLayout
                {
                    Spacing = 0, // Remove spacing between child elements
                    Padding = new Thickness(0), // Remove padding
                };

                // Create the first Grid
                var grid = new Grid
                {
                    RowSpacing = 1,
                    ColumnSpacing = 1,
                    Padding = 1,
                    BackgroundColor = Color.FromArgb("#adaea7"),
                    BindingContext = docWare
                };

                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) }); // Назва/Код
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) }); // Замовлення
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) }); // Введено

                if (IsViewReason)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) }); // Причина
                }

                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

                // 2. Назва (Span на всі колонки)
                var nameLabel = new Label
                {
                    Text = docWare.NameWares,
                    BackgroundColor = Color.FromArgb(docWare.GetBackgroundColor)
                };
                Grid.SetColumnSpan(nameLabel, IsViewReason ? 4 : 3);
                grid.Children.Add(nameLabel);

                // 3. Код товару
                var codeLabel = new Label
                {
                    Text = docWare.CodeWares.ToString(),
                    BackgroundColor = Color.FromArgb(docWare.GetBackgroundColor)
                };
                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += async (s, e) =>
                {
                    await Navigation.PushAsync(new WareInfo(new ParseBarCode() { CodeWares = docWare.CodeWares }));
                };
                codeLabel.GestureRecognizers.Add(tapGesture);
                Grid.SetRow(codeLabel, 1);
                Grid.SetColumn(codeLabel, 0);
                grid.Children.Add(codeLabel);

                // 4. Кількість замовлення
                var quantityOrderLabel = new Label
                {
                    Text = docWare.QuantityOrder.ToString(),
                    BackgroundColor = Color.FromArgb(docWare.GetBackgroundColor)
                };
                Grid.SetRow(quantityOrderLabel, 1);
                Grid.SetColumn(quantityOrderLabel, 1);
                grid.Children.Add(quantityOrderLabel);

                // 5. Введена кількість
                var inputQuantityLabel = new Label
                {
                    Text = docWare.InputQuantity.ToString(),
                    BackgroundColor = Color.FromArgb(docWare.GetBackgroundColor)
                };
                Grid.SetRow(inputQuantityLabel, 1);
                Grid.SetColumn(inputQuantityLabel, 2);
                grid.Children.Add(inputQuantityLabel);

                // 6. Умовне додавання колонки Причини
                if (IsViewReason)
                {
                    var quantityReasonLabel = new Label
                    {
                        Text = docWare.QuantityReason.ToString(),
                        BackgroundColor = Color.FromArgb(docWare.GetBackgroundColor)
                    };
                    Grid.SetRow(quantityReasonLabel, 1);
                    Grid.SetColumn(quantityReasonLabel, 3);
                    grid.Children.Add(quantityReasonLabel);
                }

                mainStackLayout.Children.Add(grid);

                // Блок проблемних товарів (без змін)
                if (docWare.IsVisProblematic)
                {
                    var problematicGrid = new Grid { IsVisible = docWare.IsVisProblematic };
                    problematicGrid.Children.Add(new Label { Text = "Проблемні позиції..." }); // Приклад
                    mainStackLayout.Children.Add(problematicGrid);
                }

                DocWaresStackLayout.Children.Add(mainStackLayout);
            }
        }
        private async void F2Save(object sender, EventArgs e)
        {
            if (TypeDoc.KindDoc == eKindDoc.Lot) return;

            Doc.NumberOutInvoice = NumberOutInvoice;
            Doc.DateOutInvoice = ListDataStr[SelectedDataStr].DateString;
            if(TypeDoc.LinkedCodeDoc!=0)
            {
               Doc dl =(Doc) Doc.Clone();
                dl.TypeDoc = TypeDoc.LinkedCodeDoc;
                db.ReplaceDoc([dl]);
            }

            var d = db.GetDocWares(Doc, 2, eTypeOrder.Scan);
            var r = await c.SendDocsDataAsync(Doc,d );
            if (r?.State != 0) _ = DisplayAlert("Помилка", r.TextError, "OK");
            else
            {
                var toast = Toast.Make("Документ успішно збережений");
                _ = toast.Show();
            }
        }
        private async void F3Scan(object sender, EventArgs e) { await Navigation.PushAsync(new DocScan(Doc, TypeDoc)); }
        private async void F4WrOff(object sender, EventArgs e) { await Navigation.PushAsync(new ManualInput(Doc, TypeDoc));  }
        private void F6Doc(object sender, EventArgs e)
        {
            IsVisibleDocF6 = !IsVisibleDocF6;
            if (IsVisibleDocF6) DocDate.Focus();
        }
        private void DocNameFocus(object sender, FocusEventArgs e) {  DocName.Focus(); }
        private async void F5Act(object sender, TappedEventArgs e)
        {
            if(TypeDoc.KindDoc==eKindDoc.Lot||TypeDoc.IsViewAct)  await Navigation.PushAsync(new Act(Doc, TypeDoc));
        }
        //private void SelectReason(object sender, EventArgs e)
        //{
        //    Doc.CodeReason = SelectedReason?.CodeReason ?? 0;
        //    PopulateDocWaresStackLayout();
        //}

#if ANDROID
        public void OnPageKeyDown(Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
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
                case Keycode.F5:
                    F5Act(null, null);
                    return;
                case Keycode.F6:
                    F6Doc(null, EventArgs.Empty);
                    return;

                default:
                    return;
            }
         }
        #endif
    }
    public class AlternateColorDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate EvenTemplate { get; set; }
        public DataTemplate UnevenTemplate { get; set; }

        private int indexer = 0;
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            // TODO: Maybe some more error handling here
            return indexer++ % 2 == 0 ? EvenTemplate : UnevenTemplate;
        }
    }
}