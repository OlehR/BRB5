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
        private ObservableCollection<DocWaresEx> _products = [];
        private bool _isLoading;
        private string _title = "Лист викладки";

        public ObservableCollection<DocWaresEx> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
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
