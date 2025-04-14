using BL.Connector;
using BL;
using BRB5;
using BRB5.Model;
using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Alerts;

#if ANDROID
using Android.Views;
#endif

namespace BRB6.View;

public partial class LotsCheck : ContentPage
{
    private Connector c = ConnectorBase.GetInstance();
    private TypeDoc TypeDoc;
    DB db = DB.GetDB();
    private ObservableCollection<DocVM> MyDocs = new ObservableCollection<DocVM>();

    private DocVM SelectedDoc;
    public LotsCheck(TypeDoc vTypeDoc)
	{
		InitializeComponent();
        TypeDoc = vTypeDoc;
        PopulateStackLayout();
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        Config.BarCode = BarCode;
        _ = c.LoadDocsDataAsync(TypeDoc.CodeDoc, null, false);
    }
    async void BarCode(string pBarCode) // BarCode
    {
        if (SelectedDoc != null)
            SelectedDoc.SelectedColor = false;
        var r = StackLayoutDocs.Children
                        .OfType<Microsoft.Maui.Controls.View>()
                        .Select(view => view.BindingContext as DocVM)
                        .FirstOrDefault(item => item != null && item.BarCode == pBarCode);
        if (r != null)
        {
            SelectedDoc = r;
            r.SelectedColor = true;

            ScrollToSelected();
        }
        else
        {
            var result = await c.GetNameWarehouseFromDoc(new DocId { TypeDoc = TypeDoc.CodeDoc, NumberDoc = pBarCode });
            if (result.State == 0) // Assuming 0 means success
            {
                await DisplayAlert("", "Даний товар належить " + result.Info, "OK");
            }
            else
            {
                await DisplayAlert("Помилка", "Не вдалося отримати назву " + result.TextError, "OK");
            }
        }
    }
    public void Dispose() { Config.BarCode -= BarCode; }
    private void PopulateStackLayout()
    {
        MyDocs = new ObservableCollection<DocVM>(db.GetDoc(TypeDoc));

        foreach (var doc in MyDocs)
        {
            var grid = new Grid
            {
                RowSpacing = 1,
                ColumnSpacing = 1,
                Padding = 1,
                BackgroundColor = Color.FromArgb("#adaea7"),
                BindingContext = doc
            };

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += SaveLot;
            grid.GestureRecognizers.Add(tapGestureRecognizer);

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });

            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var dateLabel = new Label {  Text = doc.DateDoc.ToString("dd.MM.yyyy"),  };
            dateLabel.SetBinding(Label.BackgroundColorProperty, new Binding("GetColor", source: doc));

            var numberLabel = new Label  {  Text = doc.NumberDoc  };
            numberLabel.SetBinding(Label.BackgroundColorProperty, new Binding("GetColor", source: doc));
            Grid.SetColumn(numberLabel, 1);

            var emptyLabel = new Label  {   Text = ""   };
            emptyLabel.SetBinding(Label.BackgroundColorProperty, new Binding("GetColor", source: doc));
            Grid.SetRow(emptyLabel, 1);

            var extInfoStackLayout = new StackLayout();
            extInfoStackLayout.SetBinding(Label.BackgroundColorProperty, new Binding("GetColor", source: doc));
            Grid.SetColumn(extInfoStackLayout, 1);
            Grid.SetRow(extInfoStackLayout, 1);

            var extInfoLines = doc.ExtInfo.Split(new[] { "\r" }, StringSplitOptions.None);
            foreach (var line in extInfoLines)
            {
                extInfoStackLayout.Children.Add(new Label { Text = line });
            }

            grid.Children.Add(dateLabel);
            grid.Children.Add(numberLabel);
            grid.Children.Add(emptyLabel);
            grid.Children.Add(extInfoStackLayout);

            StackLayoutDocs.Children.Add(grid);
        }
    }

    private async void SaveLot(object sender, TappedEventArgs e)
    {
        if (SelectedDoc != null)
        {
            var r = await c.SendDocsDataAsync(SelectedDoc, null);
            if (r.State != 0) _ = DisplayAlert("Помилка", r.TextError, "OK");
            else
            {
                var toast = Toast.Make("Документ успішно збережений");
                _ = toast.Show();
            }
        }
    }

    public async void ScrollToSelected()
    {
        //if (SelectedWare == null)
        //    return;

        //// Iterate through the children of WareItemsContainer
        //foreach (var child in WareItemsContainer.Children)
        //{
        //    if (child is Microsoft.Maui.Controls.View view && view.BindingContext is ExpirationDateElementVM itemModel &&
        //        itemModel.CodeWares == SelectedWare.CodeWares)
        //    {
        //        var childBounds = view.Bounds;

        //        // Ensure you're calling ScrollToAsync on the correct ScrollView instance
        //        await ScrollView.ScrollToAsync(0, childBounds.Y, false);
        //        break;
        //    }
        //}
    }
    private void F2Save(object sender, EventArgs e)
    {
        if (SelectedDoc == null)
            return;
        Task.Run(async () =>
        {
            var r = await c.SendDocsDataAsync(SelectedDoc, null);
            var toast = Toast.Make("Збереження: " + r.TextError, ToastDuration.Long, 14);
            MainThread.BeginInvokeOnMainThread(async () => await toast.Show());
        });
    }

#if ANDROID
    public void OnPageKeyDown(Keycode keyCode, KeyEvent e)
    {
        switch (keyCode)
        {
            case Keycode.F2:
                F2Save(null, EventArgs.Empty);
                return;
            default:
                return;
        }
    }
#endif
}