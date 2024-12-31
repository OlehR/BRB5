using BL;
using BRB5.Model;
using BRB6.View;

namespace BRB6.Template;

public partial class ExpirationDateElementTemplate : ContentView
{
    DB db = DB.GetDB();

    public event Action RequestReturnToMainContent;
    public ExpirationDateElementVM DM { get; set; } = new();
    private ExpirationDateElementVM _DM;
    public ExpirationDateElementTemplate()
    {
        InitializeComponent();       

        var backgroundColorBinding = new Binding
        {
            Path = "DM.GetPercentColor.Color",
            Converter = new ColorConverter()
        };
        StackBackground.SetBinding(Label.BackgroundColorProperty, backgroundColorBinding);

        this.BindingContext = this;
    }

    public void Set(ExpirationDateElementVM pED)
    {
        _DM = pED;

        DM = (ExpirationDateElementVM)pED.Clone();
        DM.QuantityInput = DM.Quantity;

        if (DM.ExpirationDate == DateTime.MinValue) DM.ExpirationDate = DateTime.Today;

        DM.ExpirationDateInput = DM.ExpirationDate;
        DM.ProductionDateInput = DM.ExpirationDate.AddDays(-(double)DM.Expiration);
        OnPropertyChanged(nameof(DM));
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
        db.ReplaceDocWaresExpiration(DM.GetDocWaresExpiration());

        _DM.ExpirationDateInput = DM.ExpirationDateInput;
        _DM.QuantityInput = DM.QuantityInput;

        RequestReturnToMainContent?.Invoke();
    }

    private void OnNotFound(object sender, EventArgs e)
    {
        DM.QuantityInput = 0;
        db.ReplaceDocWaresExpiration(DM.GetDocWaresExpiration());

        _DM.ExpirationDateInput = DM.ExpirationDateInput;
        _DM.QuantityInput = DM.QuantityInput;

        RequestReturnToMainContent?.Invoke();
    }

    private void Back(object sender, EventArgs e)
    {
        RequestReturnToMainContent?.Invoke();
    }
}