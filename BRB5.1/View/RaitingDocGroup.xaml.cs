using BL;
using BRB5;
using BRB6.Template;

namespace BRB6.View;

public partial class RaitingDocGroup : ContentPage
{
    IEnumerable<BRB5.Model.RaitingDocItem> Questions;
    List<IViewRDI> AllViewRDI;
    BRB5.Model.RaitingDocItem Header;
    BL.BL Bl = BL.BL.GetBL();
    public RaitingDocGroup(BRB5.Model.RaitingDocItem header, List<BRB5.Model.RaitingDocItem> pQuestions)
    {
		InitializeComponent();
        Questions = pQuestions;
        Header = header;
        BildViewRDI();
        var navigationBarHeight = 70;
        QuestionsGrid.HeightRequest = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density - navigationBarHeight;
    }
    void BildViewRDI()
    {
        AllViewRDI = [];
        var head = new QuestionHeadTemplate(Header, OnButtonClicked, null);
        MainThread.BeginInvokeOnMainThread(() => { QuestionsStackLayout.Children.Add(head); });
        AllViewRDI.Add(head);

        foreach (var el in Questions)
        {
            IViewRDI e = new QuestionItemTemplate(el, OnButtonClicked);
            MainThread.BeginInvokeOnMainThread(() => { QuestionsStackLayout.Children.Add(e); });

            AllViewRDI.Add(e);
        }

    }
    private void OnButtonClicked(object sender, EventArgs e)
    {
        Microsoft.Maui.Controls.View button = (Microsoft.Maui.Controls.View)sender;
        var vQuestion = GetRaiting(sender);
        Bl.ChangeRaiting(vQuestion, button.ClassId, Questions);
        Bl.CalcSumValueRating(vQuestion, Questions);
    }
    private BRB5.Model.RaitingDocItem GetRaiting(object sender)
    {
        Microsoft.Maui.Controls.View V = (Microsoft.Maui.Controls.View)sender;
        return (BRB5.Model.RaitingDocItem)V.BindingContext;
    }
}