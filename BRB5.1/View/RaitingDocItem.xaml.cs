﻿using BRB5.Model;
using BRB6.View;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Utils;
using Microsoft.Maui.Controls.Compatibility;
using BRB5;
using Grid = Microsoft.Maui.Controls.Grid;
using StackLayout = Microsoft.Maui.Controls.StackLayout;
using BarcodeScanning;
using BRB6.Template;
using System.Runtime.CompilerServices;

namespace BRB6
{
    //[QueryProperty(nameof(NumberDoc), nameof(NumberDoc))]
    //[QueryProperty(nameof(TypeDoc), nameof(TypeDoc))]
    //[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RaitingDocItem : ContentPage
    {       
        BL.BL Bl = BL.BL.GetBL();
        DocVM cDoc;
        bool IsLoad = false;
        
        bool _IsVisBarCode = false;
        public bool IsVisBarCode { get { return _IsVisBarCode; } set { _IsVisBarCode = value; OnPropertyChanged(nameof(IsVisBarCode)); } }
        //ObservableCollection<BRB5.Model.RaitingDocItem> _Questions;
        //public ObservableCollection<BRB5.Model.RaitingDocItem> Questions { get { return _Questions; } set { _Questions = value; OnPropertyChanged(nameof(Questions)); } }
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

                //Questions = new ObservableCollection<BRB5.Model.RaitingDocItem>();
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
                //if (IsRefreshList)Bl.LoadDataRDI(cDoc,GetData);

                IsRefreshList = true;
                _ = LocationBrb.GetCurrentLocation(Bl.db.GetWarehouse());

#if IOS
            HeaderLabel.SizeChanged += OnElementSizeChanged;
            BottomGrid.SizeChanged += OnElementSizeChanged;

            // Обчислюємо висоту після рендерингу
            Dispatcher.Dispatch(() =>
            {
                CalculateAvailableHeight();
            });
#endif
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

        void BildViewRDI()
        {
            AllViewRDI = [];
            foreach (var el in All)
            {
                IViewRDI e = el.IsHead ? new QuestionHeadTemplate(el, OnButtonClicked, OnHeadTapped) : new QuestionItemTemplate(el, OnButtonClicked);
                if(el.IsHead || el.Parent == 9999999)
                    MainThread.BeginInvokeOnMainThread(() => { QuestionsStackLayout.Children.Add(e); });
                AllViewRDI.Add(e);
            }
            IsLoad = true;
            Choice = eTypeChoice.OnlyHead;
        }
        
        async void ViewDoc()
        {
            if (IsLoad)
            {
                try
                {
                    IsLoad = false;                   
                    if (Choice == eTypeChoice.NoAnswer && DeviceInfo.Platform == DevicePlatform.iOS)
                    {
                        int n=AllViewRDI.Count(el => ( el.Data.Rating == 0 ||                    //Без відповіді  
                        (el.Data.Rating == 3 && String.IsNullOrEmpty(el.Data.Note) && el.Data.QuantityPhoto == 0)));  //Без опису      
                        if (n > 65)
                        {
                            MainThread.BeginInvokeOnMainThread(async () =>
                            {
                                await DisplayAlert("Увага", "Надто велика кількість елементів для iOS. Дайте відповідь на більше питань.", "OK");
                            });
                            Choice = eTypeChoice.NotDefine;
                            RefreshHead();
                            return;
                        }
                    }

                    bool IsAddItem = true;
                    MainThread.BeginInvokeOnMainThread(() => { QuestionsStackLayout.Children.Clear(); });

                    foreach (var el in AllViewRDI.Where(el => (el.Data.IsHead || el.Data.Parent == 9999999 || // Заголовки Всього
                    Choice == eTypeChoice.All ||                                             // Розгорнути      
                    (Choice == eTypeChoice.NoAnswer && (el.Data.Rating == 0 ||                    //Без відповіді  
                    (el.Data.Rating == 3 && String.IsNullOrEmpty(el.Data.Note) && el.Data.QuantityPhoto == 0)))  //Без опису           
                    )))
                    {
                        if (IsAddItem || el.Data.IsHead)
                        {                            
                            MainThread.BeginInvokeOnMainThread(() => { QuestionsStackLayout.Children.Add(el); });
                        }
                        if (el.Data.IsHead)
                            el.Data.IsVisible = Choice == eTypeChoice.All && IsAddItem;
                        if (IsAddItem && DeviceInfo.Platform == DevicePlatform.iOS && QuestionsStackLayout.Children.Count > 60)
                            IsAddItem = false;
                    }
                    RefreshHead();
                    Bl.CalcValueRating(All);
                    if (!IsAddItem)
                        MainThread.BeginInvokeOnMainThread(async () =>
                    {
                         await DisplayAlert("Увага", "Надто велика кількість елементів для iOS. Згорніть лишні групи.", "OK");
                    });
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
        }

        void GetData(IEnumerable<BRB5.Model.RaitingDocItem> pDocItem)
        {
            CountAll = pDocItem.Count(el => !el.IsHead);
            All = pDocItem;
            IsVisibleBarcodeScanning = All.Any(el => el.Id == -1);
            OnPropertyChanged(nameof(IsVisibleBarcodeScanning));
            BildViewRDI();            
        }

        private void OnButtonClicked(object sender, System.EventArgs e)
        {
            Microsoft.Maui.Controls.View button = (Microsoft.Maui.Controls.View)sender;
            //Grid cc = button.Parent as Grid;
            var vQuestion = GetRaiting(sender);//cc.BindingContext as Raiting;
            Bl.ChangeRaiting(vQuestion, button.ClassId, All);

            //if (vQuestion.IsHead) ChangeItemBlok(vQuestion);

            Bl.CalcSumValueRating(vQuestion, All);
            RefreshHead();
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
            } else IsSaving = false;
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

        private async void OnHeadTapped(object sender, EventArgs e)
        {
            var s = sender as Grid;
            var cc = s.Parent as QuestionHeadTemplate;
            var vRait = cc.Data;

            if(DeviceInfo.Platform == DevicePlatform.iOS )
            {
                var relatedItems = All.Where(el => el.Parent == vRait.Id).ToList();
                await Navigation.PushAsync(new RaitingDocGroup(vRait, relatedItems));
            }
            else
            {
                vRait.IsVisible = !vRait.IsVisible;
                Choice = eTypeChoice.NotDefine;
                ChangeItemBlok(vRait);
            }
        }

        private void ChangeItemBlok(BRB5.Model.RaitingDocItem vRait)
        {
            if (!IsLoad)
                return;
            //MainThread.BeginInvokeOnMainThread(() => { QuestionsStackLayout.Children.Add(el); });
            //Task.Run(() =>
        {
            try
            {
                IsLoad = false;
                var aa = QuestionsStackLayout.Children.Select(el => (IViewRDI)el).ToList();
                int index = 0;
                foreach (var el in aa)
                {
                    index++;
                    if (el.Data == vRait)
                        break;
                }
                if (vRait.IsVisible)
                {
                    foreach (var el in AllViewRDI.Where(el => el.Data.Parent == vRait.Id))
                    {
                        MainThread.BeginInvokeOnMainThread(() => { QuestionsStackLayout.Children.Insert(index++, el); });

                        if (DeviceInfo.Platform == DevicePlatform.iOS && QuestionsStackLayout.Children.Count > 60)
                        {
                                MainThread.BeginInvokeOnMainThread(async () =>
                                {
                                    await DisplayAlert("Увага", "Надто велика кількість елементів для iOS. Згорніть лишні групи.", "OK");
                                });
                                break;
                        }
                    }
                }
                else
                {
                    foreach (var el in AllViewRDI)
                    {
                        if (el.Data.Parent == vRait.Id)
                            MainThread.BeginInvokeOnMainThread(() => { QuestionsStackLayout.Children.Remove(el); });
                    }
                }
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
            finally
            {
                IsLoad = true;
            }
        }
           // );
        }

        /*private void ChangeItemBlok(BRB5.Model.RaitingDocItem vRait)
        {
            Dispatcher.Dispatch(() =>
            {
                var index = Questions.IndexOf(vRait) + 1;
                foreach (var el in All.Where(el => el.Parent == vRait.Id))
                {
                    if (vRait.IsVisible)
                    {
                        if (!Questions.Any(e => el.Id == e.Id))
                        {
                            Questions.Insert(index, el);
                            index++;
                        }
                    }
                    else Questions.Remove(el);
                }
            });
        }*/

        private void BarCode(object sender, EventArgs e)
        {
            IsVisBarCode = !IsVisBarCode;
            BarcodeScaner.CameraEnabled = IsVisBarCode;
        }
        /*private void OnScanBarCode(string result)
        {
            Dispatcher.Dispatch(() =>
            {
                var resultText = "[" + result + "]";
                var temp = Questions.Where(el => el.Id == -1).FirstOrDefault();
                
                if (string.IsNullOrEmpty(temp.Note)) temp.Note = resultText;
                else if (Regex.IsMatch(temp.Note, @"\[\d+\]")) temp.Note = Regex.Replace(temp.Note, @"\[\d+\]", resultText);
                     else temp.Note = resultText + temp.Note;

                Bl.db.ReplaceRaitingDocItem(temp);
                Questions[Questions.IndexOf(temp)] = temp;

                //ListQuestions.ScrollTo(Questions.Last(), ScrollToPosition.Center, false);
            });
        }*/

        private void ShowButton(object sender, EventArgs e)
        {
            if (IsLoad)
            {
                switch (((ImageButton)sender).AutomationId)
                {
                    case "All":
                        Choice = eTypeChoice.All;
                        foreach (var el in All.Where(el => el.IsHead))
                            el.IsVisible = true;
                        break;
                    case "OnlyHead":
                        Choice = eTypeChoice.OnlyHead;
                        foreach (var el in All.Where(el => el.IsHead))                            
                                el.IsVisible = false;
                        break;
                    case "NoAnswer":
                        Choice = eTypeChoice.NoAnswer;
                        foreach (var el in All.Where(el => el.IsHead))
                            el.IsVisible = false;
                        break;
                }
                ViewDoc();
            }
            //ListQuestions.ScrollTo( Questions.First(), ScrollToPosition.Start, false);
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

        private void OnElementSizeChanged(object sender, EventArgs e)
        {
            CalculateAvailableHeight();
        }

        private void CalculateAvailableHeight()
        {
            var screenHeight = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;
            var navigationBarHeight = 50;
            double otherElementsHeight = HeaderLabel.Height + GPSLabel.Height + BottomGrid.Height + navigationBarHeight + 60;
            var availableHeight = screenHeight - otherElementsHeight;
            QuestionsGrid.HeightRequest = availableHeight;
        }

        private BRB5.Model.RaitingDocItem GetRaiting(object sender) => (BRB5.Model.RaitingDocItem)((Microsoft.Maui.Controls.View)sender).BindingContext;
    }
}
    