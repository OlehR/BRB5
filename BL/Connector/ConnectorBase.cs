using BRB5;
using BRB5.Model;
using BRB5.Model.DB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BL.Connector
{
    public class ConnectorBase : BRB5.Model.Connector
    {
        protected static string TAG = "BRB5/ConnectorBase";

        protected DB db = DB.GetDB();
        protected GetDataHTTP Http = GetDataHTTP.GetInstance();

        public static BRB5.Model.Connector GetInstance()
        {
            if (Instance == null || Instance is BRB5.Model.Connector)
            {
                switch (Config.Company)
                {
                    case eCompany.Sim23:
                        Instance = new ConnectorSE();
                        break;
                    //case eCompany.Sim23FTP:
                    //    Instance = new ConnectorSE_FTP();
                    //    break;
                    case eCompany.PSU:
                    //case eCompany.VPSU:
                        Instance = new ConnectorPSU();
                        break;
                    default:
                        Instance = new ConnectorPSU();
                        break;
                }
            }
            return Instance;
        }

        protected bool SaveGuide(BRB5.Model.Guid pG, bool pIsFull)
        {
            Config.OnProgress?.Invoke(0.60);
            if (pG.Wares?.Any() == true)
                db.ReplaceWares(pG.Wares, pIsFull);
            //Log.d(TAG, "Nomenclature");
            Config.OnProgress?.Invoke(0.70);
            if (pG.AdditionUnit?.Any() == true)
                db.ReplaceAdditionUnit(pG.AdditionUnit, pIsFull);
            //Log.d(TAG, "Units");
            Config.OnProgress?.Invoke(0.80);
            if (pG.BarCode?.Any() == true)
                db.ReplaceBarCode(pG.BarCode, pIsFull);
            if (pG.Warehouse?.Any() == true)
                db.ReplaceWarehouse(pG.Warehouse);
            Config.OnProgress?.Invoke(0.87);
            //Log.d(TAG, "Barcodes");
            Config.OnProgress?.Invoke(0.90);
            if (pG.UnitDimension?.Any() == true)
                db.ReplaceUnitDimension(pG.UnitDimension, pIsFull);
            //Log.d(TAG, "GroupWares");
            Config.OnProgress?.Invoke(0.95);
            if (pG.GroupWares?.Any() == true)
                db.ReplaceGroupWares(pG.GroupWares, pIsFull);
            Config.OnProgress?.Invoke(0.97);
            if (pG.Reason?.Any() == true)
                db.ReplaceReason(pG.Reason, pIsFull);
            return true;
        }
    }
}
