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
        if (BindingContext is BRB5.Model.RaitingDocItem head)
        {
            // Знайти батьківську сторінку, яка реалізує інтерфейс
            var handler = GetParentPage() as IHeadTapHandler;
            handler?.OnHeadTapped(head);
        }
    }

    private Page GetParentPage()
    {
        Element parent = this;
        while (parent != null)
        {
            if (parent is Page page)
                return page;
            parent = parent.Parent;
        }
        return null;
    }

    private void OnButtonClicked(object sender, EventArgs e)
    {
        if (BindingContext is BRB5.Model.RaitingDocItem item)
        {
            var handler = GetParentPage() as IRatingButtonHandler;
            handler?.OnRatingButtonClicked(sender, item);
        }
    }

}