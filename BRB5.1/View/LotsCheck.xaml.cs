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
    public bool IsSoftKeyboard { get { return Config.IsSoftKeyboard; } }

    private DocVM SelectedDoc;
    private bool IsWares;
    public double height { get { return DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density - 150; } }
    public bool IsMandatory { get; set; } = false;
    public string FilterLabel => IsMandatory ? "F3-Обов'язкові" : "F3-Всі";
    public LotsCheck(TypeDoc vTypeDoc)
    {
        InitializeComponent();
        TypeDoc = vTypeDoc;

        PopulateStackLayout();
        Task.Run(async () =>
        {
            await c.LoadDocsDataAsync(TypeDoc.CodeDoc, null, false);
            PopulateStackLayout();
        });

        BindingContext = this;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        Config.BarCode = BarCode;

        if (!IsSoftKeyboard)
        {
#if ANDROID
            MainActivity.Key += OnPageKeyDown;
#endif
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
    async void BarCode(string pBarCode) // BarCode
    {
        if (SelectedDoc != null)
            SelectedDoc.SelectedColor = false;
         /*StackLayoutDocs.Children
                        .OfType<Microsoft.Maui.Controls.View>()
                        .Select(view => view.BindingContext as DocVM)
                        .FirstOrDefault(item => item != null && item.BarCode == pBarCode);*/

        var r = MyDocs.FirstOrDefault(item => item != null && item.BarCode == pBarCode);
        if (r != null)
        {
            SelectedDoc = r;
            r.SelectedColor = true;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ScrollToSelected();
            });
        }
        else
        {
            var result = await c.GetNameWarehouseFromDoc(new DocId { TypeDoc = TypeDoc.CodeDoc, NumberDoc = pBarCode });
            MainThread.BeginInvokeOnMainThread(async() =>
            {
                if (result.State == 0) // Assuming 0 means success
                {
                    await DisplayAlert("", "Даний товар належить " + result.Info, "OK");
                }
                else
                {
                    await DisplayAlert("Помилка", "Не вдалося отримати назву " + result.TextError, "OK");
                }
            });
        }
    }
    public void Dispose() { Config.BarCode -= BarCode; }
    private void PopulateStackLayout()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            StackLayoutDocs.Children.Clear();
        });
        MyDocs = new ObservableCollection<DocVM>(db.GetDoc(TypeDoc));

        /*
        //// --- Add this block to multiply documents for testing ---
        //int multiplyFactor = 10; // Change this to get more items (e.g., 10x20=200)
        //var docsList = MyDocs.ToList();
        //for (int i = 1; i < multiplyFactor; i++)
        //{
        //    foreach (var doc in docsList)
        //    {
        //        // Clone the doc to avoid reference issues (implement Clone if needed)
        //        var newDoc = new DocVM
        //        {
        //            DateDoc = doc.DateDoc,
        //            TypeDoc = doc.TypeDoc,
        //            NumberDoc = doc.NumberDoc + $"_{i}", // Make NumberDoc unique
        //            ExtInfo = doc.ExtInfo,
        //            BarCode = doc.BarCode + $"_{i}"
        //            // Copy other properties as needed
        //        };
        //        MyDocs.Add(newDoc);
        //    }
        //}
        */

        var firstDoc = MyDocs.FirstOrDefault();
        if (firstDoc != null)
        {
            var temp = db.GetDocWares(firstDoc, 1, eTypeOrder.Scan);
            IsWares = temp?.Any() == true;
            F2SaveLabel.IsVisible = !IsWares;
            F3FilterLabel.IsVisible = IsWares;
        }
        else
        {
            IsWares = false;
        }

        var tempStackLayout = new StackLayout();
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
            tapGestureRecognizer.Tapped += OpenDoc;
            grid.GestureRecognizers.Add(tapGestureRecognizer);

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });

            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var dateLabel = new Label { Text = doc.DateDoc.ToString("dd.MM.yyyy"), };
            dateLabel.SetBinding(Label.BackgroundColorProperty, new Binding("GetColor", source: doc));

            var numberLabel = new Label { Text = doc.NumberDoc };
            numberLabel.SetBinding(Label.BackgroundColorProperty, new Binding("GetColor", source: doc));
            Grid.SetColumn(numberLabel, 1);

            var emptyLabel = new Label { Text = "" };
            emptyLabel.SetBinding(Label.BackgroundColorProperty, new Binding("GetColor", source: doc));
            Grid.SetRow(emptyLabel, 1);

            var extInfoStackLayout = new StackLayout();
            extInfoStackLayout.SetBinding(Label.BackgroundColorProperty, new Binding("GetColor", source: doc));
            Grid.SetColumn(extInfoStackLayout, 1);
            Grid.SetRow(extInfoStackLayout, 1);

            if (!string.IsNullOrEmpty(doc.ExtInfo))
            {
                var extInfoLines = doc.ExtInfo.Split(new[] { "\r" }, StringSplitOptions.None);
                foreach (var line in extInfoLines)
                {
                    extInfoStackLayout.Children.Add(new Label { Text = line });
                }
            }

            grid.Children.Add(dateLabel);
            grid.Children.Add(numberLabel);
            grid.Children.Add(emptyLabel);
            grid.Children.Add(extInfoStackLayout);

            tempStackLayout.Children.Add(grid);
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            StackLayoutDocs.Children.Add(tempStackLayout);
        });
    }

    private async void OpenDoc(object sender, TappedEventArgs e)
    {
        if (SelectedDoc != null)
            SelectedDoc.SelectedColor = false;
        if (sender is Grid grid && grid.BindingContext is DocVM doc)
        {
            SelectedDoc = doc;
            doc.SelectedColor = true;

            if (IsWares) await Navigation.PushAsync(new DocItem(doc, TypeDoc));
        }
    }

    public async void ScrollToSelected()
    {
        if (SelectedDoc == null)
            return;

        foreach (var child in StackLayoutDocs.Children)
        {
            if (child is Microsoft.Maui.Controls.View view && view.BindingContext is DocVM doc &&
                doc == SelectedDoc)
            {
                var childBounds = view.Bounds;

                // Ensure you're calling ScrollToAsync on the correct ScrollView instance
                await DocsScrollView.ScrollToAsync(0, childBounds.Y, false);
                break;
            }
        }
    }
    private void F2Save(object sender, EventArgs e)
    {
        if (IsWares) return;
        if (SelectedDoc == null)
            return;
        Task.Run(async () =>
        {
            var r = await c.SendDocsDataAsync(SelectedDoc, null);
            var toast = Toast.Make("Збереження: " + r.TextError, ToastDuration.Long, 14);
            MainThread.BeginInvokeOnMainThread(async () => await toast.Show());
        });
    }
    private void F3Filter(object sender, EventArgs e)
    {
        IsMandatory = !IsMandatory;
        OnPropertyChanged(nameof(IsMandatory));
    }
#if ANDROID
    public void OnPageKeyDown(Keycode keyCode, KeyEvent e)
    {
        switch (keyCode)
        {
            case Keycode.F2:
                F2Save(null, EventArgs.Empty);
                return;
            case Keycode.F3:
                F3Filter(null, EventArgs.Empty);
                return;
            default:
                return;
        }
    }
#endif
}