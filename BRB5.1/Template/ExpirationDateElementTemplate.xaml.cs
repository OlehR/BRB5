using BL;
using BRB5;
using BRB5.Model;
using BRB6.View;
//using Intents;

namespace BRB6.Template;

public partial class ExpirationDateElementTemplate : ContentView
{
    string NumberDoc;
    DB db = DB.GetDB();
    public event Action<ExpirationDateElementVM> RequestReturnToMainContent;
    public ExpirationDateElementVM DM { get; set; } = new();
    private ExpirationDateElementVM _DM;
    IEnumerable<ExpirationDateElementVM> ListED { get; set; }
    public bool IsManual { get; set; }

    public ExpirationDateElementTemplate()
    {
        InitializeComponent();

        NokeyBoard();
        var backgroundColorBinding = new Binding
        {
            Path = "DM.GetColor",
            Converter = new ColorConverter()
        };
        StackBackground.SetBinding(Label.BackgroundColorProperty, backgroundColorBinding);

        this.BindingContext = this;
    }

    public void Set(ExpirationDateElementVM pED, string pNumberDoc,IEnumerable<ExpirationDateElementVM> pListED )
    {
     NumberDoc=pNumberDoc;
        _DM = pED;
        ListED= pListED;
        DM = (ExpirationDateElementVM)pED.Clone();
        DM.QuantityInput = DM.QuantityInput ?? DM.Quantity;

        if (DM.ExpirationDate == DateTime.MinValue && DM.ExpirationDateInput > DateTime.Today) DM.ExpirationDate = DM.ExpirationDateInput;
        if (DM.ExpirationDate == DateTime.MinValue)
            DM.ExpirationDate = DateTime.Today;
        IsManual = DM.IsManual;
        OnPropertyChanged(nameof(IsManual));

        FillLotHistory(ListED);

        DM.ExpirationDateInput = DM.ExpirationDate;
        DM.ProductionDateInput = DM.ExpirationDate.AddDays(-(double)DM.Expiration);
        KeyboardSelection();
        OnPropertyChanged(nameof(DM));
    }
    private void KeyboardSelection()
    {
        QuantityEntry.Keyboard = DM.CodeUnit == Config.GetCodeUnitWeight
            ? Keyboard.Telephone
            : Keyboard.Numeric;
    }

    bool IsExpirationDateSelected = false;
    private void ExpirationDateSelected(object sender, DateChangedEventArgs e)
    {
        IsExpirationDateSelected = true;
        DM.ProductionDateInput = DM.ExpirationDateInput.AddDays(-(double)DM.Expiration);
        IsExpirationDateSelected = false;

        ButtonSave.IsEnabled = (DM.ExpirationDateInput - DateTime.Today).TotalDays <= 32;

        //OnPropertyChanged(nameof(DM.GetPercentColor));
        //OnPropertyChanged(nameof(DM.GetColor));
    }

    private void ProductionDateSelected(object sender, DateChangedEventArgs e)
    {
        if(!IsExpirationDateSelected)
            DM.ExpirationDateInput = DM.ProductionDateInput.AddDays((double)DM.Expiration);
        //OnPropertyChanged(nameof(DM.GetPercentColor));
        //OnPropertyChanged(nameof(DM.GetColor));
    }

    private void OnAdd(object sender, EventArgs e)
    {
        if (String.IsNullOrEmpty(DM.NumberDoc)) DM.NumberDoc = NumberDoc;
        if (!String.IsNullOrEmpty(DM.DocId) && !String.IsNullOrEmpty(DM.NumberDoc))
        {
            db.ReplaceDocWaresExpiration(DM.GetDocWaresExpiration());
            if (_DM.DocId?.Equals(DM.DocId) == true)
            {
                _DM.ExpirationDateInput = DM.ExpirationDateInput;
                _DM.QuantityInput = DM.QuantityInput;
            }
        }
        RequestReturnToMainContent?.Invoke(DM);
    }

    private void OnNotFound(object sender, EventArgs e)
    {
        DM.QuantityInput = 0;
        db.ReplaceDocWaresExpiration(DM.GetDocWaresExpiration());

        _DM.ExpirationDateInput = DM.ExpirationDateInput;
        _DM.QuantityInput = DM.QuantityInput;
        RequestReturnToMainContent?.Invoke(_DM);
    }

    private void OnAddNewItem(object sender, EventArgs e)
    { 
        DM.Quantity = 0;
        DM.DocId = "zz" + DateTime.Now.ToString("yyyyMMddHHmmssffff") ;
        DM.ExpirationDateInput = new DateTime( DateTime.Now.Date.Year, DateTime.Now.Date.Month, 1);
        //DM.ProductionDateInput = DM.ExpirationDate.AddDays(-(double)DM.Expiration);
        DM.QuantityInput = 0;

        IsManual = DM.IsManual;
        OnPropertyChanged(nameof(IsManual));

    }
    private void FillLotHistory(IEnumerable<ExpirationDateElementVM> listED)
    {
        LotHistoryStackLayout.Children.Clear();

        // Створюємо одну сітку для всієї історії
        var historyGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
        {
            new ColumnDefinition { Width = GridLength.Star }, // Ліва колонка (System)
            new ColumnDefinition { Width = GridLength.Star }  // Права колонка (Manual)
        },
            RowSpacing = 2,
            ColumnSpacing = 5
        };

        // Розділяємо дані на дві групи
        var systemItems = listED.Where(x => !x.IsManual).ToList();
        var manualItems = listED.Where(x => x.IsManual).ToList();

        // Визначаємо максимальну кількість рядків, яка нам знадобиться
        int rowCount = Math.Max(systemItems.Count, manualItems.Count);

        for (int i = 0; i < rowCount; i++)
        {
            // Додаємо опис рядка в Grid
            historyGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Якщо є системний елемент для цього рядка
            if (i < systemItems.Count)
            {
                var item = systemItems[i];
                var label = new Label
                {
                    Text = $"{item.ExpirationDate:dd.MM.yyyy} - {item.Quantity} {item.NameUnit}",
                    TextColor = Colors.Blue,
                    FontSize = 12,
                    HorizontalOptions = LayoutOptions.Start
                };
                historyGrid.Add(label, 0, i); // Колонка 0, Рядок i
            }

            // Якщо є ручний елемент для цього рядка
            if (i < manualItems.Count)
            {
                var item = manualItems[i];
                var label = new Label
                {
                    Text = $"{item.ExpirationDateInput:dd.MM.yyyy} - {item.QuantityInput} {item.NameUnit}",
                    TextColor = Colors.Gray,
                    FontSize = 12,
                    HorizontalOptions = LayoutOptions.Start
                };
                historyGrid.Add(label, 1, i); // Колонка 1, Рядок i
            }
        }

        LotHistoryStackLayout.Children.Add(historyGrid);
    }
    private void QuantityFocused(object sender, FocusEventArgs e)
    {
        Dispatcher.Dispatch(() =>
        {
            QuantityEntry.CursorPosition = 0;
            QuantityEntry.SelectionLength = QuantityEntry.Text == null ? 0 : QuantityEntry.Text.Length;
            
        });
    }

    private void Back(object sender, EventArgs e)
    {
        RequestReturnToMainContent?.Invoke(_DM);
    }

    protected void NokeyBoard()
    {
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("MyCustomization", (handler, view) =>
        {
            if (Config.TypeScaner != eTypeScaner.Camera)
            {
                if (view is CustomEntry)
                {
#if ANDROID
                    handler.PlatformView.ShowSoftInputOnFocus = false;
#endif
                }
            }
        });
    }
}