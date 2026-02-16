using BL;
using BRB5.Model;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BRB6.View;
public partial class CreateDoc : ContentPage
{
    DB db = DB.GetDB();
    BL.BL Bl = BL.BL.GetBL();
    private int _initialCode; // Зберігаємо початковий код тут

    public ObservableCollection<Warehouse> QuickAccessWarehouses { get; } = new ObservableCollection<Warehouse>();
    public ObservableCollection<Warehouse> FilteredWarehousePicker { get; } = new ObservableCollection<Warehouse>();

    private List<Warehouse> _allWarehouses;
    public List<Warehouse> ListWarehouse => _allWarehouses ??= db.GetWarehouse()?.OrderBy(q => q.Name).ToList() ?? new List<Warehouse>();

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
                    qw.IsChecked = (qw.Code == value.Code);
            }
            OnPropertyChanged(nameof(SelectedWarehouse));
        }
    }

    public string Comment { get; set; }
    public ICommand SelectQuickWarehouseCommand { get; }

    public CreateDoc()
    {
        InitializeComponent();

        // 1. Читаємо конфіг один раз
        _initialCode = Config.CodeWarehouse;

        // 2. Команда з логікою віджаття та очищення фільтра
        SelectQuickWarehouseCommand = new Command<Warehouse>((clickedBtn) =>
        {
            if (SelectedWarehouse != null && SelectedWarehouse.Code == clickedBtn.Code)
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
            }
        });

        this.BindingContext = this;

        // Первинне заповнення
        foreach (var w in ListWarehouse)
            FilteredWarehousePicker.Add(w);

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
                s.IsChecked = (s.Code == _initialCode);
                QuickAccessWarehouses.Add(s);
            }
        }
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

        // Тут використовуйте SelectedWarehouse.Code та Comment для збереження
        await DisplayAlert("Успіх", $"Документ створено для {SelectedWarehouse.Name}", "ОК");
    }
}