using BL.Connector;
using BRB5;
using BRB5.Model;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;


namespace BL
{
    public partial class BL
    {
        Timer t;
        Doc cDoc;

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

        public void LoadDataRDI(Doc pDoc, Action<IEnumerable<RaitingDocItem>> pA)
        {
            Task.Run(() =>
            {
                var Q = db.GetRaitingDocItem(pDoc);
                var R = new List<RaitingDocItem>();
                foreach (var e in Q.Where(d => d.IsHead).OrderBy(d => d.OrderRS))
                {
                    R.Add(e);
                    foreach (var el in Q.Where(d => d.Parent == e.Id).OrderBy(d => d.OrderRS))
                    {
                        if (e.Rating == 4)
                            el.IsVisible = false;
                        R.Add(el);
                    }
                }
                var Tottal = Q.Where(d => d.Id == -1).FirstOrDefault();
                if (Tottal != null) R.Add(Tottal);
                pA?.Invoke(R);
            });

        }

        public void InitTimerRDI(Doc pDoc)
        {
            cDoc = pDoc;
            t = new Timer(3 * 60 * 1000); //3 хв
            t.AutoReset = true;
            t.Elapsed += new ElapsedEventHandler(OnTimedEvent);
        }
        public void StartTimerRDI() => t?.Start();
        
        public void StopTimerRDI() => t?.Stop();
        

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            var task = Task.Run(() =>
            {
                Bl.c.SendRaitingFiles(cDoc?.NumberDoc, 1, 3 * 60, 10 * 60);
            });
        }

        public void SaveRDI(Doc pDoc,Action pAction)
        {
            Task.Run(() =>
            {
                Result res;
                try
                {
                    var r = db.GetRaitingDocItem(cDoc);
                    Doc d = db.GetDoc(cDoc);
                    res = c.SendRaiting(r, d);
                    if (res.State == 0)
                    {
                        cDoc.State = 1;
                        db.SetStateDoc(cDoc);
                    }
                }
                catch (Exception ex)
                { res = new Result(ex); }
                finally
                {
                    pAction?.Invoke();
                }
            }
            );
        }
    }
}
