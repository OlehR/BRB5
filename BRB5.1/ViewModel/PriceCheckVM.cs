using BarcodeScanning;
using BL;
using BRB5;
using BRB5.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UtilNetwork;

namespace BRB6.ViewModel
{
    interface ForMVVM {
        void Focused(string pName);
        void DisplayAlert(string title, string message, string cancel);
        void ShowToast(string message, bool isLong = false);
    }
    internal class PriceCheckVM : ObservableObject, IDisposable
    {
        ForMVVM ForMVVM;
        DB db = DB.GetDB();
        public BL.BL bl = BL.BL.GetBL();

        // Кількість для МР
        private int _mrQuantity = 0;
        public int MrQuantity
        {
            get => _mrQuantity;
            set => SetProperty(ref _mrQuantity, value);
        }

        private bool _isMrDialogVisible;
        public bool IsMrDialogVisible
        {
            get => _isMrDialogVisible;
            set => SetProperty(ref _isMrDialogVisible, value);
        }

        public ICommand OpenMrDialogCommand { get; }
        public ICommand IncrementCommand { get; }
        public ICommand DecrementCommand { get; }
        public ICommand ConfirmMrCommand { get; }
        public ICommand CloseMRDialogCommand { get; }
        public ICommand BarCodeHandInputCommand { get; }
        public ICommand ModifyValueCommand { get; }
        public ICommand UpdateReplenishmentCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand PrintBlockCommand { get; }
        public ICommand AddPrintBlockCommand { get; }
        public ICommand F2Command { get; }
        public ICommand F3Command { get; }
        public ICommand F4Command { get; }
        public ICommand F5Command { get; }
        public ICommand DoubleScanReactCommand { get; }
        public ICommand PrintOneCommand { get; }
        public ICommand DeletePromoItemCommand { get; }
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

        public bool IsVisF4 { get { return _TypeDoc.IsOffLine; /*Config.LocalCompany == eCompany.Sim23 || Config.LocalCompany == eCompany.PSU; */ } }
        public string F4Text { get { return IsOnline ? "OnLine" : "OffLine"; } }
        private bool _IsOnline = true;
        public bool IsOnline { get { return _IsOnline; } set { if (IsVisF4) _IsOnline = value; OnPropertyChanged(nameof(F4Text)); } }

        bool _IsVisRepl = false;
        public bool IsVisRepl { get => _IsVisRepl || IsPromoProposalMode; set { _IsVisRepl = value; OnPropertyChanged(nameof(IsVisRepl)); } }
        public bool IsSoftKeyboard { get { return Config.IsSoftKeyboard; } }
        public bool IsVisPromotion => !string.IsNullOrEmpty(WP?.PromotionName) && WP?.PromotionEnd != default(DateTime);

        WaresPrice _WP;
        public WaresPrice WP
        {
            get { return _WP; }
            set
            {
                _WP = value;
                BarCodeInput = _WP?.BarCodes?.Split(',').FirstOrDefault() ?? string.Empty; OnPropertyChanged(nameof(AllBarCodes));
                OnPropertyChanged(nameof(ExtraBarCodesCount)); OnPropertyChanged(nameof(HasExtraBarCodes)); OnPropertyChanged(nameof(HasWare));
                OnPropertyChanged(nameof(WP)); OnPropertyChanged(nameof(TextColorPrice)); OnPropertyChanged(nameof(BackgroundColorPrice));
                OnPropertyChanged(nameof(IsVisPriceOpt)); OnPropertyChanged(nameof(IsVisPriceNormal)); OnPropertyChanged(nameof(TextColorHttp));
                OnPropertyChanged(nameof(ColorPrintColorType)); OnPropertyChanged(nameof(IsVisPriceOptQ));
                OnPropertyChanged(nameof(IsVisPromotion));

            }
        }
        public bool HasWare => WP != null;
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
        public string TextColorPrice { get { return (WP != null && WP.Price != 0 && WP.Price == WP.PriceOld && (WP.PriceOpt == WP.PriceOptOld || WP.PriceOpt == WP.Price)) ? "#009800" : "#ff5c5c"; } set { OnPropertyChanged(nameof(TextColorPrice)); } }
        public string BackgroundColorPrice { get { return (WP == null || (WP.Price != 0 && WP.Price == WP.PriceOld && (WP.PriceOpt == WP.PriceOptOld || WP.PriceOpt == WP.Price))) ? "#F8F9FA" : "#fff0f0"; } }

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

        private bool _isBarCodesDropdownVisible;
        public bool IsBarCodesDropdownVisible { get => _isBarCodesDropdownVisible; set => SetProperty(ref _isBarCodesDropdownVisible, value); }

        public List<string> AllBarCodes => WP?.BarCodes?.Split(',').Select(x => x.Trim()).ToList() ?? new List<string>();
        public int ExtraBarCodesCount => AllBarCodes.Count > 1 ? AllBarCodes.Count - 1 : 0;
        public bool HasExtraBarCodes => ExtraBarCodesCount > 0;

        public ICommand ShowAllBarCodesCommand => new Command(() => IsBarCodesDropdownVisible = true);
        public ICommand CloseBarCodesCommand => new Command(() => IsBarCodesDropdownVisible = false);
        public string QuantityToAddText => $"+{QuantityToAdd}";
        private bool _autoSave;
        public bool IsPromoProposalMode => _TypeDoc.CodeDoc == 16;
        public bool IsNotPromoProposalMode => !IsPromoProposalMode;

        private readonly TypeDoc _TypeDoc;
        private DateTime _selectedDate = DateTime.Today;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (SetProperty(ref _selectedDate, value))
                {
                    UpdateReasonByExpirationDate();
                }
            }
        }

        private bool _isAutoDiscountReason = false;
        public bool IsAutoDiscountReason
        {
            get => _isAutoDiscountReason;
            set
            {
                if (SetProperty(ref _isAutoDiscountReason, value))
                {
                    OnPropertyChanged(nameof(IsNotAutoDiscountReason));
                    OnPropertyChanged(nameof(IsReasonSelectionEnabled));
                }
            }
        }
        public bool IsNotAutoDiscountReason => !IsAutoDiscountReason;
        public bool IsReasonSelectionEnabled => HasWare && !IsAutoDiscountReason;

        private string _autoDiscountReasonText = string.Empty;
        public string AutoDiscountReasonText
        {
            get => _autoDiscountReasonText;
            set => SetProperty(ref _autoDiscountReasonText, value);
        }

        private BRB5.Model.DB.Reason _selectedReason;
        public BRB5.Model.DB.Reason SelectedReason
        {
            get => _selectedReason;
            set => SetProperty(ref _selectedReason, value);
        }

        // Тип списку тепер містить об'єкти класу Reason
        private List<BRB5.Model.DB.Reason> _reasonList;
        public List<BRB5.Model.DB.Reason> ReasonList
        {
            get => _reasonList;
            set => SetProperty(ref _reasonList, value);
        }
        public ObservableCollection<DocWaresPromo> PromoItems { get; } = [];

        private bool _isPromoListVisible;
        public bool IsPromoListVisible
        {
            get => _isPromoListVisible;
            set { if (SetProperty(ref _isPromoListVisible, value)) { OnPropertyChanged(nameof(IsPromoListNotVisible)); } }
        }
        private string _proposedPrice = string.Empty;
        public string ProposedPrice
        {
            get => _proposedPrice;
            set => SetProperty(ref _proposedPrice, value);
        }
        public bool IsPromoListNotVisible => !IsPromoListVisible;

        public ICommand ShowPromoListCommand { get; }
        public ICommand ClosePromoListCommand { get; }
        public ICommand SubmitPromoCommand { get; }
        public ICommand AddPromoItemCommand { get; }

        public bool IsAutoSave
        {
            get => _autoSave;
            set => SetProperty(ref _autoSave, value);
        }
        public PriceCheckVM(TypeDoc pTypeDoc, ForMVVM pForMVVM, bool autoSave = true)
        {
            ForMVVM = pForMVVM;
            IsAutoSave = autoSave;
            bl.ClearWPH();
            var r = db.GetCountScanCode();
            IsVisDoubleScan = pTypeDoc.CodeDoc == 15;
            //IsVisF4 = pTypeDoc.IsOffLine    ;
            _TypeDoc = pTypeDoc;

            if (Config.TypeUsePrinter == eTypeUsePrinter.StationaryWithCutAuto) PrintType = -1;

            if (r != null)
            {
                AllScan = r.AllScan;
                BadScan = r.BadScan;
                LineNumber = r.LineNumber;
                PackageNumber = r.PackageNumber;
                OnClickAddPrintBlock();
            }
            if (!IsVisScan)
                Config.BarCode = BarCode;

            BarCodeHandInputCommand = new RelayCommand(BarCodeHandInput);
            UpdateReplenishmentCommand = new RelayCommand(OnUpdateReplenishment);
            ClearCommand = new RelayCommand(() => NumberOfReplenishment = "0");
            ModifyValueCommand = new RelayCommand<object>(p =>
            {
                if (p == null) return;
                int delta = Convert.ToInt32(p);
                ModifyValue(delta);
            });
            PrintBlockCommand = new RelayCommand(async () => await PrintBlock());

            AddPrintBlockCommand = new RelayCommand(() =>
            {
                PackageNumber++;
                ListPrintBlockItems.Add(new PrintBlockItems() { PackageNumber = PackageNumber });
            });

            F2Command = new RelayCommand(() =>
            {
                if (IsPromoProposalMode) return; // Блокуємо дію
                var Res = bl.SendLogPrice();
                ForMVVM.DisplayAlert("Збереження", Res?.TextError ?? "Помилка збереження", "OK");
            });

            F3Command = new RelayCommand(() =>
            {
                if (IsPromoProposalMode) return; // Блокуємо дію (блок і так завжди видимий)
                IsVisRepl = !IsVisRepl;
                if (IsVisRepl) ForMVVM.Focused("NumberOfReplenishment");
            });

            F4Command = new RelayCommand(() => IsOnline = !IsOnline);

            F5Command = new RelayCommand(() => IsMultyLabel = !IsMultyLabel);

            DoubleScanReactCommand = new RelayCommand(DoubleScanReact);

            PrintOneCommand = new RelayCommand(() =>
            {
                if (IsEnabledPrint && WP != null)
                    ForMVVM.DisplayAlert("Друк", bl.c.PrintHTTP(new[] { WP.CodeWares }), "OK");
            });

            OpenMrDialogCommand = new RelayCommand(() =>
            {
                MrQuantity = 0;
                IsMrDialogVisible = true;
                ForMVVM.Focused("MRQuantityEntry");
            });

            IncrementCommand = new RelayCommand(() => MrQuantity++);

            DecrementCommand = new RelayCommand(() =>
            {
                if (MrQuantity > 0) MrQuantity--;
            });

            ConfirmMrCommand = new RelayCommand(() =>
            {
                db.UpdateMR(LineNumber, MrQuantity);

                IsMrDialogVisible = false;
            });
            CloseMRDialogCommand = new RelayCommand(() =>
            {
                IsMrDialogVisible = false;
            });
            ShowPromoListCommand = new Command(() =>
            {
                // 1. Очищуємо поточний список на екрані
                PromoItems.Clear();

                // 2. Зчитуємо свіжі дані з локальної бази даних
                var savedItems = db.GetPromoProposalItems();
                foreach (var item in savedItems)
                {
                    PromoItems.Add(item);
                }

                // 3. Відкриваємо модальне вікно
                IsPromoListVisible = true;
            });
            ClosePromoListCommand = new Command(() => IsPromoListVisible = false);

            SubmitPromoCommand = new Command(async () =>
            {
                var Res = bl.SendLogPrice();
                ForMVVM.DisplayAlert("Збереження", Res?.TextError ?? "Помилка збереження", "OK");

                PromoItems.Clear();
                IsPromoListVisible = false;
            });

            // Майбутнє джерело даних з БД (закоментовано):
            ReasonList = db.GetReason(pTypeDoc.LevelReason, true).ToList();

            // Тимчасове заповнення реальними об'єктами для збереження вигляду:
            //ReasonList = new List<BRB5.Model.DB.Reason>
            //{
            //    new() { CodeReason = 1, NameReason = "Заблоковані СКЮ" },
            //    new() { CodeReason = 2, NameReason = "Моніторинг" },
            //    new() { CodeReason = 3, NameReason = "Надмірні залишки" },
            //    new() { CodeReason = 4, NameReason = "Терміни що спливають" }
            //};
            AddPromoItemCommand = new Command(AddPromoItem);
            DeletePromoItemCommand = new RelayCommand<DocWaresPromo>(DeletePromoItem);
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
                    UriPicture = new Uri(Config.ApiUrl1 + $"Wares/{WP.CodeWares}.png");
                    if (IsPriceRedAlert())
                    {
                        ProtoBRB.PlayNativeBeep();
                    }
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
        private bool IsPriceRedAlert()
        {
            if (WP == null) return false;

            // Умова з BackgroundColorPrice/TextColorPrice
            bool isPriceMatch = WP.PriceOld == 0 || (WP.Price != 0
                             && WP.Price == WP.PriceOld
                             && (WP.PriceOpt == WP.PriceOptOld || WP.PriceOpt == WP.Price));

            return !isPriceMatch;
        }
        public void Dispose() { Config.BarCode -= BarCode; }

        public void UnloadedEvent()
        {
            if (IsVisDoubleScan && bl.WPH != null) // При виході фіксувати останне не збережене в подвійному скануванні.
                bl.SaveDoubleScan(bl.WPH.IsBarCode ? 101 : 102, bl.WPH, PackageNumber, LineNumber);
        }

        private void OnClickAddPrintBlock()
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

        private void BarCodeHandInput()
        {
            var text = BarCodeInput;
            FoundWares(text, true);
        }

        private void OnUpdateReplenishment()
        {
            if (WP != null)
            {
                if (decimal.TryParse(NumberOfReplenishment, out decimal d))
                    db.UpdateReplenishment(LineNumber, d);

                int TypeDoc = Config.TypeDoc.Where(el => el.KindDoc == eKindDoc.DocCheck).FirstOrDefault()?.CodeDoc ?? 0;
                var DWId = new DocWaresId() { CodeWares = WP.CodeWares, NumberDoc = DateTime.Now.ToString("yyyyMMdd"), TypeDoc = TypeDoc };

                db.ReplaceDoc([new(DWId)]);
                var xx = db.GetDocWaresSample(DWId);
                decimal r = (xx?.Quantity ?? 0) + d;
                db.ReplaceDocWaresSample([new(DWId) { Quantity = r, QuantityMax=WP.Rest , ExtInfo=WP.PromotionName}]);
                ForMVVM.ShowToast("Додано");
            }
        }

        private void DoubleScanReact()
        {
            if (IsWareScaned == eCheckWareScaned.PriceTagScaned || IsWareScaned == eCheckWareScaned.WareNotFit)//Відсутній товар
            {
                bl.SaveDoubleScan(102, WP, PackageNumber, LineNumber);
                WP = null;
            }
            else if (IsWareScaned == eCheckWareScaned.WareScaned || IsWareScaned == eCheckWareScaned.PriceTagNotFit)//Відсутній ціник
            {
                bl.SaveDoubleScan(101, WP, PackageNumber, LineNumber);
                WP = null;
            }
        }

        private void ModifyValue(int delta)
        {
            if (WP != null)
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
                if (IsAutoSave && IsNotPromoProposalMode)
                    OnUpdateReplenishment();
            }
        }

        public void AddPromoItem()
        {
            if (WP == null)
                return;

            // Перевірка на дублікат перед додаванням
            if (PromoItems.Any(x =>
                x.CodeWares == WP.CodeWares &&
                x.ExpirationDate.Date == SelectedDate.Date))
            {
                ForMVVM.ShowToast("Товар з цією датою вже додано.");
                return;
            }

            decimal.TryParse(NumberOfReplenishment, out decimal qty);
            decimal.TryParse(ProposedPrice, out decimal price);

            // 1. Оновлюємо локальну базу даних SQLite
            db.UpdatePromoProposalItem(LineNumber, qty, price, SelectedReason?.CodeReason ?? 0, SelectedDate);

            // 2. Оновлюємо візуальний список на екрані виключно з бази даних
            PromoItems.Clear();
            var savedItems = db.GetPromoProposalItems();
            foreach (var item in savedItems)
            {
                PromoItems.Add(item);
            }

            ForMVVM.ShowToast($"Додано");

            // Очищення полів введення
            WP = null;
            SelectedReason = null;
            SelectedDate = DateTime.Today;
            NumberOfReplenishment = "0";
            ProposedPrice = string.Empty;
            IsAutoDiscountReason = false;
            AutoDiscountReasonText = string.Empty;

            ForMVVM.Focused("BarCodeInput");
        }

        private void DeletePromoItem(DocWaresPromo item)
        {
            if (item != null && PromoItems.Contains(item))
            {
                // Оновлюємо кількість на 0 в локальній базі даних перед вилученням з екрану
                db.UpdatePromoProposalItem(item.LineNumber, 0, item.Price, item.CodeReason, item.ExpirationDate);

                PromoItems.Remove(item);
                ForMVVM.ShowToast("Вилучено");
            }
        }

        private void UpdateReasonByExpirationDate()
        {
            if (WP == null)
            {
                IsAutoDiscountReason = false;
                AutoDiscountReasonText = string.Empty;
                SelectedReason = null;
                return;
            }

            WP.ExpirationDateInput = SelectedDate;
            var percentColor = WP.GetPercentColor;

            if (percentColor != null && (percentColor.Percent == 30 || percentColor.Percent == 50))
            {
                IsAutoDiscountReason = true;
                AutoDiscountReasonText = $"-{percentColor.Percent}%";
                SelectedReason = new BRB5.Model.DB.Reason
                {
                    CodeReason = -percentColor.Percent,
                    NameReason = AutoDiscountReasonText
                };
            }
            else
            {
                IsAutoDiscountReason = false;
                AutoDiscountReasonText = string.Empty;
                if (SelectedReason?.CodeReason == -30 || SelectedReason?.CodeReason == -50)
                {
                    SelectedReason = null;
                }
            }
        }
    }
}