namespace PriceChecker.View;

public partial class PrintPage : ContentPage
{
	public PrintPage()
	{
		InitializeComponent();
	}
    private void OnSwipedRight(object sender, SwipedEventArgs e)
    {
        if (e.Direction == SwipeDirection.Right)
        {
            Shell.Current.FlyoutIsPresented = true;
        }
    }

}