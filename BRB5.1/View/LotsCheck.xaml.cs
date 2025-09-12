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
    //public bool IsMandatory { get; set; } = true;
    //public string FilterLabel => IsMandatory ? "F3-Обов'язкові" : "F3-Всі";
    public ObservableCollection<BRB5.Model.DB.Reason> AllReasons { get; set; }
    public LotsCheck(TypeDoc vTypeDoc)
    {
        InitializeComponent();
        TypeDoc = vTypeDoc;

        IsWares = TypeDoc.KindDoc == eKindDoc.Lot;
        F2SaveLabel.IsVisible = !IsWares;
        //F3FilterLabel.IsVisible = IsWares;

        var reasonsFromDb = db.GetReason(TypeDoc.KindDoc);
        AllReasons = new ObservableCollection<BRB5.Model.DB.Reason>(reasonsFromDb);
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
                    await DisplayAlert("Помилка не цей магазин", "Не вдалося отримати назву " + result.TextError, "OK");
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
        // Завантажуємо всі документи
        var allDocs = db.GetDoc(TypeDoc);

        
        if (IsWares)
            MyDocs = new ObservableCollection<DocVM>(allDocs.Where(el => el.CodeReason != 0));
        else
            MyDocs = new ObservableCollection<DocVM>(allDocs);
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

        //MyDocs = new ObservableCollection<DocVM>(allDocs);

        var tempStackLayout = new StackLayout();
        foreach (var doc in MyDocs)
        {
            UpdateDocColor(doc);
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
            // === Picker для вибору причини ===
            var reasonPicker = new Picker
            {
                Title = "Причина",
                ItemsSource = AllReasons,
                ItemDisplayBinding = new Binding("NameReason"),
                IsEnabled = false,
                IsVisible = false,
            };
            // встановлюємо вибір з документа
            if (doc.CodeReason != 0 )
            {
                var current = AllReasons.FirstOrDefault(r => r.CodeReason == doc.CodeReason);
                if (current != null)
                {
                    reasonPicker.SelectedItem = current;
                    reasonPicker.IsVisible = true;

                    if (!IsWares)
                        reasonPicker.IsEnabled = true;
                }
            }

            // при зміні вибору оновлюємо Doc.CodeReason
            reasonPicker.SelectedIndexChanged += (s, e) =>
            {
                if (reasonPicker.SelectedItem is BRB5.Model.DB.Reason r)
                {
                    doc.CodeReason = r.CodeReason;
                    var t = db.SetDocReason(doc);
                }

            };
            reasonPicker.SetBinding(Picker.BackgroundColorProperty, new Binding("GetColor", source: doc));
            Grid.SetRow(reasonPicker, 1);
            var emptyLabel = new Label { Text = "" };
            emptyLabel.SetBinding(Label.BackgroundColorProperty, new Binding("GetColor", source: doc));
            Grid.SetRow(emptyLabel, 1);

            grid.Children.Add(dateLabel);
            grid.Children.Add(numberLabel);
            grid.Children.Add(extInfoStackLayout);
            grid.Children.Add(emptyLabel);
            grid.Children.Add(reasonPicker);

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
            if (IsWares)
            {
                // у будь-якому випадку (навіть якщо це той самий doc)
                SelectedDoc = doc;
                await Navigation.PushAsync(new DocItem(doc, TypeDoc));
            }
            else
            {
                if (SelectedDoc != doc)
                {
                    SelectedDoc = doc;
                }
                else
                {
                    // другий тап по тому ж doc → показати picker
                    var picker = grid.Children.OfType<Picker>().FirstOrDefault();
                    if (picker != null)
                    {
                        picker.IsEnabled = true;
                        picker.IsVisible = true;
                        picker.Focus();
                    }
                }
            }

            doc.SelectedColor = true;
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
    private async void F2Save(object sender, EventArgs e)
    {
        if (IsWares) return;
        if (SelectedDoc == null)
            return;

        await SaveAndResendAsync(SelectedDoc);
    }
    private async Task SaveAndResendAsync(DocVM doc)
    {

        var result = await c.SendDocsDataAsync(SelectedDoc, null);

        ////TMP!!!!
        //var result = new UtilNetwork.Result(0, "успішно");

        if (result.State == 0) // 0 = success
        {
            var toast = Toast.Make("Збереження: " + result.TextError + " " + result.Info, ToastDuration.Long, 14); 
            SelectedDoc.State = 1;
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await toast.Show();                
                UpdateDocColor(SelectedDoc);
            });
        }
        else
        {
            SelectedDoc.State = -1;
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("Помилка", "Не вдалося зберегти (Збережено локально)" + result.TextError + " " + result.Info, "OK");
                UpdateDocColor(SelectedDoc);
            });
        }
        var t = db.SetStateDoc(SelectedDoc);
        if (TypeDoc.LinkedCodeDoc != 0)
        {
            Doc dl = (Doc)SelectedDoc.Clone();
            dl.TypeDoc = TypeDoc.LinkedCodeDoc;
            db.ReplaceDoc([dl]);
        }

        // Якщо успіх, пробуємо надіслати решту
        if (result.State == 0)
        {
            int successCount = 0;
            int failCount = 0;

            foreach (var d in MyDocs.Where(x => x.State == -1).ToList())
            {
                if (d == SelectedDoc) continue;

                var subResult = await c.SendDocsDataAsync(d, null);
               
                ////TMP!!!!
                //var subResult = new UtilNetwork.Result(-1, "не успішно");

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (subResult.State == 0)
                    {
                        d.State = 1; // успішно
                        successCount++;
                    }
                    else
                    {
                        d.State = -1; // помилка
                        failCount++;
                    }
                    UpdateDocColor(d);
                });

                t = db.SetStateDoc(SelectedDoc);
            }
            // після циклу — показати результат
            if (successCount + failCount > 0) {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    var toast = Toast.Make(
                        $"Збереження: OK. Додатково надіслано: {successCount}, Помилок: {failCount}",
                        ToastDuration.Long, 14);
                    await toast.Show();
                }); 
            }
        }
    }
    private void UpdateDocColor(DocVM doc)
    {        
        switch (doc.State)
        {
            case 1: // успіх
                doc.Color = 1; // зелений
                break;
            case -1: // помилка
                doc.Color = 9; // червоний
                break;
            default: // за замовчуванням
                doc.Color = 0; // жовтий
                break;
        }

        // оповіщаємо що змінились властивості для UI
        doc.RefreshColor();
    }

    private void F3Filter(object sender, EventArgs e)
    {
        //if (!IsWares) return;
        //IsMandatory = !IsMandatory;
        //OnPropertyChanged(nameof(FilterLabel));
        //OnPropertyChanged(nameof(IsMandatory));
        //PopulateStackLayout();
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