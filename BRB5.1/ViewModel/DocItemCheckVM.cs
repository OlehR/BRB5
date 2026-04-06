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
        ForMVVM ForMVVM;
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
        public event Action<DocWaresEx>? ScrollToItem;

        private DocWaresEx? _selectedWare;
        public DocWaresEx? SelectedWare
        {
            get => _selectedWare;
            set
            {
                if (_selectedWare != null)
                    _selectedWare.IsSelected = false;   // знімаємо підсвітку з попереднього

                SetProperty(ref _selectedWare, value);

                if (_selectedWare != null)
                    _selectedWare.IsSelected = true;    // вмикаємо на новому
            }
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
        public DocItemCheckVM( ForMVVM pForMVVM)
        {
            ForMVVM = pForMVVM;
            LoadCartCommand = new Command(async () => await LoadCartAsync());
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

            SelectedWare = null;
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
                    SelectedWare = item;
                    ScrollToItem?.Invoke(item);
                }
                else
                {
                    ForMVVM.DisplayAlert("", "Даної позиції немає в листі поповнення", "OK");
                }
            }
            else
            {
                ForMVVM.DisplayAlert("", "Даний штрихкод відсутній в базі", "OK");
            }
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
