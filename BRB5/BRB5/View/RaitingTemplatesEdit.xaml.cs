using BRB5.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace BRB5.View
{
    public partial class RaitingTemplatesEdit        
    {

        private ObservableCollection<RaitingTemplate> _RTemplate;
        public ObservableCollection<RaitingTemplate> RTemplate { get { return _RTemplate; } set { _RTemplate = value; OnPropertyChanged(nameof(RTemplate)); } }

        DB db = DB.GetDB();
        BRB5.Connector.Connector c;
        private bool ShowHidden = false;

        public RaitingTemplatesEdit()
        {
            InitializeComponent();
            c = Connector.Connector.GetInstance();
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
            await Navigation.PushAsync(new RaitingTemplateCreate(c.GetIdRaitingTemplate().Info));
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
                var text = File.ReadAllText(result.FullPath);
                var t = text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                RaitingTemplateItem[] RS = new RaitingTemplateItem[t.Length];
                int i = 0;
                foreach (var v in t)
                {
                    var p = v.Split(',');
                    RS[i] = new RaitingTemplateItem();
                    int temp = 0;

                    Int32.TryParse(p[0], out temp);
                    RS[i].Id = temp;

                    Int32.TryParse(p[1], out temp);
                    RS[i].Parent = temp;

                    RS[i].Text = p[2];
                    if (!String.IsNullOrEmpty(p[3])) RS[i].ValueRating = Convert.ToDecimal(p[3]);

                    RS[i].IdTemplate = vRaitingTemplate.IdTemplate;

                    RS[i].IsEnableBad = true;
                    RS[i].IsEnableSoSo = true;
                    RS[i].IsEnableNotKnow = true;
                    RS[i].IsEnableOk = true;
                    i++;
                }

                var tdi = db.ReplaceRaitingTemplateItem(RS);
            }
        }
                
        private void SaveRaiting(object sender, EventArgs e)
        {
            var b = sender as ImageButton;
            var s = b.Parent as Grid;

            var vRaitingTemplate = s.BindingContext as RaitingTemplate;
            db.ReplaceRaitingTemplate(new List<RaitingTemplate>() { vRaitingTemplate });
            vRaitingTemplate.Item = db.GetRaitingTemplateItem(vRaitingTemplate);
            _ = DisplayAlert("збереження", c.SaveTemplate(vRaitingTemplate).TextError, "OK");
        }

        private async void Download(object sender, EventArgs e)
        {
            var tempbool = await DisplayAlert("Завантаження", "Не збережені зміни можуть бути видалені", "OK" , "Cancel");
            if (tempbool)
            {
                var temp = c.GetRaitingTemplate();
                if (temp.Info == null)
                    _ = DisplayAlert("Помилка", temp.TextError, "OK");
                else
                {
                    db.ReplaceRaitingTemplate(temp.Info);
                    foreach(var el in temp.Info) {
                        if(el.Item.Any())
                          db.ReplaceRaitingTemplateItem(el.Item);
                    }
                    RTemplate = new ObservableCollection<RaitingTemplate>(db.GetRaitingTemplate());
                }
            }
        }
    }
}