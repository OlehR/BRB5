﻿using BRB5.Model;
using BRB5.View;
using BRB5.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Utils;
using Xamarin.Essentials;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace BRB5
{
    //[QueryProperty(nameof(NumberDoc), nameof(NumberDoc))]
    //[QueryProperty(nameof(TypeDoc), nameof(TypeDoc))]
    //[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RaitingDocItem
    {       
        BL.BL Bl = BL.BL.GetBL();
        DocVM cDoc;
        
        bool _IsVisBarCode = false;
        public bool IsVisBarCode { get { return _IsVisBarCode; } set { _IsVisBarCode = value; OnPropertyChanged(nameof(IsVisBarCode)); } }
        ObservableCollection<Model.RaitingDocItem> _Questions;
        public ObservableCollection<Model.RaitingDocItem> Questions { get { return _Questions; } set { _Questions = value; OnPropertyChanged(nameof(Questions)); } }
        IEnumerable<Model.RaitingDocItem> All;

        int CountAll, CountChoice;
        public bool IsSave { get { return CountAll == CountChoice; } }
        bool IsAll = true;
        public string TextAllNoChoice { get { return IsAll ? "Без відповіді" : "Всі"; } }
        public string QuantityAllChoice { get { return CountAll>0? $"{CountChoice}/{CountAll}":""; } }
        
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

        public string SizeWarehouse { get { return (IsOkWh ? "25" : "50"); } }

        public string TextSave { get; set; } = "";
        bool _IsSaving = false;
        public bool IsSaving { get { return _IsSaving; } set { _IsSaving = value; OnPropertyChanged(nameof(IsSaving)); } }

        bool _IsSaved = false;
        public bool IsSaved { get {return _IsSaved;} set { _IsSaved = value; OnPropertyChanged(nameof(IsSaved)); OnPropertyChanged(nameof(TextButtonSaved)); } } 
        public string TextButtonSaved { get { return IsSaved ? "Закрити":"Зупинити"; } }

        bool IsAllOpen { get; set; } = true;
        public string TextAllOpen { get { return IsAllOpen ? "Згорнути" : "Розгорнути"; }  }
        private bool IsRefreshList = true;
        private eTypeChoice _typeChoice = eTypeChoice.OnlyHead;
        public eTypeChoice Choice { get { return _typeChoice; } set { _typeChoice = value; OnPropertyChanged(nameof(OpacityAll)); OnPropertyChanged(nameof(OpacityOnlyHead)); OnPropertyChanged(nameof(OpacityNoAnswer)); } }
        public double OpacityAll { get { return Choice == eTypeChoice.All ? 1d : 0.4d; } }
        public double OpacityOnlyHead { get { return Choice == eTypeChoice.OnlyHead ? 1d : 0.4d; } }
        public double OpacityNoAnswer { get { return Choice == eTypeChoice.NoAnswer ? 1d : 0.4d; } }
        ZXingScannerView zxing;
        public bool IsVisScan { get { return Config.TypeScaner == eTypeScaner.Camera; } }

        public RaitingDocItem(DocVM pDoc)
        {
            FileLogger.WriteLogMessage($"Item Start=>{pDoc.NumberDoc}");
            cDoc = pDoc;
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, Device.RuntimePlatform == Device.iOS);
            this.BindingContext = this;
            Bl.InitTimerRDI(cDoc);            
           
            Questions = new ObservableCollection<Model.RaitingDocItem>();
            Bl.c.OnSave += (Res) => Device.BeginInvokeOnMainThread(() =>
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
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (IsVisScan) zxing = ZxingBRB5.SetZxing(GridZxing, zxing, (BarCode) => OnScanBarCode(BarCode));
            
            Bl.StartTimerRDI();
            if (IsRefreshList)Bl.LoadDataRDI(cDoc,GetData);
            IsRefreshList = true;
            _ = LocationBrb.GetCurrentLocation(Bl.db.GetWarehouse());
        }

        protected override void OnDisappearing() 
        {  
            base.OnDisappearing(); 
            Bl.StopTimerRDI();
            if (IsVisScan) zxing.IsScanning = false;
        }

        void ViewDoc()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Questions.Clear();
                foreach (var el in All.Where(el => (el.IsHead || el.Parent == 9999999 || // Заголовки Всього
                Choice == eTypeChoice.All ||                                             // Розгорнути      
                (Choice == eTypeChoice.NoAnswer && (el.Rating == 0 ||                    //Без відповіді  
                (el.Rating == 3 && String.IsNullOrEmpty(el.Note) && el.QuantityPhoto == 0)))  //Без опису           
                )))
                {
                    Questions.Add(el);
                }
                RefreshHead();
                Bl.CalcValueRating(All);
            });
        }


        void GetData(IEnumerable<Model.RaitingDocItem> pDocItem)
        {
            CountAll = pDocItem.Count(el => !el.IsHead);
            All = pDocItem;
            ViewDoc();
        }   

        private void OnButtonClicked(object sender, System.EventArgs e)
        {
            Xamarin.Forms.View button = (Xamarin.Forms.View)sender;
            //Grid cc = button.Parent as Grid;
            var vQuestion = GetRaiting(sender);//cc.BindingContext as Raiting;
            Bl.ChangeRaiting(vQuestion, button.ClassId, All);

            if (vQuestion.IsHead) ChangeItemBlok(vQuestion);

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
            var vQuestion = button.BindingContext as Model.RaitingDocItem;
            var FileName = $"{vQuestion.Id}_{DateTime.Now.ToString("yyyyMMdd_HHmmssfff")}";

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

        private void OnHeadTapped(object sender, EventArgs e)
        {
            var s = sender as Grid;
            var cc = s.Parent as StackLayout;
            var vRait = cc.BindingContext as Model.RaitingDocItem;
            vRait.IsVisible = !vRait.IsVisible;
            Choice = eTypeChoice.NotDefine;
            ChangeItemBlok(vRait);
        }

        private void ChangeItemBlok(Model.RaitingDocItem vRait)
        {
            MainThread.BeginInvokeOnMainThread(() =>
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
        }

        private void BarCode(object sender, EventArgs e)
        {
            IsVisBarCode = !IsVisBarCode;
            zxing.IsScanning = IsVisBarCode;
            zxing.IsAnalyzing = IsVisBarCode;
        }
        private void OnScanBarCode(string result)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                zxing.IsAnalyzing = false;
                var resultText = "[" + result + "]";
                var temp = Questions.Where(el => el.Id == -1).FirstOrDefault();

                if (string.IsNullOrEmpty(temp.Note)) temp.Note = resultText;
                else if (Regex.IsMatch(temp.Note, @"\[\d+\]")) temp.Note = Regex.Replace(temp.Note, @"\[\d+\]", resultText);
                     else temp.Note = resultText + temp.Note;

                Bl.db.ReplaceRaitingDocItem(temp);
                Questions[Questions.IndexOf(temp)] = temp;

                ListQuestions.ScrollTo(Questions.Last(), ScrollToPosition.Center, false);
                zxing.IsAnalyzing = true;
            });
        }

        private void ShowButton(object sender, EventArgs e)
        {
            switch ((sender as ImageButton).AutomationId)
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
            ListQuestions.ScrollTo( Questions.First(), ScrollToPosition.Start, false);
        }

        private Model.RaitingDocItem GetRaiting(object sender)
        {
            Xamarin.Forms.View V = (Xamarin.Forms.View)sender;
            return V.BindingContext as Model.RaitingDocItem;
        }
    }
}
    