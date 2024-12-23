using BRB5.Model;


namespace BRB6.View;

public partial class ExpirationDateElement : ContentPage
{
	
    public ExpirationDateElementVM DM { get; set; } = new();
    public ExpirationDateElement(ExpirationDateElementVM pED)
	{
        DM=pED;

        InitializeComponent();
		this.BindingContext = DM;
    }
    void BarCode(string pBarCode) => CheckDiscount(pBarCode);

    void CheckDiscount(string pBarCode)
    {

    }
}