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
        public Pictures(string pFileName)
        {
            FileName = pFileName;
        }
        public string FileName { get; set; }
        public Image image { get; set; }
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditPhoto : ContentPage
    {
        DB db = DB.GetDB();
        public ObservableCollection<Pictures> MyFiles { get; set; }
        string Mask;
        Raiting Raiting;
        string NumberDoc;
        public EditPhoto(string pNumberDoc,Raiting pRaiting)
        {
            NumberDoc = pNumberDoc;
            Raiting = pRaiting;
            Mask = $"{pNumberDoc}_{pRaiting.Id}_*.*";
            InitializeComponent();
            var d = Directory.GetFiles(FileSystem.AppDataDirectory, Mask);
            IEnumerable<Pictures> r = d.Select(e => new Pictures(e));
            MyFiles = new ObservableCollection<Pictures>(r);
            BindingContext = this;
        }
        private async void OnButtonAdd(object sender, System.EventArgs e)
        {
            var photo = await MediaPicker.PickPhotoAsync();
            if (photo != null && File.Exists(photo.FullPath))
            {
                // для примера сохраняем файл в локальном хранилище
                var ext = Path.GetExtension(photo.FileName);
                var FileName = $"{NumberDoc}_{Raiting.Id}_{DateTime.Now.ToString("yyyyMMdd_hhmmss")}";
                var newFile = Path.Combine(FileSystem.AppDataDirectory, FileName + ext);
                using (var stream = await photo.OpenReadAsync())
                using (var newStream = File.OpenWrite(newFile))
                    await stream.CopyToAsync(newStream);
                Raiting.QuantityPhoto++;
                db.ReplaceRaiting(Raiting);
                MyFiles.Add(new Pictures(newFile));
            }
        }
        private void OnButtonDel(object sender, System.EventArgs e)
        {
            Xamarin.Forms.View V = (Xamarin.Forms.View)sender;
            var el= V.BindingContext as Pictures;
            File.Delete(el.FileName);
            MyFiles.Remove(el);
            Raiting.QuantityPhoto--;
        }
        private void OnButtonClicked(object sender, System.EventArgs e)
        {
        }
    }
}