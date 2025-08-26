using BRB5.Model;
using Utils;
using BRB6.View;
using BRB5;

namespace BRB6.Template;

public partial class QuestionItemTemplate : ContentView, IViewRDI
{
    BL.BL Bl = BL.BL.GetBL();
    public Action<object , EventArgs> OnButtonClick { get; set; }
    public BRB5.Model.RaitingDocItem Data {  get; set; }
    public QuestionItemTemplate(BRB5.Model.RaitingDocItem pQuestion, Action<object, EventArgs> pOnButtonClick=null)
	{
		InitializeComponent();
        Data = pQuestion;
        BindingContext = Data;
        OnButtonClick = pOnButtonClick;
    }
    public QuestionItemTemplate()
    {
        InitializeComponent();
    }

    private void OnButtonClicked(object sender, EventArgs e)
    {
        if (BindingContext is BRB5.Model.RaitingDocItem item)
        {
            var handler = GetParentPage() as IRatingButtonHandler;
            handler?.OnRatingButtonClicked(sender, item);
        }
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
        var FileName = $"{vQuestion.NumberDoc}_{vQuestion.Id}_{DateTime.Now.ToString("yyyyMMdd_HHmmssfff")}";

        try
        {
            var dir = Path.Combine(Config.PathFiles, vQuestion.NumberDoc);
            double Size = FileAndDir.GetFreeSpace(dir);
            if (Size < 10d * 1024d * 1024d)
            {
                var parentPage = this.GetParentPage();
                if (parentPage != null)
                    await parentPage.DisplayAlert($"Недостатньо місця", $"Залишок=> {Size / (1024d * 1024):n3} Mb", "OK");
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
            var parentPage = this.GetParentPage();
            if (parentPage != null)
                await parentPage.DisplayAlert("Помилка!", ex.Message, "OK");
        }
    }
    private void Editor_Completed(object sender, EventArgs e) => Bl.db.ReplaceRaitingDocItem(GetRaiting(sender));
    private BRB5.Model.RaitingDocItem GetRaiting(object sender)
    {
        Microsoft.Maui.Controls.View V = (Microsoft.Maui.Controls.View)sender;
        return (BRB5.Model.RaitingDocItem)V.BindingContext;
    }

    private async void OnQuestionTapped(object sender, TappedEventArgs e)
    {
        if (BindingContext is BRB5.Model.RaitingDocItem item)
        {
            Data = BindingContext as BRB5.Model.RaitingDocItem;
            if (Data?.Explanation != null)
            {
                var parentPage = this.GetParentPage();
                if (parentPage != null)
                    await parentPage.DisplayAlert("Explanation", Data.Explanation, "OK");
            }
        }
    }
    private Page GetParentPage()
    {
        Element parent = this;
        while (parent != null)
        {
            if (parent is Page page)
            {
                return page;
            }
            parent = parent.Parent;
        }
        return null;
    }
}