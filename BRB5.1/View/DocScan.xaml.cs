using BL;
using BL.Connector;
using BRB5.Model;
using System.Collections.ObjectModel;
using BRB5;
using BarcodeScanning;

#if ANDROID
using Android.Views;
#endif

namespace BRB6.View
{
    public partial class DocScan
    {
        private ObservableCollection<DocWaresEx> _ListWares;
        public ObservableCollection<DocWaresEx> ListWares { get { return _ListWares; } set { _ListWares = value; OnPropertyChanged("ListWares"); } }
        
        DocWaresEx _ScanData;
        public DocWaresEx ScanData { get { return _ScanData; } set { _ScanData = value; OnPropertyChanged("ScanData"); } }
        protected DB db = DB.GetDB();
        private Connector c;
        BL.BL Bl = BL.BL.GetBL();
        public TypeDoc TypeDoc { get; set; }
        public int OrderDoc { get; set; }
        public bool IsSoftKeyboard { get { return Config.IsSoftKeyboard; } }
        public bool IsVisScan { get { return Config.TypeScaner == eTypeScaner.Camera; } }
        public bool IsViewReason { get { return TypeDoc.IsViewReason; } }
        private DocId DocId;
        private bool _IsVisQ = false;
        public bool IsVisQ { get { return _IsVisQ; } set { _IsVisQ = value; OnPropertyChanged(nameof(IsVisQ)); } }
        private bool _IsVisQOk = false;
        public bool IsVisQOk { get { return _IsVisQOk; } set { _IsVisQOk = value; OnPropertyChanged(nameof(IsVisQOk)); } }
        private string _DisplayQuestion;
        public string DisplayQuestion { get { return _DisplayQuestion; } set { _DisplayQuestion = value; OnPropertyChanged(nameof(DisplayQuestion)); } }
        private string TempBarcode;
        public IEnumerable<BRB5.Model.DB.Reason> Reason { get; set; }

        //private Grid _selectedGrid; 
        private List<BRB5.Model.DB.Reason> Reasons = new();
        private BRB5.Model.DB.Reason _defaultReason = new BRB5.Model.DB.Reason { CodeReason = 0, NameReason = "— без причини —" };


        CameraView BarcodeScaner;
        //ZXingScannerView zxing;
        private ObservableCollection<DocWaresEx> _originalListWares;

        public DocScan(DocId pDocId, TypeDoc pTypeDoc = null)
        {
            InitializeComponent();
            NokeyBoard();
            DocId = pDocId;
            TypeDoc = pTypeDoc != null ? pTypeDoc : Config.GetDocSetting(pDocId.TypeDoc);
            c = ConnectorBase.GetInstance();
            var tempListWares = db.GetDocWares(pDocId, 2, eTypeOrder.Scan);
            foreach (var t in tempListWares) { t.Ord = -1; }
            _originalListWares = tempListWares == null ? new ObservableCollection<DocWaresEx>() : new ObservableCollection<DocWaresEx>(tempListWares);
            ListWares = new ObservableCollection<DocWaresEx>(_originalListWares);
            OrderDoc = ListWares.Count > 0 ? ListWares.Max(el => el.OrderDoc) : 0;
            if (ListWares.Count > 0) ListViewWares.SelectedItem = ListWares[0];
            NavigationPage.SetHasNavigationBar(this, DeviceInfo.Platform == DevicePlatform.iOS || Config.TypeScaner == eTypeScaner.BitaHC61 || Config.TypeScaner == eTypeScaner.ChainwayC61 || Config.TypeScaner == eTypeScaner.Zebra || Config.TypeScaner == eTypeScaner.PM550 || Config.TypeScaner == eTypeScaner.PM351 || Config.TypeScaner == eTypeScaner.MetapaceM_K4);
            
            Reason = db.GetReason(TypeDoc.KindDoc, true);
            _defaultReason = Reason.FirstOrDefault(r => r.CodeReason == 0);
            if (_defaultReason == null)  _defaultReason = new BRB5.Model.DB.Reason {CodeReason = 0,NameReason = "— без причини —"};  
            PopulateReasonOptions();

            this.BindingContext = this;
        }

        private void HandBarCode(object sender, EventArgs e)
        {
            BarCode(inputBarCode.Text, true);
        }
        void BarCode(string pBarCode) => BarCode(pBarCode, false);
        void BarCode(string pBarCode, bool IsHandInput)
        {
            ScanData = db.GetScanData(DocId, c.ParsedBarCode(pBarCode, IsHandInput));
            FindWareByBarCodeAsync(pBarCode);
            if (ScanData != null)
            {
                ScanData.BarCode = pBarCode;

                ScanData.CodeReason = _defaultReason.CodeReason; // завжди дефолт
                ReasonPicker.SelectedItem = _defaultReason;

                if (ScanData.QuantityBarCode > 0) ScanData.InputQuantity = ScanData.QuantityBarCode;
                else inputQ.Text = "";
                inputQ.Keyboard = DeviceInfo.Platform == DevicePlatform.iOS ? Keyboard.Default : ScanData.CodeUnit == Config.GetCodeUnitWeight ? Keyboard.Telephone : Keyboard.Numeric;
                inputBarCode.IsReadOnly = true;

                inputQ.IsReadOnly = true;
                inputQ.Focus();
                inputQ.IsReadOnly = false;
                //AddWare();
            }
        }
        public void Dispose() { Config.BarCode -= BarCode; }
        private async void AddWare()
        {
            if (ScanData?.InputQuantity > 0)
            {
                if (TypeDoc.TypeControlQuantity == eTypeControlDoc.Control && ScanData.BeforeQuantity + ScanData.InputQuantity * ScanData.Coefficient > ScanData.QuantityMax && ScanData.QuantityMax > 0)
                {
                    await DisplayAlert("Перевищено ліміт по позиції", $"{ScanData.NameWares}\nМаксимальна кількість: {ScanData.QuantityMax}\nВже додано: {ScanData.BeforeQuantity}\nСпроба додати: {ScanData.InputQuantity * ScanData.Coefficient}", "Ok");
                    return;
                }
                ScanData.Quantity = ScanData.InputQuantity * ScanData.Coefficient;
                ScanData.OrderDoc = ++OrderDoc;
                ScanData.Ord = -1;
                if (db.ReplaceDocWares(ScanData))
                {
                    ListWares.Insert(0, ScanData);
                    foreach (var ware in ListWares)
                    {
                        if (ware.CodeWares == ScanData.CodeWares) ware.Ord = -1;
                    }
                    ListViewWares.SelectedItem = ListWares[0];
                    ScanData = null;

                    ReasonPicker.SelectedItem = _defaultReason;
                }
                inputQ.Unfocus();
                inputBarCode.IsReadOnly = false;
            }
        }    

        private void UnfocusedInputQ(object sender, FocusEventArgs e)
        {
            if (ScanData != null)
            {
                if (!inputQ.IsFocused && ScanData.InputQuantity == 0)
                {
                    inputBarCode.IsReadOnly = true;
                    inputQ.Focus();
                }
                else
                    AddWare();
            }
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
                    CameraEnabled = true,
                    VibrationOnDetected = false,
                    BarcodeSymbologies = BarcodeFormats.Ean13 | BarcodeFormats.Ean8 | BarcodeFormats.QRCode,
                };

                BarcodeScaner.OnDetectionFinished += CameraView_OnDetectionFinished;

                GridZxing.Children.Add(BarcodeScaner);
            }
            else Config.BarCode = BarCode;
            if (!IsSoftKeyboard)
            {
#if ANDROID
            MainActivity.Key+= OnPageKeyDown;
#endif
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

            if (IsVisScan) BarcodeScaner.CameraEnabled = false;
        }

        public async void FindWareByBarCodeAsync(string BarCode)
        {
            if (ScanData == null)
            {
                await DisplayAlert("Товар не знайдено", "Даний штрихкод => " + BarCode + " відсутній в базі", "Ok");
            }
            else
            {
                if (!ScanData.IsRecord)
                {
                    if (TypeDoc.TypeControlQuantity == eTypeControlDoc.Ask)
                    {
                        bool addItem = await DisplayAlert("Додати відсутній товар?", ScanData.NameWares, "Ok", "Cancel");
                        TempBarcode = BarCode;
                        if (addItem)
                        {
                            ScanData.IsRecord = true;
                            ScanData.QuantityMax = decimal.MaxValue;
                            FindWareByBarCodeAsync(TempBarcode);
                        }
                        else ScanData = null;
                        return;
                    }
                    if (TypeDoc.TypeControlQuantity == eTypeControlDoc.Control)
                    {
                        await DisplayAlert("Товар відсутній в документі", ScanData.NameWares, "Ok");
                        ScanData = null;
                        return;
                    }
                }

                ScanData.BeforeQuantity = Bl.CountBeforeQuantity(ScanData.CodeWares, ListWares);

                if (TypeDoc.IsSimpleDoc)
                {
                    if (ScanData.BeforeQuantity > 0)
                    {
                        await DisplayAlert("Вже добавлено в документ!", ScanData.NameWares, "Ok");
                        return;
                    }
                }
            }
        }

        private void Reset(object sender, EventArgs e) { Bl.Reset(ScanData, ListWares); }

        private void CalcQuantity(object sender, TextChangedEventArgs e)
        {
            if (ScanData != null)
                ScanData.Quantity = ScanData.InputQuantity * ScanData.Coefficient;
        }       

        private void Up(object sender, EventArgs e)
        {
            var selectedItem = (DocWaresEx)ListViewWares.SelectedItem;
            if (selectedItem != null)
            {
                var selectedIndex = ListWares.IndexOf(selectedItem);
                if (selectedIndex > 0)
                {
                    ListViewWares.SelectedItem = ListWares[selectedIndex - 1];
                    ListViewWares.ScrollTo(ListViewWares.SelectedItem, ScrollToPosition.Center, false);
                }
                OnPropertyChanged(nameof(ListWares));
            }
        }

        private void Down(object sender, EventArgs e)
        {
            var selectedItem = (DocWaresEx)ListViewWares.SelectedItem;
            if (selectedItem != null)
            {
                var selectedIndex = ListWares.IndexOf(selectedItem);

                if (selectedIndex < ListWares.Count - 1)
                {
                    ListViewWares.SelectedItem = ListWares[selectedIndex + 1];
                    ListViewWares.ScrollTo(ListViewWares.SelectedItem, ScrollToPosition.Center, false);
                }
                OnPropertyChanged(nameof(ListWares));
            }
        }

        private void CameraView_OnDetectionFinished(object sender, OnDetectionFinishedEventArg e)
        {
            if (e.BarcodeResults.Length > 0)
            {
                BarcodeScaner.PauseScanning = true;
                BarCode(e.BarcodeResults[0].DisplayValue);
                Task.Run(async () => {
                    await Task.Delay(1000);
                    BarcodeScaner.PauseScanning = false;
                });
            }
        }

        private void CompletedInputQ(object sender, EventArgs e) {  AddWare(); }

        /*private void PopulateReasonOptions()
        {
            ReasonOptions.Children.Clear();
            ReasonOptions.RowDefinitions.Clear();
            ReasonOptions.ColumnDefinitions.Clear();

            // Define two columns
            ReasonOptions.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            ReasonOptions.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            int row = 0, column = 0;

            if (Reason == null) return;

            foreach (var reason in Reason)
            {
                var reasonGrid = new Grid
                {
                    Margin = new Thickness(5),
                    Padding = new Thickness(5),
                    BackgroundColor = Color.FromArgb("#E0E0E0"),
                    BindingContext = reason
                };

                var reasonLabel = new Label
                {
                    Text = reason.NameReason,
                    FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    VerticalOptions = LayoutOptions.CenterAndExpand
                };

                reasonGrid.Children.Add(reasonLabel);
                reasonGrid.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() => OnReasonTapped(reasonGrid))
                });

                // Add a new row definition if needed
                if (column == 0)
                {
                    ReasonOptions.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                }

                ReasonOptions.Children.Add(reasonGrid);
                Grid.SetRow(reasonGrid, row);
                Grid.SetColumn(reasonGrid, column);

                column++;
                if (column >= 2)
                {
                    column = 0;
                    row++;
                }
            }
        }
        */
        /*private void OnReasonTapped(Grid selectedGrid)
        {
            var reason = (BRB5.Model.DB.Reason)selectedGrid.BindingContext;

            if (_selectedGrid == selectedGrid)
            {
                // Deselect the currently selected grid
                selectedGrid.BackgroundColor = Color.FromArgb("#E0E0E0");
                _selectedGrid = null;
                if (ScanData != null)
                {
                    ScanData.CodeReason = 0; // Set to 0 when deselected
                }
            }
            else
            {
                // Reset the background color of the previously selected grid
                if (_selectedGrid != null)
                {
                    _selectedGrid.BackgroundColor = Color.FromArgb("#E0E0E0");
                }

                // Highlight the newly selected grid
                selectedGrid.BackgroundColor = Color.FromArgb("#FFD700");
                _selectedGrid = selectedGrid;
                if (ScanData != null)
                {
                    ScanData.CodeReason = reason.CodeReason; // Set to the selected reason's CodeReason
                }
            }
        }
        */

        private void PopulateReasonOptions()
        {
            Reasons.Clear();
            Reasons.Add(_defaultReason);
            foreach (var r in Reason.Where(r => r.CodeReason != 0))
                Reasons.Add(r);

            ReasonPicker.ItemsSource = Reasons;
            ReasonPicker.SelectedItem = _defaultReason;

            if (ScanData != null)
                ScanData.CodeReason = _defaultReason.CodeReason;
        }


        private void ReasonPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ReasonPicker.SelectedItem is BRB5.Model.DB.Reason selectedReason && ScanData != null)
            {
                ScanData.CodeReason = selectedReason.CodeReason;
            }
        }

        private void OnListViewWaresItemTapped(object sender, TappedEventArgs e)
        {
            if (ScanData == null) return;

            if (ListWares.Count == _originalListWares.Count)
            {
                ListWares = new ObservableCollection<DocWaresEx>(_originalListWares.Where(w => w.CodeWares == ScanData.CodeWares));
            }
            else
            {
                ListWares = new ObservableCollection<DocWaresEx>(_originalListWares);
            }

            OnPropertyChanged(nameof(ListWares));
        }
#if ANDROID
        public void OnPageKeyDown(Keycode keyCode, KeyEvent e)
        { 
           switch (keyCode)
           {
            case Keycode.F1:
               Reset(null, EventArgs.Empty);
               return;
            case Keycode.F2:
               Up(null, EventArgs.Empty);
               return;
            case Keycode.F3:
               Down(null, EventArgs.Empty);
               return;
            case Keycode.F8:
                    Reset(null, EventArgs.Empty);
                    AddWare();
                    return;
            default:
               return;
           }
         }

#endif
    }
}