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
        public bool IsVisF5Act => TypeDoc.KindDoc == eKindDoc.LotsCheck;
        public DocItem(DocId pDocId,  TypeDoc pTypeDoc)
        {
            NokeyBoard();
            TypeDoc = pTypeDoc;
            Doc = new DocVM(pDocId);           
            BindingContext = this;
            InitializeComponent();
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

            foreach (var docWare in MyDocWares)
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

                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });

                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

                // Add Labels to the Grid
                var nameLabel = new Label
                {
                    Text = docWare.NameWares,
                    BackgroundColor = Color.FromArgb(docWare.GetBackgroundColor) // Convert string to Color
                };
                Grid.SetColumnSpan(nameLabel, 4); // Set column span using Grid.SetColumnSpan
                grid.Children.Add(nameLabel);

                var codeLabel = new Label
                {
                    Text = docWare.CodeWares.ToString(),
                    BackgroundColor = Color.FromArgb(docWare.GetBackgroundColor)
                };
                Grid.SetRow(codeLabel, 1);
                Grid.SetColumn(codeLabel, 0);
                grid.Children.Add(codeLabel);

                var quantityOrderLabel = new Label
                {
                    Text = docWare.QuantityOrder.ToString(),
                    BackgroundColor = Color.FromArgb(docWare.GetBackgroundColor)
                };
                Grid.SetRow(quantityOrderLabel, 1);
                Grid.SetColumn(quantityOrderLabel, 1);
                grid.Children.Add(quantityOrderLabel);

                var inputQuantityLabel = new Label
                {
                    Text = docWare.InputQuantity.ToString(),
                    BackgroundColor = Color.FromArgb(docWare.GetBackgroundColor)
                };
                Grid.SetRow(inputQuantityLabel, 1);
                Grid.SetColumn(inputQuantityLabel, 2);
                grid.Children.Add(inputQuantityLabel);

                var quantityReasonLabel = new Label
                {
                    Text = docWare.QuantityReason.ToString(),
                    BackgroundColor = Color.FromArgb(docWare.GetBackgroundColor)
                };
                Grid.SetRow(quantityReasonLabel, 1);
                Grid.SetColumn(quantityReasonLabel, 3);
                grid.Children.Add(quantityReasonLabel);

                mainStackLayout.Children.Add(grid);

                // Create the second Grid for Problematic Items
                if (docWare.IsVisProblematic)
                {
                    var problematicGrid = new Grid
                    {
                        IsVisible = docWare.IsVisProblematic
                    };

                    var collectionView = new CollectionView
                    {
                        ItemsSource = docWare.ProblematicItems,
                        ItemTemplate = new DataTemplate(() =>
                        {
                            var itemGrid = new Grid
                            {
                                BackgroundColor = Colors.Beige
                            };

                            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4, GridUnitType.Star) });

                            var quantityLabel = new Label();
                            quantityLabel.SetBinding(Label.TextProperty, "Quantity");
                            itemGrid.Children.Add(quantityLabel);

                            var reasonLabel = new Label();
                            reasonLabel.SetBinding(Label.TextProperty, "ReasonName");
                            Grid.SetColumn(reasonLabel, 1);
                            itemGrid.Children.Add(reasonLabel);

                            return itemGrid;
                        })
                    };

                    problematicGrid.Children.Add(collectionView);
                    mainStackLayout.Children.Add(problematicGrid);
                }

                // Add the main StackLayout to the parent StackLayout
                DocWaresStackLayout.Children.Add(mainStackLayout);
            }
        }


        private async void F2Save(object sender, EventArgs e)
        {
            Doc.NumberOutInvoice = NumberOutInvoice;
            Doc.DateOutInvoice = ListDataStr[SelectedDataStr].DateString;
            var r = await c.SendDocsDataAsync(Doc, db.GetDocWares(Doc, 2, eTypeOrder.Scan));
            if (r.State != 0) _ = DisplayAlert("Помилка", r.TextError, "OK");
            else
            {
                var toast = Toast.Make("Документ успішно збережений");
                _ = toast.Show();
                //_ = this.DisplayToastAsync("Документ успішно збережений");
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
            if(TypeDoc.KindDoc==eKindDoc.LotsCheck)  await Navigation.PushAsync(new Act(Doc, TypeDoc));
        }

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