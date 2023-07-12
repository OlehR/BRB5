using BRB5.Connector;
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
    public partial class CreateRaitingSample : ContentPage
    {
        private ObservableCollection<Raiting> _RS;
        public ObservableCollection<Raiting> RS { get { return _RS; } set { _RS = value; OnPropertyChanged(nameof(RS)); } }

        DB db = DB.GetDB();

        public CreateRaitingSample(int id)
        {
            InitializeComponent();
            var DocId = new DocId();
            DocId.NumberDoc = id.ToString();
            DocId.TypeDoc = -1;

            RS = new ObservableCollection<Raiting>(db.GetRating(DocId));
            this.BindingContext = this;
        }
    }
}