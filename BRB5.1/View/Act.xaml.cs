using BRB5;
using BRB5.Model;
using System.Collections.ObjectModel;
using BL;
using CommunityToolkit.Maui.Alerts;
using BL.Connector;



#if ANDROID
using Android.Views;
#endif

namespace BRB6.View;

public partial class Act 
{
    private readonly TypeDoc TypeDoc;
    private DocVM Doc;
    protected DB db = DB.GetDB();
    private Connector c = ConnectorBase.GetInstance();
    public bool IsSoftKeyboard { get { return Config.IsSoftKeyboard; } }
    public ObservableCollection<WaresAct> MyDocWares { get; set; } = new ObservableCollection<WaresAct>();
    public Act(DocId pDocId, TypeDoc pTypeDoc)
    {
        NokeyBoard();
        TypeDoc = pTypeDoc;
        Doc = new DocVM(pDocId);
        BindingContext = this;
        InitializeComponent();
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (!IsSoftKeyboard)
        {
#if ANDROID
            MainActivity.Key += OnPageKeyDown;
#endif
        }
        MyDocWares = new ObservableCollection<WaresAct>(db.GetWaresAct(Doc)); 
        if (MyDocWares != null)
        {
            PopulateStackLayoutDocs();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (!IsSoftKeyboard)
        {
#if ANDROID
            MainActivity.Key -= OnPageKeyDown;
#endif
        }
    }
    private void PopulateStackLayoutDocs()
    {
        if (MyDocWares == null || !MyDocWares.Any())
            return;

        StackLayoutDocs.Children.Clear(); 
        StackLayoutDocs.Spacing = 0; 

        foreach (var docWare in MyDocWares)
        {
            // Create the main container StackLayout
            var mainStackLayout = new StackLayout
            {
                Spacing = 0, 
                Padding = new Thickness(0), 
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
                BackgroundColor = Color.FromArgb("#ffffff"),
            };
            Grid.SetColumnSpan(nameLabel, 4); // Set column span using Grid.SetColumnSpan
            grid.Children.Add(nameLabel);

            var codeLabel = new Label
            {
                Text = docWare.CodeWares.ToString(),
                BackgroundColor = Color.FromArgb("#ffffff"),
            };
            Grid.SetRow(codeLabel, 1);
            Grid.SetColumn(codeLabel, 0);
            grid.Children.Add(codeLabel);

            var quantityOrderLabel = new Label
            {
                Text = docWare.Plan.ToString(),
                BackgroundColor = Color.FromArgb("#ffffff"),
            };
            Grid.SetRow(quantityOrderLabel, 1);
            Grid.SetColumn(quantityOrderLabel, 1);
            grid.Children.Add(quantityOrderLabel);

            var inputQuantityLabel = new Label
            {
                Text = docWare.Fact.ToString(),
                BackgroundColor = Color.FromArgb("#ffffff"),
            };
            Grid.SetRow(inputQuantityLabel, 1);
            Grid.SetColumn(inputQuantityLabel, 2);
            grid.Children.Add(inputQuantityLabel);

            var quantityReasonLabel = new Label
            {
                Text = docWare.QuantityDifference.ToString(),
                BackgroundColor = Color.FromArgb("#ffffff"),
                TextColor = Color.FromArgb(docWare.GetColor)
            };
            Grid.SetRow(quantityReasonLabel, 1);
            Grid.SetColumn(quantityReasonLabel, 3);
            grid.Children.Add(quantityReasonLabel);

            mainStackLayout.Children.Add(grid);

            StackLayoutDocs.Children.Add(mainStackLayout);
        }
    }



    private async void F1Create(object sender, TappedEventArgs e)
    {
        var r = await c.SendDocsDataAsync(Doc, db.GetDocWares(Doc, 2, eTypeOrder.Scan));
        if (r.State != 0) _ = DisplayAlert("Помилка", r.TextError, "OK");
        else
        {
            var toast = Toast.Make("Документ успішно збережений");
            _ = toast.Show();
        }
    }


#if ANDROID
    public void OnPageKeyDown(Keycode keyCode, KeyEvent e)
    {
        switch (keyCode)
        {
            case Keycode.F1:
                F1Create(null, null);
                return;

            default:
                return;
        }
    }
#endif
}