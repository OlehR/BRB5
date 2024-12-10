using BRB5;
using BRB5.Model;
using System;
using System.Collections.Generic;
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
                    case eCompany.Sim23FTP:
                        Instance = new ConnectorSE_FTP();
                        break;
                    case eCompany.SparPSU:
                    case eCompany.VPSU:
                        Instance = new ConnectorPSU();
                        break;
                    default:
                        Instance = new ConnectorPSU();
                        break;
                }
            }
            return Instance;
        }
    }
}
