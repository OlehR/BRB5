using BRB5;
using BRB5.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Utils;

namespace BL
{
    public partial class BL
    {
        public void RefreshWarehouses(string tempAutomationId, bool tempIsChecked)
        {
            
            if (int.TryParse(tempAutomationId, out int code))
            {
                if (tempIsChecked)
                {
                    if (!Config.CodesWarehouses.Contains(code)) Config.CodesWarehouses.Add(code);
                }
                else if (Config.CodesWarehouses.Contains(code)) Config.CodesWarehouses.Remove(code);
            }
        }
        public void SaveSettings()
        {
            db.SetConfig<bool>("IsAutoLogin", Config.IsAutoLogin);
            db.SetConfig<bool>("IsVibration", Config.IsVibration);
            db.SetConfig<bool>("IsViewAllWH", Config.IsViewAllWH);
            db.SetConfig<bool>("IsSound", Config.IsSound);
            db.SetConfig<bool>("IsTest", Config.IsTest);
            db.SetConfig<bool>("IsFilterSave", Config.IsFilterSave);
            //db.SetConfig<bool>("IsFullScreenScan", IsFullScreenScan);

            db.SetConfig<string>("ApiUrl1", Config.ApiUrl1 ?? "");
            db.SetConfig<string>("ApiUrl2", Config.ApiUrl2 ?? "");
            db.SetConfig<string>("ApiUrl3", Config.ApiUrl3 ?? "");
            db.SetConfig<string>("ApiUrl4", Config.ApiUrl4 ?? "");
            db.SetConfig<int>("Compress", Config.Compress);

            db.SetConfig<eCompany>("Company", Config.Company);
            db.SetConfig<eTypeLog>("TypeLog", FileLogger.TypeLog);
            db.SetConfig<ePhotoQuality>("PhotoQuality", Config.PhotoQuality);
            db.SetConfig<eTypeUsePrinter>("TypeUsePrinter", Config.TypeUsePrinter);
            db.SetConfig<int>("CodeWarehouse", Config.CodeWarehouse);
            db.SetConfig<string>("CodesWarehouses", Config.CodesWarehouses.ToJSON());
        }

        public void GenApiUrl()
        {
            Config.ApiUrl1 = "";
            Config.ApiUrl2 = "";
            Config.ApiUrl3 = "";
            Config.ApiUrl4 = "";

            switch (Config.Company)
            {
                //case eCompany.Sim23FTP:
                case eCompany.NotDefined:
                    break;

                case eCompany.Sim23:
                    Config.ApiUrl1 = "http://10.100.0.34/;http://vpn.sim23.ua:6380/";
                    Config.ApiUrl2 = "http://qlik.sim23.ua/TK/hs/TSD/;http://vpn.sim23.ua/TK/hs/TSD/";  //"http://37.53.84.148/TK/hs/TSD/";// "http://93.183.216.37/TK/hs/TSD/;http://37.53.84.148/TK/hs/TSD/";
                    Config.ApiUrl3 = "https://bitrix.sim23.ua/rest/233/ax02yr7l9hia35vj/";
                    Config.ApiUrl4 = "http://93.183.216.37:80/dev1/hs/TSD/";
                    break;
                //case eCompany.VPSU:
                case eCompany.PSU:
                    Config.ApiUrl1 = "https://apitest.spar.uz.ua/";
                    break;

                case eCompany.Universal:
                    Config.ApiUrl1 = "https://dct.sim23.ua:5443/";
                    break;
            }
        }

        public Warehouse FindWhIP(IEnumerable<Warehouse> pWarehouses)
        {
            Warehouse res = null;
            if (pWarehouses == null)
                return res;
            try
            {
                string Ip = Config.NativeBase.GetIP();
                if (Ip == null)
                    return res;
                String[] IP = Ip.Split('.');//192.168.1.235
                if (IP.Length != 4)
                    return res;
                foreach (var el in pWarehouses)
                {
                    if (el.InternalIP == null) continue;
                    String[] WhIp = el.InternalIP.Split('.');
                    if (WhIp.Length != 4) continue;
                    if (IP[0].Equals(WhIp[0]) && IP[1].Equals(WhIp[1]) && IP[2].Equals(WhIp[2])) return el;
                }
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
            }
            return res;
        }
    }
}
