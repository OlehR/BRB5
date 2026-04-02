using BarcodeScanning;
using BRB5;
using BRB5.Model;
using BRB6.ViewModel;
using BL;
using CommunityToolkit.Maui.Core.Platform;
using UtilNetwork;

#if ANDROID
using Android.Views;
#endif

namespace BRB6.View
{
    public partial class PriceCheckUniversal : IDisposable, ForMVVM
    {
        PriceCheckVM vm;

        double _PB = 0;
        public double PB { get { return _PB; } set { _PB = value; OnPropertyChanged(nameof(PB)); } }
        void Progress(double pProgress) => MainThread.BeginInvokeOnMainThread(() => PB = pProgress);
        public PriceCheckUniversal(TypeDoc pTypeDoc)
        {
            InitializeComponent();
            NokeyBoard();
            vm = new PriceCheckVM(pTypeDoc, this);
            this.BindingContext = vm;
            NavigationPage.SetHasNavigationBar(this, DeviceInfo.Platform == DevicePlatform.iOS);
            this.Unloaded += (s, e) => vm.UnloadedEvent();
        }

        // ── ForMVVM ────────────────────────────────────────────────────────────
        public void Focused(string pName)
        {
            if (pName == "BarCodeInput") BarCodeFocused(null, null);
            if (pName == "NumberOfReplenishment") NumberOfReplenishment.Focus();
        }

        public void DisplayAlert(string title, string message, string cancel)
            => _ = base.DisplayAlert(title, message, cancel);
               
        // ── Lifecycle ──────────────────────────────────────────────────────────
        CameraView BarcodeScaner;

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (!vm.IsSoftKeyboard)
            {
#if ANDROID
                MainActivity.Key += OnPageKeyDown;
#endif
            }
            if (vm.IsVisScan)
            {
                BarcodeScaner = Helper.GetCameraView(true);
                BarcodeScaner.OnDetectionFinished += CameraView_OnDetectionFinished;
                GridZxing.Children.Add(BarcodeScaner);
            }
            if (!vm.IsVisScan)
                BarCodeFocused(null, null);

            Config.OnProgress += Progress;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (vm.IsVisScan) BarcodeScaner.CameraEnabled = false;
            if (!vm.IsSoftKeyboard)
            {
#if ANDROID
                MainActivity.Key -= OnPageKeyDown;
#endif
            }
            BL.BL.GetBL().SendLogPrice();
            Config.OnProgress -= Progress;
        }

        // ── Navigation (залишається тут бо VM не знає про Navigation) ──────────
        private async void OnClickWareInfo(object sender, EventArgs e)
        {
            if (vm.WP != null)
            {
                if (vm.IsVisScan) BarcodeScaner.PauseScanning = true;
                await Navigation.PushAsync(new WareInfo(vm.WP.ParseBarCode));
            }
        }

        // ── Barcode entry focus ────────────────────────────────────────────────
        private void BarCodeFocused(object sender, FocusEventArgs e)
        {
            Dispatcher.Dispatch(() =>
            {
                BarCodeInput.CursorPosition = 0;
                BarCodeInput.SelectionLength = BarCodeInput.Text?.Length ?? 0;
                if (!BarCodeInput.IsFocused || vm.IsVisScan)
                    BarCodeInput.Focus();
            });
        }

        // ── Camera ─────────────────────────────────────────────────────────────
        private void CameraView_OnDetectionFinished(object sender, OnDetectionFinishedEventArg e)
        {
            if (e.BarcodeResults.Length > 0)
            {
                BarcodeScaner.PauseScanning = true;
                vm.FoundWares(e.BarcodeResults[0].DisplayValue);
                Task.Run(async () =>
                {
                    await Task.Delay(1000);
                    BarcodeScaner.PauseScanning = false;
                });
            }
        }

        // ── Hardware keys (Android) ────────────────────────────────────────────
#if ANDROID
        public void OnPageKeyDown(Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {
                case Keycode.F1: vm.PrintBlockCommand.Execute(null); return;
                case Keycode.F2: vm.F2Command.Execute(null); return;
                case Keycode.F4: vm.F4Command.Execute(null); return;
                case Keycode.F5: vm.F5Command.Execute(null); return;
                case Keycode.F6: vm.PrintOneCommand.Execute(null); return;
            }
        }
#endif

        public void Dispose() => vm.Dispose();
    }
}