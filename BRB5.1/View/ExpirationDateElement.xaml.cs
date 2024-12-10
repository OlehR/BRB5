using BRB6.ViewModel;


namespace BRB6.View;

public partial class ExpirationDateElement : ContentPage
{
	
    public ExpirationDateElementVM DM { get; set; } = new();
    public ExpirationDateElement()
	{        
		InitializeComponent();
		this.BindingContext = DM;
    }
    void BarCode(string pBarCode) => CheckDiscount(pBarCode);

    void CheckDiscount(string pBarCode)
    {

    }
}