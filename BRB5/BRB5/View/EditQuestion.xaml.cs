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
    public partial class EditQuestion : ContentPage
    {

        private Raiting _RQ;
        public Raiting RQ { get { return _RQ; } set { _RQ = value; OnPropertyChanged(nameof(RQ)); } }
        DB db = DB.GetDB();
        private DocId DocId;
        public List<Raiting> ListHeads 
        { get
            {
                List<Raiting> lh = null;
                try {
                    var plh = db.GetRating(DocId);
                    lh = plh.Where(rs => rs.IsHead).ToList(); 
                }
                catch (Exception ex) { 
                    string msg = ex.Message; }

                return lh;
            }
        }

        public EditQuestion(int Id, DocId docId)
        {
            InitializeComponent();
            DocId = docId;
            RQ = new Raiting();
            RQ.Id = Id;
            RQ.OrderRS = Id;
            RQ.NumberDoc = docId.NumberDoc;
            RQ.TypeDoc = docId.TypeDoc;


            this.BindingContext = this;
        }

        private void Save(object sender, EventArgs e)
        {
            db.ReplaceRaitingSample(new List<Raiting>(){RQ});
        }
        //Parent, IsHead, Text, RatingTemplate, 
        // TypeDoc, NumberDoc, Id, OrderRS
    }
}