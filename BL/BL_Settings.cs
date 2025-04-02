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
        public void SaveSettings(bool IsAutoLogin, bool IsVibration, bool IsViewAllWH, bool IsSound, bool IsTest, bool IsFilterSave,/* bool IsFullScreenScan,*/ string ApiUrl1,
            string ApiUrl2, string ApiUrl3, string ApiUrl4, int Compress, eCompany SelectedCompany, eTypeLog SelectedTypeLog, ePhotoQuality SelectedPhotoQuality, 
            eTypeUsePrinter SelectedTypePrinter, int SelectedWarehouse, int ListWarehouseCode, IEnumerable<Warehouse> Warehouses)
        {
            db.SetConfig<bool>("IsAutoLogin", IsAutoLogin);
            db.SetConfig<bool>("IsVibration", IsVibration);
            db.SetConfig<bool>("IsViewAllWH", IsViewAllWH);
            db.SetConfig<bool>("IsSound", IsSound);
            db.SetConfig<bool>("IsTest", IsTest);
            db.SetConfig<bool>("IsFilterSave", IsFilterSave);
            //db.SetConfig<bool>("IsFullScreenScan", IsFullScreenScan);

            db.SetConfig<string>("ApiUrl1", ApiUrl1 ?? "");
            db.SetConfig<string>("ApiUrl2", ApiUrl2 ?? "");
            db.SetConfig<string>("ApiUrl3", ApiUrl3 ?? "");
            db.SetConfig<string>("ApiUrl4", ApiUrl4 ?? "");
            db.SetConfig<int>("Compress", Compress);

            db.SetConfig<eCompany>("Company", SelectedCompany);
            db.SetConfig<eTypeLog>("TypeLog", SelectedTypeLog);
            db.SetConfig<ePhotoQuality>("PhotoQuality", SelectedPhotoQuality);
            db.SetConfig<eTypeUsePrinter>("TypeUsePrinter", SelectedTypePrinter);
            if (SelectedWarehouse > -1) db.SetConfig<int>("CodeWarehouse", ListWarehouseCode);
            db.SetConfig<string>("CodesWarehouses", Warehouses.Where(el => el.IsChecked == true).Select(el => el.CodeWarehouse).ToList().ToJSON());
        }

        public string[] GenApiUrl()
        {
            var result = new string[4];
            result[0] = "";
            result[1] = "";
            result[2] = "";
            result[3] = "";
            switch (Config.Company)
            {
                //case eCompany.Sim23FTP:
                case eCompany.NotDefined:
                    break;
                case eCompany.Sim23:
                    result[0] = "http://10.100.0.34/;http://vpn.sim23.ua:6380/"; 
                    result[1] = "http://qlik.sim23.ua/TK/hs/TSD/;http://vpn.sim23.ua/TK/hs/TSD/";  //"http://37.53.84.148/TK/hs/TSD/";// "http://93.183.216.37/TK/hs/TSD/;http://37.53.84.148/TK/hs/TSD/";
                    result[2] = "https://bitrix.sim23.ua/rest/233/ax02yr7l9hia35vj/";
                    result[3] = "http://93.183.216.37:80/dev1/hs/TSD/";
                    break; 
                //case eCompany.VPSU:
                case eCompany.PSU:
                    result[0] = "https://apitest.spar.uz.ua/";                    
                    break;
                case eCompany.Universal:
                    result[0] = "http://vpn.sim23.ua:6380/";
                    break;
            }
            return result;
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
