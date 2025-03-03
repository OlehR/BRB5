namespace BRB6.Template;

public partial class QuestionHeadTemplate : ContentView
{
    BRB5.Model.RaitingDocItem QuestionHead { get; set; }
    public QuestionHeadTemplate(BRB5.Model.RaitingDocItem pQuestion)
	{
		InitializeComponent();
        QuestionHead = pQuestion;
        BindingContext = QuestionHead;
    }

    private void OnHeadTapped(object sender, TappedEventArgs e)
    {

    }

    private void OnButtonClicked(object sender, EventArgs e)
    {

    }
}