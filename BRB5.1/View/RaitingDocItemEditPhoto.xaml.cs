using BL;
using BRB5.Model;
//using NativeMedia;
using System.Collections.ObjectModel;
using File = System.IO.File;
using System.Threading;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Media;

namespace BRB51.View
{
    public class Pictures
    {
        const string VideoExt =".WEBM ·.MPG, .MP2, .MPEG, .MPE, .MPV ·.OGG ·.MP4, .M4P, .M4V ·.AVI ·.WMV ·.MOV, .QT ·.FLV, .SWF";
        public Pictures() { }
        public Pictures(string pFileName, bool pIsNotSend=true)
        {
            FileName = pFileName;
            IsNotSend = pIsNotSend;
        }
        public string FileName { get; set; }
        public Image image { get; set; }
        public bool IsNotSend { get; set; }

        public bool IsPhoto { get { return !VideoExt.Contains(Path.GetExtension(FileName).ToUpper()); } }
        public bool IsVideo { get { return VideoExt.Contains(Path.GetExtension(FileName).ToUpper()); } }

    }

    public partial class RaitingDocItemEditPhoto : ContentPage
    {
        DB db = DB.GetDB();
        public ObservableCollection<Pictures> MyFiles { get; set; }
        string Mask;
        BRB5.Model.RaitingDocItem Raiting;
        string dir;
        public RaitingDocItemEditPhoto(BRB5.Model.RaitingDocItem pRaiting)
        {
            dir = Path.Combine(Config.PathFiles, pRaiting.NumberDoc);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                // Directory.CreateDirectory(Path.Combine(dir, "Send"));
            }
            // TODO Xamarin.Forms.Device.RuntimePlatform is no longer supported. Use Microsoft.Maui.Devices.DeviceInfo.Platform instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
            NavigationPage.SetHasNavigationBar(this, Device.RuntimePlatform == Device.iOS);
            Raiting = pRaiting;
            Mask = $"{pRaiting.Id}_*.*";
            InitializeComponent();
            var d = Directory.GetFiles(dir, Mask);
            IEnumerable<Pictures> r = d.Select(e => new Pictures(e)).OrderByDescending(el => el.FileName);
            
            MyFiles = new ObservableCollection<Pictures>(r);


            var arx = Path.Combine(Config.PathDownloads, "arx", pRaiting.NumberDoc);
            if (Directory.Exists(arx))
            {
                d = Directory.GetFiles(arx, Mask);
                 r = d.Select(e => new Pictures(e,false)).OrderByDescending(el => el.FileName);
                foreach (var el in r)
                    MyFiles.Add(el);
            }

            BindingContext = this;
        }
        //private async void OnButtonAdd(object sender, System.EventArgs e)
        //{
        //    Button b = sender as Button;
        //    if (b == null)
        //        return;
        //    bool IsVideo = b.Text.Equals("Додати відео");
        //    var photo = IsVideo ? await MediaPicker.PickVideoAsync() : await MediaPicker.PickPhotoAsync();
            

        //    //await MediaPicker.PickVideoAsync();
        //    if (photo != null && File.Exists(photo.FullPath))
        //    {
        //        // для примера сохраняем файл в локальном хранилище
        //        var ext = Path.GetExtension(photo.FileName);
        //        var FileName = $"{Raiting.Id}_{DateTime.Now:yyyyMMdd_hhmmssfff}";
        //        var newFile = Path.Combine(dir, FileName + ext);
        //        using (var stream = await photo.OpenReadAsync())
        //        using (var newStream = File.OpenWrite(newFile))
        //            await stream.CopyToAsync(newStream);
        //        Raiting.QuantityPhoto++;
        //        db.ReplaceRaiting(Raiting);
        //        MyFiles.Insert(0, new Pictures(newFile));
        //    }
        //}

        async  private void OnButtonDel(object sender, System.EventArgs e)
        {
             Microsoft.Maui.Controls.View V = (Microsoft.Maui.Controls.View)sender;
            var el= V.BindingContext as Pictures;

            bool res= await DisplayAlert("Ви хочете видалити файл", el.FileName, "OK", "Cancel");
            if (res)
            {
                File.Delete(el.FileName);
                MyFiles.Remove(el);
                Raiting.QuantityPhoto--;
                if(Raiting.QuantityPhoto < 0) Raiting.QuantityPhoto = 0;
                db.ReplaceRaitingDocItem(Raiting);
            }
        }

        private async void OnPhotosAdd(object sender, EventArgs e)
        {
            var cts = new CancellationTokenSource();
            List<FileResult> files = null;

            try
            {
                // Cancel the operation after 5 minutes
                cts.CancelAfter(TimeSpan.FromMinutes(5));

                // Picking images and videos (one at a time in MAUI)
                var pickPhotoTask = MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Select an Image"
                });

                var pickVideoTask = MediaPicker.PickVideoAsync(new MediaPickerOptions
                {
                    Title = "Select a Video"
                });

                // Await both tasks
                await Task.WhenAll(pickPhotoTask, pickVideoTask);

                // Collect the results
                files = new List<FileResult> { await pickPhotoTask, await pickVideoTask };
            }
            catch (OperationCanceledException)   {  /* handling a cancellation request  */  }
            catch (Exception)   {   /* handling other exceptions*/     }
            finally  {  cts.Dispose();  }

            if (files == null)  return;            

            foreach (var file in files)
            {
                var ext = Path.GetExtension(file.FullPath);
                var FileName = $"{Raiting.Id}_{DateTime.Now:yyyyMMdd_HHmmssfff}";
                var newFile = Path.Combine(dir, FileName + ext);

                using (var stream = await file.OpenReadAsync())
                using (var newStream = File.OpenWrite(newFile))
                    await stream.CopyToAsync(newStream);

                Raiting.QuantityPhoto++;
                db.ReplaceRaitingDocItem(Raiting);
                MyFiles.Insert(0, new Pictures(newFile));
            }
        }
    }
}