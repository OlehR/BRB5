using BRB5.Model;
using System.Collections.ObjectModel;
using BL.Connector;
using BL;
using Microsoft.Maui.Controls.Compatibility;
using BRB5;
using Grid = Microsoft.Maui.Controls.Grid;
using BarcodeScanning;

namespace BRB6.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PlanCheckerPrice
    {
        Connector c;
        protected DB db = DB.GetDB();
        BL.BL Bl = BL.BL.GetBL();
        //ZXingScannerView zxing;
        private DocVM Doc;
        public bool IsVisScan { get { return Config.TypeScaner == eTypeScaner.Camera; } }
        public ObservableCollection<DocWaresEx> WaresList { get; set; }
        //private object Sender;
        private int ShelfType;
        public bool IsSoftKeyboard { get { return Config.IsSoftKeyboard; } }
        CameraView BarcodeScaner;
        public PlanCheckerPrice(DocVM pDoc, int Selection)
        {
            Doc = pDoc;
            ShelfType = Selection;
            c = Connector.GetInstance();
            // TODO Xamarin.Forms.Device.RuntimePlatform is no longer supported. Use Microsoft.Maui.Devices.DeviceInfo.Platform instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
            NavigationPage.SetHasNavigationBar(this, Device.RuntimePlatform == Device.iOS || Config.TypeScaner == eTypeScaner.BitaHC61 || Config.TypeScaner == eTypeScaner.Zebra || Config.TypeScaner == eTypeScaner.PM550 || Config.TypeScaner == eTypeScaner.PM351);
            InitializeComponent();
            NokeyBoard();

            GetData();

            if (!IsVisScan)
                Config.BarCode = BarCode;
            this.BindingContext = this;
        }

        private void GetData()
        {
            var temp = c.GetPromotionData(Doc.NumberDoc);
            if (temp == null || temp.Info == null)
            {
                WaresList = new ObservableCollection<DocWaresEx>();
                _ = DisplayAlert("Помилка", temp?.TextError, "OK");
            }
            else WaresList = Bl.GetDataPCP(temp.Info, Doc, ShelfType);
                  
        }

        void BarCode(string pBarCode) {   WareFocus(pBarCode);  }

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
        }
        protected override void OnDisappearing()
        {
            if (IsVisScan) BarcodeScaner.CameraEnabled = false;
            base.OnDisappearing();
        }

        public void Dispose()  { Config.BarCode -= BarCode; }

        private void WareFocus(string pBarCode)
        {
            var parseBarCode = c.ParsedBarCode(pBarCode, true);
            var temp = db.GetScanData(Doc, parseBarCode);

            var tempSelected = WaresList.FirstOrDefault(item => item.CodeWares == temp?.CodeWares);

            if (tempSelected != null)
            {
                Dispatcher.Dispatch(() =>
                {
                    // Scroll to the selected item using the ScrollTo method
                    ListWares.ScrollTo(tempSelected, position: ScrollToPosition.Start, animate: false);

                    // Get the IVisualTreeElement for the CollectionView
                    var visualCollectionViewElement = (IVisualTreeElement)ListWares;

                    // Get the root views and their descendants
                    var rootViewsAndTheirDescendants = visualCollectionViewElement.GetVisualTreeDescendants();

                    // Find the Entry with the matching AutomationId (CodeWares)
                    var entryToFocus = rootViewsAndTheirDescendants
                        .OfType<Entry>()
                        .FirstOrDefault(e => e.AutomationId == tempSelected.CodeWares.ToString());

                    if (entryToFocus != null)
                    {
                        // Optionally move the Entry to a specific position before focusing
                        entryToFocus.TranslateTo(0, 0);

                        // Focus the Entry
                        entryToFocus.Focus();
                    }
                });
            }
            else
            {
                _ = DisplayAlert("", "Товар відсутній", "OK");
            }
        }


        private void Save(object sender, EventArgs e)
        {
            var res = c.SendDocsData(Doc, WaresList);
            _ = DisplayAlert("Збереження", res.TextError, "ok");            
        }

        private void EntryFocused(object sender, FocusEventArgs e)
        {
            var entry = sender as Entry;
                Dispatcher.Dispatch(() =>
                {
                    entry.CursorPosition = 0;
                    entry.SelectionLength = entry.Text == null ? 0 : entry.Text.Length;
                });
        }
        private void SaveItem(object sender, EventArgs e)
        {
            var tempCodeWares = ((Entry)sender).AutomationId; 
            var tempSelected = WaresList.FirstOrDefault(item => item.CodeWares.ToString() == tempCodeWares);
            if (tempSelected != null)
            {
                tempSelected.Quantity = tempSelected.InputQuantity;
                tempSelected.CodeReason = ShelfType;
                db.ReplaceDocWares(tempSelected);
            }
        }
        private void CameraView_OnDetectionFinished(object sender, OnDetectionFinishedEventArg e)
        {
            if (e.BarcodeResults.Length > 0)
            {
                BarcodeScaner.PauseScanning = true;
                WareFocus(e.BarcodeResults[0].DisplayValue); 
                Task.Run(async () => {
                    await Task.Delay(1000);
                    BarcodeScaner.PauseScanning = false;
                });
            }
        }

        private void OnTapped(object sender, TappedEventArgs e)
        {
            var stackLayout = sender as Microsoft.Maui.Controls.StackLayout;
            var tappedItem = stackLayout.BindingContext as DocWaresEx;

            if (tappedItem == null) return;
            if (IsVisScan) BarcodeScaner.PauseScanning = true;
            _ = Navigation.PushAsync(new WareInfo(new ParseBarCode() { CodeWares = tappedItem.CodeWares }));
        }

    }    
   
}