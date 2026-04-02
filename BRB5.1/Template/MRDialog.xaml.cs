namespace BRB6.Template;

public partial class MRDialog : ContentView
{
	public MRDialog()
	{
		InitializeComponent();
	}
    public void Show() => IsVisible = true;
    public void Hide() => IsVisible = false;
}