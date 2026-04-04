using BL;
using BRB5;
using BRB5.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace BRB6.ViewModel
{
    class DocItemCheckVM : ObservableObject
    {
        DB db = DB.GetDB();
        private ObservableCollection<DocWaresEx> _wares = [];
        private bool _isLoading;
        private string _title = "Лист викладки";

        public ObservableCollection<DocWaresEx> Wares
        {
            get => _wares;
            set => SetProperty(ref _wares, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public ICommand LoadCartCommand { get; }
        public ICommand ToggleSelectCommand { get; }

        public DocItemCheckVM()
        {
            LoadCartCommand = new Command(async () => await LoadCartAsync());
            ToggleSelectCommand = new Command<DocWaresEx>(ToggleSelect);

            LoadSampleData();
        }

        private void LoadSampleData()
        {
            var DId = new DocId() { NumberDoc = DateTime.Now.ToString("yyyyMMdd"), TypeDoc = 6 };
            var xx = db.GetDocWares(DId, eTypeResult.All, eTypeOrder.Scan);
            Wares = new ObservableCollection<DocWaresEx>(xx);
            //    Products =
            //    [
            //        new(1, "Ш-д \"Мілка\" з карамарах начинкою і повітр рисом 276г /Монделіс/", 334, 8, true),
            //    new(2, "Ш-д \"Корона\" Max Fun мол з мармеладом 150г /Монделіс/", 45, 8, true),
            //    new(3, "Ш-д \"Несквік\" молочний 100г /Нестле/", 45, 8, true),
            //    new(4, "Халва соняш. ванільна 60г/Ференро/", 45, 8, true),
            //    new(5, "Ц-ки \"Яблучні\" натуральні 20г/ФРУТІМ/", 45, 8, true),
            //];
        }

        private void ToggleSelect(DocWaresEx? item)
        {
            if (item is null) return;
            item.IsRecord = !item.IsRecord;
        }

        private async Task LoadCartAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                // TODO: замінити на реальний API-виклик
                await Task.Delay(1500); // симуляція завантаження
                                        // наприклад: var items = await _apiService.GetDisplayItemsAsync();
                                        // Products = new ObservableCollection<ProductDisplayItem>(items);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
