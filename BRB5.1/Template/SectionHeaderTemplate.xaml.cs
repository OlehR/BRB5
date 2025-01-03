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

public partial class SectionHeaderTemplate : ContentView
{
    BL.BL Bl = BL.BL.GetBL();
    public SectionHeaderTemplate()
	{
		InitializeComponent();
	}

    private void OnButtonClicked(object sender, EventArgs e)
    {
        //Microsoft.Maui.Controls.View button = (Microsoft.Maui.Controls.View)sender;
        //var vQuestion = GetRaiting(sender);
        //Bl.ChangeRaiting(vQuestion, button.ClassId, All);

        //if (vQuestion.IsHead) ChangeItemBlok(vQuestion);

        //Bl.CalcSumValueRating(vQuestion, All);
        //RefreshHead();
    }
    void RefreshHead()
    {
        try
        {
            //CountChoice = All.Count(el => !el.IsHead && el.Rating > 0);
            //OnPropertyChanged(nameof(QuantityAllChoice));
            //OnPropertyChanged(nameof(IsSave));
        }
        catch (Exception ex)
        {
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
        }
    }
    private BRB5.Model.RaitingDocItem GetRaiting(object sender)
    {
        Microsoft.Maui.Controls.View V = (Microsoft.Maui.Controls.View)sender;
        return (BRB5.Model.RaitingDocItem)V.BindingContext;
    }

    private void OnHeadTapped(object sender, TappedEventArgs e)
    {

    }


}