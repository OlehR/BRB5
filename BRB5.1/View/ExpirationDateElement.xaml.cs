using BL;
using BRB5.Model;
using BRB5.Model.DB;


namespace BRB6.View;

public partial class ExpirationDateElement : BaseContentPage
{

    DB db = DB.GetDB();
    public ExpirationDateElementVM DM { get; set; } = new();
    public ExpirationDateElement(ExpirationDateElementVM pED)
	{
        InitializeComponent();
        NokeyBoard();
        DM =pED;

        DM.QuantityInput = DM.Quantity;

        if(DM.ExpirationDate == DateTime.MinValue) DM.ExpirationDate = DateTime.Today;

        DM.ExpirationDateInput = DM.ExpirationDate;
        DM.ProductionDateInput = DM.ExpirationDate.AddDays(-(double)DM.Expiration);

        var backgroundColorBinding = new Binding
        {
            Path = "GetPercentColor.Color",
            Converter = new ColorConverter()
        };
        StackBackground.SetBinding(Label.BackgroundColorProperty, backgroundColorBinding);

        this.BindingContext = DM;
    }

    void BarCode(string pBarCode) => CheckDiscount(pBarCode);

    void CheckDiscount(string pBarCode)
    {

    }

    private void ExpirationDateSelected(object sender, DateChangedEventArgs e)
    {
        DM.ProductionDateInput = DM.ExpirationDateInput.AddDays(-(double)DM.Expiration);
        OnPropertyChanged(nameof(DM.GetPercentColor));
    }

    private void ProductionDateSelected(object sender, DateChangedEventArgs e)
    {
        DM.ExpirationDateInput = DM.ProductionDateInput.AddDays((double)DM.Expiration);
        OnPropertyChanged(nameof(DM.GetPercentColor));
    }

    private void OnAdd(object sender, EventArgs e)
    {
        db.ReplaceDocWaresExpiration(null);
    }

    private void OnNotFound(object sender, EventArgs e)
    {
        DM.QuantityInput = 0;
        db.ReplaceDocWaresExpiration(DM.GetDocWaresExpiration());
    }
}