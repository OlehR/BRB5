using BRB5.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace BRB5
{
    public class BL
    {
        protected DB db = DB.GetDB();
        Connector.Connector c = Connector.Connector.GetInstance();
        public void SendLogPrice()
        {
            for (int i = 0; i < 20; i++)
            {  
                var List = db.GetSendData(100);              
                if (List == null && List.Count() == 0)
                    break;
               
                Result res = c.SendLogPrice(List);
                if (res.State == 0)
                {
                    try
                    {
                        db.AfterSendData();
                        //int[] varRes = db.GetCountScanCode();
                        //LI.AllScan = varRes[0];
                        //LI.BadScan = varRes[1];
                    }
                    catch (Exception e)
                    {
                        //Utils.WriteLog("e", TAG, "SendLogPricePSU  >>", e);
                        break;
                    }

                }
                else
                    break;
            }
            
        }

        // Зміна стану документа і відправляємо в 1С
       /* public Result UpdateDocState(int pState, int pTypeDoc, String pNumberDoc, DateTime pDateOutInvoice, String pNumberOutInvoice, int pIsClose)
        {
            DocSetting DS = Config.GetDocSetting(pTypeDoc);
            if (DS != null && !DS.IsMultipleSave)
            {
                int State = db.GetStateDoc(pTypeDoc, pNumberDoc);
                if (State >= 1)
                    return new Result(-2, "Даний документ не можна повторно зберігати!");
            }

            db.UpdateDocState(pState, pTypeDoc, pNumberDoc);
            List<WaresItemModel> wares = db.GetDocWares(pTypeDoc, pNumberDoc, (DS == null || DS.IsSaveOnlyScan ? 2 : 1), eTypeOrder.Scan);
            return c.SyncDocsData(pTypeDoc, pNumberDoc, wares, pDateOutInvoice, pNumberOutInvoice, pIsClose);

        }*/
    }
}
