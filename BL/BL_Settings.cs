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
            string ApiUrl2, string ApiUrl3, int Compress, eCompany SelectedCompany, eTypeLog SelectedTypeLog, ePhotoQuality SelectedPhotoQuality, 
            eTypeUsePrinter SelectedTypePrinter, int SelectedWarehouse, int ListWarehouseCode, ObservableCollection<Warehouse> Warehouses)
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
            var result = new string[3];

            switch (Config.Company)
            {
                case eCompany.NotDefined:
                    result[0] = "";
                    result[1] = "";
                    result[2] = "";
                    break;
                case eCompany.Sim23:

                    result[0] = "http://93.183.216.37:80/dev1/hs/TSD/";//
                    result[1] = "http://37.53.84.148/TK/hs/TSD/";// "http://93.183.216.37/TK/hs/TSD/;http://37.53.84.148/TK/hs/TSD/";
                    result[2] = "https://bitrix.sim23.ua/rest/233/ax02yr7l9hia35vj/";
                    break;
                case eCompany.Sim23FTP:
                    result[0] = "";
                    result[1] = "";
                    result[2] = "";
                    break;
                case eCompany.VPSU:
                case eCompany.SparPSU:
                    result[0] = "https://apitest.spar.uz.ua/";
                    result[1] = "";
                    result[2] = "";
                    break;
            }
            return result;
        }
    }
}
