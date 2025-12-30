using BL;
using BL.Connector;
using BRB5.Model;
using System.Collections.ObjectModel;
using Grid = Microsoft.Maui.Controls.Grid;

namespace BRB6.View
{
    public partial class RaitingTemplatesEdit : ContentPage    
    {

        private ObservableCollection<RaitingTemplate> _RTemplate;
        public ObservableCollection<RaitingTemplate> RTemplate { get { return _RTemplate; } set { _RTemplate = value; OnPropertyChanged(nameof(RTemplate)); } }


        BL.BL Bl = BL.BL.GetBL();
        DB db = DB.GetDB();
        Connector c;
        private bool ShowHidden = false;

        public RaitingTemplatesEdit()
        {
            InitializeComponent();
            c = ConnectorBase.GetInstance();
            this.BindingContext = this;
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            RTemplate = new ObservableCollection<RaitingTemplate>(db.GetRaitingTemplate());
        }

        private void OnHiddenClick(object sender, EventArgs e)
        {
            ShowHidden = !ShowHidden;
            if (ShowHidden) foreach (var rt in  RTemplate) rt.IsHidden = true;
            else foreach (var rt in RTemplate) rt.IsHidden = rt.IsActive;

        }

        private async void Create(object sender, EventArgs e)  
        {
            var r = await c.GetIdRaitingTemplate();
            await Navigation.PushAsync(new RaitingTemplateCreate(r.Data)); 
        }

        private async void Edit(object sender, EventArgs e)
        {
            var b = sender as ImageButton;
            var s = b.Parent as Grid;

            var vRaitingTemplate = s.BindingContext as RaitingTemplate;
            await Navigation.PushAsync(new RaitingTemplateItemCreate(vRaitingTemplate.IdTemplate));
        }

        private async void Import(object sender, EventArgs e)
        {
            var b = sender as ImageButton;
            var s = b.Parent as Grid;

            var vRaitingTemplate = s.BindingContext as RaitingTemplate;
            var customFileType =
            new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
               { DevicePlatform.Android, new[] { "text/csv" } },
            });
            var options = new PickOptions
            {
                PickerTitle = "Please select a file",
                FileTypes = customFileType,
            };
            var result = await FilePicker.PickAsync(options);
            if (result != null) 
            {
                Bl.ImportExcelRT(vRaitingTemplate, result.FullPath);

            }
        }
                
        private async void SaveRaiting(object sender, EventArgs e)
        {
            var b = sender as ImageButton;
            var s = b.Parent as Grid;

            var vRaitingTemplate = s.BindingContext as RaitingTemplate;
            db.ReplaceRaitingTemplate(new List<RaitingTemplate>() { vRaitingTemplate });
            vRaitingTemplate.Item = db.GetRaitingTemplateItem(vRaitingTemplate);
            var r = await c.SaveTemplate(vRaitingTemplate);
            _ = DisplayAlert("збереження", r?.TextError, "OK");
        }

        private async void Download(object sender, EventArgs e)
        {
            var tempbool = await DisplayAlert("Завантаження", "Не збережені зміни можуть бути видалені", "OK" , "Cancel");
            if (tempbool)
            {
                var temp = await c.GetRaitingTemplateAsync();
                if (temp.Data == null)
                    _ = DisplayAlert("Помилка", temp.TextError, "OK");
                else
                {
                    RTemplate = Bl.DownloadRT(temp);
                }
            }
        }
    }
}