using BRB5.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace BRB5.View
{
    public partial class RaitingTemplateItemCreate
    {
        private ObservableCollection<RaitingTemplateItem> _RS;
        public ObservableCollection<RaitingTemplateItem> RS { get { return _RS; } set { _RS = value; OnPropertyChanged(nameof(RS)); } }
        private RaitingTemplateItem Draged;
        private bool ShowDeleted = false;

        //private DocId DocId;

        RaitingTemplate RT;
        DB db = DB.GetDB();

        public RaitingTemplateItemCreate(int pId)
        {
            InitializeComponent();            
            RT= new RaitingTemplate() { IdTemplate = pId };
            
            this.BindingContext = this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            RS = SortRS(db.GetRaitingTemplateItem(RT));
        }
        private ObservableCollection<RaitingTemplateItem> SortRS (IEnumerable<RaitingTemplateItem> temp)
        {
            var res = new List<RaitingTemplateItem>();

            foreach (RaitingTemplateItem r in temp.Where(rs => rs.Parent == 0).OrderBy(el => el.OrderRS))
            {
                res.Add(r);
                res.AddRange(temp.Where(rs => rs.Parent == r.Id).OrderBy(el => el.OrderRS));                
            }
            if (!ShowDeleted) foreach (RaitingTemplateItem r in temp)  r.IsVisible = !r.IsDelete;

            return new ObservableCollection<RaitingTemplateItem>(res);
        }


        private void OnDrop(object sender, DropEventArgs e)
        {
            var b = sender as DropGestureRecognizer;
            var s = b.Parent as Grid;
            var Droped = s.BindingContext as RaitingTemplateItem;

            if(Draged.IsItem) DragDropItem(Droped);   
            else DragDropHead(Droped);
        }

        private void OnDrag(object sender, DragStartingEventArgs e)
        {
            var b = sender as DragGestureRecognizer;
            var s = b.Parent as Grid;
            Draged = s.BindingContext as RaitingTemplateItem;
        }

        private void DragDropHead(RaitingTemplateItem Droped)
        {
            if(Droped.IsItem){
                var temp=RS.Where(rs => rs.Parent == Droped.Id).FirstOrDefault();
                if (temp!= null) Droped=temp;
            }

            foreach (var el in RS.Where(rs => rs.Parent == 0 && rs.OrderRS > Droped.OrderRS)) el.OrderRS += 1;

            Draged.OrderRS = Droped.OrderRS + 1;
            RS = SortRS(RS);
        }
        private void DragDropItem(RaitingTemplateItem Droped)
        {
            var dropedIndex = RS.IndexOf(Droped);

            if (Droped.IsHead) Draged.Parent = Droped.Id;
            else Draged.Parent = Droped.Parent;

            List<RaitingTemplateItem> temp = new List<RaitingTemplateItem>(RS);
            temp.Remove(Draged);
            temp.Insert(dropedIndex, Draged);
            int i = 1;
            foreach (RaitingTemplateItem r in temp)
            {
                r.OrderRS = i;
                i++;
            }
            RS.Clear();
            RS = new ObservableCollection<RaitingTemplateItem>(temp);

        }

        private void Click(object sender, EventArgs e)
        {
            var ss = sender as Grid;
            var rs = ss.BindingContext as RaitingTemplateItem;

            if (rs.IsHead)
            {
                var temp = RS.Where(r => r.Parent == rs.Id);
                foreach (RaitingTemplateItem r in temp) r.IsVisible = !r.IsVisible && !r.IsDelete;
            }
        }

        private void Save(object sender, EventArgs e)
        {
            db.ReplaceRaitingTemplateItem(RS);
        }

        private async void Edit(object sender, EventArgs e)
        {
            var b = sender as ImageButton;
            var s = b.Parent as Grid;

            var vRaiting = s.BindingContext as RaitingTemplateItem;
            if(!vRaiting.IsDelete) await Navigation.PushAsync(new RaitingTemplateEditQuestion(vRaiting));
        }

        private async void AddHead(object sender, EventArgs e)
        {
            var vRaiting = new RaitingTemplateItem
            {
                Id = db.GetIdRaitingTemplateItem(RT),
                OrderRS = db.GetIdRaitingTemplateItem(RT),
                IdTemplate = RT.IdTemplate,                
                IsEnableBad = true,
                IsEnableSoSo = true,
                IsEnableNotKnow = true,
                IsEnableOk = true,
                Parent = 0
            };
            await Navigation.PushAsync(new RaitingTemplateEditQuestion(vRaiting));
        }

        private async void AddItem(object sender, EventArgs e)
        {
            var b = sender as ImageButton;
            var s = b.Parent as Grid;

            var temp = s.BindingContext as RaitingTemplateItem;

            var vRaiting = new RaitingTemplateItem
            {
                Id = db.GetIdRaitingTemplateItem(RT),
                OrderRS = db.GetIdRaitingTemplateItem(RT),
                IdTemplate = RT.IdTemplate,
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
            var vRaiting = s.BindingContext as RaitingTemplateItem;

            if(vRaiting.IsDelete)
            {
                if (vRaiting.IsItem && RS.Where(rs => rs.Id == vRaiting.Parent).FirstOrDefault().IsDelete) return;

                vRaiting.DTDelete = default;
                db.ReplaceRaitingTemplateItem(RS);
                RS.Clear();
                RS = SortRS(db.GetRaitingTemplateItem(RT));
                return;
            }

            var Question = "Ви точно хочете видалти ";
            if (vRaiting.IsHead) Question += "групу '" + vRaiting.Text + "' з " + RS.Where(rs => rs.Parent == vRaiting.Id).Count() + " питаннями";
            else Question += "питання '" + vRaiting.Text + "'";
            

            if (await DisplayAlert("Видалення", Question, "Видалити", "Ні"))
            {
                vRaiting.DTDelete = DateTime.Now;

                if (vRaiting.IsHead)
                    foreach (RaitingTemplateItem r in RS.Where(rs => rs.Parent == vRaiting.Id))
                    {
                        r.DTDelete = DateTime.Now;
                    }

                db.ReplaceRaitingTemplateItem(RS);

                RS.Clear();
                RS = SortRS(db.GetRaitingTemplateItem(RT));
            }
        }

        private void DeletedShow(object sender, EventArgs e)
        {
            ShowDeleted = !ShowDeleted;
            foreach (RaitingTemplateItem r in RS)
            {
                if (ShowDeleted) r.IsVisible = true;
                else r.IsVisible = !r.IsDelete;
            }
        }
    }
}