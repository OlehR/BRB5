using BL;
using BRB5.Model;
using System.Collections.ObjectModel;

namespace BRB6.View;

public partial class CreateDoc : ContentPage
{
    DB db = DB.GetDB();
    BL.BL Bl = BL.BL.GetBL();
    // Повний список складів з БД
    List<Warehouse> wh = null;
    public List<Warehouse> ListWarehouse
    {
        get
        {
            if (wh == null)
                try { wh = db.GetWarehouse()?.OrderBy(q => q.Name).ToList(); }
                catch (Exception ex) { /* логування помилки */ }

            if (wh == null || !wh.Any())
                wh = new List<Warehouse>() { new Warehouse() { Code = 0, Name = "ddd" } };
            return wh;
        }
    }

    // Поточний вибраний склад (зв'язаний з Picker)
    private Warehouse _selectedWarehouse;
    public Warehouse SelectedWarehouse
    {
        get => _selectedWarehouse;
        set
        {
            if (_selectedWarehouse == value) return;
            _selectedWarehouse = value;

            if (value != null)
                Config.CodeWarehouse = value.Code; // Збереження в конфіг при зміні

            OnPropertyChanged(nameof(SelectedWarehouse));
        }
    }

    // Відфільтрований список, який бачить користувач у Picker
    public ObservableCollection<Warehouse> FilteredWarehousePicker { get; } = new ObservableCollection<Warehouse>();
    public CreateDoc()
    {
        InitializeComponent();
        this.BindingContext = this;

        FilteredWarehousePicker.Clear();
        foreach (var w in ListWarehouse)
            FilteredWarehousePicker.Add(w);

        // Якщо в конфігу вже є збережений склад — вибираємо його
        if (Config.CodeWarehouse != 0)
        {
            SelectedWarehouse = FilteredWarehousePicker
                .FirstOrDefault(x => x.Code == Config.CodeWarehouse);
        }
    }

    // Основний метод фільтрації
    private void ApplyPickerFilter(string text)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var selectedCode = Config.CodeWarehouse; // Запам'ятовуємо поточний вибір

            FilteredWarehousePicker.Clear();
            IEnumerable<Warehouse> source;

            // Фільтруємо лише якщо введено більше 2 символів
            if (string.IsNullOrWhiteSpace(text) || text.Length < 3)
            {
                source = ListWarehouse;
            }
            else
            {
                text = text.ToLower();
                source = ListWarehouse.Where(w => w.Name?.ToLower().Contains(text) == true);
            }

            foreach (var w in source)
                FilteredWarehousePicker.Add(w);

            // ВІДНОВЛЕННЯ ВИБОРУ: щоб після фільтрації курсор не стрибав
            SelectedWarehouse = FilteredWarehousePicker
                .FirstOrDefault(x => x.Code == selectedCode);
        });
    }

    // Обробник події втрати фокусу полем вводу
    private void OnWarehouseFilterUnfocused(object sender, FocusEventArgs e)
    {
        ApplyPickerFilter(WarehouseFilterEntry.Text);
    }

    // Обробник події натискання Enter (Done) на клавіатурі
    private void OnWarehouseFilterCompleted(object sender, EventArgs e)
    {
        ApplyPickerFilter(WarehouseFilterEntry.Text);
    }
}