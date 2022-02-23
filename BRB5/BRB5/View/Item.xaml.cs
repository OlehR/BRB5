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
        DocId cDocId;
        public ObservableCollection<Raiting> Questions { get; set; }

        int CountAll,CountChoice;
        public bool  IsSave { get { return CountAll == CountChoice; } }
        bool IsAll = true;
        public string TextAllNoChoice { get {return IsAll?"Без відповіді":"Всі"; }  }
        public string QuantityAllChoice { get { return $"{CountChoice}/{CountAll}"; } }

        /*public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }*/
        public Item(DocId pDocId)
        {
            cDocId = pDocId;
            
            InitializeComponent();

             var Q = db.GetRating(cDocId);

            //Ховаємо непотрібні питання
            foreach (var e in Q.Where(d => d.IsHead && d.Rating==4) )
            foreach (var el in Q.Where(d => d.Parent == e.Id))
            {
                el.IsVisible = e.Rating != 4;
            }
            CountAll = Q.Count(el => !el.IsHead);

            Questions = new ObservableCollection<Raiting>(Q);
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
                        el.Rating = 4;
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
            var r=db.GetRating(cDocId);
            var c = Connector.Connector.GetInstance();
            c.SendRaiting(r);
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
                var dir = Path.Combine(Config.GetPathFiles, vQuestion.NumberDoc);
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

        private Raiting GetRaiting(object sender)
        {
            Xamarin.Forms.View V = (Xamarin.Forms.View)sender;
            return V.BindingContext as Raiting;
        }
    }
}
    