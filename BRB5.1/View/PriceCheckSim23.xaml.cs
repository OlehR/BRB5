using BRB5.Model;
using BRB6.View;
using BL;
using BRB5;
using BarcodeScanning;
using CommunityToolkit.Maui.Core.Platform;
using UtilNetwork;
using BRB6.ViewModel;




#if ANDROID
using Android.Views;
#endif

//using BRB5.Connector;
namespace BRB6
{
    public partial class PriceCheckSim23:ForMVVM
    {
        PriceCheckVM VM;
        CameraView BarcodeScaner;
        double _PB = 0;
        public double PB { get { return _PB; } set { _PB = value; OnPropertyChanged(nameof(PB)); } }

        public PriceCheckSim23(TypeDoc pTypeDoc)
        {
            InitializeComponent();
            NokeyBoard();
            NavigationPage.SetHasNavigationBar(this, DeviceInfo.Platform == DevicePlatform.iOS);
            VM = new PriceCheckVM(pTypeDoc,this);
            this.BindingContext = VM;
     
        this.Unloaded += UnloadedEvent;
        }
        void UnloadedEvent(object sender, EventArgs e)
        {
            VM.UnloadedEvent();
        }

        void Progress(double pProgress) => MainThread.BeginInvokeOnMainThread(() => PB = pProgress);
        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (!Config.IsSoftKeyboard)
            {
#if ANDROID
                MainActivity.Key += OnPageKeyDown;
#endif
            }
            if (Config.IsVisScan)
            {
                BarcodeScaner = Helper.GetCameraView(true);

                BarcodeScaner.OnDetectionFinished += CameraView_OnDetectionFinished;

                GridZxing.Children.Add(BarcodeScaner);

            }
            if (!Config.IsVisScan)
                BarCodeInput.Focus();
            //if (IsVisDoubleScan && WP!=null) WP.StateDoubleScan = eCheckWareScaned.Nothing;
            Config.OnProgress += Progress;
        }
        private void CameraView_OnDetectionFinished(object sender, BarcodeScanning.OnDetectionFinishedEventArg e)
        {
            if (e.BarcodeResults.Length > 0)
            {
                BarcodeScaner.PauseScanning = true;
                VM.FoundWares(e.BarcodeResults[0].DisplayValue);
                Task.Run(async () =>
                {
                    await Task.Delay(1000);
                    BarcodeScaner.PauseScanning = false;
                });
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (Config.IsVisScan) BarcodeScaner.CameraEnabled = false;

            if (!Config.IsSoftKeyboard)
            {
#if ANDROID
                MainActivity.Key -= OnPageKeyDown;
#endif
            }
            VM.bl.SendLogPrice();
            Config.OnProgress -= Progress;
        }

        private async void OnClickWareInfo(object sender, EventArgs e)
        {
            if (VM.WP != null)
            {
                if (Config.IsVisScan) BarcodeScaner.PauseScanning = true;
                await Navigation.PushAsync(new WareInfo(VM.WP.ParseBarCode));
            }
        }

        public void Focused(string pName)
        {
            if (pName == "BarCodeInput")
                BarCodeFocused(null, null);
            else
                NumberOfReplenishment.Focus();
        }

        public void DisplayAlert(string title, string message, string cancel)
        {
            Dispatcher.Dispatch(() => DisplayAlert(title, message, cancel));
        }

        private void BarCodeFocused(object sender, FocusEventArgs e)
        {
            Dispatcher.Dispatch(() =>
            {
                BarCodeInput.CursorPosition = 0;
                BarCodeInput.SelectionLength = BarCodeInput.Text == null ? 0 : BarCodeInput.Text.Length;
                if (!BarCodeInput.IsFocused || Config.IsVisScan)
                    BarCodeInput.Focus();
            });
        }
#if ANDROID
        public void OnPageKeyDown(Keycode keyCode, KeyEvent e)
        {
         /*  switch (keyCode)
           {
            case Keycode.F1:
            OnClickPrintBlock(null, EventArgs.Empty);
               return;
            case Keycode.F2:
            OnF2(null, EventArgs.Empty);
               return;
            case Keycode.F4:
            OnF4(null, EventArgs.Empty);
               return;
            case Keycode.F5:
            OnF5(null, EventArgs.Empty);
               return;
            case Keycode.F6:
            OnClickPrintOne(null, EventArgs.Empty);
               return;

            default:
               return;
           }*/
         }
#endif
    }
}