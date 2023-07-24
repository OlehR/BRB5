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

        private int _SelectedTemplate;
        public int SelectedTemplate { get { return _SelectedTemplate; } set { _SelectedTemplate = value; OnPropertyChanged(nameof(SelectedTemplate)); } }
        public List<RaitingTemplate> RT { get { return db.GetRaitingTemplate().ToList(); } }
        public Doc RD { get { return _RD; } set { _RD = value; OnPropertyChanged(nameof(RD)); } }
        public RaitingEdit (Doc doc)
		{
			InitializeComponent ();
            RD = doc;

            this.BindingContext = this;
        }

        private void Save(object sender, EventArgs e)
        {

        }
    }
}