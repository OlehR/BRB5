using BRB5;
using BRB5.Model;
using BRB6.View;
using Utils;

namespace BRB6.Template;

/// <summary>
/// Шаблон питання для чекліста з часом виконання.
/// </summary>
public partial class QuestionChecklistTemplate : ContentView, IViewRDI
{
    BL.BL Bl = BL.BL.GetBL();
    public Action<object, EventArgs> OnButtonClick { get; set; }
    public BRB5.Model.RaitingDocItem Data { get; set; }

    public QuestionChecklistTemplate(BRB5.Model.RaitingDocItem pQuestion, Action<object, EventArgs> pOnButtonClick = null)
    {
        InitializeComponent();
        Data = pQuestion;
        BindingContext = Data;
        OnButtonClick = pOnButtonClick;
    }
    public QuestionChecklistTemplate()
    {
        InitializeComponent();
    }

    /// <summary>Чекбокс: ставимо/знімаємо виконання, фіксуємо час і зберігаємо в БД.</summary>
    private void OnDoneChanged(object sender, CheckedChangedEventArgs e)
    {
        if (BindingContext is not BRB5.Model.RaitingDocItem item) return;
        if (item.IsDone == e.Value) return;   // подія від початкового біндингу – ігноруємо

        item.IsDone = e.Value;                // Rating = 1/0, DTInsert = зараз/порожньо
        Bl.db.ReplaceRaitingDocItem(item);
        (GetParentPage() as IChecklistHandler)?.OnChecklistChanged(item);
    }

    private void EditPhoto(object sender, EventArgs e)
    {
        var vQuestion = GetRaiting(sender);
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
            var photo = await MediaPicker.CapturePhotoAsync(new MediaPickerOptions { Title = FileName });

            await Task.Delay(10);

            if (photo != null)
            {
                var ext = Path.GetExtension(photo.FileName);
                var newFile = Path.Combine(dir, FileName + ext);
                byte[] imageData;
                using (var stream = await photo.OpenReadAsync())
                {
                    imageData = NativeBase.ReadFully(stream);
                    byte[] resizedImage = Config.NativeBase.ResizeImage(imageData, Config.PhotoQuality.GetValue(), Config.Compress);
                    File.WriteAllBytes(newFile, resizedImage);
                }
                vQuestion.QuantityPhoto++;
                Bl.db.ReplaceRaitingDocItem(vQuestion);
            }
        }
        catch (Exception ex)
        {
            FileLogger.WriteLogMessage($"ChecklistItem.TakePhotoAsync", eTypeLog.Error);
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
        if (BindingContext is BRB5.Model.RaitingDocItem item && item.Explanation != null)
        {
            var parentPage = this.GetParentPage();
            if (parentPage != null)
                await parentPage.DisplayAlert("Критерій оцінки", item.Explanation, "OK");
        }
    }

    private Page GetParentPage()
    {
        Element parent = this;
        while (parent != null)
        {
            if (parent is Page page) return page;
            parent = parent.Parent;
        }
        return null;
    }
}
