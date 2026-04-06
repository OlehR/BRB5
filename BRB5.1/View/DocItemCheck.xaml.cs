using BRB5;
using BRB6.ViewModel;
using BRB5.Model;

namespace BRB6.View;

public partial class DocItemCheck : ForMVVM
{
    private readonly DocItemCheckVM _viewModel;
    public DocItemCheck()
	{
		InitializeComponent();
        _viewModel = new DocItemCheckVM(this);
        BindingContext = _viewModel;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        //if (Parent is NavigationPage navPage)
        //{
        //    navPage.BarBackgroundColor = Colors.White;
        //    navPage.BarTextColor = Color.FromArgb("#1A1A1A");
        //}
        Config.BarCode = BacCode;
        _viewModel.ScrollToItem += OnScrollToItem;
    }

    public void BacCode(string pBarCode)=> _viewModel.BarCode(pBarCode);
    
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        //if (Parent is NavigationPage navPage)
        //{
        //    navPage.BarBackgroundColor = Colors.Transparent; 
        //    navPage.BarTextColor = Color.FromArgb("#2196F3");
        //}
        Config.BarCode -= BacCode;
        _viewModel.ScrollToItem -= OnScrollToItem;
    }
    private void OnScrollToItem(DocWaresEx item)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            WaresCollection.ScrollTo(item, animate: true);
        });
    }

    void ForMVVM.Focused(string pName)
    {
       // throw new NotImplementedException();
    }

    void ForMVVM.DisplayAlert(string title, string message, string cancel)
    {
        Dispatcher.Dispatch(() => DisplayAlert(title, message, cancel));
    }
}