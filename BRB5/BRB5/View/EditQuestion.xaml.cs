using BRB5.Model;
using System;
using System.Collections.Generic;

namespace BRB5.View
{
    public partial class EditQuestion
    {

        private Raiting _RQ;
        public Raiting RQ { get { return _RQ; } set { _RQ = value; OnPropertyChanged(nameof(RQ)); } }

        DB db = DB.GetDB();
        private DocId DocId;

        public EditQuestion(Raiting rq)
        {
            DocId = new DocId();
            DocId.NumberDoc = rq.NumberDoc;
            DocId.TypeDoc = rq.TypeDoc;
            RQ = rq;
            RQ.IsTemplate = true;

            this.BindingContext = this;
            InitializeComponent();
        }

        private async void Save(object sender, EventArgs e)
        {
            db.ReplaceRaitingSample(new List<Raiting>(){RQ});
            await Navigation.PushAsync(new CreateRatingSample(int.Parse(RQ.NumberDoc)));
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