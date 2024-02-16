using BL.Connector;
using BRB5;
using BRB5.Model;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;


namespace BL
{
    public partial class BL
    {
        public void ChangeRaiting(RaitingDocItem vQuestion, string pButtonName,IEnumerable<RaitingDocItem> Questions)
        {
           
            var OldRating = vQuestion.Rating;
            switch (pButtonName)
            {
                case "Ok":
                    vQuestion.Rating = 1;
                    break;
                case "SoSo":
                    vQuestion.Rating = 2;
                    break;
                case "Bad":
                    vQuestion.Rating = 3;
                    break;
                case "NotKnow":
                    vQuestion.Rating = 4;
                    break;

                default:
                    vQuestion.Rating = 0;
                    break;
            }
            if (OldRating == vQuestion.Rating)
                vQuestion.Rating = 0;

            if (vQuestion.IsItem)
            {
                var el = Questions.FirstOrDefault(i => i.Id == vQuestion.Parent);
                if (el != null) el.Rating = 0;
            }


            if (OldRating != vQuestion.Rating && (vQuestion.Rating == 4 || vQuestion.Rating == 0) && vQuestion.IsHead)
            {
                foreach (var el in Questions.Where(d => d.Parent == vQuestion.Id))
                {
                    el.IsVisible = vQuestion.Rating != 4;
                    if (el.Rating == 0 && vQuestion.Rating == 4)
                    {
                        el.Rating = 4;
                        db.ReplaceRaitingDocItem(el);
                    }
                }
            }
            db.ReplaceRaitingDocItem(vQuestion);

        }
    }
}
