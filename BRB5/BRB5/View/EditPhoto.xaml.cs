using BRB5.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BRB5.View
{
    public class Pictures
    {
        public Pictures(string pFileName, bool pIsNotSend=true)
        {
            FileName = pFileName;
            IsNotSend = pIsNotSend;
        }
        public string FileName { get; set; }
        public Image image { get; set; }
        public bool IsNotSend { get; set; }
}

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditPhoto : ContentPage
    {
        DB db = DB.GetDB();
        public ObservableCollection<Pictures> MyFiles { get; set; }
        string Mask;
        Raiting Raiting;
        string dir;
        public EditPhoto(Raiting pRaiting)
        {
            dir = Path.Combine(Config.PathFiles, pRaiting.NumberDoc);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                // Directory.CreateDirectory(Path.Combine(dir, "Send"));
            }

            Raiting = pRaiting;
            Mask = $"{pRaiting.Id}_*.*";
            InitializeComponent();
            var d = Directory.GetFiles(dir, Mask);
            IEnumerable<Pictures> r = d.Select(e => new Pictures(e)).OrderByDescending(el => el.FileName);
            
            MyFiles = new ObservableCollection<Pictures>(r);


            var arx = Path.Combine(Config.PathFiles, "arx", pRaiting.NumberDoc);
            if (Directory.Exists(dir))
            {
                d = Directory.GetFiles(dir, Mask);
                 r = d.Select(e => new Pictures(e,false)).OrderByDescending(el => el.FileName);
                foreach (var el in r)
                    MyFiles.Add(el);
            }

            BindingContext = this;
        }
        private async void OnButtonAdd(object sender, System.EventArgs e)
        {
            Button b = sender as Button;
            if (b == null)
                return;
            
                var photo =  b.Text.Equals("Додати відео") ? await MediaPicker.PickVideoAsync(): await MediaPicker.PickPhotoAsync();
            //await MediaPicker.PickVideoAsync();
            if (photo != null && File.Exists(photo.FullPath))
            {
                // для примера сохраняем файл в локальном хранилище
                var ext = Path.GetExtension(photo.FileName);
                var FileName = $"{Raiting.Id}_{DateTime.Now:yyyyMMdd_hhmmssfff}";
                var newFile = Path.Combine(dir, FileName + ext);
                using (var stream = await photo.OpenReadAsync())
                using (var newStream = File.OpenWrite(newFile))
                    await stream.CopyToAsync(newStream);
                Raiting.QuantityPhoto++;
                db.ReplaceRaiting(Raiting);
                MyFiles.Insert(0, new Pictures(newFile));
            }
        }

        async  private void OnButtonDel(object sender, System.EventArgs e)
        {
             Xamarin.Forms.View V = (Xamarin.Forms.View)sender;
            var el= V.BindingContext as Pictures;

            bool res= await DisplayAlert("Ви хочете видалити файл", el.FileName, "OK", "Cancel");
            if (res)
            {
                File.Delete(el.FileName);
                MyFiles.Remove(el);
                Raiting.QuantityPhoto--;
                db.ReplaceRaiting(Raiting);
            }
        }

        private void OnButtonClicked(object sender, System.EventArgs e)
        {
        }
    }
}