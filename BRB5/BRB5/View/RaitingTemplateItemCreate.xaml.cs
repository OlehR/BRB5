﻿using BL;
using BRB5.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

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
        BL.BL Bl = BL.BL.GetBL();

        public RaitingTemplateItemCreate(int pId)
        {
            InitializeComponent();            
            RT= new RaitingTemplate() { IdTemplate = pId };
            
            this.BindingContext = this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            RS = Bl.SortRS(db.GetRaitingTemplateItem(RT),ShowDeleted);
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
            Bl.DragDropHead(Droped, RS);
            Draged.OrderRS = Droped.OrderRS + 1;
            RS = Bl.SortRS(RS, ShowDeleted);
        }
        private void DragDropItem(RaitingTemplateItem Droped)
        {
            var temp = Bl.DragDropItem(Droped, RS, Draged);
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

        private void Save(object sender, EventArgs e)   {  db.ReplaceRaitingTemplateItem(RS);     }

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

            bool shouldUpdateRS = false;
            // unDel
            if(vRaiting.IsDelete)
            {
                shouldUpdateRS = Bl.UnDeleteRTI(vRaiting, RS);
            }
            else
            {
                var Question = "Ви точно хочете видалти ";
                if (vRaiting.IsHead) Question += "групу '" + vRaiting.Text + "' з " + RS.Where(rs => rs.Parent == vRaiting.Id).Count() + " питаннями";
                else Question += "питання '" + vRaiting.Text + "'";

                if (await DisplayAlert("Видалення", Question, "Видалити", "Ні"))
                {
                    Bl.DeleteRTI(vRaiting, RS);
                    shouldUpdateRS = true;
                }
            }

            if (shouldUpdateRS)
            {
                RS.Clear();
                RS = Bl.SortRS(db.GetRaitingTemplateItem(RT), ShowDeleted);
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