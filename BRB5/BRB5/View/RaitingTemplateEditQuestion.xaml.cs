﻿using BL;
using BRB5.Model;
using System;
using System.Collections.Generic;

namespace BRB5.View
{
    public partial class RaitingTemplateEditQuestion
    {
        public bool IsSoftKeyboard { get { return Config.IsSoftKeyboard; } }

        private Model.RaitingTemplateItem _RQ;
        public RaitingTemplateItem RQ { get { return _RQ; } set { _RQ = value; OnPropertyChanged(nameof(RQ)); } }

        DB db = DB.GetDB();
        private RaitingTemplateItem RT;

        public RaitingTemplateEditQuestion(RaitingTemplateItem rq)
        {          
            RQ = rq;  
            RQ.IsTemplate=true;

            this.BindingContext = this;
            InitializeComponent();
        }

        private async void Save(object sender, EventArgs e)
        {
            db.ReplaceRaitingTemplateItem(new List<RaitingTemplateItem>() { RQ });
            await Navigation.PopAsync();
        }

        private void OnButtonClicked(object sender, EventArgs e)
        {
            Xamarin.Forms.View button = (Xamarin.Forms.View)sender;
            switch (button.ClassId)
            {
                case "Ok":
                    RQ.IsEnableOk = !RQ.IsEnableOk;
                    break;
                case "SoSo":
                    RQ.IsEnableSoSo = !RQ.IsEnableSoSo;
                    break;
                case "Bad":
                    RQ.IsEnableBad = !RQ.IsEnableBad;
                    break;
                case "NotKnow":
                    RQ.IsEnableNotKnow = !RQ.IsEnableNotKnow;
                    break;
            }
        }
    }
}