using BRB5.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace BRB5.View
{
    public partial class CreateRatingTemplateItem
    {
        private ObservableCollection<Model.RaitingDocItem> _RS;
        public ObservableCollection<Model.RaitingDocItem> RS { get { return _RS; } set { _RS = value; OnPropertyChanged(nameof(RS)); } }
        private Model.RaitingDocItem Draged;
        private bool ShowDeleted = false;

        private DocId DocId;
        DB db = DB.GetDB();

        public CreateRatingTemplateItem(int id)
        {
            InitializeComponent();
            DocId = new DocId();
            DocId.NumberDoc = id.ToString();
            DocId.TypeDoc = -1;

            RS = SortRS(db.GetRaiting(DocId));

            this.BindingContext = this;
        }

        private ObservableCollection<Model.RaitingDocItem> SortRS (IEnumerable<Model.RaitingDocItem> temp)
        {
            var res = new List<Model.RaitingDocItem>();

            foreach (Model.RaitingDocItem r in temp.Where(rs => rs.Parent == 0).OrderBy(el => el.OrderRS))
            {
                res.Add(r);
                res.AddRange(temp.Where(rs => rs.Parent == r.Id).OrderBy(el => el.OrderRS));                
            }
            if (!ShowDeleted) foreach (Model.RaitingDocItem r in temp)  r.IsVisible = !r.IsDelete;

            return new ObservableCollection<Model.RaitingDocItem>(res);
        }


        private void OnDrop(object sender, DropEventArgs e)
        {
            var b = sender as DropGestureRecognizer;
            var s = b.Parent as Grid;
            var Droped = s.BindingContext as Model.RaitingDocItem;

            if(Draged.IsItem) DragDropItem(Droped);   
            else DragDropHead(Droped);
        }

        private void OnDrag(object sender, DragStartingEventArgs e)
        {
            var b = sender as DragGestureRecognizer;
            var s = b.Parent as Grid;
            Draged = s.BindingContext as Model.RaitingDocItem;
        }

        private void DragDropHead(Model.RaitingDocItem Droped)
        {
            if(Droped.IsItem){
                var temp=RS.Where(rs => rs.Parent == Droped.Id).FirstOrDefault();
                if (temp!= null) Droped=temp;
            }

            foreach (var el in RS.Where(rs => rs.Parent == 0 && rs.OrderRS > Droped.OrderRS)) el.OrderRS += 1;

            Draged.OrderRS = Droped.OrderRS + 1;
            RS = SortRS(RS);
        }
        private void DragDropItem(Model.RaitingDocItem Droped)
        {
            var dropedIndex = RS.IndexOf(Droped);

            if (Droped.IsHead) Draged.Parent = Droped.Id;
            else Draged.Parent = Droped.Parent;

            List<Model.RaitingDocItem> temp = new List<Model.RaitingDocItem>(RS);
            temp.Remove(Draged);
            temp.Insert(dropedIndex, Draged);
            int i = 1;
            foreach (Model.RaitingDocItem r in temp)
            {
                r.OrderRS = i;
                i++;
            }
            RS.Clear();
            RS = new ObservableCollection<Model.RaitingDocItem>(temp);

        }

        private void Click(object sender, EventArgs e)
        {
            var ss = sender as Grid;
            var rs = ss.BindingContext as Model.RaitingDocItem;

            if (rs.IsHead)
            {
                var temp = RS.Where(r => r.Parent == rs.Id);
                foreach (Model.RaitingDocItem r in temp) r.IsVisible = !r.IsVisible && !r.IsDelete;
            }
        }

        private void Save(object sender, EventArgs e)
        {
            db.ReplaceRaitingSample(RS);
        }

        private async void Edit(object sender, EventArgs e)
        {
            var b = sender as ImageButton;
            var s = b.Parent as Grid;

            var vRaiting = s.BindingContext as Model.RaitingDocItem;
            if(!vRaiting.IsDelete) await Navigation.PushAsync(new RaitingTemplateEditQuestion(vRaiting));
        }

        private async void AddHead(object sender, EventArgs e)
        {
            var vRaiting = new Model.RaitingDocItem
            {
                Id = db.GetIdRaitingSample(DocId),
                OrderRS = db.GetIdRaitingSample(DocId),
                NumberDoc = DocId.NumberDoc,
                TypeDoc = DocId.TypeDoc,
                IsEnableBad = true,
                IsEnableSoSo = true,
                IsEnableNotKnow = true,
                IsEnableOk = true,
                IsHead = true,
                Parent = 0
            };
            await Navigation.PushAsync(new RaitingTemplateEditQuestion(vRaiting));
        }

        private async void AddItem(object sender, EventArgs e)
        {
            var b = sender as ImageButton;
            var s = b.Parent as Grid;

            var temp = s.BindingContext as Model.RaitingDocItem;

            var vRaiting = new Model.RaitingDocItem
            {
                Id = db.GetIdRaitingSample(DocId),
                OrderRS = db.GetIdRaitingSample(DocId),
                NumberDoc = DocId.NumberDoc,
                TypeDoc = DocId.TypeDoc,
                IsEnableBad = true,
                IsEnableSoSo = true,
                IsEnableNotKnow = true,
                IsEnableOk = true,
                Parent = temp.Id
            };
            if (!temp.IsDelete) await Navigation.PushAsync(new RaitingTemplateEditQuestion(vRaiting));
        }

        private async void Delete(object sender, EventArgs e)
        {
            var b = sender as ImageButton;
            var s = b.Parent as Grid;
            var vRaiting = s.BindingContext as Model.RaitingDocItem;

            if(vRaiting.IsDelete)
            {
                vRaiting.DTDelete = default;
                db.ReplaceRaitingSample(RS);
                RS.Clear();
                RS = SortRS(db.GetRaiting(DocId));
                return;
            }

            var Question = "Ви точно хочете видалти ";
            if (vRaiting.IsHead) Question += "групу '" + vRaiting.Text + "' з " + RS.Where(rs => rs.Parent == vRaiting.Id).Count() + " питаннями";
            else Question += "питання '" + vRaiting.Text + "'";
            

            if (await DisplayAlert("Видалення", Question, "Видалити", "Ні"))
            {
                vRaiting.DTDelete = DateTime.Now;

                if (vRaiting.IsHead)
                    foreach (Model.RaitingDocItem r in RS.Where(rs => rs.Parent == vRaiting.Id))
                    {
                        r.DTDelete = DateTime.Now;
                    }

                db.ReplaceRaitingSample(RS);

                RS.Clear();
                RS = SortRS(db.GetRaiting(DocId));
            }
        }

        private void DeletedShow(object sender, EventArgs e)
        {
            ShowDeleted = !ShowDeleted;
            foreach (Model.RaitingDocItem r in RS)
            {
                if (ShowDeleted) r.IsVisible = true;
                else r.IsVisible = !r.IsDelete;
            }
        }
    }
}