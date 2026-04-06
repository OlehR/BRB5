namespace BRB6.Template;

public partial class MRDialog : ContentView
{
    public static readonly BindableProperty DialogTitleProperty =
        BindableProperty.Create(nameof(DialogTitle), typeof(string), typeof(MRDialog), "Вкажіть актуальну кількість товару");

    public string DialogTitle
    {
        get => (string)GetValue(DialogTitleProperty);
        set => SetValue(DialogTitleProperty, value);
    }
    public MRDialog()
	{
		InitializeComponent();
	}
    public void Show() => IsVisible = true;
    public void Hide() => IsVisible = false;
}