using BarcodeScanning;
using BRB5;
using BRB5.Model;
using BRB6.Template;
using BRB6.View;
using Microsoft.Maui.Controls.Compatibility;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Utils;
using Grid = Microsoft.Maui.Controls.Grid;
using StackLayout = Microsoft.Maui.Controls.StackLayout;

namespace BRB6
{
    public partial class RaitingDocItem : ContentPage, IHeadTapHandler, IRatingButtonHandler
    {
        BL.BL Bl = BL.BL.GetBL();
        DocVM cDoc;
        bool IsLoad = false;

        bool _IsVisBarCode = false;
        public bool IsVisBarCode { get { return _IsVisBarCode; } set { _IsVisBarCode = value; OnPropertyChanged(nameof(IsVisBarCode)); } }
      
        IEnumerable<BRB5.Model.RaitingDocItem> All;
        List<IViewRDI> AllViewRDI;

        int CountAll, CountChoice;
        public bool IsSave => CountAll == CountChoice;
        bool IsAll = true;
        public string TextAllNoChoice => IsAll ? "Без відпові" : "Всі";
        public string QuantityAllChoice => CountAll > 0 ? $"{CountChoice}/{CountAll}" : "";

        bool IsOkWh { get {return LocationBrb.LocationWarehouse?.CodeWarehouse == cDoc.CodeWarehouse; } }

        public string NameWarehouse
        {
            get
            {
                try {
                    string res = cDoc.ShortAddress;
                    if (!IsOkWh)
                    {
                        var Wh = Bl.GetWarehouse(cDoc.CodeWarehouse);
                        if (Wh != null)
                            res += $"( {Wh.Location}){Environment.NewLine}Найближчий:\" + {LocationBrb.LocationWarehouse?.Name} ({LocationBrb.LocationWarehouse?.Location})";
                    }
                    return res;
                }
                catch( Exception ex) 
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                }
                return null;
            }
        }

        public System.Drawing.Color GetGPSColor { get { if(LocationBrb.LocationWarehouse==null) return System.Drawing.Color.FromArgb(200, 200, 200);
                return IsOkWh ? System.Drawing.Color.FromArgb(100, 250, 100) :
                        System.Drawing.Color.FromArgb(250, 100, 100);
            } }
        public string SizeWarehouse => IsOkWh ? "25" : "50";

        public string TextSave { get; set; } = "";
        bool _IsSaving = false;
        public bool IsSaving { get { return _IsSaving; } set { _IsSaving = value; OnPropertyChanged(nameof(IsSaving)); } }

        bool _IsSaved = false;
        public bool IsSaved { get {return _IsSaved;} set { _IsSaved = value; OnPropertyChanged(nameof(IsSaved)); OnPropertyChanged(nameof(TextButtonSaved)); } }
        public string TextButtonSaved => IsSaved ? "Закрити" : "Зупинити";

        bool IsAllOpen { get; set; } = true;
        public string TextAllOpen => IsAllOpen ? "Згорнути" : "Розгорнути";
        private bool IsRefreshList = true;
        private eTypeChoice _typeChoice = eTypeChoice.NotDefine;
        public eTypeChoice Choice { get { return _typeChoice; } set { _typeChoice = value; OnPropertyChanged(nameof(OpacityAll)); OnPropertyChanged(nameof(OpacityOnlyHead)); OnPropertyChanged(nameof(OpacityNoAnswer)); } }
        public double OpacityAll => Choice == eTypeChoice.All ? 1d : 0.4d;
        public double OpacityOnlyHead => Choice == eTypeChoice.OnlyHead ? 1d : 0.4d;
        public double OpacityNoAnswer => Choice == eTypeChoice.NoAnswer ? 1d : 0.4d;
        public bool IsVisScan => Config.TypeScaner == eTypeScaner.Camera;
        CameraView BarcodeScaner;
        public bool IsVisibleBarcodeScanning { get; set; } = false;

        public RaitingDocItem(DocVM pDoc)
        {
            try
            {
                FileLogger.WriteLogMessage($"Item Start=>{pDoc.NumberDoc}");
                cDoc = pDoc;
                InitializeComponent();

                Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetUseSafeArea(this, true);

                NavigationPage.SetHasNavigationBar(this, DeviceInfo.Platform == DevicePlatform.iOS);
                this.BindingContext = this;
                Bl.InitTimerRDI(cDoc);

                Bl.c.OnSave += (Res) => Dispatcher.Dispatch(() =>
                {
                    TextSave += Res + Environment.NewLine;
                    OnPropertyChanged(nameof(TextSave));
                });

                LocationBrb.OnLocation += (Location) =>
                {
                    OnPropertyChanged(nameof(GetGPSColor));
                    OnPropertyChanged(nameof(NameWarehouse));
                    OnPropertyChanged(nameof(SizeWarehouse));
                };
                Bl.LoadDataRDI(cDoc, GetData);
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        protected override void OnAppearing()
        {
            try
            {
                base.OnAppearing();

                if (IsVisScan)
                {
                    BarcodeScaner = new CameraView
                    {
                        VerticalOptions = LayoutOptions.FillAndExpand,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        CameraEnabled = false,
                        VibrationOnDetected = false,
                        BarcodeSymbologies = BarcodeFormats.Ean13 | BarcodeFormats.Ean8 | BarcodeFormats.QRCode,
                    };
                    BarcodeScaner.OnDetectionFinished += CameraView_OnDetectionFinished;
                    GridZxing.Children.Add(BarcodeScaner);
                }

                Bl.StartTimerRDI();
                IsRefreshList = true;
                _ = LocationBrb.GetCurrentLocation(Bl.db.GetWarehouse());
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Bl.StopTimerRDI();
            if (IsVisScan) BarcodeScaner.CameraEnabled = false;
        }
        async void ViewDoc()
        {
            if (!IsLoad)
                return;

            try
            {
                IsLoad = false;

                //if (DeviceInfo.Platform == DevicePlatform.Android)
                //{
                //    MainThread.BeginInvokeOnMainThread(() => QuestionsStackLayout.Children.Clear());

                //    bool IsAddItem = true;
                //    foreach (var el in AllViewRDI.Where(el =>
                //        el.Data.IsHead ||
                //        el.Data.Parent == 9999999 ||
                //        Choice == eTypeChoice.All ||
                //        (Choice == eTypeChoice.NoAnswer &&
                //            (el.Data.Rating == 0 ||
                //            (el.Data.Rating == 3 && string.IsNullOrEmpty(el.Data.Note) && el.Data.QuantityPhoto == 0)))))
                //    {
                //        if (IsAddItem || el.Data.IsHead)
                //            MainThread.BeginInvokeOnMainThread(() => QuestionsStackLayout.Children.Add(el));

                //        if (el.Data.IsHead)
                //            el.Data.IsVisible = Choice == eTypeChoice.All && IsAddItem;
                //    }
                //}
                //else if (DeviceInfo.Platform == DevicePlatform.iOS)
                //{
                    IEnumerable<BRB5.Model.RaitingDocItem> filtered = All.Where(el =>
                        el.IsHead ||
                        el.Parent == 9999999 || // підсумкове питання
                        (el.IsVisible && el.Parent != 9999999) || // підпитання, які мають бути видимі
                        Choice == eTypeChoice.All ||
                        (Choice == eTypeChoice.NoAnswer &&
                            (el.Rating == 0 || (el.Rating == 3 && string.IsNullOrEmpty(el.Note) && el.QuantityPhoto == 0)))
                    );

                    var limited = filtered.ToList();                                       

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        QuestionsCollectionView.ItemsSource = limited;
                    });
                //}

                RefreshHead();
                Bl.CalcValueRating(All);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
            }
            finally
            {
                IsLoad = true;
            }
        }

        void GetData(IEnumerable<BRB5.Model.RaitingDocItem> pDocItem)
        {
            All = pDocItem.ToList();
            CountAll = All.Count(el => !el.IsHead);
            IsVisibleBarcodeScanning = All.Any(el => el.Id == -1);
            OnPropertyChanged(nameof(IsVisibleBarcodeScanning));

            var headsOnly = All.Where(x => x.IsHead).ToList();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                CalculateAvailableHeight();
                QuestionsCollectionView.ItemsSource = headsOnly;
            });
            IsLoad = true;
        }

        private void OnButtonClicked(object sender, System.EventArgs e)
        {
            Microsoft.Maui.Controls.View button = (Microsoft.Maui.Controls.View)sender;
            var vQuestion = GetRaiting(sender);
            Bl.ChangeRaiting(vQuestion, button.ClassId, All);

            Bl.CalcSumValueRating(vQuestion, All);
            RefreshHead();
        }
        public void OnRatingButtonClicked(object sender, BRB5.Model.RaitingDocItem item)
        {
            var button = (Microsoft.Maui.Controls.View)sender;

            // запам'ятати стан до зміни
            bool wasNotKnow = item.Rating == 4;

            Bl.ChangeRaiting(item, button.ClassId, All);
            Bl.CalcSumValueRating(item, All);
            RefreshHead();

            // Якщо це заголовок і ми змінили стан "NotKnow"
            if (item.IsHead)
            {
                if (!wasNotKnow && item.Rating == 4)
                {
                    // поставили "NotKnow" → згортаємо
                    item.IsVisible = true;
                    OnHeadTapped(item);
                }
                else if (wasNotKnow && item.Rating != 4)
                {
                    // зняли "NotKnow" → розгортаємо
                    item.IsVisible = false;
                    OnHeadTapped(item);
                }
            }
        }

        void RefreshHead()
        {
            try {
                CountChoice = All.Count(el => !el.IsHead && el.Rating > 0);
                OnPropertyChanged(nameof(QuantityAllChoice));
                OnPropertyChanged(nameof(IsSave));
            }
            catch(Exception ex)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private void OnButtonSaved(object sender, System.EventArgs e)
        {
            if (IsSaving && !IsSaved)
            {
                IsSaved = true;
                Bl.c.IsStopSave = true;
            }
            else
            {
                IsSaving = false;
            }

        }
        private void OnButtonSave(object sender, System.EventArgs e)
        {
            //TextSave = "";
            IsSaving = true;
            IsSaved = false;
            Bl.SaveRDI(cDoc, () => IsSaved = true);
        }

        private void OnFindGPS(object sender, System.EventArgs e) =>  _ = LocationBrb.GetCurrentLocation(Bl.db.GetWarehouse());        

        private void EditPhoto(object sender, System.EventArgs e)
        {
            var vQuestion = GetRaiting(sender);
            IsRefreshList = false;
            Navigation.PushAsync(new RaitingDocItemEditPhoto(vQuestion));
        }

        async void TakePhotoAsync(object sender, EventArgs e)
        {
            ImageButton button = (ImageButton)sender;
            var vQuestion = button.BindingContext as BRB5.Model.RaitingDocItem;
            var FileName = $"{vQuestion.NumberDoc}_{vQuestion.Id}_{DateTime.Now.ToString("yyyyMMdd_HHmmssfff")}";

            try
            {
                var dir = Path.Combine(Config.PathFiles, vQuestion.NumberDoc);
                double Size = FileAndDir.GetFreeSpace(dir);
                if (Size < 10d * 1024d * 1024d)
                {
                    await DisplayAlert($"Недостатньо місця", $"Залишок=> {Size / (1024d * 1024):n3} Mb", "OK");
                    return;
                }
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    Directory.CreateDirectory(Path.Combine(dir, "Send"));
                }
                IsRefreshList = false;
                var photo = await MediaPicker.CapturePhotoAsync(new MediaPickerOptions { Title = FileName });

                await Task.Delay(10);

                if (photo != null) // && File.Exists(photo.FullPath))
                {
                    var ext = Path.GetExtension(photo.FileName);
                    var newFile = Path.Combine(dir, FileName + ext);
                    byte[] imageData;
                    using (var stream = await photo.OpenReadAsync())
                    {
                        imageData = NativeBase.ReadFully(stream);
                        byte[] resizedImage = Config.NativeBase.ResizeImage(imageData, Config.PhotoQuality.GetValue(), Config.Compress);
                        File.WriteAllBytes(newFile, resizedImage);
                        //using (var newStream = File.OpenWrite(newFile))
                        //    await stream.CopyToAsync(newStream);
                    }
                    vQuestion.QuantityPhoto++;
                    Bl.db.ReplaceRaitingDocItem(vQuestion);
                }
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage($"Item.TakePhotoAsync", eTypeLog.Error);
                await DisplayAlert("Помилка!", ex.Message, "OK");
            }
        }

        private void Editor_Completed(object sender, EventArgs e) => Bl.db.ReplaceRaitingDocItem(GetRaiting(sender));
        public void OnHeadTapped(BRB5.Model.RaitingDocItem head)
        {
            head.IsVisible = !head.IsVisible;
            Choice = eTypeChoice.NotDefine;

            var current = (QuestionsCollectionView.ItemsSource as IEnumerable<BRB5.Model.RaitingDocItem>)?.ToList() ?? new();
            var children = All.Where(el => el.Parent == head.Id).ToList();

            List<BRB5.Model.RaitingDocItem> updated;

            if (head.IsVisible)
            {
                int headIndex = current.FindIndex(el => el == head);
                if (headIndex >= 0)
                {
                    updated = current.ToList();
                    updated.InsertRange(headIndex + 1, children);
                }
                else
                {
                    updated = current.Concat(children).ToList();
                }
            }
            else
            {
                updated = current.Where(el => el.Parent != head.Id).ToList();
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                QuestionsCollectionView.ItemsSource = updated;

                if (DeviceInfo.Platform != DevicePlatform.iOS)
                    QuestionsCollectionView.ScrollTo(head, position: ScrollToPosition.Start, animate: false);
            });
        }
        private void BarCode(object sender, EventArgs e)
        {
            IsVisBarCode = !IsVisBarCode;
            BarcodeScaner.CameraEnabled = IsVisBarCode;
        }

        private void ShowButton(object sender, EventArgs e)
        {
            if (!IsLoad)
                return;

            var button = (ImageButton)sender;
            var mode = button.AutomationId switch
            {
                "All" => eTypeChoice.All,
                "OnlyHead" => eTypeChoice.OnlyHead,
                "NoAnswer" => eTypeChoice.NoAnswer,
                _ => eTypeChoice.NotDefine
            };

            Choice = mode;

            foreach (var el in All.Where(el => el.IsHead))
                el.IsVisible = mode == eTypeChoice.All;

            ViewDoc();
        }

        private void CameraView_OnDetectionFinished(object sender, BarcodeScanning.OnDetectionFinishedEventArg e)
        {
            if (e.BarcodeResults.Length > 0)
            {
                BarcodeScaner.PauseScanning = true;
                //OnScanBarCode(e.BarcodeResults[0].DisplayValue);
                Task.Run(async () => {
                    await Task.Delay(1000);
                    BarcodeScaner.PauseScanning = false;
                });
            }
        }
       
        private void CalculateAvailableHeight()
        {
            var screenHeight = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;
            var navigationBarHeight = 50;
            double otherElementsHeight = HeaderLabel.Height + GPSLabel.Height + BottomGrid.Height + navigationBarHeight + 60;
            var availableHeight = screenHeight - otherElementsHeight;
            QuestionsGrid.HeightRequest = availableHeight;
            QuestionsCollectionView.HeightRequest = availableHeight;
        }

        private BRB5.Model.RaitingDocItem GetRaiting(object sender) => (BRB5.Model.RaitingDocItem)((Microsoft.Maui.Controls.View)sender).BindingContext;
    }
}