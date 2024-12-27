using BRB6.View;

namespace BRB6.Template;

public partial class WareItemTemplate : ContentView
{
    public WareItemTemplate()
    {
        InitializeComponent();
    }

    public void BindData(object bindingContext)
    {
        BindingContext = bindingContext;

        // Прив’язка даних до Label
        NameWaresLabel.SetBinding(Label.TextProperty, new Binding("NameWares"));
        CodeWaresLabel.SetBinding(Label.TextProperty, new Binding("CodeWares"));
        QuantityInputLabel.SetBinding(Label.TextProperty, new Binding("QuantityInput"));

        // Прив’язка кольору (новий Binding для кожної властивості)
        NameWaresLabel.SetBinding(Label.BackgroundColorProperty, new Binding("GetPercentColor.Color", converter: new ColorConverter()));
        CodeWaresLabel.SetBinding(Label.BackgroundColorProperty, new Binding("GetPercentColor.Color", converter: new ColorConverter()));
        QuantityInputLabel.SetBinding(Label.BackgroundColorProperty, new Binding("GetPercentColor.Color", converter: new ColorConverter()));
    }
}

