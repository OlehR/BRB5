using BRB5.Model;
using System;
using System.Collections.Generic;

namespace BRB5.View
{
	public partial class RaitingTemplateCreate
	{
		private RaitingTemplate _RT;
		public RaitingTemplate RT { get { return _RT; } set { _RT = value; OnPropertyChanged(nameof(RT.Text)); } }
        public bool AddTotal { get; set; }
        public bool IsSoftKeyboard { get { return Config.IsSoftKeyboard; } }

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

            if (AddTotal)
            {
                var temp = new Model.RaitingTemplateItem() { IdTemplate = RT.IdTemplate, Id = -1, Parent = 9999999, Text = "Всього", RatingTemplate = 8, OrderRS = 9999999 };
                db.ReplaceRaitingTemplateItem(new List<RaitingTemplateItem>() { temp });
            }

            await Navigation.PopAsync();
        }
    }
}