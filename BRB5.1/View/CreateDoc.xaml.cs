using BL;
using BL.Connector;
using BRB5;
using BRB5.Model;
using BRB5.Model.DB;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BRB6.View;
public partial class CreateDoc : ContentPage
{
    DB db = DB.GetDB();
    BL.BL Bl = BL.BL.GetBL();
    TypeDoc TD;
    Action<DocVM> CreatedDoc;
    private int _initialCode; // Зберігаємо початковий код тут

    public ObservableCollection<Warehouse> QuickAccessWarehouses { get; } = [];
    public ObservableCollection<Warehouse> FilteredWarehousePicker { get; set; } = [];

    public ObservableCollection<Reason> FilteredReasonPicker { get; set; } = [];
    public bool IsVisibleReason { get { return FilteredReasonPicker?.Any() == true; } }

    private List<Warehouse> _allWarehouses;
    public List<Warehouse> ListWarehouse => _allWarehouses ??= db.GetWarehouse()?.OrderBy(q => q.Name).ToList() ?? [];

    public List<Reason> ListReason = [];

    public Reason SelectedReason { get; set; }

    private Warehouse _selectedWarehouse;
    public Warehouse SelectedWarehouse
    {
        get => _selectedWarehouse;
        set
        {
            if (_selectedWarehouse == value) return;
            _selectedWarehouse = value;

            // Оновлюємо IsChecked для кнопок, щоб спрацювали кольори в XAML
            if (value != null)
            {
                foreach (var qw in QuickAccessWarehouses)
                    qw.IsChecked = (qw.TypeWarehouse == value.TypeWarehouse);
            }
            OnPropertyChanged(nameof(SelectedWarehouse));
            FilteredReason();

        }
    }

    public string Comment { get; set; }
    public ICommand SelectQuickWarehouseCommand { get; }

    public CreateDoc(TypeDoc pTD,Action<DocVM> pActionNumberDoc)
    {

        TD = pTD;
        InitializeComponent();
        CreatedDoc= pActionNumberDoc;

        ListReason =  db.GetReason(TD.LevelReason)?.OrderBy(q => q.NameReason).ToList() ?? [];

        // 1. Читаємо конфіг один раз
        _initialCode = Config.CodeWarehouse;

        // 2. Команда з логікою віджаття та очищення фільтра
        SelectQuickWarehouseCommand = new Command<Warehouse>((clickedBtn) =>
        {
            WarehouseFilterEntry.Text = "";

            WarehouseFilterEntry.Unfocus();
            if (clickedBtn.IsChecked)
            {
                FilteredWarehousePicker = new(ListWarehouse);
            }
            else
            { 
            FilteredWarehousePicker.Clear();
            var Wh = ListWarehouse.Where(r => r.TypeWarehouse == clickedBtn.TypeWarehouse);
            foreach (var r in Wh)
                FilteredWarehousePicker.Add(r);

            SelectedWarehouse = FilteredWarehousePicker.FirstOrDefault(); //FilteredWarehousePicker.Count() > 1?null: FilteredWarehousePicker.FirstOrDefault();
            }
            /*if (SelectedWarehouse != null && SelectedWarehouse.Code == clickedBtn.Code)
            {
                WarehouseFilterEntry.Text = string.Empty;
                ApplyPickerFilter(null);
                SelectedWarehouse = ListWarehouse.FirstOrDefault(x => x.Code == _initialCode);
            }
            else
            {
                // ЗВИЧАЙНЕ НАТИСКАННЯ (вибір іншого складу)
                var target = ListWarehouse.FirstOrDefault(x => x.Code == clickedBtn.Code);
                if (target != null)
                {
                    WarehouseFilterEntry.Text = string.Empty;
                    ApplyPickerFilter(null);

                    SelectedWarehouse = target;
                }
            }*/
        });
        if (pTD.IsViewReasonHead )
            FilteredReasonPicker = new();

        this.BindingContext = this;

        // Первинне заповнення
        //foreach (var w in ListWarehouse)
        //    FilteredWarehousePicker.Add(w);
       

        InitData();
    }

    private void InitData()
    {
        SelectedWarehouse = ListWarehouse.FirstOrDefault(x => x.Code == _initialCode);

        var subs = db.GetWarehouseCreateDoc(_initialCode);
        if (subs != null)
        {
            foreach (var s in subs)
            {
                //s.IsChecked = (s.Code == _initialCode);
                QuickAccessWarehouses.Add(s);
            }
        }
    }
    void FilteredReason()
    {
        FilteredReasonPicker.Clear();
        if (SelectedWarehouse != null)
        {
            var reasons = ListReason.Where(r => r.TypeWarehouse == SelectedWarehouse.TypeWarehouse);
            foreach (var r in reasons)
                FilteredReasonPicker.Add(r);
        }
        OnPropertyChanged(nameof(IsVisibleReason));
    }

    private void ApplyPickerFilter(string text)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            // Запам'ятовуємо, що зараз вибрано, щоб не злетіло при перемальовуванні
            int currentId = SelectedWarehouse?.Code ?? _initialCode;

            FilteredWarehousePicker.Clear();

            IEnumerable<Warehouse> filtered;
            if (string.IsNullOrWhiteSpace(text) || text.Length < 3)
            {
                filtered = ListWarehouse;
            }
            else
            {
                string search = text.ToLower();
                filtered = ListWarehouse.Where(w => w.Name?.ToLower().Contains(search) == true);
            }

            foreach (var w in filtered)
                FilteredWarehousePicker.Add(w);

            // Відновлюємо вибраний елемент
            SelectedWarehouse = FilteredWarehousePicker.FirstOrDefault(x => x.Code == currentId);
        });
    }

    private void OnWarehouseFilterUnfocused(object sender, FocusEventArgs e) => ApplyPickerFilter(WarehouseFilterEntry.Text);
    private void OnWarehouseFilterCompleted(object sender, EventArgs e) => ApplyPickerFilter(WarehouseFilterEntry.Text);

    private async void OnCreateDocumentClicked(object sender, EventArgs e)
    {
        // Логіка створення документа
        if (SelectedWarehouse == null)
        {
            await DisplayAlert("Помилка", "Будь ласка, оберіть склад", "ОК");
            return;
        }
        if (FilteredReasonPicker?.Any()==true && SelectedReason == null)
        {
            await DisplayAlert("Помилка", "Будь ласка, оберіть причину", "ОК");
            return;
        }

        var c = ConnectorBase.GetInstance();
        var R = await c.CreateDoc(new () { TypeDoc = TD.CodeDoc,CodeWarehouseFrom= Config.CodeWarehouse, CodeWarehouseTo = SelectedWarehouse?.CodeWarehouse??0, CodeReason= SelectedReason?.CodeReason??0, Description = Comment, CodeUser=Config.CodeUser, ExtInfo = SelectedWarehouse?.Name??" "+ SelectedReason?.CodeReason??"" });
        if (R.State != 0)
        {
            await DisplayAlert("Помилка", $"Не вдалося створити документ: State=>{R.State} {R.TextError}", "ОК");
            return;
        }
        db.ReplaceDoc([R.Data]);
        
        // Тут використовуйте SelectedWarehouse.Code та Comment для збереження
        await DisplayAlert("Успіх", $"Документ {R.Data.NumberDoc} створено для {SelectedWarehouse.Name}", "ОК");
        await Navigation.PopAsync();
        CreatedDoc?.Invoke(R.Data);
    }
}