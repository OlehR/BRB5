namespace BRB6.Template;

public partial class QuestionHeadTemplate : ContentView, IViewRDI
{
    public BRB5.Model.RaitingDocItem Data { get; set; }
    public Action<object, EventArgs> OnButtonClick { get; set; }

    public Action<object, EventArgs> OnHeadTapp { get; set; }
    public QuestionHeadTemplate(BRB5.Model.RaitingDocItem pQuestion, Action<object, EventArgs> pOnButtonClick = null, Action<object, EventArgs> pOnHeadTapp=null)
    { 
        InitializeComponent();
        Data = pQuestion;
        BindingContext = Data;
        OnButtonClick = pOnButtonClick;
        OnHeadTapp = pOnHeadTapp;
    }
    public QuestionHeadTemplate()
    {
        InitializeComponent();
    }
    private void OnHeadTapped(object sender, TappedEventArgs e)
    {
        OnHeadTapp?.Invoke(sender, e);
    }

    private void OnButtonClicked(object sender, EventArgs e)
    {
        OnButtonClick?.Invoke(sender, e);
    }
}