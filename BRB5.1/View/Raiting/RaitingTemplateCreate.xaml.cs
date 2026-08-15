using BL;
using BRB5.Model;
using System;
using System.Collections.Generic;

namespace BRB6.View
{
	public partial class RaitingTemplateCreate 
    {
		private RaitingTemplate _RT;
		public RaitingTemplate RT { get { return _RT; } set { _RT = value; OnPropertyChanged(nameof(RT.Text)); } }
        public bool AddTotal { get; set; }
        public bool IsSoftKeyboard { get { return Config.IsSoftKeyboard; } }

        DB db = DB.GetDB();
        BL.BL Bl = BL.BL.GetBL();
        public RaitingTemplateCreate (int id)
		{
            InitializeComponent();
            RT = new RaitingTemplate();
			RT.IdTemplate = id;

            BindingContext = this;
        }

        private async void Create(object sender, EventArgs e)
        {			
            Bl.CreateRTC(RT, AddTotal);
            await Navigation.PopAsync();
        }
    }
}