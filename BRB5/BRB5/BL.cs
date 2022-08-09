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
    }
}
