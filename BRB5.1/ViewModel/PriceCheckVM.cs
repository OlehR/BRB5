using BarcodeScanning;
using BL;
using BRB5;
using BRB5.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilNetwork;

namespace BRB6.ViewModel
{
    interface ForMVVM {
        void Focused(string pName);
        void DisplayAlert(string title, string message, string cancel);
    }
    internal class PriceCheckVM: ObservableObject, IDisposable
    {
        ForMVVM ForMVVM;
        DB db = DB.GetDB();
        public BL.BL bl = BL.BL.GetBL();

        public ICommand BarCodeHandInputCommand { get; }
        public ICommand ModifyValueCommand { get; }
        public ICommand UpdateReplenishmentCommand { get; }
        public ICommand ClearCommand { get; }

        public List<PrintBlockItems> ListPrintBlockItems { get { return db.GetPrintBlockItemsCount().ToList(); } }

        public int SelectedPrintBlockItems { get { return ListPrintBlockItems.Count > 0 ? ListPrintBlockItems.Last().PackageNumber : -1; } }
        private PrintBlockItems _selectedPrintItem;
        public PrintBlockItems SelectedPrintItem
        {
            get => _selectedPrintItem;
            set
            {
                if (_selectedPrintItem != value)
                {
                    _selectedPrintItem = value;
                    OnPropertyChanged(nameof(SelectedPrintItem));
                }
            }
        }
        public bool IsVisPriceNormal { get { return WP != null && (WP.PriceOld != WP.PriceNormal) && WP.PriceNormal != 0; } }
        public bool IsVisPriceOpt { get { return WP != null && (WP.PriceOpt != 0 || WP.PriceOptOld != 0); } }
        public bool IsVisPriceOptQ { get { return WP != null && WP.QuantityOpt != 0; } }

        public bool IsVisF4 { get { return Config.LocalCompany == eCompany.Sim23; } }
        public string F4Text { get { return IsOnline ? "OnLine" : "OffLine"; } }
        private bool _IsOnline = true;
        public bool IsOnline { get { return _IsOnline; } set { _IsOnline = value; OnPropertyChanged(nameof(F4Text)); } }

        bool _IsVisRepl = false;
        public bool IsVisRepl { get { return _IsVisRepl; } set { _IsVisRepl = value; OnPropertyChanged(nameof(IsVisRepl)); } }
        public bool IsSoftKeyboard { get { return Config.IsSoftKeyboard; } }
        

        WaresPrice _WP;
        public WaresPrice WP
        {
            get { return _WP; }
            set
            {
                _WP = value; OnPropertyChanged(nameof(WP)); OnPropertyChanged(nameof(TextColorPrice)); OnPropertyChanged(nameof(BackgroundColorPrice));
                OnPropertyChanged("IsVisPriceOpt"); OnPropertyChanged(nameof(IsVisPriceNormal)); OnPropertyChanged(nameof(TextColorHttp));
                OnPropertyChanged("ColorPrintColorType"); OnPropertyChanged(nameof(IsVisPriceOptQ));
            }
        }
        //ZXingScannerView zxing;
        //ZXingDefaultOverlay overlay;

        int _PrintType = 0;//Колір чека 0-звичайний 1-жовтий, -1 не розділяти.        
        public int PrintType { get { return _PrintType; } set { _PrintType = value; OnPropertyChanged(nameof(PrintType)); OnPropertyChanged(nameof(ColorPrintColorType)); } }
        public bool IsEnabledPrint { get { return Config.TypeUsePrinter != eTypeUsePrinter.NotDefined; } }
        /// <summary>
        /// Номер сканування цінників за день !!!TMP Треба зберігати в базі.
        /// </summary>
        int LineNumber = 0;
        public int AllScan { get; set; } = 0;
        public int BadScan { get; set; } = 0;
        /// <summary>
        /// Номер пакета цінників за день !!!TMP Треба зберігати в базі.
        /// </summary>
        int _PackageNumber = 1;
        public int PackageNumber { get { return _PackageNumber; } set { _PackageNumber = value; OnPropertyChanged(nameof(PackageNumber)); OnPropertyChanged(nameof(ListPrintBlockItems)); OnPropertyChanged(nameof(SelectedPrintBlockItems)); } }


        //public int ColorPrintColorType() { return Color.parseColor(HttpState != eStateHTTP.HTTP_OK ? "#ffb3b3" : (PrintType == 0 ? "#ffffff" : "#3fffff00")); }

        public string ColorPrintColorType { get { return WP == null ? "#ffffff" : WP.MinQuantity == 0 ? "#ffd8d8" : WP.ActionType > 0 ? "#F0DC82" : "#ffffff"; } }
        public string TextColorPrice { get { return (WP != null && WP.Price != 0 && WP.Price == WP.PriceOld && WP.PriceOpt == WP.PriceOptOld) ? "#009800" : "#ff5c5c"; } set { OnPropertyChanged(nameof(TextColorPrice)); } }
        public string BackgroundColorPrice { get { return (WP == null ||( WP.Price != 0 && WP.Price == WP.PriceOld && WP.PriceOpt == WP.PriceOptOld)) ? "#F8F9FA" : "#fff0f0"; }  }

        public string TextColorHttp { get { return (bl.LastResult != null && bl.LastResult.StateHTTP == eStateHTTP.HTTP_OK) ? "#009800" : "#ff5c5c"; } }

        public bool _IsMultyLabel = false;
        public bool IsMultyLabel { get { return _IsMultyLabel; } set { _IsMultyLabel = value; OnPropertyChanged(nameof(IsMultyLabel)); OnPropertyChanged(nameof(F5Text)); } }
        public string F5Text { get { return IsMultyLabel ? "Дубл." : "Унік."; } }
        public bool IsVisScan { get { return Config.IsVisScan; } }
        /// <summary>
        /// 0 - нічого , 1 - сканований цінник, 2 - сканований товар, 3 - штрихкод товату не підходить, 4 - цінник не підходить, 5 - успішно
        /// </summary>
        private eCheckWareScaned _IsWareScaned = eCheckWareScaned.Nothing;
        public eCheckWareScaned IsWareScaned { get { return _IsWareScaned; } set { _IsWareScaned = value; OnPropertyChanged(nameof(ColorDoubleScan)); OnPropertyChanged(nameof(IsWareScaned)); /*OnPropertyChanged(nameof(ButtonDoubleScan));*/ OnPropertyChanged(nameof(MessageDoubleScan)); } }
        public bool IsVisDoubleScan { get; set; }
        public bool IsVisBarcode { get { return !IsVisDoubleScan; } }
        // private string _MessageDoubleScan;
        public string MessageDoubleScan { get { return EnumMethods.GetDescription(WP?.StateDoubleScan ?? eCheckWareScaned.Success); } } //set {  OnPropertyChanged(nameof(MessageDoubleScan)); } }
        //public string ButtonDoubleScan { get { return IsWareScaned == eCheckWareScaned.Nothing || IsWareScaned == eCheckWareScaned.Success ? "" :  IsWareScaned == eCheckWareScaned.WareScaned || IsWareScaned == eCheckWareScaned.PriceTagNotFit ? "Відсутній ціник" : "Відсутній товар"; } }
        public string ColorDoubleScan
        {
            get
            {
                return IsWareScaned == eCheckWareScaned.Success ? "#C5FFC4" : IsWareScaned == eCheckWareScaned.Bad || IsWareScaned == eCheckWareScaned.BadPrice ? "#FFC4C4" :
                                                     IsWareScaned == eCheckWareScaned.PriceTagScaned || IsWareScaned == eCheckWareScaned.WareScaned ? "#FEFFC4" : "#FFFFFF";
            }
        }

        public int QuantityToAdd
        {
            get
            {
                if (WP == null || WP.QuantityShelf <= 0)
                    return 6;
                if (WP.QuantityShelf > 10)
                    return 10;

                return WP.QuantityShelf;
            }
        }

        private string _numberOfReplenishment = "0";
        public string NumberOfReplenishment
        {
            get => _numberOfReplenishment;
            set => SetProperty(ref _numberOfReplenishment, value);
        }
        private string _barCodeInput;
        public string BarCodeInput
        {
            get => _barCodeInput;
            set => SetProperty(ref _barCodeInput, value);
        }
        private Uri _uriPicture;
        public Uri UriPicture
        {
            get => _uriPicture;
            set { _uriPicture = value; OnPropertyChanged(); }
        }

        public string QuantityToAddText => $"+{QuantityToAdd}";
        public PriceCheckVM(TypeDoc pTypeDoc, ForMVVM pForMVVM)
        {
            ForMVVM=pForMVVM;
            bl.ClearWPH();
            var r = db.GetCountScanCode();
            IsVisDoubleScan = pTypeDoc.CodeDoc == 15;

            if (Config.TypeUsePrinter == eTypeUsePrinter.StationaryWithCutAuto) PrintType = -1;

            if (r != null)
            {
                AllScan = r.AllScan;
                BadScan = r.BadScan;
                LineNumber = r.LineNumber;
                PackageNumber = r.PackageNumber;
                OnClickAddPrintBlock(null, null);
            }
            if (!IsVisScan)
                Config.BarCode = BarCode;

            BarCodeHandInputCommand = new RelayCommand(BarCodeHandInput);
            UpdateReplenishmentCommand = new RelayCommand(OnUpdateReplenishment);
            ClearCommand = new RelayCommand(() => NumberOfReplenishment = "0"); 
            ModifyValueCommand = new RelayCommand<object>(p => {
                if (p == null) return;
                int delta = Convert.ToInt32(p);
                ModifyValue(delta);
            });
        }
            
        void BarCode(string pBarCode) => FoundWares(pBarCode, false);

        public void FoundWares(string pBarCode, bool pIsHandInput = false)
        {
            if (!String.IsNullOrWhiteSpace(pBarCode))
            {
                LineNumber++;
                Config.OnProgress?.Invoke(0.2d);

                WP = bl.FoundWares(pBarCode, PackageNumber, LineNumber, pIsHandInput, IsVisDoubleScan, IsOnline);
                NumberOfReplenishment = "0";
                OnPropertyChanged(nameof(QuantityToAdd));
                OnPropertyChanged(nameof(QuantityToAddText));
                if (WP != null)
                {
                    AllScan++;
                    if (!WP.IsPriceOk)
                        BadScan++;
                    IsWareScaned = WP.StateDoubleScan;
                    UriPicture = new Uri(Config.ApiUrl1 + $"Wares/{WP.CodeWares:D9}.png");
                }
                if (Config.IsVibration)
                {
                    var duration = TimeSpan.FromMilliseconds(WP?.IsPriceOk == true ? 50 : 250);
                    Vibration.Vibrate(duration);
                }

                Config.OnProgress?.Invoke(0.9d);
                if (DeviceInfo.Platform != DevicePlatform.iOS) ForMVVM.Focused("BarCodeInput");
            }
            OnPropertyChanged(nameof(ListPrintBlockItems));
        }

        public void Dispose() { Config.BarCode -= BarCode; }

        public void UnloadedEvent()
        {
            if (IsVisDoubleScan && bl.WPH != null) // При виході фіксувати останне не збережене в подвійному скануванні.
                bl.SaveDoubleScan(bl.WPH.IsBarCode ? 101 : 102, bl.WPH, PackageNumber, LineNumber);
        }

        private void OnClickAddPrintBlock(object sender, EventArgs e)
        {
            PackageNumber++;
            ListPrintBlockItems.Add(new PrintBlockItems() { PackageNumber = PackageNumber });
        }

        public async Task PrintBlock()
        {
            // Тепер ми беремо дані з властивості SelectedPrintItem, а не з Xaml по імені
            if (SelectedPrintItem != null && IsEnabledPrint)
            {
                var message = bl.PrintPackage(PrintType, SelectedPrintItem.PackageNumber, IsMultyLabel);
                ForMVVM.DisplayAlert("Друк", message, "OK");
            }
        }

        private void OnF2(object sender, EventArgs e)
        {
            IsVisRepl = !IsVisRepl;
            if (IsVisRepl) ForMVVM.Focused("NumberOfReplenishment");
        }

        private void OnF4(object sender, EventArgs e) { IsOnline = !IsOnline; }

        private void OnF5(object sender, EventArgs e) { IsMultyLabel = !IsMultyLabel; }
               
        private void BarCodeHandInput()
        {
            var text = BarCodeInput;
            FoundWares(text, true);
        }

        private void OnClickPrintOne(object sender, EventArgs e)
        {
            if (IsEnabledPrint && WP != null)
                ForMVVM.DisplayAlert("Друк", bl.c.PrintHTTP(new[] { WP.CodeWares }), "OK");
        }

        private void OnUpdateReplenishment()
        {
            decimal d;
            if (decimal.TryParse(NumberOfReplenishment, out d))
                db.UpdateReplenishment(LineNumber, d);
        }

        private void DoubleScanReact(object sender, EventArgs e)
        {

            if (IsWareScaned == eCheckWareScaned.PriceTagScaned || IsWareScaned == eCheckWareScaned.WareNotFit)//Відсутній товар
            {
                bl.SaveDoubleScan(102, WP, PackageNumber, LineNumber);
                WP = null;
                //IsWareScaned = eCheckWareScaned.Nothing;
                //MessageDoubleScan = "Скануйте цінник чи товар";
            }
            else if (IsWareScaned == eCheckWareScaned.WareScaned || IsWareScaned == eCheckWareScaned.PriceTagNotFit)//Відсутній ціник
            {
                bl.SaveDoubleScan(101, WP, PackageNumber, LineNumber);
                WP = null;
                //IsWareScaned = eCheckWareScaned.Nothing;
                //MessageDoubleScan = "Скануйте цінник чи товар";
            }

        }

        
        //private void OnClearClicked(object sender, EventArgs e)
        //{
        //    NumberOfReplenishment = "0";

        //    OnUpdateReplenishment();
        //}

        //private void OnMinus1Clicked(object sender, EventArgs e)
        //{
        //    ModifyValue(-1);
        //}

        //private void OnPlus1Clicked(object sender, EventArgs e)
        //{
        //    ModifyValue(1);
        //}

        //private void OnPlusDynamicClicked(object sender, EventArgs e)
        //{
        //    ModifyValue(QuantityToAdd);
        //}

        private void ModifyValue(int delta)
        {
            if (int.TryParse(NumberOfReplenishment, out int currentVal))
            {
                int newVal = currentVal + delta;
                NumberOfReplenishment = (newVal < 0 ? 0 : newVal).ToString();
            }
            else
            {
                NumberOfReplenishment = delta > 0 ? delta.ToString() : "0";
            }
            OnUpdateReplenishment();
        }
    }
}
