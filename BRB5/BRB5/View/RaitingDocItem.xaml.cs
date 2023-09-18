using BRB5.Model;
using BRB5.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Utils;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace BRB5
{
    //[QueryProperty(nameof(NumberDoc), nameof(NumberDoc))]
    //[QueryProperty(nameof(TypeDoc), nameof(TypeDoc))]
    //[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RaitingDocItem
    {
        Timer t;
        Utils u = Utils.GetUtils();

        DB db = DB.GetDB();
        BL Bl = BL.GetBL();
        Doc cDoc;
        Connector.Connector c = Connector.Connector.GetInstance();
        bool _IsVisBarCode = false;
        public bool IsVisBarCode { get { return _IsVisBarCode; } set { _IsVisBarCode = value; OnPropertyChanged("IsVisBarCode"); } }
        public ObservableCollection<Model.RaitingDocItem> Questions { get; set; }

        int CountAll, CountChoice;
        public bool IsSave { get { return CountAll == CountChoice; } }
        bool IsAll = true;
        public string TextAllNoChoice { get { return IsAll ? "Без відповіді" : "Всі"; } }
        public string QuantityAllChoice { get { return $"{CountChoice}/{CountAll}"; } }
        
        bool IsOkWh { get {return Config.LocationWarehouse?.CodeWarehouse == cDoc.CodeWarehouse; } }
        public string NameWarehouse
        {
            get
            {
                string res = cDoc.ShortAddress;
                if (!IsOkWh)
                {
                    var Wh = Bl.GetWarehouse(cDoc.CodeWarehouse);
                    if (Wh != null)
                        res += $"( {Wh.Location}){Environment.NewLine}Найближчий:\" + {Config.LocationWarehouse?.Name} ({Config.LocationWarehouse?.Location})";
                }
                return res;
            }
        }       
        public System.Drawing.Color GetGPSColor { get { if(Config.LocationWarehouse==null) return System.Drawing.Color.FromArgb(200, 200, 200);
                return IsOkWh ? System.Drawing.Color.FromArgb(100, 250, 100) :
                        System.Drawing.Color.FromArgb(250, 100, 100);
            } }

        public string SizeWarehouse { get { return (IsOkWh ? "25" : "50"); } }

        public string TextSave { get; set; } = "";
        public bool IsSaving { get; set; } = false;
        //public string TextButtonSave { get { return IsSaving ? "Зупинити" : "Зберегти"; } }

        public bool IsSaved { get; set; } = false;
        public string TextButtonSaved { get { return IsSaved ? "Закрити":"Зупинити"; } }

        bool IsAllOpen { get; set; } = true;
        public string TextAllOpen { get { return IsAllOpen ? "Згорнути" : "Розгорнути"; } set { OnPropertyChanged("IsAllOpen"); } }

        /*public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }*/
        public RaitingDocItem(Doc pDoc)
        {
            cDoc = pDoc;
            InitializeComponent();
            _ = LocationBrb.GetCurrentLocation(db.GetWarehouse());
            LocationBrb.OnLocation += (Location) =>
            {
                OnPropertyChanged("GetGPSColor");
                OnPropertyChanged("NameWarehouse");
                OnPropertyChanged("SizeWarehouse");
            };
            var Q = db.GetRaitingDocItem(cDoc);
            var R = new List<Model.RaitingDocItem>();
            foreach (var e in Q.Where(d => d.IsHead).OrderBy(d => d.OrderRS))
            {
                R.Add(e);
                foreach (var el in Q.Where(d => d.Parent == e.Id).OrderBy(d => d.OrderRS))
                {
                    if (e.Rating == 4)
                        el.IsVisible = false;
                    R.Add(el);
                }
            }
            //Костиль заради Всього
            var Total = Q.Where(d => d.Id == -1);
            if (Total.Count() == 1)
                R.Add(Total.FirstOrDefault());

            c.OnSave += (Res) => Device.BeginInvokeOnMainThread(() => 
            {
                TextSave += Res + Environment.NewLine; 
                OnPropertyChanged("TextSave"); 
            });

            CountAll = R.Count(el => !el.IsHead);
            NavigationPage.SetHasNavigationBar(this, Device.RuntimePlatform == Device.iOS);
            Questions = new ObservableCollection<Model.RaitingDocItem>(R);
            RefreshHead();
            this.BindingContext = this;
            StartTimer();
            FileLogger.WriteLogMessage($"Item Start=>{pDoc.NumberDoc}");
        }

        void StartTimer()
        {
            t = new System.Timers.Timer(3 * 60 * 1000); //3 хв
            t.AutoReset = true;
            t.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            t.Start();            
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            var task = Task.Run(() =>
            {
                c.SendRaitingFiles(cDoc.NumberDoc, 1, 3 * 60, 10 * 60);
            });
          
        }

        private void OnButtonClicked(object sender, System.EventArgs e)
        {
            Xamarin.Forms.View button = (Xamarin.Forms.View)sender;
            //Grid cc = button.Parent as Grid;
            var vQuestion = GetRaiting(sender);//cc.BindingContext as Raiting;
            var OldRating = vQuestion.Rating;
            switch (button.ClassId)
            {
                case "Ok":
                    vQuestion.Rating = 1;
                    break;
                case "SoSo":
                    vQuestion.Rating = 2;
                    break;
                case "Bad":
                    vQuestion.Rating = 3;
                    break;
                case "NotKnow":
                    vQuestion.Rating = 4;
                    break;

                default:
                    vQuestion.Rating = 0;
                    break;
            }
            if (OldRating == vQuestion.Rating)
                vQuestion.Rating = 0;

            if (vQuestion.IsItem)
            {
                var el = Questions.FirstOrDefault(i => i.Id == vQuestion.Parent);
                if (el != null) el.Rating = 0;
            }


            if (OldRating != vQuestion.Rating && (vQuestion.Rating == 4 || vQuestion.Rating == 0) && vQuestion.IsHead)
            {
                foreach (var el in Questions.Where(d => d.Parent == vQuestion.Id))
                {
                    el.IsVisible = vQuestion.Rating != 4;
                    if (el.Rating == 0 && vQuestion.Rating == 4)
                    {
                        el.Rating = 4;
                        db.ReplaceRaitingDocItem(el);
                    }
                }
            }
            db.ReplaceRaitingDocItem(vQuestion);
            RefreshHead();
        }

        void RefreshHead()
        {
            CountChoice = Questions.Count(el => !el.IsHead && el.Rating > 0);
            OnPropertyChanged("QuantityAllChoice");
            OnPropertyChanged("IsSave");
        }

        private void OnSetView(object sender, System.EventArgs e)
        {
            IsAll = !IsAll;
            if (IsAll)
                foreach (var el in Questions.Where(d => !d.IsVisible))
                    el.IsVisible = true;
            else
                foreach (var el in Questions.Where(d => !d.IsHead && d.Rating > 0))
                    if (el.Rating != 3) el.IsVisible = false;
                    else if (!String.IsNullOrEmpty(el.Note) || el.QuantityPhoto > 0) el.IsVisible = false;

            OnPropertyChanged("TextAllNoChoice");
        }

        private void OnButtonSaved(object sender, System.EventArgs e)
        {
            if (IsSaving && !IsSaved)
            {
                IsSaved = true;
                c.StopSave = true;                            
            }
            else
            {
                IsSaving = false;
                OnPropertyChanged("IsSaving");
            }
               
        }

        private void OnButtonSave(object sender, System.EventArgs e)
        {
            Task.Run(() =>
            {
                Result res;
                try
                {
                    TextSave = "";
                    IsSaving = true;
                    OnPropertyChanged("IsSaving");
                    IsSaved = false;
                    OnPropertyChanged("IsSaved");
                    OnPropertyChanged("TextButtonSaved");
                    var r = db.GetRaitingDocItem(cDoc);
                    Doc d = db.GetDoc(cDoc);
                    res = c.SendRaiting(r, d);
                    if (res.State == 0)
                    {
                        cDoc.State = 1;
                        db.SetStateDoc(cDoc);
                    }
                }
                catch (Exception ex)
                { res = new Result(ex); }
                finally
                {
                    IsSaved = true;
                    OnPropertyChanged("IsSaved");
                    OnPropertyChanged("TextButtonSaved");
                }

                // Navigation.PopAsync();
            }
            );
        }

        private void OnFindGPS(object sender, System.EventArgs e)
        {
            _ = LocationBrb.GetCurrentLocation(db.GetWarehouse());
        }

        private void EditPhoto(object sender, System.EventArgs e)
        {
            var vQuestion = GetRaiting(sender);
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
                double Size = u.GetFreeSpace(dir);
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

                var photo = await MediaPicker.CapturePhotoAsync(new MediaPickerOptions { Title = FileName });
                if (photo != null && File.Exists(photo.FullPath))
                {
                    var ext = Path.GetExtension(photo.FileName);
                    var newFile = Path.Combine(dir, FileName + ext);
                    using (var stream = await photo.OpenReadAsync())
                    using (var newStream = File.OpenWrite(newFile))
                        await stream.CopyToAsync(newStream);
                    vQuestion.QuantityPhoto++;
                    db.ReplaceRaitingDocItem(vQuestion);
                }
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage($"Item.TakePhotoAsync", eTypeLog.Error);
                await DisplayAlert("Сообщение об ошибке", ex.Message, "OK");
            }
        }

        private void Editor_Completed(object sender, EventArgs e)
        {
            db.ReplaceRaitingDocItem(GetRaiting(sender));
        }

        private void OnHeadTapped(object sender, EventArgs e)
        {
            var s = sender as Grid;
            var cc = s.Parent as StackLayout;

            var vRait = cc.BindingContext as Model.RaitingDocItem;
            var id = vRait.Id;
            foreach (var xx in Questions.Where(el => el.Parent == id))
            {
                xx.IsVisible = !xx.IsVisible;
            }

        }

        private void OnAllOpen(object sender, EventArgs e)
        {
            IsAllOpen = !IsAllOpen;
            if (IsAllOpen)
                foreach (var el in Questions)
                    el.IsVisible = true;
            else
                foreach (var el in Questions.Where(el => el.Parent != 9999999))
                    el.IsVisible = false;

            OnPropertyChanged("TextAllOpen");
        }

        private void BarCode(object sender, EventArgs e)
        {
            IsVisBarCode = !IsVisBarCode;
            zxing.IsScanning = IsVisBarCode;
        }
        private void OnScanBarCode(ZXing.Result result)
        {
            zxing.IsAnalyzing = false;

            var temp = Questions.Where(el => el.Id==-1).FirstOrDefault();
            if (temp.Note == null || !temp.Note.StartsWith(result.Text)) { temp.Note = result.Text + temp.Note; }
            db.ReplaceRaitingDocItem(temp);
            Questions[Questions.IndexOf(temp)] = temp;
            zxing.IsAnalyzing = true;
        }

        private Model.RaitingDocItem GetRaiting(object sender)
        {
            Xamarin.Forms.View V = (Xamarin.Forms.View)sender;
            return V.BindingContext as Model.RaitingDocItem;
        }
    }
}
    