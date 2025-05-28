using BL.Connector;
using BRB5.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using UtilNetwork;
using Utils;


namespace BL
{
    public partial class BL
    {
        Timer t;
        DocVM cDoc;

        public void ChangeRaiting(RaitingDocItem vQuestion, string pButtonName, IEnumerable<RaitingDocItem> Questions)
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
            {
                if (vQuestion.IsHead && vQuestion.Rating == 4)
                    foreach (var el in Questions.Where(d => d.Parent == vQuestion.Id))
                    {
                        if (el.Rating == 4) el.Rating = 0;
                    }

                vQuestion.Rating = 0;
            }

            if (vQuestion.IsItem)
            {
                var el = Questions.FirstOrDefault(i => i.Id == vQuestion.Parent);
                if (el != null) el.Rating = 0;
            }


            if (OldRating != vQuestion.Rating && (vQuestion.Rating == 4 || vQuestion.Rating == 0) && vQuestion.IsHead)
            {
                vQuestion.IsVisible = vQuestion.Rating != 4;
                foreach (var el in Questions.Where(d => d.Parent == vQuestion.Id))
                {
                    if (el.Rating == 0 && vQuestion.Rating == 4)
                    {
                        el.Rating = 4;
                        db.ReplaceRaitingDocItem(el);
                    }
                }
            }
            db.ReplaceRaitingDocItem(vQuestion);

        }

        public void LoadDataRDI(DocVM pDoc, Action<IEnumerable<RaitingDocItem>> pA)
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
                        //if (e.Rating == 4)  el.IsVisible = false;
                        el.ParrentRDI = e;
                        R.Add(el);
                    }
                }
                var Tottal = Q.Where(d => d.Id == -1).FirstOrDefault();
                if (Tottal != null) R.Add(Tottal);
                pA?.Invoke(R);
            });

        }

        public void InitTimerRDI(DocVM pDoc)
        {
            cDoc = pDoc;
            t = new Timer(3 * 60 * 1000) { AutoReset = true }; //3 хв
            t.Elapsed += new ElapsedEventHandler(OnTimedEvent);
        }
        public void StartTimerRDI() => t?.Start();

        public void StopTimerRDI() => t?.Stop();

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            var task = Task.Run(() => Bl.c.SendRatingFilesAsync(cDoc?.NumberDoc, 1, 2 * 60, 5 * 60));
        }

        public void SaveRDI(DocVM pDoc, Action pAction)
        {
            Task.Run(async () =>
            {
                Result res;
                try
                {
                    var r = db.GetRaitingDocItem(pDoc);
                    DocVM d = db.GetDoc(pDoc);
                    res = await c.SendRatingAsync(r, d);
                    c.OnSave?.Invoke(res.TextError);
                    if (res.State == 0)
                    {
                        pDoc.State = 1;
                        db.SetStateDoc(pDoc);
                    }
                }
                catch (Exception ex)
                {
                    FileLogger.WriteLogMessage(this, "SaveRDI", ex);
                    res = new Result(ex);
                }
                finally
                {
                    pAction?.Invoke();
                }
            }
            );
        }

        public void ImportExcelRT(RaitingTemplate vRaitingTemplate, string resultFullPath)
        {

            var B = File.ReadAllBytes(resultFullPath);

            var cp1251 = Encoding.GetEncoding(1251);
            var textBytes = Encoding.Convert(cp1251, Encoding.UTF8, B);
            var text = Encoding.UTF8.GetString(textBytes);

            var t = text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            List<RaitingTemplateItem> RS = new List<RaitingTemplateItem>();

            foreach (var v in t)
            {
                var p = v.Split(';');
                if (p.Count() < 4)
                    break;
                var el = new RaitingTemplateItem();
                int temp = 0;

                Int32.TryParse(p[0], out temp);
                el.Id = temp;

                Int32.TryParse(p[1], out temp);
                el.Parent = temp;

                el.Text = p[3];
                if (!String.IsNullOrEmpty(p[2])) el.ValueRating = Convert.ToDecimal(p[2]);

                el.IdTemplate = vRaitingTemplate.IdTemplate;

                el.IsEnableBad = true;
                el.IsEnableSoSo = true;
                el.IsEnableNotKnow = true;
                el.IsEnableOk = true;
                RS.Add(el);
            }

            var tdi = db.ReplaceRaitingTemplateItem(RS);
        }

        public ObservableCollection<RaitingTemplate> DownloadRT(Result<IEnumerable<RaitingTemplate>> temp)
        {
          
                db.ReplaceRaitingTemplate(temp.Info);
                foreach (var el in temp.Info)
                {
                    if (el.Item.Any())
                        db.ReplaceRaitingTemplateItem(el.Item);
                }
                return new ObservableCollection<RaitingTemplate>(db.GetRaitingTemplate());
            
        }

        public ObservableCollection<RaitingTemplateItem> SortRS(IEnumerable<RaitingTemplateItem> temp, bool ShowDeleted)
        {
            var res = new List<RaitingTemplateItem>();

            foreach (RaitingTemplateItem r in temp.Where(rs => rs.Parent == 0).OrderBy(el => el.OrderRS))
            {
                res.Add(r);
                res.AddRange(temp.Where(rs => rs.Parent == r.Id).OrderBy(el => el.OrderRS));
            }
            if (!ShowDeleted) foreach (RaitingTemplateItem r in temp) r.IsVisible = !r.IsDelete;

            return new ObservableCollection<RaitingTemplateItem>(res);
        }

        public void CreateRTC(RaitingTemplate RT, bool AddTotal)
        {
            RT.IsActive = true;
            db.ReplaceRaitingTemplate(new List<RaitingTemplate>() { RT });

            if (AddTotal)
            {
                var temp = new RaitingTemplateItem() { IdTemplate = RT.IdTemplate, Id = -1, Parent = 9999999, Text = "Всього", RatingTemplate = 8, OrderRS = 9999999 };
                db.ReplaceRaitingTemplateItem(new List<RaitingTemplateItem>() { temp });
            }

        }

        public void CalcValueRating(IEnumerable<RaitingDocItem> All)
        {
            try
            {
                decimal res = 0;
                foreach (var q in All.Where(el => el.Parent == 0))
                {
                    res = All?.Where(e => e.Parent == q.Id)?.Sum(el => el.ValueRating) ?? 0;
                    q.ValueRating = res;
                    if (q.Rating != 4)
                    {
                        res = All?.Where(e => e.Parent == q.Id)?.Sum(el => el.SumValueRating) ?? 0;
                        q.SumValueRating = res;
                    }
                    else q.SumValueRating = 0;
                }
                var Total = All.Where(el => el.Id == -1).FirstOrDefault();
                if (Total != null)
                {
                    res = All?.Where(el => el.Parent == 0 && el.Id != -1)?.Sum(el => el.ValueRating) ?? 0;
                    Total.ValueRating = res;
                    res = All?.Where(el => el.Parent == 0 && el.Id != -1)?.Sum(el => el.SumValueRating) ?? 0;
                    Total.SumValueRating = res;
                }
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }
        public void CalcSumValueRating(RaitingDocItem pRDI, IEnumerable<RaitingDocItem> All)
        {
            try
            {
                decimal res = 0;
                var Head = All.Where(el => el.Id == pRDI.Parent).FirstOrDefault();
                if (Head != null)
                {
                    res = All?.Where(el => el.Parent == Head.Id)?.Sum(el => el.SumValueRating) ?? 0;
                    Head.SumValueRating = res;
                    Head.Rating = Head.Rating;
                }
                else
                {
                    if (pRDI.Rating == 4)
                    {
                        pRDI.SumValueRating = 0;
                        pRDI.Rating = pRDI.Rating;
                    }
                    if (pRDI.Rating == 0)
                    {
                        pRDI.SumValueRating = All?.Where(el => el.Parent == pRDI.Id)?.Sum(el => el.SumValueRating) ?? 0;
                        pRDI.Rating = pRDI.Rating;
                    }
                }

                var Total = All.Where(el => el.Id == -1).FirstOrDefault();
                if (Total != null)
                {
                    res = All?.Where(el => el.Parent == 0 && el.Id != -1)?.Sum(el => el.SumValueRating) ?? 0;
                    Total.SumValueRating = res;
                    Total.Rating = Total.Rating;
                }
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }
        
        public bool UnDeleteRTI(RaitingTemplateItem vRaiting, ObservableCollection<RaitingTemplateItem> RS)
        {
            if (vRaiting.IsItem && RS.Where(rs => rs.Id == vRaiting.Parent).FirstOrDefault().IsDelete) return false;

            vRaiting.DTDelete = default;
            db.ReplaceRaitingTemplateItem(RS);
            return true;

        }
        public void DeleteRTI(RaitingTemplateItem vRaiting, ObservableCollection<RaitingTemplateItem> RS)
        {
            vRaiting.DTDelete = DateTime.Now;

            if (vRaiting.IsHead)
                foreach (RaitingTemplateItem r in RS.Where(rs => rs.Parent == vRaiting.Id))
                {
                    r.DTDelete = DateTime.Now;
                }

            db.ReplaceRaitingTemplateItem(RS);

        }
        public void DragDropHead(RaitingTemplateItem Droped, ObservableCollection<RaitingTemplateItem> RS)
        {
            if (Droped.IsItem)
            {
                var temp = RS.Where(rs => rs.Parent == Droped.Id).FirstOrDefault();
                if (temp != null) Droped = temp;
            }

            foreach (var el in RS.Where(rs => rs.Parent == 0 && rs.OrderRS > Droped.OrderRS)) el.OrderRS += 1;

        }
        public List<RaitingTemplateItem> DragDropItem(RaitingTemplateItem Droped, ObservableCollection<RaitingTemplateItem> RS, RaitingTemplateItem Draged)
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
            return temp;

        }

        
    }
}
