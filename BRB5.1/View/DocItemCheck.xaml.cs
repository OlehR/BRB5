using BRB5;
using BRB6.ViewModel;

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
}