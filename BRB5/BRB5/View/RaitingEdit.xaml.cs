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
	public partial class RaitingEdit : ContentPage
	{
        private Doc _RD;
        DB db = DB.GetDB();
        private readonly TypeDoc TypeDoc;

        private RaitingTemplate _SelectedTemplate;
        public RaitingTemplate SelectedTemplate { get { return _SelectedTemplate; } set { _SelectedTemplate = value; OnPropertyChanged(nameof(SelectedTemplate)); } }
        public List<RaitingTemplate> RT { get { return db.GetRaitingTemplate().ToList(); } }
        public Doc RD { get { return _RD; } set { _RD = value; OnPropertyChanged(nameof(RD)); } }
        public RaitingEdit (Doc doc, TypeDoc vTypeDoc)
		{
			InitializeComponent ();
            RD = doc;
            TypeDoc = vTypeDoc;

            this.BindingContext = this;
        }

        private async void Save(object sender, EventArgs e)
        {
            RD.IdTempate = SelectedTemplate.Id;
            await Navigation.PushAsync(new RaitingDocs(TypeDoc));

        }
    }
}