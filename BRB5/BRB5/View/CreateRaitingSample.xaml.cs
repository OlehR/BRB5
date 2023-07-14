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
        private Raiting Draged;

        private DocId DocId;
        DB db = DB.GetDB();

        public CreateRaitingSample(int id)
        {
            InitializeComponent();
            DocId = new DocId();
            DocId.NumberDoc = id.ToString();
            DocId.TypeDoc = -1;
                        
            RS = SortRS(db.GetRating(DocId));

            this.BindingContext = this;
        }

        private ObservableCollection<Raiting> SortRS (IEnumerable<Raiting> temp)
        {
            var res = new List<Raiting>();

            foreach (Raiting r in temp.Where(rs => rs.Parent == 0).OrderBy(el => el.OrderRS))
            {
                res.Add(r);
                res.AddRange(temp.Where(rs => rs.Parent == r.Id).OrderBy(el => el.OrderRS));                
            }

            return new ObservableCollection<Raiting>(res);
        }


        private void OnDrop(object sender, DropEventArgs e)
        {
            var b = sender as DropGestureRecognizer;
            var s = b.Parent as Grid;
            var Droped = s.BindingContext as Raiting;

            if(Draged.IsItem) DragDrop(Droped);          
        }

        private void OnDrag(object sender, DragStartingEventArgs e)
        {
            var b = sender as DragGestureRecognizer;
            var s = b.Parent as Grid;
            Draged = s.BindingContext as Raiting;
        }

        private void DragDrop(Raiting Droped)
        {
            var dropedIndex = RS.IndexOf(Droped);

            if (Droped.IsHead) Draged.Parent = Droped.Id;
            else Draged.Parent = Droped.Parent;

            List<Raiting> temp = new List<Raiting>(RS);
            temp.Remove(Draged);
            temp.Insert(dropedIndex, Draged);
            int i = 1;
            foreach (Raiting r in temp)
            {
                r.OrderRS = i;
                i++;
            }
            RS.Clear();
            RS= new ObservableCollection<Raiting>(temp);

        }

        private void Click(object sender, EventArgs e)
        {
            var ss = sender as Grid;
            var rs = ss.BindingContext as Raiting;

            if (rs.IsHead)
            {
                var temp = RS.Where(r => r.Parent == rs.Id);
                foreach (Raiting r in temp) r.IsVisible = !r.IsVisible;
            }
        }

        private void Save(object sender, EventArgs e)
        {
            db.ReplaceRaitingSample(RS);
        }

        private async void Edit(object sender, EventArgs e)
        {
            Button b = sender as Button;
            var s = b.Parent as Grid;

            var vRaiting = s.BindingContext as Raiting;
            await Navigation.PushAsync(new EditQuestion(vRaiting.Id, DocId));
        }

        private async void Add(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new EditQuestion(db.GetIdRaitingSample(DocId), DocId));
        }
    }
}