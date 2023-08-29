using BRB5.Model;
using System;
using System.Collections.Generic;

namespace BRB5.View
{
	public partial class RaitingTemplateCreate
	{
		private RaitingTemplate _RT;
		public RaitingTemplate RT { get { return _RT; } set { _RT = value; OnPropertyChanged(nameof(RT.Text)); } }

        DB db = DB.GetDB();
        public RaitingTemplateCreate (int id)
		{
            InitializeComponent();
            RT = new RaitingTemplate();
			RT.IdTemplate = id;

            BindingContext = this;
        }

        private async void Create(object sender, EventArgs e)
        {
			RT.IsActive = true;
			db.ReplaceRaitingTemplate(new List<RaitingTemplate>() { RT });

            await Navigation.PushAsync(new RaitingTemplatesEdit());
        }
    }
}