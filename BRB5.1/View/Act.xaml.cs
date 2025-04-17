using BRB5;
using BRB5.Model;
using System.Collections.ObjectModel;
#if ANDROID
using Android.Views;
#endif

namespace BRB6.View;

public partial class Act 
{
    private readonly TypeDoc TypeDoc;
    private DocVM Doc;
    public ObservableCollection<DocWaresEx> MyDocWares { get; set; } = new ObservableCollection<DocWaresEx>();
    public Act(DocId pDocId, TypeDoc pTypeDoc)
    {
        NokeyBoard();
        TypeDoc = pTypeDoc;
        Doc = new DocVM(pDocId);
        BindingContext = this;
        InitializeComponent();
    }

    private void PopulateDocWaresStackLayout()
    {
        if (MyDocWares == null || !MyDocWares.Any())
            return;

        StackLayoutDocs.Children.Clear(); // Clear existing children
        StackLayoutDocs.Spacing = 0; // Remove vertical spacing between elements

        foreach (var docWare in MyDocWares)
        {
            // Create the main container StackLayout
            var mainStackLayout = new StackLayout
            {
                Spacing = 0, // Remove spacing between child elements
                Padding = new Thickness(0), // Remove padding
            };

            // Create the first Grid
            var grid = new Grid
            {
                RowSpacing = 1,
                ColumnSpacing = 1,
                Padding = 1,
                BackgroundColor = Color.FromArgb("#adaea7"),
                BindingContext = docWare
            };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });

            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

            // Add Labels to the Grid
            var nameLabel = new Label
            {
                Text = docWare.NameWares,
                BackgroundColor = Color.FromArgb(docWare.GetBackgroundColor) // Convert string to Color
            };
            Grid.SetColumnSpan(nameLabel, 4); // Set column span using Grid.SetColumnSpan
            grid.Children.Add(nameLabel);

            var codeLabel = new Label
            {
                Text = docWare.CodeWares.ToString(),
                BackgroundColor = Color.FromArgb(docWare.GetBackgroundColor)
            };
            Grid.SetRow(codeLabel, 1);
            Grid.SetColumn(codeLabel, 0);
            grid.Children.Add(codeLabel);

            var quantityOrderLabel = new Label
            {
                Text = docWare.QuantityOrder.ToString(),
                BackgroundColor = Color.FromArgb(docWare.GetBackgroundColor)
            };
            Grid.SetRow(quantityOrderLabel, 1);
            Grid.SetColumn(quantityOrderLabel, 1);
            grid.Children.Add(quantityOrderLabel);

            var inputQuantityLabel = new Label
            {
                Text = docWare.InputQuantity.ToString(),
                BackgroundColor = Color.FromArgb(docWare.GetBackgroundColor)
            };
            Grid.SetRow(inputQuantityLabel, 1);
            Grid.SetColumn(inputQuantityLabel, 2);
            grid.Children.Add(inputQuantityLabel);

            var quantityReasonLabel = new Label
            {
                Text = docWare.QuantityReason.ToString(),
                BackgroundColor = Color.FromArgb(docWare.GetBackgroundColor)
            };
            Grid.SetRow(quantityReasonLabel, 1);
            Grid.SetColumn(quantityReasonLabel, 3);
            grid.Children.Add(quantityReasonLabel);

            mainStackLayout.Children.Add(grid);

            // Add the main StackLayout to the parent StackLayout
            StackLayoutDocs.Children.Add(mainStackLayout);
        }
    }



    private void F1Create(object sender, TappedEventArgs e)
    {

    }


#if ANDROID
    public void OnPageKeyDown(Keycode keyCode, KeyEvent e)
    {
        switch (keyCode)
        {
            case Keycode.F2:
                //F2Save(null, EventArgs.Empty);
                return;

            default:
                return;
        }
    }
#endif
}