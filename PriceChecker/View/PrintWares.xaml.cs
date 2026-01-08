using BL;
using BRB5;
using BRB5.Model;
using System.Collections.ObjectModel;

namespace PriceChecker.View;

public partial class PrintWares : ContentPage
{
    protected DB db = DB.GetDB();
    BL.BL bl = BL.BL.GetBL();

    public bool IsOnlyRest { get; set; }
    public ObservableCollection<DocWaresEx> Wares { get; set; }
    public PrintWares(DocVM pDocId, TypeDoc pTypeDoc)
	{
		InitializeComponent();
        Wares = new ObservableCollection<DocWaresEx>(db.GetDocWares(pDocId, 1, eTypeOrder.Scan));
        BindingContext=this;
	}
    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnPrintClicked(object sender, EventArgs e)
    {
        if (Wares == null || Wares.Count == 0)
            return;

        IEnumerable<long> codesToPrint = Wares
            .Where(w => w.Even && w.QuantityOrder > 0)
            .SelectMany(w =>
                Enumerable.Repeat(w.CodeWares, (int)w.QuantityOrder)
            );
        if (!codesToPrint.Any())
        {
            await DisplayAlert("Print", "No items selected", "OK");
            return;
        }
        string result = bl.c.PrintHTTP(codesToPrint,IsOnlyRest); 

        await DisplayAlert("Print result", result, "OK");

        await Navigation.PopAsync();

    }

    private void OnPlusClicked(object sender, EventArgs e)
    {
        if (sender is Button button &&
        button.BindingContext is DocWaresEx ware)
        {
            ware.QuantityOrder++;
            ware.Even = true;
        }
    }

    private void OnMinusClicked(object sender, EventArgs e)
    {
        if (sender is Button button &&
         button.BindingContext is DocWaresEx ware)
        {
            if (ware.QuantityOrder > 0)
            {
                ware.QuantityOrder--;

                if (ware.QuantityOrder == 0)
                    ware.Even = false;
            }
        }
    }
}