using BL;
using BRB5;
using BRB5.Model;
using BRB6.View;
//using Intents;

namespace BRB6.Template;

public partial class ExpirationDateElementTemplate : ContentView
{
    DB db = DB.GetDB();
    public event Action<ExpirationDateElementVM> RequestReturnToMainContent;
    public ExpirationDateElementVM DM { get; set; } = new();
    private ExpirationDateElementVM _DM;
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

    public void Set(ExpirationDateElementVM pED)
    {
        _DM = pED;
        DM = (ExpirationDateElementVM)pED.Clone();
        DM.QuantityInput = DM.QuantityInput ?? DM.Quantity;

        if (DM.ExpirationDate == DateTime.MinValue) DM.ExpirationDate = DateTime.Today;

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