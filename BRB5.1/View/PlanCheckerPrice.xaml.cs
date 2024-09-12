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
        private string CurrentCodeWares;
        private int CurrentEntryType;
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

            ListWares.ItemTapped += (object sender, ItemTappedEventArgs e) => {
                if (e.Item == null) return;
                var temp = e.Item as DocWaresEx;
                ((ListView)sender).SelectedItem = null;

                if (IsVisScan) BarcodeScaner.PauseScanning = true;
                _ = Navigation.PushAsync(new WareInfo(new ParseBarCode() { CodeWares = temp.CodeWares}));
            };

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

            if (!IsSoftKeyboard)
            {
                MessagingCenter.Subscribe<KeyEventMessage>(this, "BackPressed", message => { KeyBack(); });
                MessagingCenter.Subscribe<KeyEventMessage>(this, "EnterPressed", message => { SaveAndFocusNext(CurrentCodeWares, CurrentEntryType); });
            }
        }
        protected override void OnDisappearing()
        {
            //if (IsVisScan) zxing.IsScanning = false;
            base.OnDisappearing();

            if (!IsSoftKeyboard)
            {
                MessagingCenter.Unsubscribe<KeyEventMessage>(this, "BackPressed");
                MessagingCenter.Unsubscribe<KeyEventMessage>(this, "EnterPressed");
            }

            if (IsVisScan) BarcodeScaner.CameraEnabled = false;
        }

        public void Dispose()  { Config.BarCode -= BarCode; }

        private void WareFocus(string pBarCode)
        {
            var parseBarCode = c.ParsedBarCode(pBarCode, true);
            var temp = db.GetScanData(Doc, parseBarCode);

            var tempSelected = WaresList.FirstOrDefault(item => item.CodeWares == temp?.CodeWares);

            if (tempSelected != null) Dispatcher.Dispatch(() =>
            {
                ListWares.ScrollTo(tempSelected, ScrollToPosition.Start, false);
                var index = WaresList.IndexOf(tempSelected);
                var list = ListWares.TemplatedItems.ToList();
                var Scaned = (((list[index] as ViewCell).View as Grid).Children.ElementAt(2) as Frame).Content as Entry;

                Scaned.Focus();
            });
            else _ = DisplayAlert("", "Товар відсутній", "ok"); 
        }

        private void Save(object sender, EventArgs e)
        {
            var res = c.SendDocsData(Doc, WaresList);
            _ = DisplayAlert("Збереження", res.TextError, "ok");            
        }

        private void SaveItemAvailable(object sender, EventArgs e) {    SaveAndFocusNext((sender as Entry).AutomationId, 1);   }

        private void EntryFocused(object sender, FocusEventArgs e)
        {
            var entry = sender as Entry;
            if (IsVisScan)
                Dispatcher.Dispatch(() =>
                {
                    entry.CursorPosition = 0;
                    entry.SelectionLength = entry.Text == null ? 0 : entry.Text.Length;
                });
        }

        private void SaveItemAdd(object sender, EventArgs e)    {  SaveAndFocusNext((sender as Entry).AutomationId, 2);   }

        private void SaveItem(object sender, FocusEventArgs e)   {   SaveAndFocusNext((sender as Entry).AutomationId, 3);    }

        private void SaveAndFocusNext(string codeWares, int Type)
        {
            var tempSelected = WaresList.FirstOrDefault(item => item.CodeWares.ToString() == codeWares);
            if (tempSelected != null)
            {
                tempSelected.Quantity = tempSelected.InputQuantity;
                tempSelected.CodeReason = ShelfType;
                db.ReplaceDocWares(tempSelected);
            }

            if (Type == 3) return;

            var list = ListWares.TemplatedItems.ToList();
            var index = WaresList.IndexOf(tempSelected);
            var nextIndex = (index + 1) >= list.Count ? -1 : index + 1;

            if (Type == 1 || nextIndex >= 0)
            {
                Dispatcher.Dispatch(() =>
                {
                    var nextEntry = (((list[Type == 1 ? index : nextIndex] as ViewCell).View as Grid).Children.ElementAt(Type == 1 ? 3 : 2) as Frame).Content as Entry;

                    if (nextEntry != null)
                    {
                        nextEntry.Focus();
                        CurrentCodeWares = nextEntry.AutomationId;
                        CurrentEntryType = Type == 1 ? 2 : 1;
                    }
                });
            }
        }


        private async void KeyBack()  {  await Navigation.PopAsync();   }

        private void TextChangedAdd(object sender, TextChangedEventArgs e)
        {
            var entry = sender as Entry;
            CurrentCodeWares = entry.AutomationId;
            CurrentEntryType = 2;
        }

        private void TextChangedAvailable(object sender, TextChangedEventArgs e)
        {
            var entry = sender as Entry;
            CurrentCodeWares = entry.AutomationId;
            CurrentEntryType = 1;
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
    }
    public class KeyboardTypeDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SoftKeyboardTemplate { get; set; }
        public DataTemplate HardKeyboardTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            return Config.IsSoftKeyboard ? SoftKeyboardTemplate : HardKeyboardTemplate;
        }
    }
   
}