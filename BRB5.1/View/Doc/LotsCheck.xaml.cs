using BL.Connector;
using BL;
using BRB5;
using BRB5.Model;
using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Alerts;
using Utils;
using CommunityToolkit.Maui.Views;



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
        //F4ResaveLabel.IsVisible = !IsWares;
        //F3FilterLabel.IsVisible = IsWares;

        var reasonsFromDb = db.GetReason(TypeDoc.LevelReason);
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
        F4ResaveLabel.IsVisible = MyDocs.Any(x => x.State == -1);
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
            ScrollToSelected();
        }
        else
        {
            var result = await c.GetNameWarehouseFromDoc(new DocId { TypeDoc = TypeDoc.CodeDoc, NumberDoc = pBarCode });
            MainThread.BeginInvokeOnMainThread((Action)(async() =>
            {
                if (result.State == 0) // Assuming 0 means success
                {
                    await DisplayAlert("", (string)("Даний товар належить " + result.Data), "OK");
                }
                else
                {
                    await DisplayAlert("Помилка не цей магазин", "Не вдалося отримати назву " + result.TextError, "OK");
                }
            }));
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
            grid.SetBinding(Grid.IsVisibleProperty, nameof(doc.IsVisDoc));

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += OpenDoc;
            grid.GestureRecognizers.Add(tapGestureRecognizer);

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });

            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var dateLabel = new Label { Text = doc.DateDoc.ToString("dd.MM.yyyy") };
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

            // ==================== БЛОК REASON LABEL ====================
            var reasonLabel = new Label
            {
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                VerticalTextAlignment = Microsoft.Maui.TextAlignment.Center,
                Padding = new Thickness(4, 5),
                FontSize = 14
            };
            reasonLabel.SetBinding(Label.BackgroundColorProperty, new Binding("GetColor", source: doc));
            Grid.SetRow(reasonLabel, 1);
            Grid.SetColumn(reasonLabel, 0);

            // Початковий стан тексту
            if (doc.CodeReason != 0)
            {
                var current = AllReasons.FirstOrDefault(r => r.CodeReason == doc.CodeReason);
                if (current != null)
                {
                    reasonLabel.Text = current.NameReason;
                    reasonLabel.TextColor = Colors.Black;
                }
            }
            else
            {
                reasonLabel.Text = " ";
                reasonLabel.TextColor = Colors.Gray;
            }

            // Налаштовуємо логіку доступу: 2 кліки якщо немає причини, 1 клік якщо причина вже є
            SetupLabelGestures(reasonLabel, doc);
            // ===========================================================

            grid.Children.Add(dateLabel);
            grid.Children.Add(numberLabel);
            grid.Children.Add(extInfoStackLayout);
            grid.Children.Add(reasonLabel);

            tempStackLayout.Children.Add(grid);
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            StackLayoutDocs.Children.Add(tempStackLayout);
        });
    }
    // Прапорець для запобігання подвійного відкриття вікна при швидких кліках
    private bool _isDialogOpen = false;

    private void SetupLabelGestures(Label label, DocVM doc)
    {
        label.GestureRecognizers.Clear();

        if (IsWares) return;

        if (doc.CodeReason != 0)
        {
            // Причина встановлена -> відкриваємо за 1 тап
            var singleTap = new TapGestureRecognizer { NumberOfTapsRequired = 1 };
            singleTap.Tapped += async (s, e) =>
            {
                if (_isDialogOpen) return;
                await ShowReasonDialogAsync(doc, label);
            };
            label.GestureRecognizers.Add(singleTap);
        }
        else
        {
            // Причини немає -> відкриваємо за 2 тапи
            var doubleTap = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
            doubleTap.Tapped += async (s, e) =>
            {
                if (_isDialogOpen) return;
                await ShowReasonDialogAsync(doc, label);
            };
            label.GestureRecognizers.Add(doubleTap);
        }
    }

    private async Task ShowReasonDialogAsync(DocVM doc, Label reasonLabel)
    {
        if (_isDialogOpen) return;
        _isDialogOpen = true;

        try
        {
            var reasonSource = (doc.CodeReason != 1
                ? AllReasons.Where(x => x.CodeReason != 1)
                : AllReasons).ToList();

            string[] reasonNames = reasonSource.Select(x => x.NameReason).ToArray();

#if ANDROID
            var tcs = new TaskCompletionSource<string>();
            var activity = Platform.CurrentActivity;

            if (activity != null)
            {
                var builder = new Android.App.AlertDialog.Builder(activity);
                builder.SetTitle("Причина");

                // Список елементів у нативному стилі Picker/Dialog
                builder.SetItems(reasonNames, (s, args) =>
                {
                    tcs.TrySetResult(reasonNames[args.Which]);
                });

                // Ліва кнопка (NeutralButton в Android AlertDialog завжди розташовується зліва)
                builder.SetNeutralButton("Скасувати причину", (s, args) =>
                {
                    tcs.TrySetResult("__CLEAR__");
                });

                // Права кнопка (NegativeButton розташовується справа)
                builder.SetNegativeButton("Скасувати", (s, args) =>
                {
                    tcs.TrySetResult("__CANCEL__");
                });

                builder.SetOnCancelListener(new DialogCancelListener(() =>
                {
                    tcs.TrySetResult("__CANCEL__");
                }));

                var dialog = builder.Create();
                dialog.Show();

                string selected = await tcs.Task;

                if (string.IsNullOrEmpty(selected) || selected == "__CANCEL__")
                {
                    return; // Закрито без змін
                }

                if (selected == "__CLEAR__")
                {
                    doc.CodeReason = 0;
                    reasonLabel.Text = " ";
                    reasonLabel.TextColor = Colors.Gray;
                    db.SetDocReason(doc);
                    SetupLabelGestures(reasonLabel, doc);
                }
                else
                {
                    var pickedReason = reasonSource.FirstOrDefault(x => x.NameReason == selected);
                    if (pickedReason != null)
                    {
                        doc.CodeReason = pickedReason.CodeReason;
                        reasonLabel.Text = pickedReason.NameReason;
                        reasonLabel.TextColor = Colors.Black;
                        db.SetDocReason(doc);
                        SetupLabelGestures(reasonLabel, doc);
                    }
                }
            }
#else
            // Fallback для інших платформ (якщо тестується на Windows/iOS)
            string selected = await DisplayActionSheet("Причина", "Скасувати", "Скасувати причину", reasonNames);
            if (!string.IsNullOrEmpty(selected) && selected != "Скасувати")
            {
                if (selected == "Скасувати причину")
                {
                    doc.CodeReason = 0;
                    reasonLabel.Text = " ";
                    reasonLabel.TextColor = Colors.Gray;
                    db.SetDocReason(doc);
                    SetupLabelGestures(reasonLabel, doc);
                }
                else
                {
                    var pickedReason = reasonSource.FirstOrDefault(x => x.NameReason == selected);
                    if (pickedReason != null)
                    {
                        doc.CodeReason = pickedReason.CodeReason;
                        reasonLabel.Text = pickedReason.NameReason;
                        reasonLabel.TextColor = Colors.Black;
                        db.SetDocReason(doc);
                        SetupLabelGestures(reasonLabel, doc);
                    }
                }
            }
#endif
        }
        finally
        {
            // Невелика затримка перед скиданням, щоб випадковий повторний клік не відкрив вікно знову
            await Task.Delay(250);
            _isDialogOpen = false;
        }
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

    public void ScrollToSelected()
    {
        if (SelectedDoc == null)
            return;

        foreach (var container in StackLayoutDocs.Children.OfType<Layout>())
        {
            foreach (var child in container.Children)
            {
                if (child is Microsoft.Maui.Controls.View view && view.BindingContext is DocVM doc && doc == SelectedDoc)
                {
                    var childBounds = view.Bounds;
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await DocsScrollView.ScrollToAsync(0, childBounds.Y, false);
                    });
                    return;
                }
            }
        }
    }
    private async void F2Save(object sender, EventArgs e)
    {
        if (IsWares || SelectedDoc == null)
            return;

        await SaveCurrentDocAsync(SelectedDoc, true);
    }
    private async Task SaveCurrentDocAsync(DocVM doc, bool tryResendOthers)
    {
        var result = await c.SendDocsDataAsync(doc, null);

        if (result.State == 0) // success
        {
            var toast = Toast.Make("Збереження: " + result.TextError + " " + result.Data, ToastDuration.Long, 14);
            doc.State = 1;
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await toast.Show();
                UpdateDocColor(doc);
            });
        }
        else
        {
            doc.State = -1;
            MainThread.BeginInvokeOnMainThread((Action)(async () =>
            {
                await DisplayAlert("Помилка", (string)("Не вдалося зберегти в 1C \n(Збережено локально)\n"
                    + result.TextError + " " + result.Data), "OK");
                UpdateDocColor(doc);
            }));
        }

        db.SetStateDoc(doc);

        if (TypeDoc.LinkedCodeDoc != 0)
        {
            Doc dl = (Doc)doc.Clone();
            dl.TypeDoc = TypeDoc.LinkedCodeDoc;
            dl.State = 0;
            db.ReplaceDoc([dl]);
            FileLogger.WriteLogMessage(this, "SaveCurrentDocAsync",dl.ToJSON());
        }

        // оновлення видимості F4
        MainThread.BeginInvokeOnMainThread(() =>
        {
            F4ResaveLabel.IsVisible = MyDocs.Any(x => x.State == -1);
        });

        if (result.State == 0 && tryResendOthers)
        {
            await ResendFailedDocsAsync();
        }
    }

    private async Task ResendFailedDocsAsync()
    {
        int successCount = 0;
        int failCount = 0;

        foreach (var d in MyDocs.Where(x => x.State == -1).ToList())
        {
            if (d == SelectedDoc) continue;

            var subResult = await c.SendDocsDataAsync(d, null);

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

            db.SetStateDoc(d);
        }

        // керування видимістю кнопки
        MainThread.BeginInvokeOnMainThread(() =>
        {
            F4ResaveLabel.IsVisible = MyDocs.Any(x => x.State == -1);
        });

        if (successCount + failCount > 0)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var toast = Toast.Make(
                    $"Дозбереження завершено. Успішно: {successCount}, Помилок: {failCount}",
                    ToastDuration.Long, 14);
                await toast.Show();
            });
        }
    }
    private async void F4Resave(object sender, EventArgs e)
    {
        if (!F4ResaveLabel.IsVisible|| IsWares)
            return;
        await ResendFailedDocsAsync();
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
            case Keycode.F4:
                F4Resave(null, EventArgs.Empty);
                return;
            default:
                return;
        }
    }

    public class DialogCancelListener : Java.Lang.Object, Android.Content.IDialogInterfaceOnCancelListener
    {
        private readonly Action _onCancel;
        public DialogCancelListener(Action onCancel) => _onCancel = onCancel;
        public void OnCancel(Android.Content.IDialogInterface dialog) => _onCancel?.Invoke();
    }

#endif
}