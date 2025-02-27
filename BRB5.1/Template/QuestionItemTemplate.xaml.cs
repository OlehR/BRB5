using BL;
using BRB5.Model;
using Utils;
using BRB6.View;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Microsoft.Maui.Controls.Compatibility;
using BRB5;
using Grid = Microsoft.Maui.Controls.Grid;
using StackLayout = Microsoft.Maui.Controls.StackLayout;
using BarcodeScanning;


namespace BRB6.Template;

public partial class QuestionItemTemplate : ContentView
{
    BL.BL Bl = BL.BL.GetBL();
    BRB5.Model.RaitingDocItem Question {  get; set; }
    public QuestionItemTemplate(BRB5.Model.RaitingDocItem pQuestion)
	{
		InitializeComponent();
        Question = pQuestion;
        BindingContext = Question;
    }

    private void OnButtonClicked(object sender, EventArgs e)
    {

    }
    private void EditPhoto(object sender, System.EventArgs e)
    {
        var vQuestion = GetRaiting(sender);
        //IsRefreshList = false;
        Navigation.PushAsync(new RaitingDocItemEditPhoto(vQuestion));
    }
    async void TakePhotoAsync(object sender, EventArgs e)
    {
        ImageButton button = (ImageButton)sender;
        var vQuestion = button.BindingContext as BRB5.Model.RaitingDocItem;
        var FileName = $"{vQuestion.Id}_{DateTime.Now.ToString("yyyyMMdd_HHmmssfff")}";

        try
        {
            var dir = Path.Combine(Config.PathFiles, vQuestion.NumberDoc);
            double Size = FileAndDir.GetFreeSpace(dir);
            if (Size < 10d * 1024d * 1024d)
            {
                //await DisplayAlert($"Недостатньо місця", $"Залишок=> {Size / (1024d * 1024):n3} Mb", "OK");
                return;
            }
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                Directory.CreateDirectory(Path.Combine(dir, "Send"));
            }
            //IsRefreshList = false;
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
            //await DisplayAlert("Помилка!", ex.Message, "OK");
        }
    }
    private void Editor_Completed(object sender, EventArgs e) => Bl.db.ReplaceRaitingDocItem(GetRaiting(sender));
    private BRB5.Model.RaitingDocItem GetRaiting(object sender)
    {
        Microsoft.Maui.Controls.View V = (Microsoft.Maui.Controls.View)sender;
        return (BRB5.Model.RaitingDocItem)V.BindingContext;
    }
}