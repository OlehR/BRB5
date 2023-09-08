using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BRB5.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WareInfo : ContentPage
    {
        DB db = DB.GetDB();
        public WareInfo()
        {
            InitializeComponent();
            this.BindingContext = this;
        }
    }
}