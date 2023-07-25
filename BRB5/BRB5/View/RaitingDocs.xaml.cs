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
	public partial class RaitingDocs : ContentPage
	{
        private readonly TypeDoc TypeDoc;
        DB db = DB.GetDB();
        private ObservableCollection<Doc> _RD;
        public ObservableCollection<Doc> RD { get { return _RD; } set { _RD = value; OnPropertyChanged(nameof(RD)); } }

        public RaitingDocs (TypeDoc vTypeDoc)
		{
			InitializeComponent ();
            TypeDoc = vTypeDoc;
            TypeDoc.CodeDoc = 11;

            RD = new ObservableCollection<Doc>(db.GetDoc(TypeDoc));
            this.BindingContext = this;
        }

        private async void Create(object sender, EventArgs e)
        {

            await Navigation.PushAsync(new RaitingEdit(new Doc() { DateDoc = DateTime.Today }, TypeDoc));
        }

        private async void Edit(object sender, EventArgs e)
        {
            ImageButton cc = sender as ImageButton;
            var vDoc = cc.BindingContext as Doc;
            await Navigation.PushAsync(new RaitingEdit(vDoc, TypeDoc));
        }
    }
}