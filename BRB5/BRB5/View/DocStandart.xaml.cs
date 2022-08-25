using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BRB5.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DocStandart : ContentPage
    {
        public ICommand F3Scan => new Command(OnF3Scan);
        public DocStandart()
        {
            InitializeComponent();
        }
        private async void OnF3Scan()
        {
            await Navigation.PushAsync(new Scan());
        }
    }
}