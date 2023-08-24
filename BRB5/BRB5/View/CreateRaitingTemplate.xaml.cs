using BRB5.Model;
using System;
using System.Collections.Generic;

namespace BRB5.View
{
	public partial class CreateRaitingTemplate
	{
		private RaitingTemplate _RT;
		public RaitingTemplate RT { get { return _RT; } set { _RT = value; OnPropertyChanged(nameof(RT.Text)); } }

        DB db = DB.GetDB();
        public CreateRaitingTemplate (int id)
		{
            InitializeComponent();
            RT = new RaitingTemplate();
			RT.Id = id;

            BindingContext = this;
        }

        private async void Create(object sender, EventArgs e)
        {
			RT.IsActive = true;
			db.ReplaceRaitingTemplate(new List<RaitingTemplate>() { RT });

            await Navigation.PushAsync(new TemplateRating());
        }
    }
}