using BRB5.Model;
using BRB5.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BRB5
{
    //[QueryProperty(nameof(NumberDoc), nameof(NumberDoc))]
    //[QueryProperty(nameof(TypeDoc), nameof(TypeDoc))]
    //[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Item : ContentPage, INotifyPropertyChanged
    {

        DB db = DB.GetDB();
        Doc cDoc;
        Connector.Connector c = Connector.Connector.GetInstance();
        public ObservableCollection<Raiting> Questions { get; set; }

        int CountAll,CountChoice;
        public bool  IsSave { get { return CountAll == CountChoice; } }
        bool IsAll = true;
        public string TextAllNoChoice { get {return IsAll?"Без відповіді":"Всі"; }  }
        public string QuantityAllChoice { get { return $"{CountChoice}/{CountAll}"; } }

        public string TextSave { get; set; } = "";
        public bool IsSaving { get; set; } = false;
        public string TextButtonSave { get { return IsSaving ? "Зупинити" : "Зберегти"; } }

        bool IsAllOpen { get; set; } = true;
        public string TextAllOpen { get { return IsAllOpen ? "Згорнути" : "Розгорнути"; } set { OnPropertyChanged("IsAllOpen"); } }

        /*public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }*/
        public Item(Doc pDoc)
        {
            cDoc = pDoc;
            
            InitializeComponent();
            var Q = db.GetRating(cDoc);
            var R = new List<Raiting>();
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
            var Total= Q.Where(d => d.Id == -1);
            if (Total.Count() == 1)
                R.Add(Total.FirstOrDefault());

            c.OnSave += (Res) => Device.BeginInvokeOnMainThread(()=>{ TextSave = Res; OnPropertyChanged("TextSave"); });
             //Ховаємо непотрібні питання
             /*foreach (var e in Q.Where(d => d.IsHead && d.Rating==4) )
             foreach (var el in Q.Where(d => d.Parent == e.Id))
             {
                 el.IsVisible = e.Rating != 4;
             }

             */
             CountAll = R.Count(el => !el.IsHead);

            Questions = new ObservableCollection<Raiting>(R);
            RefreshHead();
            this.BindingContext = this;
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
            if(OldRating == vQuestion.Rating)
                vQuestion.Rating = 0;
            
            if(OldRating != vQuestion.Rating && (vQuestion.Rating == 4  || vQuestion.Rating == 0) && vQuestion.IsHead)
            {
                foreach (var el in Questions.Where(d => d.Parent == vQuestion.Id))
                {
                    el.IsVisible = vQuestion.Rating != 4;
                    if (el.Rating == 0 && vQuestion.Rating == 4)
                    {
                        el.Rating = 4;
                        db.ReplaceRaiting(el);
                    }
                }
            }
            db.ReplaceRaiting(vQuestion);
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
                    el.IsVisible = false;

            OnPropertyChanged("TextAllNoChoice");
        }

        private void OnButtonSave(object sender, System.EventArgs e)
        {
            Task.Run(() =>
            {
                TextSave = "";
                IsSaving = true;
                OnPropertyChanged("IsSaving");
                OnPropertyChanged("TextButtonSave");
                var r = db.GetRating(cDoc);
                var res = c.SendRaiting(r, cDoc);
                if (res.State == 0)
                {
                    cDoc.State = 1;
                    db.SetStateDoc(cDoc);
                }

                IsSaving = false;
                OnPropertyChanged("IsSaving");
                OnPropertyChanged("TextButtonSave");

                DisplayAlert("Збереження", res.TextError, "OK");//.Wait();
                Navigation.PopAsync();
            }
            );
        }

        private void EditPhoto(object sender, System.EventArgs e)
        {
            var vQuestion = GetRaiting(sender);
             Navigation.PushAsync(new EditPhoto( vQuestion));
        }

            async void TakePhotoAsync(object sender, EventArgs e)
        {
            ImageButton button = (ImageButton)sender;          
          
            var vQuestion = button.BindingContext as Raiting;
 
            var FileName = $"{vQuestion.Id}_{DateTime.Now.ToString("yyyyMMdd_hhmmssfff")}";
     
            try
            {
                var dir = Path.Combine(Config.PathFiles, vQuestion.NumberDoc);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    Directory.CreateDirectory(Path.Combine(dir, "Send"));
                }
                
                var photo = await MediaPicker.CapturePhotoAsync(new MediaPickerOptions{Title = FileName});
                if (photo!=null && File.Exists(photo.FullPath))
                {
                    // для примера сохраняем файл в локальном хранилище
                    var ext=Path.GetExtension(photo.FileName);
                    var newFile = Path.Combine(dir, FileName+ext);
                    using (var stream = await photo.OpenReadAsync())
                    using (var newStream = File.OpenWrite(newFile))
                        await stream.CopyToAsync(newStream);
                    vQuestion.QuantityPhoto++;
                    db.ReplaceRaiting(vQuestion);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Сообщение об ошибке", ex.Message, "OK");
            }
        }

        private void Editor_Completed(object sender, EventArgs e)
        {
            db.ReplaceRaiting(GetRaiting(sender));
        }

        private void OnHeadTapped(object sender, EventArgs e)
        {
            var s = sender as Grid;
            var cc = s.Parent as StackLayout;

            var vRait = cc.BindingContext as Raiting;
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
                foreach (var el in Questions)
                    el.IsVisible = false;

            OnPropertyChanged("TextAllOpen");
        }

        private Raiting GetRaiting(object sender)
        {
            Xamarin.Forms.View V = (Xamarin.Forms.View)sender;
            return V.BindingContext as Raiting;
        }
    }
}
    