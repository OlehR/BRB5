using BRB5.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BRB5.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TemplateRaiting : ContentPage        
    {

        public ObservableCollection<RaitingTemplate> RTemplate { get; set; }

        DB db = DB.GetDB();

        public TemplateRaiting()
        {
            InitializeComponent();
        }

        private void OnHiddenClick(object sender, EventArgs e)
        {

        }

        private async void Create(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CreateRaitingTemplate(db.GetIdRaitingTemplate()));
        }
    }
}