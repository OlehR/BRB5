using BRB5;
using BRB6.ViewModel;
using BRB5.Model;

namespace BRB6.View;

public partial class DocItemCheck : ContentPage
{
    private readonly DocItemCheckVM _viewModel;
    public DocItemCheck()
	{
		InitializeComponent();
        _viewModel = new DocItemCheckVM();
        BindingContext = _viewModel;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (Parent is NavigationPage navPage)
        {
            navPage.BarBackgroundColor = Colors.White;
            navPage.BarTextColor = Color.FromArgb("#1A1A1A");
        }
        Config.BarCode = BacCode;
    }

    public void BacCode(string pBarCode)=> _viewModel.BarCode(pBarCode);
    
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (Parent is NavigationPage navPage)
        {
            navPage.BarBackgroundColor = Colors.Transparent; // або ваш стандартний колір
            navPage.BarTextColor = Colors.White; // або ваш стандартний колір
        }
        Config.BarCode -= BacCode;
    }
}