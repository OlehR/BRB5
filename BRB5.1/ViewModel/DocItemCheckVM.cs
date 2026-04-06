using BL;
using BL.Connector;
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
        Connector c = ConnectorBase.GetInstance();
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
        public event Action<DocWaresEx>? ScrollToItem;

        private DocWaresEx? _selectedWare {  get; set; }
        private DocWaresEx? _selectedItem;
        public DocWaresEx? SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }
        private bool _isMrDialogVisible;
        private decimal _mrQuantity;

        public bool IsMrDialogVisible
        {
            get => _isMrDialogVisible;
            set => SetProperty(ref _isMrDialogVisible, value);
        }

        public decimal MrQuantity
        {
            get => _mrQuantity;
            set => SetProperty(ref _mrQuantity, value);
        }

        public ICommand OpenDialogCommand { get; }
        public ICommand IncrementCommand { get; }
        public ICommand DecrementCommand { get; }
        public ICommand ConfirmMrCommand { get; }
        public DocItemCheckVM()
        {
            LoadCartCommand = new Command(async () => await LoadCartAsync());
            ToggleSelectCommand = new Command<DocWaresEx>(ToggleSelect);
            OpenDialogCommand = new Command<DocWaresEx>(OpenDialog);
            IncrementCommand = new Command(() => MrQuantity++);
            DecrementCommand = new Command(() => { if (MrQuantity > 0) MrQuantity--; });
            ConfirmMrCommand = new Command(ConfirmDialog);
            LoadSampleData();
        }
        private void OpenDialog(DocWaresEx? item)
        {
            if (item is null) return;
            _selectedWare = item;
            MrQuantity = item.InputQuantity;
            IsMrDialogVisible = true;
        }

        private void ConfirmDialog()
        {
            if (_selectedWare is null) return;

            _selectedWare.InputQuantity = MrQuantity;

            if (_selectedWare.InputQuantity > 0) _selectedWare.Quantity = _selectedWare.InputQuantity;
            _selectedWare.OnPropertyChanged("IsInputQuantity");

            _selectedWare = null;
            IsMrDialogVisible = false;
        }
        private void LoadSampleData()
        {
            var DId = new DocId() { NumberDoc = DateTime.Now.ToString("yyyyMMdd"), TypeDoc = 6 };
            var xx = db.GetDocWares(DId, eTypeResult.All, eTypeOrder.Scan);
            Wares = new ObservableCollection<DocWaresEx>(xx);

            //Wares = new ObservableCollection<DocWaresEx>( Enumerable.Repeat(xx, 3).SelectMany(x => x));            
        }

        public void BarCode(string pBarCode)
        {
            var pb = c.ParsedBarCode(pBarCode, false);
            var r=db.GetCodeWares(pb);
            if (r.CodeWares != 0)
            {
                var item = Wares.FirstOrDefault(w => w.CodeWares == r.CodeWares);
                if (item != null)
                {
                    SelectedItem = item;
                    ScrollToItem?.Invoke(item);
                }
                else
                { 
                                    
                }
            }
        }

        private void ToggleSelect(DocWaresEx? item)
        {
            if (item is null) return;
            item.IsInputQuantity = !item.IsInputQuantity;
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
